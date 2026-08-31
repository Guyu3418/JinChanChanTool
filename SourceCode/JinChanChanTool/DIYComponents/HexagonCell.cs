using System.Drawing.Drawing2D;
using JinChanChanTool.DataClass;

namespace JinChanChanTool.DIYComponents
{
    /// <summary>
    /// 尖顶六边形格子控件。一个格子可堆叠多个英雄，并显示当前选中的顶层英雄。
    /// </summary>
    public class HexagonCell : Control
    {
        private const double SQRT3 = 1.7320508075688772;
        private const int LONG_PRESS_DELAY = 450;

        private int _row;
        private int _column;
        private readonly List<StackedHeroDisplay> _heroes = [];
        private int _displayIndex = -1;
        private readonly System.Windows.Forms.Timer _longPressTimer;
        private Point _mouseDownPoint;
        private bool _isMouseDown;

        private bool _isDropTarget;
        private readonly Color _emptyFillColor = Color.FromArgb(40, 45, 55);
        private readonly Color _occupiedFillColor = Color.FromArgb(50, 60, 75);
        private readonly Color _hoverColor = Color.FromArgb(70, 85, 100);
        private readonly Color _dropTargetColor = Color.FromArgb(80, 180, 80);
        private bool _isHovering;

        public int Row
        {
            get => _row;
            set => _row = value;
        }

        public int Column
        {
            get => _column;
            set => _column = value;
        }

        /// <summary>
        /// 当前显示的英雄，用于拖拽和右键取下。
        /// </summary>
        public LineUpUnit LineUpUnit => GetDisplayedHero()?.Unit;

        public IReadOnlyList<LineUpUnit> StackUnits => _heroes.Select(hero => hero.Unit).ToList();

        public bool IsDropTarget
        {
            get => _isDropTarget;
            set
            {
                if (_isDropTarget != value)
                {
                    _isDropTarget = value;
                    Invalidate();
                }
            }
        }

        public bool HasHero => GetDisplayedHero() != null;

        /// <summary>
        /// 长按是否已经触发轮盘。拖拽管理器用它区分普通左键点击。
        /// </summary>
        public bool IsLongPressTriggered { get; private set; }

        public event EventHandler<HeroPositionChangedEventArgs> HeroPositionChanged;
        public event EventHandler<HeroClearedEventArgs> HeroCleared;
        public event EventHandler<HeroDragStartEventArgs> HeroDragStart;
        public event EventHandler<HeroStackSelectionRequestedEventArgs> HeroStackSelectionRequested;

        public HexagonCell()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            Size = new Size(50, 58);

            _longPressTimer = new System.Windows.Forms.Timer { Interval = LONG_PRESS_DELAY };
            _longPressTimer.Tick += LongPressTimer_Tick;
        }

        public HexagonCell(int row, int column) : this()
        {
            _row = row;
            _column = column;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _longPressTimer.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateHexagonRegion();
        }

        private void UpdateHexagonRegion()
        {
            if (Width <= 0 || Height <= 0) return;

            using GraphicsPath path = GetHexagonPath();
            Region = new Region(path);
        }

        private PointF[] GetHexagonPoints()
        {
            float cx = Width / 2f;
            float cy = Height / 2f;
            float radius = Math.Min(Width / (float)SQRT3, Height / 2f) * 0.95f;
            PointF[] points = new PointF[6];

            for (int i = 0; i < 6; i++)
            {
                double angleRad = (-90 + i * 60) * Math.PI / 180;
                points[i] = new PointF(
                    cx + radius * (float)Math.Cos(angleRad),
                    cy + radius * (float)Math.Sin(angleRad));
            }

            return points;
        }

        private GraphicsPath GetHexagonPath()
        {
            GraphicsPath path = new GraphicsPath();
            path.AddPolygon(GetHexagonPoints());
            return path;
        }

        public bool IsPointInHexagon(Point point)
        {
            using GraphicsPath path = GetHexagonPath();
            return path.IsVisible(point);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            PointF[] hexPoints = GetHexagonPoints();
            StackedHeroDisplay displayedHero = GetDisplayedHero();

            Color fillColor = _isDropTarget
                ? _dropTargetColor
                : _isHovering
                    ? _hoverColor
                    : displayedHero != null
                        ? _occupiedFillColor
                        : _emptyFillColor;

            using (SolidBrush fillBrush = new SolidBrush(fillColor))
            {
                e.Graphics.FillPolygon(fillBrush, hexPoints);
            }

            if (displayedHero?.Image != null)
            {
                using GraphicsPath clipPath = GetHexagonPath();
                Region oldClip = e.Graphics.Clip;
                e.Graphics.SetClip(clipPath);
                e.Graphics.DrawImage(displayedHero.Image, 0, 0, Width, Height);
                e.Graphics.Clip = oldClip;
            }

            Color borderColor = displayedHero?.BorderColor ?? Color.FromArgb(80, 100, 120);
            int borderWidth = displayedHero == null ? 1 : 3;
            using (Pen borderPen = new Pen(borderColor, borderWidth))
            {
                e.Graphics.DrawPolygon(borderPen, hexPoints);
            }

            if (_heroes.Count > 1)
            {
                int badgeSize = Math.Max(16, Math.Min(22, Width / 3));
                Rectangle badgeBounds = new Rectangle(Width - badgeSize - 4, 4, badgeSize, badgeSize);
                using SolidBrush badgeBrush = new SolidBrush(Color.FromArgb(220, 20, 20, 20));
                using SolidBrush textBrush = new SolidBrush(Color.White);
                using Font badgeFont = new Font(Font.FontFamily, Math.Max(7, badgeSize / 2.2f), FontStyle.Bold);
                using StringFormat stringFormat = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                e.Graphics.FillEllipse(badgeBrush, badgeBounds);
                e.Graphics.DrawString(_heroes.Count.ToString(), badgeFont, textBrush, badgeBounds, stringFormat);
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovering = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovering = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left || !HasHero || !IsPointInHexagon(e.Location))
            {
                return;
            }

            _mouseDownPoint = e.Location;
            _isMouseDown = true;
            IsLongPressTriggered = false;
            _longPressTimer.Start();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_isMouseDown &&
                (Math.Abs(e.Location.X - _mouseDownPoint.X) > 5 || Math.Abs(e.Location.Y - _mouseDownPoint.Y) > 5))
            {
                _longPressTimer.Stop();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _isMouseDown = false;
            _longPressTimer.Stop();
            base.OnMouseUp(e);

            if (e.Button == MouseButtons.Right && HasHero && IsPointInHexagon(e.Location))
            {
                HeroCleared?.Invoke(this, new HeroClearedEventArgs(_row, _column, LineUpUnit));
            }
        }

        private void LongPressTimer_Tick(object sender, EventArgs e)
        {
            _longPressTimer.Stop();
            if (!_isMouseDown || !HasHero)
            {
                return;
            }

            IsLongPressTriggered = true;
            HeroStackSelectionRequested?.Invoke(this, new HeroStackSelectionRequestedEventArgs(this));
        }

        /// <summary>
        /// 以最近放置的英雄作为默认顶层英雄设置整个堆叠。
        /// </summary>
        public void SetHeroes(IEnumerable<StackedHeroDisplay> heroes)
        {
            _heroes.Clear();
            _heroes.AddRange(heroes.OrderBy(hero => hero.Unit.PositionLayer));
            _displayIndex = _heroes.Count - 1;
            Invalidate();
        }

        public void SetHero(LineUpUnit unit, Image image, Color borderColor)
        {
            SetHeroes([new StackedHeroDisplay(unit, image, borderColor)]);
        }

        public void Clear()
        {
            _heroes.Clear();
            _displayIndex = -1;
            IsLongPressTriggered = false;
            Invalidate();
        }

        /// <summary>
        /// 普通左键在同一站位的英雄间向下循环显示。
        /// </summary>
        public void CycleDisplayedHero()
        {
            if (_heroes.Count > 1)
            {
                _displayIndex = (_displayIndex - 1 + _heroes.Count) % _heroes.Count;
                Invalidate();
            }
            IsLongPressTriggered = false;
        }

        /// <summary>
        /// 轮盘选择指定英雄作为当前显示英雄。
        /// </summary>
        public void ShowHero(LineUpUnit unit)
        {
            int index = _heroes.FindIndex(hero => ReferenceEquals(hero.Unit, unit));
            if (index >= 0)
            {
                _displayIndex = index;
                Invalidate();
            }
        }

        public void InvokeHeroPositionChanged(int sourceRow, int sourceColumn, LineUpUnit movedUnit)
        {
            HeroPositionChanged?.Invoke(this, new HeroPositionChangedEventArgs(
                sourceRow, sourceColumn, _row, _column, movedUnit));
        }

        public void InvokeHeroDragStart()
        {
            HeroDragStart?.Invoke(this, new HeroDragStartEventArgs(this));
        }

        private StackedHeroDisplay GetDisplayedHero()
        {
            return _displayIndex >= 0 && _displayIndex < _heroes.Count ? _heroes[_displayIndex] : null;
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            // 父控件会在棋盘尺寸变化时重新布局格子。
        }
    }

    /// <summary>
    /// 棋盘格子中一个可显示的英雄层。
    /// </summary>
    public sealed class StackedHeroDisplay
    {
        public LineUpUnit Unit { get; }
        public Image Image { get; }
        public Color BorderColor { get; }

        public StackedHeroDisplay(LineUpUnit unit, Image image, Color borderColor)
        {
            Unit = unit;
            Image = image;
            BorderColor = borderColor;
        }
    }

    public class HeroPositionChangedEventArgs : EventArgs
    {
        public int SourceRow { get; }
        public int SourceColumn { get; }
        public int TargetRow { get; }
        public int TargetColumn { get; }
        public LineUpUnit MovedUnit { get; }

        public HeroPositionChangedEventArgs(int sourceRow, int sourceColumn, int targetRow, int targetColumn, LineUpUnit movedUnit)
        {
            SourceRow = sourceRow;
            SourceColumn = sourceColumn;
            TargetRow = targetRow;
            TargetColumn = targetColumn;
            MovedUnit = movedUnit;
        }
    }

    public class HeroClearedEventArgs : EventArgs
    {
        public int Row { get; }
        public int Column { get; }
        public LineUpUnit ClearedUnit { get; }

        public HeroClearedEventArgs(int row, int column, LineUpUnit clearedUnit)
        {
            Row = row;
            Column = column;
            ClearedUnit = clearedUnit;
        }
    }

    public class HeroDragStartEventArgs : EventArgs
    {
        public HexagonCell Cell { get; }

        public HeroDragStartEventArgs(HexagonCell cell)
        {
            Cell = cell;
        }
    }

    public class HeroStackSelectionRequestedEventArgs : EventArgs
    {
        public HexagonCell Cell { get; }

        public HeroStackSelectionRequestedEventArgs(HexagonCell cell)
        {
            Cell = cell;
        }
    }
}
