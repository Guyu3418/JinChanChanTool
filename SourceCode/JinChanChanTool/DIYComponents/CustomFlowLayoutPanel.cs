using System.ComponentModel;

namespace JinChanChanTool.DIYComponents
{
    /// <summary>
    /// 自定义FlowLayoutPanel，支持设置边框颜色和宽度
    /// </summary>
public class CustomFlowLayoutPanel : FlowLayoutPanel
{
private Color _borderColor = Color.Gray;
private int _borderWidth = 1;
private readonly Dictionary<Control, Point> _flowLocations = [];
private int _rowScrollOffset;

/// <summary>
/// 是否使用无滚动条的按行滚动。
/// </summary>
[DefaultValue(false)]
public bool EnableRowScroll { get; set; }

/// <summary>
/// 每次滚轮滚动的行高（设备像素）。
/// </summary>
[DefaultValue(1)]
public int RowScrollStep { get; set; } = 1;

/// <summary>
/// 每行显示的控件数量。
/// </summary>
[DefaultValue(1)]
public int RowScrollColumns { get; set; } = 1;

/// <summary>
/// 同时可见的行数。
/// </summary>
[DefaultValue(1)]
public int RowScrollVisibleRows { get; set; } = 1;

        /// <summary>
        /// 边框颜色
        /// </summary>
        [Category("自定义外观")]
        [Description("边框的颜色")]
        [DefaultValue(typeof(Color), "Gray")]
        public Color BorderColor
        {
            get => _borderColor;
            set
            {
                if (_borderColor != value)
                {
                    _borderColor = value;
                    Invalidate(); // 触发重绘
                }
            }
        }

        /// <summary>
        /// 边框宽度（像素）
        /// </summary>
        [Category("自定义外观")]
        [Description("边框的宽度（像素）")]
        [DefaultValue(1)]
        public int BorderWidth
        {
            get => _borderWidth;
            set
            {
                if (_borderWidth != value && value >= 0)
                {
                    _borderWidth = value;
                    Invalidate(); // 触发重绘
                }
            }
        }

public CustomFlowLayoutPanel()
{
            // 启用双缓冲以减少闪烁
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
UpdateStyles();
}

/// <summary>
/// 回到阵容展示的首行。
/// </summary>
public void ResetRowScroll()
{
if (!EnableRowScroll || _rowScrollOffset == 0)
{
return;
}

_rowScrollOffset = 0;
ApplyRowScrollOffset();
}

protected override void OnLayout(LayoutEventArgs levent)
{
base.OnLayout(levent);

if (!EnableRowScroll)
{
return;
}

CaptureFlowLocations();
_rowScrollOffset = Math.Min(_rowScrollOffset, GetMaxRowScrollOffset());
ApplyRowScrollOffset();
}

protected override void OnMouseWheel(MouseEventArgs e)
{
if (!EnableRowScroll)
{
base.OnMouseWheel(e);
return;
}

ScrollRows(e.Delta);
}

protected override void OnControlAdded(ControlEventArgs e)
{
base.OnControlAdded(e);
BindMouseWheel(e.Control);
}

private void BindMouseWheel(Control control)
{
control.MouseWheel -= ChildControl_MouseWheel;
control.MouseWheel += ChildControl_MouseWheel;
control.ControlAdded -= ChildControl_ControlAdded;
control.ControlAdded += ChildControl_ControlAdded;

foreach (Control child in control.Controls)
{
BindMouseWheel(child);
}
}

private void ChildControl_ControlAdded(object sender, ControlEventArgs e)
{
BindMouseWheel(e.Control);
}

private void ChildControl_MouseWheel(object sender, MouseEventArgs e)
{
if (EnableRowScroll)
{
ScrollRows(e.Delta);
}
}

private void ScrollRows(int delta)
{
if (delta == 0)
{
return;
}

int scrollDirection = delta > 0 ? -1 : 1;
int nextOffset = Math.Clamp(
_rowScrollOffset + scrollDirection * Math.Max(1, RowScrollStep),
0,
GetMaxRowScrollOffset());

if (nextOffset == _rowScrollOffset)
{
return;
}

_rowScrollOffset = nextOffset;
ApplyRowScrollOffset();
}

private void CaptureFlowLocations()
{
_flowLocations.Clear();
foreach (Control control in Controls)
{
if (control.Visible)
{
_flowLocations[control] = control.Location;
}
}
}

private int GetMaxRowScrollOffset()
{
int visibleItemCount = Controls.Cast<Control>().Count(control => control.Visible);
int totalRows = (int)Math.Ceiling(visibleItemCount / (double)Math.Max(1, RowScrollColumns));
int hiddenRows = Math.Max(0, totalRows - Math.Max(1, RowScrollVisibleRows));
return hiddenRows * Math.Max(1, RowScrollStep);
}

private void ApplyRowScrollOffset()
{
if (_flowLocations.Count == 0)
{
return;
}

SuspendLayout();
foreach ((Control control, Point location) in _flowLocations)
{
control.Location = new Point(location.X, location.Y - _rowScrollOffset);
}
ResumeLayout(false);
Invalidate();
}

        /// <summary>
        /// 重写OnPaint方法来绘制自定义边框
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // 如果边框宽度为0，不绘制边框
            if (_borderWidth <= 0)
                return;

            using (Pen pen = new Pen(_borderColor, _borderWidth))
            {
                // 计算边框绘制的矩形区域
                // 需要根据边框宽度调整，确保边框完全在控件范围内
                float halfWidth = _borderWidth / 2f;
                Rectangle rect = new Rectangle(
                    (int)halfWidth,
                    (int)halfWidth,
                    Width - _borderWidth,
                    Height - _borderWidth
                );

                // 绘制边框
                e.Graphics.DrawRectangle(pen, rect);
            }
        }

        /// <summary>
        /// 重写OnPaintBackground以确保背景正确绘制
        /// </summary>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
        }
    }
}
