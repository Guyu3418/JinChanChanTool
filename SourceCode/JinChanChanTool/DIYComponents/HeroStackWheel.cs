using JinChanChanTool.DataClass;

namespace JinChanChanTool.DIYComponents
{
    /// <summary>
    /// 浮在棋盘上的六格轮盘，用于选择同一站位的英雄显示层。
    /// </summary>
    public sealed class HeroStackWheel : Panel
    {
        private const int MaxVisibleHeroes = 6;

        public event EventHandler<HeroStackWheelSelectedEventArgs> HeroSelected;

        public HeroStackWheel()
        {
            BackColor = Color.FromArgb(230, 25, 30, 38);
            Visible = false;
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);
        }

        public void ShowFor(HexagonCell cell, IEnumerable<StackedHeroDisplay> heroes)
        {
            List<StackedHeroDisplay> visibleHeroes = heroes
                .OrderByDescending(hero => hero.Unit.PositionLayer)
                .Take(MaxVisibleHeroes)
                .ToList();
            if (visibleHeroes.Count == 0 || Parent == null)
            {
                Hide();
                return;
            }

            Controls.Clear();
            int imageSize = Math.Clamp(Math.Min(cell.Width, cell.Height) * 2 / 3, 30, 46);
            int radius = imageSize + 6;
            int wheelSize = radius * 2 + imageSize + 8;
            Size = new Size(wheelSize, wheelSize);

            Point targetCenter = new Point(cell.Left + cell.Width / 2, cell.Top + cell.Height / 2);
            int x = targetCenter.X - Width / 2;
            int y = targetCenter.Y - Height / 2;
            x = Math.Clamp(x, 0, Math.Max(0, Parent.ClientSize.Width - Width));
            y = Math.Clamp(y, 0, Math.Max(0, Parent.ClientSize.Height - Height));
            Location = new Point(x, y);

            for (int i = 0; i < visibleHeroes.Count; i++)
            {
                double angle = -90 + i * 360d / visibleHeroes.Count;
                double radians = angle * Math.PI / 180d;
                Button heroButton = new Button
                {
                    BackColor = Color.FromArgb(45, 50, 60),
                    BackgroundImage = visibleHeroes[i].Image,
                    BackgroundImageLayout = ImageLayout.Zoom,
                    FlatStyle = FlatStyle.Flat,
                    Location = new Point(
                        Width / 2 + (int)(Math.Cos(radians) * radius) - imageSize / 2,
                        Height / 2 + (int)(Math.Sin(radians) * radius) - imageSize / 2),
                    Size = new Size(imageSize, imageSize),
                    Tag = visibleHeroes[i].Unit,
                    UseVisualStyleBackColor = false
                };
                heroButton.FlatAppearance.BorderColor = visibleHeroes[i].BorderColor;
                heroButton.FlatAppearance.BorderSize = 2;
                heroButton.Click += HeroButton_Click;
                Controls.Add(heroButton);
            }

            BringToFront();
            Visible = true;
        }

        private void HeroButton_Click(object sender, EventArgs e)
        {
            if (sender is Button { Tag: LineUpUnit unit })
            {
                HeroSelected?.Invoke(this, new HeroStackWheelSelectedEventArgs(unit));
            }
            Hide();
        }
    }

    public sealed class HeroStackWheelSelectedEventArgs : EventArgs
    {
        public LineUpUnit Unit { get; }

        public HeroStackWheelSelectedEventArgs(LineUpUnit unit)
        {
            Unit = unit;
        }
    }
}
