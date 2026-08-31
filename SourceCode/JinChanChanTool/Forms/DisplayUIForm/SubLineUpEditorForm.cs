using JinChanChanTool.Services.Localization;
using JinChanChanTool.Tools;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace JinChanChanTool.Forms.DisplayUIForm
{
    /// <summary>
    /// 用于新增或重命名阵容分支的自定义输入窗体。
    /// </summary>
    public partial class SubLineUpEditorForm : Form
    {
        /// <summary>
        /// 圆角窗口在 96 DPI 下使用的圆角半径。
        /// </summary>
        private const int CornerRadius = 16;

        /// <summary>
        /// 本地化服务，用于显示窗体和输入校验文案。
        /// </summary>
        private readonly ILocalizationService _iLocalizationService;

        /// <summary>
        /// 用户确认后的分支名称。
        /// </summary>
        public string BranchName { get; private set; } = string.Empty;

        /// <summary>
        /// 用户确认后的分支玩法说明。
        /// </summary>
        public string BranchDescription { get; private set; } = string.Empty;

        /// <summary>
        /// 初始化分支编辑窗体，并在新增和重命名间复用同一套输入界面。
        /// </summary>
        /// <param name="iLocalizationService">本地化服务。</param>
        /// <param name="isRenaming">是否为重命名现有分支。</param>
        /// <param name="initialName">输入框初始名称。</param>
        /// <param name="initialDescription">输入框初始玩法说明。</param>
        public SubLineUpEditorForm(
            ILocalizationService iLocalizationService,
            bool isRenaming,
            string initialName,
            string initialDescription)
        {
            InitializeComponent();
            _iLocalizationService = iLocalizationService;

            // 复用项目现有自定义窗体的标题栏拖动模式。
            DragHelper.EnableDragForChildren(panel_标题栏);

            ApplyLocalization(isRenaming);
            textBox_变阵名称.Text = initialName ?? string.Empty;
            textBox_玩法说明.Text = initialDescription ?? string.Empty;
            AcceptButton = button_确认;
            CancelButton = button_取消;
        }

        /// <summary>
        /// 在窗口句柄创建后应用与现有选择窗体一致的圆角效果。
        /// </summary>
        /// <param name="e">句柄创建事件参数。</param>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ApplyRoundedCorners();
        }

        /// <summary>
        /// 在 DPI 或窗口尺寸变化后重新生成圆角区域。
        /// </summary>
        /// <param name="e">窗口尺寸变化事件参数。</param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (Handle != IntPtr.Zero)
            {
                ApplyRoundedCorners();
            }
        }

        /// <summary>
        /// 窗体显示后聚焦名称输入框，方便直接输入。
        /// </summary>
        /// <param name="e">窗体显示事件参数。</param>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            textBox_变阵名称.Focus();
            textBox_变阵名称.SelectAll();
        }

        /// <summary>
        /// 应用新增或重命名模式对应的本地化文本。
        /// </summary>
        /// <param name="isRenaming">是否为重命名模式。</param>
        private void ApplyLocalization(bool isRenaming)
        {
            string titleKey = isRenaming
                ? "SubLineUpEditorForm.Title.重命名"
                : "SubLineUpEditorForm.Title.新增";
            string title = _iLocalizationService.Get(titleKey);

            Text = title;
            label_标题.Text = title;
            label_变阵名称.Text = _iLocalizationService.Get("SubLineUpEditorForm.Label.变阵名称");
            label_玩法说明.Text = _iLocalizationService.Get("SubLineUpEditorForm.Label.玩法说明");
            textBox_变阵名称.PlaceholderText = _iLocalizationService.Get("SubLineUpEditorForm.Placeholder.变阵名称");
            textBox_玩法说明.PlaceholderText = _iLocalizationService.Get("SubLineUpEditorForm.Placeholder.玩法说明");
            button_确认.Text = _iLocalizationService.Get("SubLineUpEditorForm.Button.确认");
            button_取消.Text = _iLocalizationService.Get("SubLineUpEditorForm.Button.取消");
        }

        /// <summary>
        /// 对输入内容完成基础校验后返回编辑结果。
        /// </summary>
        /// <param name="sender">触发确认的按钮。</param>
        /// <param name="e">单击事件参数。</param>
        private void Button_Confirm_Click(object? sender, EventArgs e)
        {
            string branchName = textBox_变阵名称.Text.Trim();
            if (string.IsNullOrWhiteSpace(branchName))
            {
                // 名称校验不需要额外输入控件，使用项目约定的原生消息框提示。
                MessageBox.Show(
                    _iLocalizationService.Get("SubLineUpEditorForm.Msg.请输入名称"),
                    _iLocalizationService.Get("SubLineUpEditorForm.MsgTitle.提示"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                textBox_变阵名称.Focus();
                return;
            }

            BranchName = branchName;
            BranchDescription = textBox_玩法说明.Text.Trim();
            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// 取消编辑并关闭模态窗体。
        /// </summary>
        /// <param name="sender">触发取消的按钮。</param>
        /// <param name="e">单击事件参数。</param>
        private void Button_Cancel_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>
        /// 关闭按钮与取消按钮使用相同的取消语义。
        /// </summary>
        /// <param name="sender">触发关闭的按钮。</param>
        /// <param name="e">单击事件参数。</param>
        private void Button_Close_Click(object? sender, EventArgs e)
        {
            Button_Cancel_Click(sender, e);
        }

        /// <summary>
        /// 创建与当前 DPI 匹配的圆角窗体区域。
        /// </summary>
        private void ApplyRoundedCorners()
        {
            try
            {
                int cornerRadius = LogicalToDeviceUnits(CornerRadius);
                IntPtr region = CreateRoundRectRgn(0, 0, Width, Height, cornerRadius, cornerRadius);
                if (region != IntPtr.Zero)
                {
                    SetWindowRgn(Handle, region, true);
                }
            }
            catch (ExternalException exception)
            {
                // 圆角仅为视觉效果，失败时记录调试信息且不影响分支编辑流程。
                Debug.WriteLine($"变阵编辑窗体圆角应用失败：{exception.Message}");
            }
        }

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateRoundRectRgn(
            int left,
            int top,
            int right,
            int bottom,
            int widthEllipse,
            int heightEllipse);

        [DllImport("user32.dll")]
        private static extern int SetWindowRgn(IntPtr windowHandle, IntPtr regionHandle, bool redraw);
    }
}
