namespace JinChanChanTool.Forms.DisplayUIForm
{
    partial class SubLineUpEditorForm
    {
        /// <summary>
        /// 设计器生成的组件容器。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 释放由窗体持有的组件资源。
        /// </summary>
        /// <param name="disposing">是否释放托管资源。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// 初始化自定义窗体的双层背景、标题栏、输入区和底部操作区。
        /// </summary>
        private void InitializeComponent()
        {
            panel_主背景 = new Panel();
            panel_副背景 = new Panel();
            panel_内容区 = new Panel();
            textBox_玩法说明 = new TextBox();
            label_玩法说明 = new Label();
            textBox_变阵名称 = new TextBox();
            label_变阵名称 = new Label();
            panel_按钮区 = new Panel();
            button_取消 = new Button();
            button_确认 = new Button();
            panel_标题栏 = new Panel();
            button_关闭 = new Button();
            label_标题 = new Label();
            panel_主背景.SuspendLayout();
            panel_副背景.SuspendLayout();
            panel_内容区.SuspendLayout();
            panel_按钮区.SuspendLayout();
            panel_标题栏.SuspendLayout();
            SuspendLayout();
            // 
            // panel_主背景
            // 
            // 复用装备选择窗体的浅灰细边框，避免深色边框在小型编辑窗体上显得突兀。
            panel_主背景.BackColor = Color.FromArgb(250, 250, 250);
            panel_主背景.Controls.Add(panel_副背景);
            panel_主背景.Dock = DockStyle.Fill;
            panel_主背景.Padding = new Padding(3, 3, 4, 4);
            panel_主背景.Name = "panel_主背景";
            panel_主背景.TabIndex = 0;
            // 
            // panel_副背景
            // 
            panel_副背景.BackColor = Color.White;
            panel_副背景.Controls.Add(panel_内容区);
            panel_副背景.Controls.Add(panel_按钮区);
            panel_副背景.Controls.Add(panel_标题栏);
            panel_副背景.Dock = DockStyle.Fill;
            panel_副背景.Name = "panel_副背景";
            panel_副背景.TabIndex = 0;
            // 
            // panel_内容区
            // 
            panel_内容区.Controls.Add(textBox_玩法说明);
            panel_内容区.Controls.Add(label_玩法说明);
            panel_内容区.Controls.Add(textBox_变阵名称);
            panel_内容区.Controls.Add(label_变阵名称);
            panel_内容区.Dock = DockStyle.Fill;
            panel_内容区.Name = "panel_内容区";
            panel_内容区.TabIndex = 0;
            // 
            // textBox_玩法说明
            // 
            textBox_玩法说明.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textBox_玩法说明.Location = new Point(12, 74);
            textBox_玩法说明.MaxLength = 200;
            textBox_玩法说明.Multiline = true;
            textBox_玩法说明.Name = "textBox_玩法说明";
            textBox_玩法说明.ScrollBars = ScrollBars.Vertical;
            textBox_玩法说明.Size = new Size(386, 132);
            textBox_玩法说明.TabIndex = 3;
            // 
            // label_玩法说明
            // 
            label_玩法说明.AutoSize = true;
            label_玩法说明.Location = new Point(12, 51);
            label_玩法说明.Name = "label_玩法说明";
            label_玩法说明.Size = new Size(44, 17);
            label_玩法说明.TabIndex = 2;
            label_玩法说明.Text = "玩法说明";
            // 
            // textBox_变阵名称
            // 
            textBox_变阵名称.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBox_变阵名称.Location = new Point(84, 13);
            textBox_变阵名称.MaxLength = 20;
            textBox_变阵名称.Name = "textBox_变阵名称";
            textBox_变阵名称.Size = new Size(314, 23);
            textBox_变阵名称.TabIndex = 1;
            // 
            // label_变阵名称
            // 
            label_变阵名称.AutoSize = true;
            label_变阵名称.Location = new Point(12, 16);
            label_变阵名称.Name = "label_变阵名称";
            label_变阵名称.Size = new Size(56, 17);
            label_变阵名称.TabIndex = 0;
            label_变阵名称.Text = "变阵名称";
            // 
            // panel_按钮区
            // 
            panel_按钮区.Controls.Add(button_取消);
            panel_按钮区.Controls.Add(button_确认);
            panel_按钮区.Dock = DockStyle.Bottom;
            panel_按钮区.Name = "panel_按钮区";
            panel_按钮区.Size = new Size(410, 42);
            panel_按钮区.TabIndex = 1;
            // 
            // button_取消
            // 
            button_取消.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button_取消.FlatStyle = FlatStyle.Flat;
            button_取消.Location = new Point(308, 8);
            button_取消.Name = "button_取消";
            button_取消.Size = new Size(90, 26);
            button_取消.TabIndex = 1;
            button_取消.Text = "取消";
            button_取消.UseVisualStyleBackColor = true;
            button_取消.Click += Button_Cancel_Click;
            // 
            // button_确认
            // 
            button_确认.FlatStyle = FlatStyle.Flat;
            button_确认.Location = new Point(12, 8);
            button_确认.Name = "button_确认";
            button_确认.Size = new Size(90, 26);
            button_确认.TabIndex = 0;
            button_确认.Text = "确认";
            button_确认.UseVisualStyleBackColor = true;
            button_确认.Click += Button_Confirm_Click;
            // 
            // panel_标题栏
            // 
            panel_标题栏.Controls.Add(label_标题);
            panel_标题栏.Controls.Add(button_关闭);
            panel_标题栏.Dock = DockStyle.Top;
            panel_标题栏.Name = "panel_标题栏";
            panel_标题栏.Size = new Size(410, 28);
            panel_标题栏.TabIndex = 2;
            // 
            // button_关闭
            // 
            button_关闭.Dock = DockStyle.Right;
            button_关闭.FlatAppearance.BorderSize = 0;
            button_关闭.FlatStyle = FlatStyle.Flat;
            button_关闭.Name = "button_关闭";
            button_关闭.Size = new Size(30, 28);
            button_关闭.TabIndex = 1;
            button_关闭.Text = "X";
            button_关闭.UseVisualStyleBackColor = true;
            button_关闭.Click += Button_Close_Click;
            // 
            // label_标题
            // 
            label_标题.Dock = DockStyle.Fill;
            label_标题.Padding = new Padding(8, 0, 0, 0);
            label_标题.Name = "label_标题";
            label_标题.Size = new Size(380, 28);
            label_标题.TabIndex = 0;
            label_标题.Text = "新增变阵";
            label_标题.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // SubLineUpEditorForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(416, 288);
            Controls.Add(panel_主背景);
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SubLineUpEditorForm";
            StartPosition = FormStartPosition.CenterParent;
            TopMost = true;
            panel_主背景.ResumeLayout(false);
            panel_副背景.ResumeLayout(false);
            panel_内容区.ResumeLayout(false);
            panel_内容区.PerformLayout();
            panel_按钮区.ResumeLayout(false);
            panel_标题栏.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panel_主背景;
        private Panel panel_副背景;
        private Panel panel_内容区;
        private Panel panel_按钮区;
        private Panel panel_标题栏;
        private TextBox textBox_变阵名称;
        private TextBox textBox_玩法说明;
        private Label label_变阵名称;
        private Label label_玩法说明;
        private Label label_标题;
        private Button button_确认;
        private Button button_取消;
        private Button button_关闭;
    }
}
