using JinChanChanTool.DataClass;
using JinChanChanTool.DIYComponents;
using JinChanChanTool.Forms;
using JinChanChanTool.Services;
using JinChanChanTool.Services.AutoSetCoordinates;
using JinChanChanTool.Services.DataServices;
using JinChanChanTool.Services.DataServices.Interface;
using JinChanChanTool.Services.LineupCrawling;
using JinChanChanTool.Services.Localization;
using JinChanChanTool.Services.ManuallySetCoordinates;
using JinChanChanTool.Services.RecommendedEquipment;
using JinChanChanTool.Services.RecommendedEquipment.Interface;
using JinChanChanTool.Tools;
using JinChanChanTool.Tools.KeyBoardTools;
using JinChanChanTool.Tools.MouseTools;
using System.Diagnostics;
using System.Runtime.InteropServices;
namespace JinChanChanTool
{
    public partial class SettingForm : Form
    {
        /// <summary>
        /// 应用设置服务类实例，用于加载和保存应用设置。
        /// </summary>
        private readonly IManualSettingsService _iappConfigService;

        /// <summary>
        /// 推荐阵容数据服务实例
        /// </summary>
        private readonly IRecommendedLineUpService _iRecommendedLineUpService;

        /// <summary>
        /// 本地化服务实例
        /// </summary>
        private readonly ILocalizationService _iLocalizationService;

        private Screen targetScreen;//目标显示器
        private Screen[] screens;//显示器数组

        /// <summary>
        /// 鼠标测试相对移动量（96 DPI 逻辑像素）。
        /// </summary>
        private const int MouseTestMovementOffset = 30;

        /// <summary>
        /// 鼠标点击测试使用的目标屏幕坐标。
        /// </summary>
        private const int MouseClickTestTargetCoordinate = 150;

        /// <summary>
        /// Makcu 常用串口波特率。
        /// </summary>
        private const int MakcuStandardBaudRate = 115200;

        /// <summary>
        /// Makcu 高速串口波特率。
        /// </summary>
        private const int MakcuHighBaudRate = 4000000;

        /// <summary>
        /// 鼠标方式下拉框中 WinAPI 选项的固定索引。
        /// </summary>
        private const int WinApiMouseControlModeIndex = 0;

        /// <summary>
        /// 鼠标方式下拉框中 Makcu 选项的固定索引。
        /// </summary>
        private const int MakcuMouseControlModeIndex = 1;

        /// <summary>
        /// 鼠标设置分区的统一首行位置（96 DPI 逻辑像素）。
        /// </summary>
        private const int MouseSettingFirstSectionTop = 5;

        /// <summary>
        /// 鼠标设置分区之间的垂直间距（96 DPI 逻辑像素）。
        /// </summary>
        private const int MouseSettingSectionHeight = 63;

        /// <summary>
        /// 鼠标设置分区中说明文字相对标题的垂直偏移（96 DPI 逻辑像素）。
        /// </summary>
        private const int MouseSettingDescriptionOffset = 18;

        /// <summary>
        /// 鼠标设置分区中操作控件相对标题的垂直偏移（96 DPI 逻辑像素）。
        /// </summary>
        private const int MouseSettingControlTopOffset = 14;

        /// <summary>
        /// 鼠标设置分区中分隔线相对标题的垂直偏移（96 DPI 逻辑像素）。
        /// </summary>
        private const int MouseSettingSeparatorOffset = 57;

        /// <summary>
        /// 鼠标设置内容区域的左右边距（96 DPI 逻辑像素）。
        /// </summary>
        private const int MouseSettingContentMargin = 5;

        /// <summary>
        /// 鼠标设置说明与右侧操作控件间的最小间距（96 DPI 逻辑像素）。
        /// </summary>
        private const int MouseSettingDescriptionControlSpacing = 15;

        /// <summary>
        /// 鼠标设置面板的最小有效宽度（96 DPI 逻辑像素）。
        /// </summary>
        private const int MouseSettingMinimumPanelWidth = 320;

        /// <summary>
        /// 鼠标设置说明文字的最小显示宽度（96 DPI 逻辑像素）。
        /// </summary>
        private const int MouseSettingMinimumDescriptionWidth = 200;

        /// <summary>
        /// 鼠标设置说明文字的最大显示宽度（96 DPI 逻辑像素）。
        /// </summary>
        private const int MouseSettingMaximumDescriptionWidth = 660;

        /// <summary>
        /// 鼠标设置说明文字的最大显示高度（96 DPI 逻辑像素）。
        /// </summary>
        private const int MouseSettingDescriptionHeight = 34;

        /// <summary>
        /// 鼠标设置标题文字的最大显示高度（96 DPI 逻辑像素）。
        /// </summary>
        private const int MouseSettingTitleHeight = 17;

        /// <summary>
        /// 鼠标设置通用下拉框宽度（96 DPI 逻辑像素）。
        /// </summary>
        private const int MouseSettingComboBoxWidth = 140;

        /// <summary>
        /// Makcu 串口下拉框宽度（96 DPI 逻辑像素）。
        /// </summary>
        private const int MakcuPortComboBoxWidth = 150;

        /// <summary>
        /// 刷新串口按钮宽度（96 DPI 逻辑像素）。
        /// </summary>
        private const int RefreshPortButtonWidth = 105;

        /// <summary>
        /// 测试操作按钮宽度（96 DPI 逻辑像素）。
        /// </summary>
        private const int MouseTestButtonWidth = 140;

        /// <summary>
        /// 鼠标设置交互控件高度（96 DPI 逻辑像素）。
        /// </summary>
        private const int MouseSettingControlHeight = 25;

        /// <summary>
        /// 鼠标设置控件间的水平间距（96 DPI 逻辑像素）。
        /// </summary>
        private const int MouseSettingControlSpacing = 10;

        /// <summary>
        /// 鼠标设置分隔线高度（96 DPI 逻辑像素）。
        /// </summary>
        private const int MouseSettingSeparatorHeight = 1;

        /// <summary>
        /// 鼠标设置选项卡内边距（96 DPI 逻辑像素）。
        /// </summary>
        private const int MouseDeviceTabPadding = 3;

        /// <summary>
        /// 鼠标设置操作按钮边框宽度（逻辑像素）。
        /// </summary>
        private const int MouseSettingButtonBorderWidth = 1;

        /// <summary>
        /// 鼠标设置操作按钮圆角半径（逻辑像素）。
        /// </summary>
        private const int MouseSettingButtonCornerRadius = 1;

        /// <summary>
        /// 鼠标设置文字使用的字体大小（磅）。
        /// </summary>
        private const float MouseSettingFontSize = 9F;

        /// <summary>
        /// 鼠标设置说明文字颜色。
        /// </summary>
        private static readonly Color MouseSettingDescriptionColor = Color.FromArgb(133, 133, 133);

        /// <summary>
        /// 鼠标设置按钮禁用状态颜色。
        /// </summary>
        private static readonly Color MouseSettingButtonDisabledColor = Color.FromArgb(160, 160, 160);

        /// <summary>
        /// 鼠标设置按钮悬停状态颜色。
        /// </summary>
        private static readonly Color MouseSettingButtonHoverColor = Color.FromArgb(232, 232, 232);

        /// <summary>
        /// 鼠标设置按钮按下状态颜色。
        /// </summary>
        private static readonly Color MouseSettingButtonPressedColor = Color.FromArgb(222, 222, 222);

        private TabPage tabPage_鼠标设备 = null!;
        private TabControl tabControl_鼠标设备 = null!;
        private TabPage tabPage_鼠标设备_常规 = null!;
        private TabPage tabPage_鼠标设备_WinApi = null!;
        private TabPage tabPage_鼠标设备_Makcu = null!;
        private Panel panel_鼠标设备_常规 = null!;
        private Panel panel_鼠标设备_WinApi = null!;
        private Panel panel_鼠标设备_Makcu = null!;
        private Label label_鼠标移动方式 = null!;
        private Label label_鼠标移动方式描述 = null!;
        private Label label_测试鼠标移动 = null!;
        private Label label_测试鼠标移动描述 = null!;
        private Label label_测试鼠标点击 = null!;
        private Label label_测试鼠标点击描述 = null!;
        private Label label_Makcu串口 = null!;
        private Label label_Makcu串口描述 = null!;
        private Label label_Makcu波特率 = null!;
        private Label label_Makcu波特率描述 = null!;
        private Label label_Makcu连接状态 = null!;
        private Label label_Makcu连接状态描述 = null!;
        private ComboBox comboBox_鼠标移动方式 = null!;
        private ComboBox comboBox_Makcu串口 = null!;
        private ComboBox comboBox_Makcu波特率 = null!;
        private RoundedButton roundedButton_测试鼠标移动 = null!;
        private RoundedButton roundedButton_测试鼠标点击 = null!;
        private RoundedButton roundedButton_刷新Makcu串口 = null!;
        private RoundedButton roundedButton_测试Makcu连接 = null!;
        private readonly List<Panel> generalMouseDeviceSeparators = new();
        private readonly List<Panel> makcuMouseDeviceSeparators = new();
        private bool isUpdatingMouseDeviceControls;

        public SettingForm(IManualSettingsService iAppConfigService, IRecommendedLineUpService iRecommendedLineUpService, ILocalizationService iLocalizationService)
        {
            InitializeComponent();
            DragHelper.EnableDragForChildren(panel_标题栏);

            ////隐藏图标
            //this.ShowIcon = false;

            // 获取所有连接的显示器
            screens = Screen.AllScreens;
            //加载显示器到下拉框并按用户设置文件选中对应显示器
            LoadDisplays();

            //初始化应用设置服务类实例
            _iappConfigService = iAppConfigService;

            _iRecommendedLineUpService = iRecommendedLineUpService;

            //初始化本地化服务实例
            _iLocalizationService = iLocalizationService;

            //初始化鼠标设备设置页
            InitializeMouseDeviceSettings();

            //为组件绑定事件
            Initialize_AllComponents();

            //初始化显示文本
            Update_AllComponents();

            //初始化语言选择下拉框
            InitializeLanguageSelector();

            //应用本地化
            ApplyLocalization();
        }

        /// <summary>
        /// 窗体关闭时触发 ——> 检查是否有未保存的设置
        /// </summary>
        /// <param name="e"></param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // 检查设置是否已修改
            if (_iappConfigService.IsChanged())
            {
                var result = MessageBox.Show(
                    _iLocalizationService.Get("SettingForm.Msg.未保存设置"),
                    _iLocalizationService.Get("SettingForm.MsgTitle.未保存设置"),
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    _iappConfigService.Save(true);
                }
                else if (result == DialogResult.Cancel)
                {
                    // 取消关闭操作
                    e.Cancel = true;
                    return;
                }
                else
                {
                    _iappConfigService.ReLoad();
                }

            }
            if (_iappConfigService.CurrentConfig.MouseControlMode == MouseControlMode.WinApi)
            {
                MouseControlTool.DisconnectMakcu();
            }

            GlobalHotkeyTool.Enabled = true;
            base.OnFormClosing(e);
        }

        #region 显示器相关逻辑        
        /// <summary>
        /// 加载所有显示器并填充到 ComboBox 中
        /// </summary>
        private void LoadDisplays()
        {
            // 清空显示器下拉框            
            comboBox_选择显示器.Items.Clear();
            // 查询每个显示器的设备名称
            for (int i = 0; i < screens.Length; i++)
            {
                // 将显示器的序号和设备名称添加到显示器下拉框
                comboBox_选择显示器.Items.Add($"{i + 1} - {screens[i].DeviceName}");
            }
        }

        private void InitializeMouseDeviceSettings()
        {
            // 鼠标设备页沿用窗口设置页的 TabControl 分层方式，便于后续扩展更多输入设备。
            tabPage_鼠标设备 = new TabPage
            {
                Name = "tabPage_鼠标设备",
                Padding = new Padding(Dpi_S(MouseDeviceTabPadding)),
                Text = "鼠标设备",
                UseVisualStyleBackColor = true
            };
            tabControl_鼠标设备 = new TabControl
            {
                Dock = DockStyle.Fill,
                Name = "tabControl_鼠标设备",
                SelectedIndex = 0
            };
            tabPage_鼠标设备_常规 = CreateMouseDeviceTabPage("tabPage_鼠标设备_常规");
            tabPage_鼠标设备_WinApi = CreateMouseDeviceTabPage("tabPage_鼠标设备_WinApi");
            tabPage_鼠标设备_Makcu = CreateMouseDeviceTabPage("tabPage_鼠标设备_Makcu");
            panel_鼠标设备_常规 = CreateMouseDevicePanel("panel_鼠标设备_常规");
            panel_鼠标设备_WinApi = CreateMouseDevicePanel("panel_鼠标设备_WinApi");
            panel_鼠标设备_Makcu = CreateMouseDevicePanel("panel_鼠标设备_Makcu");

            int secondSectionTop = MouseSettingFirstSectionTop + MouseSettingSectionHeight;
            int thirdSectionTop = secondSectionTop + MouseSettingSectionHeight;

            // 常规子选项卡：按当前选中的输入方式执行基础移动与点击测试。
            label_鼠标移动方式 = CreateMouseSettingLabel(
                "label_鼠标移动方式",
                MouseSettingFirstSectionTop,
                true);
            label_鼠标移动方式描述 = CreateMouseSettingLabel(
                "label_鼠标移动方式描述",
                MouseSettingFirstSectionTop + MouseSettingDescriptionOffset,
                false);
            label_测试鼠标移动 = CreateMouseSettingLabel(
                "label_测试鼠标移动",
                secondSectionTop,
                true);
            label_测试鼠标移动描述 = CreateMouseSettingLabel(
                "label_测试鼠标移动描述",
                secondSectionTop + MouseSettingDescriptionOffset,
                false);
            label_测试鼠标点击 = CreateMouseSettingLabel(
                "label_测试鼠标点击",
                thirdSectionTop,
                true);
            label_测试鼠标点击描述 = CreateMouseSettingLabel(
                "label_测试鼠标点击描述",
                thirdSectionTop + MouseSettingDescriptionOffset,
                false);

            comboBox_鼠标移动方式 = CreateMouseSettingComboBox(
                "comboBox_鼠标移动方式",
                MouseSettingFirstSectionTop + MouseSettingControlTopOffset,
                MouseSettingComboBoxWidth);
            roundedButton_测试鼠标移动 = CreateMouseActionButton(
                "roundedButton_测试鼠标移动",
                secondSectionTop + MouseSettingControlTopOffset,
                MouseTestButtonWidth);
            roundedButton_测试鼠标点击 = CreateMouseActionButton(
                "roundedButton_测试鼠标点击",
                thirdSectionTop + MouseSettingControlTopOffset,
                MouseTestButtonWidth);

            generalMouseDeviceSeparators.AddRange(new Panel[]
            {
                CreateMouseSettingSeparator(MouseSettingFirstSectionTop + MouseSettingSeparatorOffset),
                CreateMouseSettingSeparator(secondSectionTop + MouseSettingSeparatorOffset),
                CreateMouseSettingSeparator(thirdSectionTop + MouseSettingSeparatorOffset)
            });
            panel_鼠标设备_常规.Controls.AddRange(new Control[]
            {
                label_鼠标移动方式,
                label_鼠标移动方式描述,
                label_测试鼠标移动,
                label_测试鼠标移动描述,
                label_测试鼠标点击,
                label_测试鼠标点击描述,
                comboBox_鼠标移动方式,
                roundedButton_测试鼠标移动,
                roundedButton_测试鼠标点击
            });
            panel_鼠标设备_常规.Controls.AddRange(generalMouseDeviceSeparators.ToArray());

            // WinAPI 子选项卡预留为空，后续 WinAPI 专属参数可直接放入该面板。

            // Makcu 子选项卡保留现有串口、波特率和连接测试功能。
            label_Makcu串口 = CreateMouseSettingLabel(
                "label_Makcu串口",
                MouseSettingFirstSectionTop,
                true);
            label_Makcu串口描述 = CreateMouseSettingLabel(
                "label_Makcu串口描述",
                MouseSettingFirstSectionTop + MouseSettingDescriptionOffset,
                false);
            label_Makcu波特率 = CreateMouseSettingLabel(
                "label_Makcu波特率",
                secondSectionTop,
                true);
            label_Makcu波特率描述 = CreateMouseSettingLabel(
                "label_Makcu波特率描述",
                secondSectionTop + MouseSettingDescriptionOffset,
                false);
            label_Makcu连接状态 = CreateMouseSettingLabel(
                "label_Makcu连接状态",
                thirdSectionTop,
                true);
            label_Makcu连接状态描述 = CreateMouseSettingLabel(
                "label_Makcu连接状态描述",
                thirdSectionTop + MouseSettingDescriptionOffset,
                false);

            comboBox_Makcu串口 = CreateMouseSettingComboBox(
                "comboBox_Makcu串口",
                MouseSettingFirstSectionTop + MouseSettingControlTopOffset,
                MakcuPortComboBoxWidth);
            comboBox_Makcu波特率 = CreateMouseSettingComboBox(
                "comboBox_Makcu波特率",
                secondSectionTop + MouseSettingControlTopOffset,
                MouseSettingComboBoxWidth);
            comboBox_Makcu波特率.Items.AddRange(new object[] { MakcuStandardBaudRate, MakcuHighBaudRate });
            roundedButton_刷新Makcu串口 = CreateMouseActionButton(
                "roundedButton_刷新Makcu串口",
                MouseSettingFirstSectionTop + MouseSettingControlTopOffset,
                RefreshPortButtonWidth);
            roundedButton_测试Makcu连接 = CreateMouseActionButton(
                "roundedButton_测试Makcu连接",
                thirdSectionTop + MouseSettingControlTopOffset,
                MouseTestButtonWidth);

            makcuMouseDeviceSeparators.AddRange(new Panel[]
            {
                CreateMouseSettingSeparator(MouseSettingFirstSectionTop + MouseSettingSeparatorOffset),
                CreateMouseSettingSeparator(secondSectionTop + MouseSettingSeparatorOffset),
                CreateMouseSettingSeparator(thirdSectionTop + MouseSettingSeparatorOffset)
            });
            panel_鼠标设备_Makcu.Controls.AddRange(new Control[]
            {
                label_Makcu串口,
                label_Makcu串口描述,
                label_Makcu波特率,
                label_Makcu波特率描述,
                label_Makcu连接状态,
                label_Makcu连接状态描述,
                comboBox_Makcu串口,
                comboBox_Makcu波特率,
                roundedButton_刷新Makcu串口,
                roundedButton_测试Makcu连接
            });
            panel_鼠标设备_Makcu.Controls.AddRange(makcuMouseDeviceSeparators.ToArray());

            tabPage_鼠标设备_常规.Controls.Add(panel_鼠标设备_常规);
            tabPage_鼠标设备_WinApi.Controls.Add(panel_鼠标设备_WinApi);
            tabPage_鼠标设备_Makcu.Controls.Add(panel_鼠标设备_Makcu);
            tabControl_鼠标设备.TabPages.AddRange(new TabPage[]
            {
                tabPage_鼠标设备_常规,
                tabPage_鼠标设备_WinApi,
                tabPage_鼠标设备_Makcu
            });
            tabPage_鼠标设备.Controls.Add(tabControl_鼠标设备);

            int developerTabIndex = tabControl_设置.TabPages.IndexOf(tabPage_开发者选项);
            tabControl_设置.TabPages.Insert(developerTabIndex, tabPage_鼠标设备);

            comboBox_鼠标移动方式.SelectedIndexChanged += comboBox_鼠标移动方式_SelectedIndexChanged;
            comboBox_Makcu串口.SelectedIndexChanged += comboBox_Makcu串口_SelectedIndexChanged;
            comboBox_Makcu串口.DropDown += (_, _) => RefreshMakcuPorts();
            comboBox_Makcu波特率.SelectedIndexChanged += comboBox_Makcu波特率_SelectedIndexChanged;
            roundedButton_刷新Makcu串口.Click += (_, _) => RefreshMakcuPorts();
            roundedButton_测试鼠标移动.Click += roundedButton_测试鼠标移动_Click;
            roundedButton_测试鼠标点击.Click += roundedButton_测试鼠标点击_Click;
            roundedButton_测试Makcu连接.Click += roundedButton_测试Makcu连接_Click;
            panel_鼠标设备_常规.Resize += (_, _) => LayoutMouseDeviceSettings();
            panel_鼠标设备_Makcu.Resize += (_, _) => LayoutMouseDeviceSettings();

            // 在首次读取配置前填充选项，避免对空下拉框设置 SelectedIndex。
            UpdateMouseControlModeOptions();
            RefreshMakcuPorts();
            LayoutMouseDeviceSettings();
        }

        /// <summary>
        /// 创建鼠标设备子选项卡，统一使用父级选项卡的可视化样式。
        /// </summary>
        /// <param name="name">子选项卡名称。</param>
        /// <returns>已创建的子选项卡。</returns>
        private TabPage CreateMouseDeviceTabPage(string name)
        {
            return new TabPage
            {
                Name = name,
                Padding = new Padding(Dpi_S(MouseDeviceTabPadding)),
                UseVisualStyleBackColor = true
            };
        }

        /// <summary>
        /// 创建鼠标设备子选项卡的内容面板。
        /// </summary>
        /// <param name="name">内容面板名称。</param>
        /// <returns>已创建的内容面板。</returns>
        private static Panel CreateMouseDevicePanel(string name)
        {
            return new Panel
            {
                AutoScroll = true,
                BackColor = Color.White,
                Dock = DockStyle.Fill,
                Name = name
            };
        }

        /// <summary>
        /// 将 96 DPI 逻辑尺寸转换为当前设置窗体的设备尺寸。
        /// </summary>
        /// <param name="logicalValue">96 DPI 下的逻辑尺寸。</param>
        /// <returns>当前 DPI 下的设备尺寸。</returns>
        private int Dpi_S(int logicalValue)
        {
            return LogicalToDeviceUnits(logicalValue);
        }

        /// <summary>
        /// 创建鼠标设置页使用的说明标签，并根据当前 DPI 转换固定尺寸。
        /// </summary>
        /// <param name="name">控件名称。</param>
        /// <param name="logicalY">96 DPI 下的纵坐标。</param>
        /// <param name="isTitle">是否为分区标题。</param>
        /// <returns>已创建的标签。</returns>
        private Label CreateMouseSettingLabel(string name, int logicalY, bool isTitle)
        {
            return new Label
            {
                Name = name,
                AutoSize = true,
                Font = new Font(
                    "Microsoft YaHei UI",
                    MouseSettingFontSize,
                    isTitle ? FontStyle.Bold : FontStyle.Regular,
                    GraphicsUnit.Point),
                ForeColor = isTitle ? SystemColors.ControlText : MouseSettingDescriptionColor,
                Location = new Point(Dpi_S(MouseSettingContentMargin), Dpi_S(logicalY)),
                MaximumSize = new Size(
                    Dpi_S(MouseSettingMaximumDescriptionWidth),
                    Dpi_S(isTitle ? MouseSettingTitleHeight : MouseSettingDescriptionHeight))
            };
        }

        /// <summary>
        /// 创建鼠标设置页使用的下拉框，并根据当前 DPI 转换固定尺寸。
        /// </summary>
        /// <param name="name">控件名称。</param>
        /// <param name="logicalY">96 DPI 下的纵坐标。</param>
        /// <param name="logicalWidth">96 DPI 下的宽度。</param>
        /// <returns>已创建的下拉框。</returns>
        private ComboBox CreateMouseSettingComboBox(string name, int logicalY, int logicalWidth)
        {
            return new ComboBox
            {
                Name = name,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FormattingEnabled = true,
                Location = new Point(0, Dpi_S(logicalY)),
                Size = new Size(Dpi_S(logicalWidth), Dpi_S(MouseSettingControlHeight)),
                TabStop = false
            };
        }

        /// <summary>
        /// 创建鼠标设置页使用的操作按钮，并根据当前 DPI 转换固定尺寸。
        /// </summary>
        /// <param name="name">控件名称。</param>
        /// <param name="logicalY">96 DPI 下的纵坐标。</param>
        /// <param name="logicalWidth">96 DPI 下的宽度。</param>
        /// <returns>已创建的操作按钮。</returns>
        private RoundedButton CreateMouseActionButton(string name, int logicalY, int logicalWidth)
        {
            return new RoundedButton
            {
                Name = name,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BorderColor = SystemColors.ScrollBar,
                BorderWidth = MouseSettingButtonBorderWidth,
                ButtonColor = Color.White,
                CornerRadius = MouseSettingButtonCornerRadius,
                DisabledColor = MouseSettingButtonDisabledColor,
                HoverColor = MouseSettingButtonHoverColor,
                Location = new Point(0, Dpi_S(logicalY)),
                PressedColor = MouseSettingButtonPressedColor,
                Size = new Size(Dpi_S(logicalWidth), Dpi_S(MouseSettingControlHeight)),
                TextColor = Color.Black,
                TextFont = new Font("Microsoft YaHei UI", MouseSettingFontSize, FontStyle.Regular, GraphicsUnit.Point)
            };
        }

        /// <summary>
        /// 创建鼠标设置页使用的水平分隔线，并根据当前 DPI 转换固定位置。
        /// </summary>
        /// <param name="logicalY">96 DPI 下的纵坐标。</param>
        /// <returns>已创建的分隔线。</returns>
        private Panel CreateMouseSettingSeparator(int logicalY)
        {
            return new Panel
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.Silver,
                Location = new Point(Dpi_S(MouseSettingContentMargin), Dpi_S(logicalY)),
                Margin = new Padding(0),
                Size = new Size(0, Dpi_S(MouseSettingSeparatorHeight))
            };
        }

        /// <summary>
        /// 按鼠标设备子选项卡的当前宽度对右侧控件、说明文字和分隔线重新布局。
        /// </summary>
        private void LayoutMouseDeviceSettings()
        {
            int generalRight = GetMouseSettingRight(panel_鼠标设备_常规);
            comboBox_鼠标移动方式.Left = generalRight - comboBox_鼠标移动方式.Width;
            roundedButton_测试鼠标移动.Left = generalRight - roundedButton_测试鼠标移动.Width;
            roundedButton_测试鼠标点击.Left = generalRight - roundedButton_测试鼠标点击.Width;
            LayoutMouseSettingDescription(label_鼠标移动方式描述, comboBox_鼠标移动方式.Left);
            LayoutMouseSettingDescription(label_测试鼠标移动描述, roundedButton_测试鼠标移动.Left);
            LayoutMouseSettingDescription(label_测试鼠标点击描述, roundedButton_测试鼠标点击.Left);
            LayoutMouseSettingSeparators(panel_鼠标设备_常规, generalMouseDeviceSeparators);

            int makcuRight = GetMouseSettingRight(panel_鼠标设备_Makcu);
            roundedButton_刷新Makcu串口.Left = makcuRight - roundedButton_刷新Makcu串口.Width;
            comboBox_Makcu串口.Left = roundedButton_刷新Makcu串口.Left -
                                     Dpi_S(MouseSettingControlSpacing) -
                                     comboBox_Makcu串口.Width;
            comboBox_Makcu波特率.Left = makcuRight - comboBox_Makcu波特率.Width;
            roundedButton_测试Makcu连接.Left = makcuRight - roundedButton_测试Makcu连接.Width;
            LayoutMouseSettingDescription(label_Makcu串口描述, comboBox_Makcu串口.Left);
            LayoutMouseSettingDescription(label_Makcu波特率描述, comboBox_Makcu波特率.Left);
            LayoutMouseSettingDescription(label_Makcu连接状态描述, roundedButton_测试Makcu连接.Left);
            LayoutMouseSettingSeparators(panel_鼠标设备_Makcu, makcuMouseDeviceSeparators);
        }

        /// <summary>
        /// 获取鼠标设置页右侧控件可使用的水平边界。
        /// </summary>
        /// <param name="panel">当前子选项卡的内容面板。</param>
        /// <returns>右侧控件的逻辑右边界。</returns>
        private int GetMouseSettingRight(Panel panel)
        {
            return Math.Max(
                Dpi_S(MouseSettingMinimumPanelWidth),
                panel.ClientSize.Width - Dpi_S(MouseSettingContentMargin));
        }

        /// <summary>
        /// 根据右侧控件位置设置说明标签的最大显示宽度。
        /// </summary>
        /// <param name="descriptionLabel">需要布局的说明标签。</param>
        /// <param name="rightControlLeft">右侧控件的左边界。</param>
        private void LayoutMouseSettingDescription(Label descriptionLabel, int rightControlLeft)
        {
            int descriptionWidth = Math.Max(
                Dpi_S(MouseSettingMinimumDescriptionWidth),
                rightControlLeft - Dpi_S(MouseSettingDescriptionControlSpacing));
            descriptionLabel.MaximumSize = new Size(
                descriptionWidth,
                Dpi_S(MouseSettingDescriptionHeight));
        }

        /// <summary>
        /// 按内容面板宽度拉伸一组鼠标设置分隔线。
        /// </summary>
        /// <param name="panel">当前子选项卡的内容面板。</param>
        /// <param name="separators">需要拉伸的分隔线集合。</param>
        private void LayoutMouseSettingSeparators(Panel panel, List<Panel> separators)
        {
            int separatorWidth = Math.Max(
                0,
                panel.ClientSize.Width - Dpi_S(MouseSettingContentMargin * 2));
            foreach (Panel separator in separators)
            {
                separator.Width = separatorWidth;
            }
        }

        private void UpdateMouseDeviceControls()
        {
            isUpdatingMouseDeviceControls = true;
            try
            {
                int targetMouseControlModeIndex =
                    _iappConfigService.CurrentConfig.MouseControlMode == MouseControlMode.Makcu
                        ? MakcuMouseControlModeIndex
                        : WinApiMouseControlModeIndex;
                comboBox_鼠标移动方式.SelectedIndex =
                    comboBox_鼠标移动方式.Items.Count > targetMouseControlModeIndex
                        ? targetMouseControlModeIndex
                        : -1;

                RefreshMakcuPorts();
                int baudRate = _iappConfigService.CurrentConfig.MakcuBaudRate;
                if (!comboBox_Makcu波特率.Items.Contains(baudRate))
                {
                    comboBox_Makcu波特率.Items.Add(baudRate);
                }
                comboBox_Makcu波特率.SelectedItem = baudRate;
            }
            finally
            {
                isUpdatingMouseDeviceControls = false;
            }

            UpdateMakcuControlState();
        }

        /// <summary>
        /// 按当前语言填充鼠标方式选项，并同步选中当前配置对应的设备。
        /// </summary>
        private void UpdateMouseControlModeOptions()
        {
            bool previousUpdatingState = isUpdatingMouseDeviceControls;
            isUpdatingMouseDeviceControls = true;
            try
            {
                comboBox_鼠标移动方式.Items.Clear();
                comboBox_鼠标移动方式.Items.Add(_iLocalizationService.Get("SettingForm.Option.WinApi"));
                comboBox_鼠标移动方式.Items.Add(_iLocalizationService.Get("SettingForm.Option.Makcu"));

                int targetMouseControlModeIndex =
                    _iappConfigService.CurrentConfig.MouseControlMode == MouseControlMode.Makcu
                        ? MakcuMouseControlModeIndex
                        : WinApiMouseControlModeIndex;
                comboBox_鼠标移动方式.SelectedIndex = targetMouseControlModeIndex;
            }
            finally
            {
                isUpdatingMouseDeviceControls = previousUpdatingState;
            }
        }

        private void RefreshMakcuPorts()
        {
            string configuredPort = _iappConfigService?.CurrentConfig.MakcuPortName ?? string.Empty;
            bool previousUpdatingState = isUpdatingMouseDeviceControls;
            isUpdatingMouseDeviceControls = true;
            try
            {
                string[] portNames = MouseControlTool.GetMakcuPortNames();
                comboBox_Makcu串口.Items.Clear();
                comboBox_Makcu串口.Items.AddRange(portNames);

                if (!string.IsNullOrWhiteSpace(configuredPort) &&
                    !comboBox_Makcu串口.Items.Contains(configuredPort))
                {
                    comboBox_Makcu串口.Items.Add(configuredPort);
                }

                comboBox_Makcu串口.SelectedItem = configuredPort;
                if (string.IsNullOrWhiteSpace(configuredPort))
                {
                    comboBox_Makcu串口.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                SetMakcuStatus(ex.Message, Color.Firebrick);
            }
            finally
            {
                isUpdatingMouseDeviceControls = previousUpdatingState;
            }
        }

        private void comboBox_鼠标移动方式_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (isUpdatingMouseDeviceControls || comboBox_鼠标移动方式.SelectedIndex < 0)
            {
                return;
            }

            _iappConfigService.CurrentConfig.MouseControlMode =
                comboBox_鼠标移动方式.SelectedIndex == MakcuMouseControlModeIndex
                ? MouseControlMode.Makcu
                : MouseControlMode.WinApi;

            if (_iappConfigService.CurrentConfig.MouseControlMode == MouseControlMode.WinApi)
            {
                MouseControlTool.DisconnectMakcu();
            }

            UpdateMakcuControlState();
        }

        private void comboBox_Makcu串口_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (isUpdatingMouseDeviceControls)
            {
                return;
            }

            _iappConfigService.CurrentConfig.MakcuPortName = comboBox_Makcu串口.SelectedItem?.ToString() ?? string.Empty;
            MouseControlTool.DisconnectMakcu();
            SetMakcuStatus(_iLocalizationService.Get("SettingForm.Status.Makcu未连接"), Color.DimGray);
        }

        private void comboBox_Makcu波特率_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (isUpdatingMouseDeviceControls || comboBox_Makcu波特率.SelectedItem is not int baudRate)
            {
                return;
            }

            _iappConfigService.CurrentConfig.MakcuBaudRate = baudRate;
            MouseControlTool.DisconnectMakcu();
            SetMakcuStatus(_iLocalizationService.Get("SettingForm.Status.Makcu未连接"), Color.DimGray);
        }

        private void roundedButton_测试Makcu连接_Click(object? sender, EventArgs e)
        {
            UseWaitCursor = true;
            roundedButton_测试Makcu连接.Enabled = false;
            try
            {
                if (MouseControlTool.TryConnectMakcu(
                        _iappConfigService.CurrentConfig.MakcuPortName,
                        _iappConfigService.CurrentConfig.MakcuBaudRate,
                        out string error))
                {
                    SetMakcuStatus(_iLocalizationService.Get("SettingForm.Status.Makcu已连接"), Color.ForestGreen);
                }
                else
                {
                    SetMakcuStatus(error, Color.Firebrick);
                    MessageBox.Show(
                        error,
                        _iLocalizationService.Get("SettingForm.MsgTitle.Makcu连接失败"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            finally
            {
                roundedButton_测试Makcu连接.Enabled = true;
                UseWaitCursor = false;
            }
        }

        /// <summary>
        /// 使用当前选择的鼠标设备执行相对 (30, 30) 测试移动。
        /// </summary>
        private void roundedButton_测试鼠标移动_Click(object? sender, EventArgs e)
        {
            UseWaitCursor = true;
            roundedButton_测试鼠标移动.Enabled = false;
            try
            {
                if (!TryGetMouseTestTargetPosition(out Point targetPosition, out string targetError))
                {
                    ShowMouseTestFailedMessage(targetError);
                    return;
                }

                if (!MouseControlTool.TrySetMousePosition(
                        targetPosition.X,
                        targetPosition.Y,
                        _iappConfigService.CurrentConfig,
                        out string error))
                {
                    ShowMouseTestFailedMessage(error);
                }
            }
            finally
            {
                roundedButton_测试鼠标移动.Enabled = true;
                UseWaitCursor = false;
            }
        }

        /// <summary>
        /// 使用当前选择的鼠标设备移动到测试坐标后单击一次左键。
        /// </summary>
        private void roundedButton_测试鼠标点击_Click(object? sender, EventArgs e)
        {
            UseWaitCursor = true;
            roundedButton_测试鼠标点击.Enabled = false;
            // 仅在成功登记程序点击后递减计数，避免移动失败路径破坏全局钩子计数。
            bool isProgramClickRegistered = false;
            try
            {
                if (!MouseControlTool.TrySetMousePosition(
                        MouseClickTestTargetCoordinate,
                        MouseClickTestTargetCoordinate,
                        _iappConfigService.CurrentConfig,
                        out string moveError))
                {
                    ShowMouseTestFailedMessage(moveError);
                    return;
                }

                // 仅将实际点击标记为程序操作，避免全局钩子将测试点击识别为用户输入。
                MouseHookTool.IncrementProgramClickCount();
                isProgramClickRegistered = true;
                if (!MouseControlTool.TryClickLeftButton(
                        _iappConfigService.CurrentConfig,
                        out string error))
                {
                    ShowMouseTestFailedMessage(error);
                }
            }
            finally
            {
                if (isProgramClickRegistered)
                {
                    MouseHookTool.DecrementProgramClickCount();
                }
                roundedButton_测试鼠标点击.Enabled = true;
                UseWaitCursor = false;
            }
        }

        /// <summary>
        /// 计算测试移动的目标屏幕坐标，避免边界情况下发生整数溢出。
        /// </summary>
        /// <param name="targetPosition">计算得到的目标屏幕坐标。</param>
        /// <param name="error">坐标不可用时的错误信息。</param>
        /// <returns>目标坐标是否有效。</returns>
        private bool TryGetMouseTestTargetPosition(out Point targetPosition, out string error)
        {
            Point currentPosition = Cursor.Position;
            long targetX = (long)currentPosition.X + MouseTestMovementOffset;
            long targetY = (long)currentPosition.Y + MouseTestMovementOffset;
            if (targetX < int.MinValue || targetX > int.MaxValue ||
                targetY < int.MinValue || targetY > int.MaxValue)
            {
                targetPosition = Point.Empty;
                error = "测试移动目标超出屏幕坐标范围。";
                return false;
            }

            targetPosition = new Point((int)targetX, (int)targetY);
            error = string.Empty;
            return true;
        }

        /// <summary>
        /// 显示鼠标测试失败的原生错误消息。
        /// </summary>
        /// <param name="error">需要显示的错误信息。</param>
        private void ShowMouseTestFailedMessage(string error)
        {
            MessageBox.Show(
                error,
                _iLocalizationService.Get("SettingForm.MsgTitle.鼠标测试失败"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        private void UpdateMakcuControlState()
        {
            bool useMakcu = _iappConfigService.CurrentConfig.MouseControlMode == MouseControlMode.Makcu;
            comboBox_Makcu串口.Enabled = useMakcu;
            comboBox_Makcu波特率.Enabled = useMakcu;
            roundedButton_刷新Makcu串口.Enabled = useMakcu;
            roundedButton_测试Makcu连接.Enabled = useMakcu;

            if (!useMakcu)
            {
                SetMakcuStatus(_iLocalizationService.Get("SettingForm.Status.WinApi无需连接"), Color.DimGray);
            }
            else if (MouseControlTool.IsMakcuConnected)
            {
                SetMakcuStatus(_iLocalizationService.Get("SettingForm.Status.Makcu已连接"), Color.ForestGreen);
            }
            else
            {
                SetMakcuStatus(_iLocalizationService.Get("SettingForm.Status.Makcu未连接"), Color.DimGray);
            }
        }

        private void SetMakcuStatus(string text, Color color)
        {
            label_Makcu连接状态描述.Text = text;
            label_Makcu连接状态描述.ForeColor = color;
        }

        /// <summary>
        /// 选择显示器下拉框选择项改变时触发 ——> targetScreen值更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            targetScreen = screens[comboBox_选择显示器.SelectedIndex];
            _iappConfigService.CurrentConfig.SelectedScreenIndex = comboBox_选择显示器.SelectedIndex;
        }

        #endregion

        #region 通用组件方法与事件
        /// <summary>
        /// 更新所有文本框的显示内容为应用设置服务类中的值
        /// </summary>
        private void Update_AllComponents()
        {
            if (comboBox_选择显示器.Items.Count > _iappConfigService.CurrentConfig.SelectedScreenIndex)
            {
                comboBox_选择显示器.SelectedIndex = _iappConfigService.CurrentConfig.SelectedScreenIndex;
            }
            textBox_召出隐藏窗口快捷键.Text = _iappConfigService.CurrentConfig.HotKey3;
            textBox_自动拿牌快捷键.Text = _iappConfigService.CurrentConfig.HotKey1;
            textBox_自动刷新商店快捷键.Text = _iappConfigService.CurrentConfig.HotKey2;
            textBox_长按自动D牌快捷键.Text = _iappConfigService.CurrentConfig.HotKey4;
            textBox_高亮提示.Text = _iappConfigService.CurrentConfig.HotKey5;
            textBox_英雄选择窗口快捷键.Text = _iappConfigService.CurrentConfig.HotKey6;
            textBox_阵容选择窗口快捷键.Text = _iappConfigService.CurrentConfig.HotKey7;
            textBox_状态窗口快捷键.Text = _iappConfigService.CurrentConfig.HotKey8;
            textBox_输出窗口快捷键.Text = _iappConfigService.CurrentConfig.HotKey9;
            radioButton_手动设置坐标.Checked = _iappConfigService.CurrentConfig.IsUseFixedCoordinates;
            radioButton_自动设置坐标.Checked = _iappConfigService.CurrentConfig.IsUseDynamicCoordinates;
            capsuleSwitch_自动识别进程.IsOn = _iappConfigService.CurrentConfig.IsAutoDetectTargetProcess;
            UpdateProcessSelectionControls();

            capsuleSwitch_避免程序与用户争夺光标控制权.IsOn = _iappConfigService.CurrentConfig.IsHighUserPriority;
            capsuleSwitch_所有窗口置顶.IsOn = _iappConfigService.CurrentConfig.IsAllWindowsTopMost;

            capsuleSwitch_自动停止拿牌.IsOn = _iappConfigService.CurrentConfig.IsAutomaticStopHeroPurchase;
            capsuleSwitch_刷新失败时自动停止刷新商店.IsOn = _iappConfigService.CurrentConfig.IsAutomaticStopRefreshStore;
            capsuleSwitch_识别错误时自动停止刷新商店.IsOn = _iappConfigService.CurrentConfig.IsStopRefreshStoreWhenErrorCharacters;


            capsuleSwitch_模拟鼠标拿牌.IsOn = _iappConfigService.CurrentConfig.IsMouseHeroPurchase;



            capsuleSwitch_按键模拟拿牌.IsOn = _iappConfigService.CurrentConfig.IsKeyboardHeroPurchase;


            capsuleSwitch_模拟鼠标刷新商店.IsOn = _iappConfigService.CurrentConfig.IsMouseRefreshStore;
            capsuleSwitch_模拟按键刷新商店.IsOn = _iappConfigService.CurrentConfig.IsKeyboardRefreshStore;

            capsuleSwitch_CPU.IsOn = _iappConfigService.CurrentConfig.IsUseCPUForInference;
            capsuleSwitch_GPU.IsOn = _iappConfigService.CurrentConfig.IsUseGPUForInference;

            textBox_拿牌按键1.Text = _iappConfigService.CurrentConfig.HeroPurchaseKey1;
            textBox_拿牌按键2.Text = _iappConfigService.CurrentConfig.HeroPurchaseKey2;
            textBox_拿牌按键3.Text = _iappConfigService.CurrentConfig.HeroPurchaseKey3;
            textBox_拿牌按键4.Text = _iappConfigService.CurrentConfig.HeroPurchaseKey4;
            textBox_拿牌按键5.Text = _iappConfigService.CurrentConfig.HeroPurchaseKey5;
            textBox_刷新商店按键.Text = _iappConfigService.CurrentConfig.RefreshStoreKey;
            textBox_自动停止拿牌次数阈值.Text = _iappConfigService.CurrentConfig.MaxTimesWithoutHeroPurchase.ToString();
            textBox_自动停止刷新商店次数阈值.Text = _iappConfigService.CurrentConfig.MaxTimesWithoutRefreshStore.ToString();
            textBox_模拟操作间隔.Text = _iappConfigService.CurrentConfig.DelayAfterOperation.ToString();
            textBox_刷新商店间隔_CPU.Text = _iappConfigService.CurrentConfig.DelayAfterRefreshStore_CPU.ToString();
            textBox_刷新商店间隔_GPU.Text = _iappConfigService.CurrentConfig.DelayAfterRefreshStore_GPU.ToString();
            capsuleSwitch_启用英雄选择面板.IsOn = _iappConfigService.CurrentConfig.IsUseSelectForm;
            capsuleSwitch_启用阵容面板.IsOn = _iappConfigService.CurrentConfig.IsUseLineUpForm;
            capsuleSwitch_启用状态面板.IsOn = _iappConfigService.CurrentConfig.IsUseStatusOverlayForm;
            capsuleSwitch_启用输出面板.IsOn = _iappConfigService.CurrentConfig.IsUseOutputForm;
            capsuleSwitch_程序启动时更新推荐装备.IsOn = _iappConfigService.CurrentConfig.IsAutomaticUpdateEquipment;
            textBox_更新推荐装备间隔.Text = _iappConfigService.CurrentConfig.UpdateEquipmentInterval.ToString();
            capsuleSwitch_程序启动时更新推荐阵容.IsOn = _iappConfigService.CurrentConfig.IsAutomaticUpdateLineup;
            textBox_更新推荐阵容间隔.Text = _iappConfigService.CurrentConfig.UpdateLineupInterval.ToString();
            textBox_英雄头像框边长.Text = _iappConfigService.CurrentConfig.SelectFormHeroPictureBoxSize.ToString();
            textBox_英雄头像框水平间隔.Text = _iappConfigService.CurrentConfig.SelectFormHeroPictureBoxHorizontalSpacing.ToString();
            textBox_英雄头像框垂直间隔.Text = _iappConfigService.CurrentConfig.SelectFormHeroPanelsVerticalSpacing.ToString();
            capsuleSwitch_排除字母.IsOn = _iappConfigService.CurrentConfig.IsFilterLetters;
            capsuleSwitch_排除数字.IsOn = _iappConfigService.CurrentConfig.IsFilterNumbers;
            capsuleSwitch_保存截图.IsOn = _iappConfigService.CurrentConfig.IsSaveCapturedImages;
            capsuleSwitch_严格匹配模式.IsOn = _iappConfigService.CurrentConfig.IsStrictMatching;
            textBox_高亮边框粗细.Text = _iappConfigService.CurrentConfig.HighlightBorderWidth.ToString();
            textBox_渐变流动速度.Text = _iappConfigService.CurrentConfig.HighlightGradientSpeed.ToString();
            button_高亮渐变色1.BackColor = _iappConfigService.CurrentConfig.HighlightColor1;
            button_高亮渐变色2.BackColor = _iappConfigService.CurrentConfig.HighlightColor2;
            numericUpDown_阵容容量.Value = _iappConfigService.CurrentConfig.LineUpCapacity;
            UpdateMouseDeviceControls();
        }


        /// <summary>
        /// 为所有文本框组件绑定事件
        /// </summary>
        private void Initialize_AllComponents()
        {
            textBox_召出隐藏窗口快捷键.KeyDown += textBox_召出隐藏窗口快捷键_KeyDown;
            textBox_召出隐藏窗口快捷键.Enter += TextBox_Enter;
            textBox_召出隐藏窗口快捷键.Leave += TextBox_Leave;

            textBox_自动拿牌快捷键.KeyDown += textBox_自动拿牌快捷键_KeyDown;
            textBox_自动拿牌快捷键.Enter += TextBox_Enter;
            textBox_自动拿牌快捷键.Leave += TextBox_Leave;

            textBox_自动刷新商店快捷键.KeyDown += textBox_自动刷新商店快捷键_KeyDown;
            textBox_自动刷新商店快捷键.Enter += TextBox_Enter;
            textBox_自动刷新商店快捷键.Leave += TextBox_Leave;

            textBox_长按自动D牌快捷键.KeyDown += textBox_长按自动D牌快捷键_KeyDown;
            textBox_长按自动D牌快捷键.Enter += TextBox_Enter;
            textBox_长按自动D牌快捷键.Leave += TextBox_Leave;

            textBox_高亮提示.KeyDown += textBox_高亮提示_KeyDown;
            textBox_高亮提示.Enter += TextBox_Enter;
            textBox_高亮提示.Leave += TextBox_Leave;

            textBox_英雄选择窗口快捷键.KeyDown += textBox_英雄选择窗口快捷键_KeyDown;
            textBox_英雄选择窗口快捷键.Enter += TextBox_Enter;
            textBox_英雄选择窗口快捷键.Leave += TextBox_Leave;

            textBox_阵容选择窗口快捷键.KeyDown += textBox_阵容选择窗口快捷键_KeyDown;
            textBox_阵容选择窗口快捷键.Enter += TextBox_Enter;
            textBox_阵容选择窗口快捷键.Leave += TextBox_Leave;

            textBox_状态窗口快捷键.KeyDown += textBox_状态窗口快捷键_KeyDown;
            textBox_状态窗口快捷键.Enter += TextBox_Enter;
            textBox_状态窗口快捷键.Leave += TextBox_Leave;

            textBox_输出窗口快捷键.KeyDown += textBox_输出窗口快捷键_KeyDown;
            textBox_输出窗口快捷键.Enter += TextBox_Enter;
            textBox_输出窗口快捷键.Leave += TextBox_Leave;


            radioButton_手动设置坐标.CheckedChanged += radioButton_手动设置坐标_CheckedChanged;

            radioButton_自动设置坐标.CheckedChanged += radioButton_自动设置坐标_CheckedChanged;

            capsuleSwitch_自动识别进程.IsOnChanged += capsuleSwitch_自动识别进程_IsOnChanged;
            capsuleSwitch_所有窗口置顶.IsOnChanged += capsuleSwitch_所有窗口置顶_IsOnChanged;

            textBox_拿牌按键1.KeyDown += TextBox6_KeyDown;
            textBox_拿牌按键1.Enter += TextBox_Enter;
            textBox_拿牌按键1.Leave += TextBox_Leave;

            textBox_拿牌按键2.KeyDown += TextBox7_KeyDown;
            textBox_拿牌按键2.Enter += TextBox_Enter;
            textBox_拿牌按键2.Leave += TextBox_Leave;

            textBox_拿牌按键3.KeyDown += TextBox16_KeyDown;
            textBox_拿牌按键3.Enter += TextBox_Enter;
            textBox_拿牌按键3.Leave += TextBox_Leave;

            textBox_拿牌按键4.KeyDown += TextBox17_KeyDown;
            textBox_拿牌按键4.Enter += TextBox_Enter;
            textBox_拿牌按键4.Leave += TextBox_Leave;

            textBox_拿牌按键5.KeyDown += TextBox18_KeyDown;
            textBox_拿牌按键5.Enter += TextBox_Enter;
            textBox_拿牌按键5.Leave += TextBox_Leave;

            textBox_刷新商店按键.KeyDown += textBox_刷新商店按键_KeyDown;
            textBox_刷新商店按键.Enter += TextBox_Enter;
            textBox_刷新商店按键.Leave += TextBox_Leave;

            textBox_自动停止拿牌次数阈值.KeyDown += TextBox_KeyDown;
            textBox_自动停止拿牌次数阈值.Enter += TextBox_Enter;
            textBox_自动停止拿牌次数阈值.Leave += textBox_MaxTimesWithoutGetCard_Leave;

            textBox_自动停止刷新商店次数阈值.KeyDown += TextBox_KeyDown;
            textBox_自动停止刷新商店次数阈值.Enter += TextBox_Enter;
            textBox_自动停止刷新商店次数阈值.Leave += textBox_MaxTimesWithoutRefresh_Leave;

            textBox_模拟操作间隔.KeyDown += TextBox_KeyDown;
            textBox_模拟操作间隔.Enter += TextBox_Enter;
            textBox_模拟操作间隔.Leave += textBox_DelayAfterMouseOperation_Leave;

            textBox_刷新商店间隔_CPU.KeyDown += TextBox_KeyDown;
            textBox_刷新商店间隔_CPU.Enter += TextBox_Enter;
            textBox_刷新商店间隔_CPU.Leave += textBox_CPUDelayAfterRefreshStore_Leave;

            textBox_刷新商店间隔_GPU.KeyDown += TextBox_KeyDown;
            textBox_刷新商店间隔_GPU.Enter += TextBox_Enter;
            textBox_刷新商店间隔_GPU.Leave += textBox_GPUDelayAfterRefreshStore_Leave;


            textBox_更新推荐装备间隔.KeyDown += TextBox_KeyDown;
            textBox_更新推荐装备间隔.Enter += TextBox_Enter;
            textBox_更新推荐装备间隔.Leave += TextBox_更新推荐装备间隔_Leave;


            textBox_英雄头像框边长.KeyDown += TextBox_KeyDown;
            textBox_英雄头像框边长.Enter += TextBox_Enter;
            textBox_英雄头像框边长.Leave += textBox_英雄头像框边长_Leave;

            textBox_英雄头像框水平间隔.KeyDown += TextBox_KeyDown;
            textBox_英雄头像框水平间隔.Enter += TextBox_Enter;
            textBox_英雄头像框水平间隔.Leave += textBox_英雄头像框水平间隔_Leave;

            textBox_英雄头像框垂直间隔.KeyDown += TextBox_KeyDown;
            textBox_英雄头像框垂直间隔.Enter += TextBox_Enter;
            textBox_英雄头像框垂直间隔.Leave += textBox_英雄头像框垂直间隔_Leave;

            textBox_高亮边框粗细.KeyDown += TextBox_KeyDown;
            textBox_高亮边框粗细.Enter += TextBox_Enter;
            textBox_高亮边框粗细.Leave += TextBox_HighlightBorderWidth_Leave;

            textBox_渐变流动速度.KeyDown += TextBox_KeyDown;
            textBox_渐变流动速度.Enter += TextBox_Enter;
            textBox_渐变流动速度.Leave += TextBox_HighlightGradientSpeed_Leave;
        }




        /// <summary>
        /// 通用的文本框进入事件 ——> 进入时清空文本框内容并禁用快捷键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_Enter(object sender, EventArgs e)
        {
            //禁用全局热键，防止冲突
            GlobalHotkeyTool.Enabled = false;
            // 当用户进入文本框时，清空现有内容
            (sender as TextBox).Text = "";
        }

        /// <summary>
        /// 通用的文本框离开事件 ——> 离开时若文本框内容为空则从应用设置服务类读取显示，否则不做任何操作，并启用快捷键。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_Leave(object sender, EventArgs e)
        {
            //启用全局热键
            GlobalHotkeyTool.Enabled = true;
            TextBox textBox = sender as TextBox;
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                Update_AllComponents();
            }

        }

        /// <summary>
        /// 通用的文本框按键按下事件 ——> 若用户键入回车，则使该组件失焦，并启用快捷键。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {

            // 捕获用户按下的键，并更新 TextBox
            var key = e.KeyCode; // 获取按键代码
            if (key == Keys.Enter)
            {
                this.ActiveControl = null;  // 将活动控件设置为null，使文本框失去焦点
            }
            //启用全局热键
            GlobalHotkeyTool.Enabled = true;
        }

        #endregion

        #region 快捷键设置
        #region 修改-召出隐藏窗口快捷键-逻辑

        /// <summary>
        /// 当textBox_召出隐藏窗口快捷键为焦点的情况下有按键按下 ——> 若用户键入回车，则使该组件失焦；若用户输入合法键值，则判断是否和已有快捷键重复，若否则更新数据类相关数据并更新显示UI。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_召出隐藏窗口快捷键_KeyDown(object sender, KeyEventArgs e)
        {
            // 捕获用户按下的键，并更新 TextBox
            var key = e.KeyCode; // 获取按键代码
            if (key == Keys.Enter)
            {
                this.ActiveControl = null;  // 将活动控件设置为null，使文本框失去焦点
                return;
            }
            if (GlobalHotkeyTool.IsRightKey(key))
            {

                if (!IsHotKeyConflict(key, 2))
                {
                    _iappConfigService.CurrentConfig.HotKey3 = key.ToString();
                    Update_AllComponents();
                }

                e.SuppressKeyPress = true; // 阻止进一步处理按键事件，如发出声音
            }

            //启用全局热键
            GlobalHotkeyTool.Enabled = true;
        }

        #endregion

        #region 修改-长按高亮提示快捷键-逻辑
        /// <summary>
        /// 当textBox_高亮提示为焦点的情况下有按键按下 ——> 若用户键入回车，则使该组件失焦；若用户输入合法键值，则判断是否和已有快捷键重复，若否则更新数据类相关数据并更新显示UI。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_高亮提示_KeyDown(object sender, KeyEventArgs e)
        {
            // 捕获用户按下的键，并更新 TextBox
            var key = e.KeyCode; // 获取按键代码
            if (key == Keys.Enter)
            {
                this.ActiveControl = null;  // 将活动控件设置为null，使文本框失去焦点
                return;
            }
            if (GlobalHotkeyTool.IsRightKey(key))
            {
                if (!IsHotKeyConflict(key, 4))
                {
                    _iappConfigService.CurrentConfig.HotKey5 = key.ToString();
                    Update_AllComponents();
                }

                e.SuppressKeyPress = true; // 阻止进一步处理按键事件，如发出声音
            }
            //启用全局热键
            GlobalHotkeyTool.Enabled = true;
        }
        #endregion

        #region 修改-自动拿牌快捷键-逻辑      
        /// <summary>
        /// 当textBox_自动拿牌快捷键为焦点的情况下有按键按下 ——> 若用户键入回车，则使该组件失焦；若用户输入合法键值，则判断是否和已有快捷键重复，若否则更新数据类相关数据并更新显示UI。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_自动拿牌快捷键_KeyDown(object sender, KeyEventArgs e)
        {

            // 捕获用户按下的键，并更新 TextBox
            var key = e.KeyCode; // 获取按键代码
            if (key == Keys.Enter)
            {
                this.ActiveControl = null;  // 将活动控件设置为null，使文本框失去焦点
                return;
            }
            if (GlobalHotkeyTool.IsRightKey(key))
            {
                if (!IsHotKeyConflict(key, 0))
                {
                    _iappConfigService.CurrentConfig.HotKey1 = key.ToString();
                    Update_AllComponents();
                }

                e.SuppressKeyPress = true; // 阻止进一步处理按键事件，如发出声音  

            }

            //启用全局热键
            GlobalHotkeyTool.Enabled = true;
        }
        #endregion

        #region 修改-自动刷新商店快捷键-逻辑
        /// <summary>
        /// 当textBox_自动刷新商店快捷键为焦点的情况下有按键按下 ——> 若用户键入回车，则使该组件失焦；若用户输入合法键值，则判断是否和已有快捷键重复，若否则更新数据类相关数据并更新显示UI。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_自动刷新商店快捷键_KeyDown(object sender, KeyEventArgs e)
        {

            // 捕获用户按下的键，并更新 TextBox
            var key = e.KeyCode; // 获取按键代码
            if (key == Keys.Enter)
            {
                this.ActiveControl = null;  // 将活动控件设置为null，使文本框失去焦点
                return;
            }
            if (GlobalHotkeyTool.IsRightKey(key))
            {
                if (!IsHotKeyConflict(key, 1))
                {
                    _iappConfigService.CurrentConfig.HotKey2 = key.ToString();
                    Update_AllComponents();
                }

                e.SuppressKeyPress = true; // 阻止进一步处理按键事件，如发出声音 
            }

            //启用全局热键
            GlobalHotkeyTool.Enabled = true;
        }
        #endregion

        #region 修改-长按自动D牌快捷键-逻辑
        /// <summary>
        /// 当textBox_长按自动D牌快捷键为焦点的情况下有按键按下 ——> 若用户键入回车，则使该组件失焦；若用户输入合法键值，则判断是否和已有快捷键重复，若否则更新数据类相关数据并更新显示UI。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_长按自动D牌快捷键_KeyDown(object sender, KeyEventArgs e)
        {

            // 捕获用户按下的键，并更新 TextBox
            var key = e.KeyCode; // 获取按键代码
            if (key == Keys.Enter)
            {
                this.ActiveControl = null;  // 将活动控件设置为null，使文本框失去焦点
                return;
            }
            if (GlobalHotkeyTool.IsRightKey(key))
            {
                if (!IsHotKeyConflict(key, 3))
                {
                    _iappConfigService.CurrentConfig.HotKey4 = key.ToString();
                    Update_AllComponents();
                }

                e.SuppressKeyPress = true; // 阻止进一步处理按键事件，如发出声音
            }
            //启用全局热键
            GlobalHotkeyTool.Enabled = true;
        }
        #endregion

        #region 修改-英雄选择窗口快捷键-逻辑
        /// <summary>
        /// 当textBox_英雄选择窗口快捷键为焦点的情况下有按键按下 ——> 若用户键入回车，则使该组件失焦；若用户输入合法键值，则判断是否和已有快捷键重复，若否则更新数据类相关数据并更新显示UI。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_英雄选择窗口快捷键_KeyDown(object sender, KeyEventArgs e)
        {
            // 捕获用户按下的键，并更新 TextBox
            var key = e.KeyCode; // 获取按键代码
            if (key == Keys.Enter)
            {
                this.ActiveControl = null;  // 将活动控件设置为null，使文本框失去焦点
                return;
            }
            if (GlobalHotkeyTool.IsRightKey(key))
            {
                if (!IsHotKeyConflict(key, 5))
                {
                    _iappConfigService.CurrentConfig.HotKey6 = key.ToString();
                    Update_AllComponents();
                }

                e.SuppressKeyPress = true; // 阻止进一步处理按键事件，如发出声音
            }
            //启用全局热键
            GlobalHotkeyTool.Enabled = true;
        }
        #endregion

        #region 修改-阵容选择窗口快捷键-逻辑
        /// <summary>
        /// 当textBox_阵容选择窗口快捷键为焦点的情况下有按键按下 ——> 若用户键入回车，则使该组件失焦；若用户输入合法键值，则判断是否和已有快捷键重复，若否则更新数据类相关数据并更新显示UI。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_阵容选择窗口快捷键_KeyDown(object sender, KeyEventArgs e)
        {
            // 捕获用户按下的键，并更新 TextBox
            var key = e.KeyCode; // 获取按键代码
            if (key == Keys.Enter)
            {
                this.ActiveControl = null;  // 将活动控件设置为null，使文本框失去焦点
                return;
            }
            if (GlobalHotkeyTool.IsRightKey(key))
            {
                if (!IsHotKeyConflict(key, 6))
                {
                    _iappConfigService.CurrentConfig.HotKey7 = key.ToString();
                    Update_AllComponents();
                }

                e.SuppressKeyPress = true; // 阻止进一步处理按键事件，如发出声音
            }
            //启用全局热键
            GlobalHotkeyTool.Enabled = true;
        }
        #endregion

        #region 修改-状态窗口快捷键-逻辑
        /// <summary>
        /// 当textBox_状态窗口快捷键为焦点的情况下有按键按下 ——> 若用户键入回车，则使该组件失焦；若用户输入合法键值，则判断是否和已有快捷键重复，若否则更新数据类相关数据并更新显示UI。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_状态窗口快捷键_KeyDown(object sender, KeyEventArgs e)
        {
            // 捕获用户按下的键，并更新 TextBox
            var key = e.KeyCode; // 获取按键代码
            if (key == Keys.Enter)
            {
                this.ActiveControl = null;  // 将活动控件设置为null，使文本框失去焦点
                return;
            }
            if (GlobalHotkeyTool.IsRightKey(key))
            {
                if (!IsHotKeyConflict(key, 7))
                {
                    _iappConfigService.CurrentConfig.HotKey8 = key.ToString();
                    Update_AllComponents();
                }

                e.SuppressKeyPress = true; // 阻止进一步处理按键事件，如发出声音
            }
            //启用全局热键
            GlobalHotkeyTool.Enabled = true;
        }
        #endregion

        #region 修改-输出窗口快捷键-逻辑
        /// <summary>
        /// 当textBox_输出窗口快捷键为焦点的情况下有按键按下 ——> 若用户键入回车，则使该组件失焦；若用户输入合法键值，则判断是否和已有快捷键重复，若否则更新数据类相关数据并更新显示UI。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_输出窗口快捷键_KeyDown(object sender, KeyEventArgs e)
        {
            // 捕获用户按下的键，并更新 TextBox
            var key = e.KeyCode; // 获取按键代码
            if (key == Keys.Enter)
            {
                this.ActiveControl = null;  // 将活动控件设置为null，使文本框失去焦点
                return;
            }
            if (GlobalHotkeyTool.IsRightKey(key))
            {
                if (!IsHotKeyConflict(key, 8))
                {
                    _iappConfigService.CurrentConfig.HotKey9 = key.ToString();
                    Update_AllComponents();
                }

                e.SuppressKeyPress = true; // 阻止进一步处理按键事件，如发出声音
            }
            //启用全局热键
            GlobalHotkeyTool.Enabled = true;
        }
        #endregion

        #region 快捷键冲突检查
        /// <summary>
        /// 判断指定按键是否与除自身外的其他快捷键冲突。
        /// </summary>
        /// <param name="key">待检查的按键</param>
        /// <param name="selfIndex">自身在快捷键列表中的索引（0对应HotKey1，依次类推）</param>
        /// <returns>若与其他快捷键冲突则返回true，否则返回false。</returns>
        private bool IsHotKeyConflict(Keys key, int selfIndex)
        {
            string keyString = key.ToString();
            string[] allHotKeys = new string[]
            {
                _iappConfigService.CurrentConfig.HotKey1,
                _iappConfigService.CurrentConfig.HotKey2,
                _iappConfigService.CurrentConfig.HotKey3,
                _iappConfigService.CurrentConfig.HotKey4,
                _iappConfigService.CurrentConfig.HotKey5,
                _iappConfigService.CurrentConfig.HotKey6,
                _iappConfigService.CurrentConfig.HotKey7,
                _iappConfigService.CurrentConfig.HotKey8,
                _iappConfigService.CurrentConfig.HotKey9,
            };

            for (int i = 0; i < allHotKeys.Length; i++)
            {
                // 跳过自身，避免与自身比较
                if (i == selfIndex)
                {
                    continue;
                }
                if (keyString == allHotKeys[i])
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #endregion

        #region 功能
        #region 常规
        /// <summary>
        /// 当“所有窗口置顶”开关状态改变时触发。
        /// </summary>
        private void capsuleSwitch_所有窗口置顶_IsOnChanged(object sender, EventArgs e)
        {
            _iappConfigService.CurrentConfig.IsAllWindowsTopMost = capsuleSwitch_所有窗口置顶.IsOn;
        }

        #region 避免程序与用户争夺光标控制权            


        /// <summary>
        /// 当“避免程序与用户争夺光标控制权”复选框状态改变时触发的事件处理程序。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void capsuleSwitch1_IsOnChanged(object sender, EventArgs e)
        {

            _iappConfigService.CurrentConfig.IsHighUserPriority = capsuleSwitch_避免程序与用户争夺光标控制权.IsOn;
        }
        #endregion

        #region 修改-键鼠操作间隔时间-逻辑
        /// <summary>
        /// 离开textBox_DelayAfterMouseOperation时触发，若用户输入为空，则显示文本从数据类读取；若用户输入合法，则更新数据类数据并更新显示文本。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_DelayAfterMouseOperation_Leave(object sender, EventArgs e)
        {
            //启用全局热键
            GlobalHotkeyTool.Enabled = true;
            if (string.IsNullOrWhiteSpace(textBox_模拟操作间隔.Text))
            {
                Update_AllComponents();
            }
            else
            {
                try
                {
                    _iappConfigService.CurrentConfig.DelayAfterOperation = int.Parse(textBox_模拟操作间隔.Text);
                    Update_AllComponents();
                }
                catch
                {
                    Update_AllComponents();
                }
            }
        }

        #endregion
        #endregion

        #region 自动拿牌
        #region 拿牌方式       
        private bool isUpdatingSwitch_鼠标模拟拿牌 = false;
        private bool isUpdatingSwitch_按键模拟拿牌 = false;

        /// <summary>
        /// 当“鼠标模拟拿牌”单选框状态改变时触发的事件处理程序。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void capsuleSwitch2_IsOnChanged(object sender, EventArgs e)
        {
            _iappConfigService.CurrentConfig.IsMouseHeroPurchase = capsuleSwitch_模拟鼠标拿牌.IsOn;
            if (isUpdatingSwitch_鼠标模拟拿牌) return;
            拿牌方式变更_鼠标拿牌();
        }

        /// <summary>
        /// 当“按键模拟拿牌”单选框状态改变时触发的事件处理程序。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void capsuleSwitch3_IsOnChanged(object sender, EventArgs e)
        {
            _iappConfigService.CurrentConfig.IsKeyboardHeroPurchase = capsuleSwitch_按键模拟拿牌.IsOn;
            if (isUpdatingSwitch_按键模拟拿牌) return;
            拿牌方式变更_按键拿牌();
        }

        private void 拿牌方式变更_鼠标拿牌()
        {
            if (_iappConfigService.CurrentConfig.IsMouseHeroPurchase)
            {
                textBox_拿牌按键1.Enabled = false;
                textBox_拿牌按键2.Enabled = false;
                textBox_拿牌按键3.Enabled = false;
                textBox_拿牌按键4.Enabled = false;
                textBox_拿牌按键5.Enabled = false;
                isUpdatingSwitch_按键模拟拿牌 = true;
                capsuleSwitch_按键模拟拿牌.IsOn = false;
                isUpdatingSwitch_按键模拟拿牌 = false;
            }
            else
            {
                textBox_拿牌按键1.Enabled = true;
                textBox_拿牌按键2.Enabled = true;
                textBox_拿牌按键3.Enabled = true;
                textBox_拿牌按键4.Enabled = true;
                textBox_拿牌按键5.Enabled = true;
                isUpdatingSwitch_按键模拟拿牌 = true;
                capsuleSwitch_按键模拟拿牌.IsOn = true;
                isUpdatingSwitch_按键模拟拿牌 = false;
            }
        }

        private void 拿牌方式变更_按键拿牌()
        {
            if (_iappConfigService.CurrentConfig.IsKeyboardHeroPurchase)
            {
                textBox_拿牌按键1.Enabled = true;
                textBox_拿牌按键2.Enabled = true;
                textBox_拿牌按键3.Enabled = true;
                textBox_拿牌按键4.Enabled = true;
                textBox_拿牌按键5.Enabled = true;

                isUpdatingSwitch_鼠标模拟拿牌 = true;
                capsuleSwitch_模拟鼠标拿牌.IsOn = false;
                isUpdatingSwitch_鼠标模拟拿牌 = false;
            }
            else
            {
                textBox_拿牌按键1.Enabled = false;
                textBox_拿牌按键2.Enabled = false;
                textBox_拿牌按键3.Enabled = false;
                textBox_拿牌按键4.Enabled = false;
                textBox_拿牌按键5.Enabled = false;
                isUpdatingSwitch_鼠标模拟拿牌 = true;
                capsuleSwitch_模拟鼠标拿牌.IsOn = true;
                isUpdatingSwitch_鼠标模拟拿牌 = false;
            }
        }
        #region 修改-按键模拟拿牌-按键1-逻辑

        /// <summary>
        /// 当TextBox6为焦点的情况下有按键按下 ——> 若用户键入回车，则使该组件失焦；
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox6_KeyDown(object sender, KeyEventArgs e)
        {
            // 捕获用户按下的键，并更新 TextBox
            var key = e.KeyCode; // 获取按键代码
            if (key == Keys.Enter)
            {
                this.ActiveControl = null;  // 将活动控件设置为null，使文本框失去焦点
                return;
            }
            if (KeyboardControlTool.IsRightKey(key))
            {
                _iappConfigService.CurrentConfig.HeroPurchaseKey1 = key.ToString();
                Update_AllComponents();
                e.SuppressKeyPress = true; // 阻止进一步处理按键事件，如发出声音 
            }
            //启用全局热键
            GlobalHotkeyTool.Enabled = true;
        }

        #endregion

        #region 修改-按键模拟拿牌-按键2-逻辑

        /// <summary>
        /// 当TextBox7为焦点的情况下有按键按下 ——> 若用户键入回车，则使该组件失焦；
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox7_KeyDown(object sender, KeyEventArgs e)
        {
            // 捕获用户按下的键，并更新 TextBox
            var key = e.KeyCode; // 获取按键代码
            if (key == Keys.Enter)
            {
                this.ActiveControl = null;  // 将活动控件设置为null，使文本框失去焦点
                return;
            }
            if (KeyboardControlTool.IsRightKey(key))
            {
                _iappConfigService.CurrentConfig.HeroPurchaseKey2 = key.ToString();
                Update_AllComponents();
                e.SuppressKeyPress = true; // 阻止进一步处理按键事件，如发出声音 
            }
            //启用全局热键
            GlobalHotkeyTool.Enabled = true;
        }

        #endregion

        #region 修改-按键模拟拿牌-按键3-逻辑

        /// <summary>
        /// 当TextBox16为焦点的情况下有按键按下 ——> 若用户键入回车，则使该组件失焦；
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox16_KeyDown(object sender, KeyEventArgs e)
        {
            // 捕获用户按下的键，并更新 TextBox
            var key = e.KeyCode; // 获取按键代码
            if (key == Keys.Enter)
            {
                this.ActiveControl = null;  // 将活动控件设置为null，使文本框失去焦点
                return;
            }
            if (KeyboardControlTool.IsRightKey(key))
            {
                _iappConfigService.CurrentConfig.HeroPurchaseKey3 = key.ToString();
                Update_AllComponents();
                e.SuppressKeyPress = true; // 阻止进一步处理按键事件，如发出声音 
            }
            //启用全局热键
            GlobalHotkeyTool.Enabled = true;
        }

        #endregion

        #region 修改-按键模拟拿牌-按键4-逻辑

        /// <summary>
        /// 当TextBox17为焦点的情况下有按键按下 ——> 若用户键入回车，则使该组件失焦；
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox17_KeyDown(object sender, KeyEventArgs e)
        {
            // 捕获用户按下的键，并更新 TextBox
            var key = e.KeyCode; // 获取按键代码
            if (key == Keys.Enter)
            {
                this.ActiveControl = null;  // 将活动控件设置为null，使文本框失去焦点
                return;
            }
            if (KeyboardControlTool.IsRightKey(key))
            {
                _iappConfigService.CurrentConfig.HeroPurchaseKey4 = key.ToString();
                Update_AllComponents();
                e.SuppressKeyPress = true; // 阻止进一步处理按键事件，如发出声音 
            }
            //启用全局热键
            GlobalHotkeyTool.Enabled = true;
        }

        #endregion

        #region 修改-按键模拟拿牌-按键5-逻辑

        /// <summary>
        /// 当TextBox18为焦点的情况下有按键按下 ——> 若用户键入回车，则使该组件失焦；
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox18_KeyDown(object sender, KeyEventArgs e)
        {
            // 捕获用户按下的键，并更新 TextBox
            var key = e.KeyCode; // 获取按键代码
            if (key == Keys.Enter)
            {
                this.ActiveControl = null;  // 将活动控件设置为null，使文本框失去焦点
                return;
            }
            if (KeyboardControlTool.IsRightKey(key))
            {
                _iappConfigService.CurrentConfig.HeroPurchaseKey5 = key.ToString();
                Update_AllComponents();
                e.SuppressKeyPress = true; // 阻止进一步处理按键事件，如发出声音 
            }
            //启用全局热键
            GlobalHotkeyTool.Enabled = true;
        }

        #endregion
        #endregion

        #region 自动停止拿牌
        private void capsuleSwitch4_IsOnChanged(object sender, EventArgs e)
        {
            _iappConfigService.CurrentConfig.IsAutomaticStopHeroPurchase = capsuleSwitch_自动停止拿牌.IsOn;
            if (_iappConfigService.CurrentConfig.IsAutomaticStopHeroPurchase)
            {
                textBox_自动停止拿牌次数阈值.Enabled = true;
            }
            else
            {
                textBox_自动停止拿牌次数阈值.Enabled = false;
            }
        }

        /// <summary>
        /// 离开textBox_MaxTimesWithoutGetCard时触发，若用户输入为空，则显示文本从数据类读取；若用户输入合法，则更新数据类数据并更新显示文本。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_MaxTimesWithoutGetCard_Leave(object sender, EventArgs e)
        {
            //启用全局热键
            GlobalHotkeyTool.Enabled = true;
            if (string.IsNullOrWhiteSpace(textBox_自动停止拿牌次数阈值.Text))
            {
                Update_AllComponents();
            }
            else
            {
                try
                {
                    _iappConfigService.CurrentConfig.MaxTimesWithoutHeroPurchase = int.Parse(textBox_自动停止拿牌次数阈值.Text);
                    Update_AllComponents();
                }
                catch
                {
                    Update_AllComponents();
                }
            }
        }
        #endregion
        #endregion

        #region 自动刷新商店
        #region 刷新方式
        private bool isUpdatingSwitch_鼠标模拟刷新商店 = false;
        private bool isUpdatingSwitch_按键模拟刷新商店 = false;

        /// <summary>
        /// 选择鼠标模拟刷新商店时触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void capsuleSwitch6_IsOnChanged(object sender, EventArgs e)
        {
            _iappConfigService.CurrentConfig.IsMouseRefreshStore = capsuleSwitch_模拟鼠标刷新商店.IsOn;
            if (isUpdatingSwitch_鼠标模拟刷新商店) return;
            刷新方式变更_鼠标刷新();
        }

        /// <summary>
        /// 选择按键模拟刷新商店时触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void capsuleSwitch5_IsOnChanged(object sender, EventArgs e)
        {
            _iappConfigService.CurrentConfig.IsKeyboardRefreshStore = capsuleSwitch_模拟按键刷新商店.IsOn;
            if (isUpdatingSwitch_按键模拟刷新商店) return;
            刷新方式变更_按键刷新();
        }

        private void 刷新方式变更_鼠标刷新()
        {
            if (_iappConfigService.CurrentConfig.IsMouseRefreshStore)
            {
                textBox_刷新商店按键.Enabled = false;
                isUpdatingSwitch_按键模拟刷新商店 = true;
                capsuleSwitch_模拟按键刷新商店.IsOn = false;
                isUpdatingSwitch_按键模拟刷新商店 = false;
            }
            else
            {
                textBox_刷新商店按键.Enabled = true;
                isUpdatingSwitch_按键模拟刷新商店 = true;
                capsuleSwitch_模拟按键刷新商店.IsOn = true;
                isUpdatingSwitch_按键模拟刷新商店 = false;
            }
        }

        private void 刷新方式变更_按键刷新()
        {
            if (_iappConfigService.CurrentConfig.IsKeyboardRefreshStore)
            {
                textBox_刷新商店按键.Enabled = true;
                isUpdatingSwitch_鼠标模拟刷新商店 = true;
                capsuleSwitch_模拟鼠标刷新商店.IsOn = false;
                isUpdatingSwitch_鼠标模拟刷新商店 = false;
            }
            else
            {
                textBox_刷新商店按键.Enabled = false;
                isUpdatingSwitch_鼠标模拟刷新商店 = true;
                capsuleSwitch_模拟鼠标刷新商店.IsOn = true;
                isUpdatingSwitch_鼠标模拟刷新商店 = false;
            }
        }
        #region 修改-按键刷新商店按键-逻辑

        /// <summary>
        /// 当TextBox_刷新商店按键为焦点的情况下有按键按下 ——> 若用户键入回车，则使该组件失焦；
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_刷新商店按键_KeyDown(object sender, KeyEventArgs e)
        {
            // 捕获用户按下的键，并更新 TextBox
            var key = e.KeyCode; // 获取按键代码
            if (key == Keys.Enter)
            {
                this.ActiveControl = null;  // 将活动控件设置为null，使文本框失去焦点
                return;
            }
            if (KeyboardControlTool.IsRightKey(key))
            {
                _iappConfigService.CurrentConfig.RefreshStoreKey = key.ToString();
                Update_AllComponents();
                e.SuppressKeyPress = true; // 阻止进一步处理按键事件，如发出声音 
            }
            //启用全局热键
            GlobalHotkeyTool.Enabled = true;
        }

        #endregion
        #endregion

        #region 自动停止刷新商店      
        /// <summary>
        /// 当“自动停止刷新商店”复选框状态改变时触发的事件处理程序。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void capsuleSwitch7_IsOnChanged(object sender, EventArgs e)
        {
            _iappConfigService.CurrentConfig.IsAutomaticStopRefreshStore = capsuleSwitch_刷新失败时自动停止刷新商店.IsOn;
            if (_iappConfigService.CurrentConfig.IsAutomaticStopRefreshStore)
            {
                textBox_自动停止刷新商店次数阈值.Enabled = true;
            }
            else
            {
                textBox_自动停止刷新商店次数阈值.Enabled = false;
            }
        }

        /// <summary>
        /// 离开textBox_MaxTimesWithoutRefresh时触发，若用户输入为空，则显示文本从数据类读取；若用户输入合法，则更新数据类数据并更新显示文本。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_MaxTimesWithoutRefresh_Leave(object sender, EventArgs e)
        {
            //启用全局热键
            GlobalHotkeyTool.Enabled = true;
            if (string.IsNullOrWhiteSpace(textBox_自动停止刷新商店次数阈值.Text))
            {
                Update_AllComponents();
            }
            else
            {
                try
                {
                    _iappConfigService.CurrentConfig.MaxTimesWithoutRefreshStore = int.Parse(textBox_自动停止刷新商店次数阈值.Text);
                    Update_AllComponents();
                }
                catch
                {
                    Update_AllComponents();
                }
            }
        }
        #endregion

        #region 当识别到错误字符时停止刷新商店               
        /// <summary>
        /// 当“当识别到错误字符时停止刷新商店”复选框状态改变时触发的事件处理程序。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void capsuleSwitch8_IsOnChanged(object sender, EventArgs e)
        {
            _iappConfigService.CurrentConfig.IsStopRefreshStoreWhenErrorCharacters = capsuleSwitch_识别错误时自动停止刷新商店.IsOn;
        }
        #endregion

        #region 修改-CPU推理模式下刷新商店后等待时间-逻辑
        /// <summary>
        /// 离开textBox_CPUDelayAfterRefreshStore时触发，若用户输入为空，则显示文本从数据类读取；若用户输入合法，则更新数据类数据并更新显示文本。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_CPUDelayAfterRefreshStore_Leave(object sender, EventArgs e)
        {
            //启用全局热键
            GlobalHotkeyTool.Enabled = true;
            if (string.IsNullOrWhiteSpace(textBox_刷新商店间隔_CPU.Text))
            {
                Update_AllComponents();
            }
            else
            {
                try
                {
                    _iappConfigService.CurrentConfig.DelayAfterRefreshStore_CPU = int.Parse(textBox_刷新商店间隔_CPU.Text);
                    Update_AllComponents();
                }
                catch
                {
                    Update_AllComponents();
                }
            }
        }


        #endregion

        #region 修改-GPU推理模式下刷新商店后等待时间-逻辑
        /// <summary>
        /// 离开textBox_GPUDelayAfterRefreshStore时触发，若用户输入为空，则显示文本从数据类读取；若用户输入合法，则更新数据类数据并更新显示文本。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_GPUDelayAfterRefreshStore_Leave(object sender, EventArgs e)
        {
            //启用全局热键
            GlobalHotkeyTool.Enabled = true;
            if (string.IsNullOrWhiteSpace(textBox_刷新商店间隔_GPU.Text))
            {
                Update_AllComponents();
            }
            else
            {
                try
                {
                    _iappConfigService.CurrentConfig.DelayAfterRefreshStore_GPU = int.Parse(textBox_刷新商店间隔_GPU.Text);
                    Update_AllComponents();
                }
                catch
                {
                    Update_AllComponents();
                }
            }
        }


        #endregion
        #endregion

        #region 高亮提示
        private void button1_Click_1(object sender, EventArgs e)
        {
            colorDialog_高亮边框渐变色1.Color = _iappConfigService.CurrentConfig.HighlightColor1;

            if (colorDialog_高亮边框渐变色1.ShowDialog() == DialogResult.OK)
            {
                _iappConfigService.CurrentConfig.HighlightColor1 = colorDialog_高亮边框渐变色1.Color;
                button_高亮渐变色1.BackColor = _iappConfigService.CurrentConfig.HighlightColor1;
                Update_AllComponents();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            colorDialog_高亮边框渐变色2.Color = _iappConfigService.CurrentConfig.HighlightColor2;

            if (colorDialog_高亮边框渐变色2.ShowDialog() == DialogResult.OK)
            {
                _iappConfigService.CurrentConfig.HighlightColor2 = colorDialog_高亮边框渐变色2.Color;
                button_高亮渐变色2.BackColor = _iappConfigService.CurrentConfig.HighlightColor2;
                Update_AllComponents();
            }
        }
        private void TextBox_HighlightBorderWidth_Leave(object? sender, EventArgs e)
        {
            //启用全局热键
            GlobalHotkeyTool.Enabled = true;
            if (string.IsNullOrWhiteSpace(textBox_高亮边框粗细.Text))
            {
                Update_AllComponents();
            }
            else
            {
                try
                {
                    int result = int.Parse(textBox_高亮边框粗细.Text);
                    if (result > 0)
                    {
                        _iappConfigService.CurrentConfig.HighlightBorderWidth = result;
                    }

                    Update_AllComponents();
                }
                catch
                {
                    Update_AllComponents();
                }
            }
        }

        private void TextBox_HighlightGradientSpeed_Leave(object? sender, EventArgs e)
        {
            //启用全局热键
            GlobalHotkeyTool.Enabled = true;
            if (string.IsNullOrWhiteSpace(textBox_渐变流动速度.Text))
            {
                Update_AllComponents();
            }
            else
            {
                try
                {
                    float result = float.Parse(textBox_渐变流动速度.Text);
                    if (result > 0f && result <= 0.2f)
                    {
                        _iappConfigService.CurrentConfig.HighlightGradientSpeed = result;
                    }

                    Update_AllComponents();
                }
                catch
                {
                    Update_AllComponents();
                }
            }
        }
        #endregion
        #endregion

        #region 坐标设置                      
        #region 坐标设置方式单选框改变
        /// <summary>
        /// 手动设置坐标单选框状态改变事件 ——> 若被选中则启用手动设置坐标相关组件并禁用自动设置坐标相关组件，同时更新数据类相关数据。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioButton_手动设置坐标_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton_手动设置坐标.Checked)
            {
                comboBox_选择显示器.Enabled = true;
                roundedButton_弈子截图坐标与大小.Enabled = true;
                roundedButton_商店刷新按钮坐标与大小.Enabled = true;
                roundedButton_高亮提示框坐标与大小.Enabled = true;
            }
            _iappConfigService.CurrentConfig.IsUseFixedCoordinates = radioButton_手动设置坐标.Checked;
            UpdateProcessSelectionControls();
        }

        /// <summary>
        /// 自动设置坐标单选框状态改变事件 ——> 若被选中则启用自动设置坐标相关组件并禁用手动设置坐标相关组件，同时更新数据类相关数据。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioButton_自动设置坐标_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton_自动设置坐标.Checked)
            {
                comboBox_选择显示器.Enabled = false;
                roundedButton_弈子截图坐标与大小.Enabled = false;
                roundedButton_商店刷新按钮坐标与大小.Enabled = false;
                roundedButton_高亮提示框坐标与大小.Enabled = false;
            }
            _iappConfigService.CurrentConfig.IsUseDynamicCoordinates = radioButton_自动设置坐标.Checked;
            UpdateProcessSelectionControls();
        }

        private void capsuleSwitch_自动识别进程_IsOnChanged(object sender, EventArgs e)
        {
            _iappConfigService.CurrentConfig.IsAutoDetectTargetProcess = capsuleSwitch_自动识别进程.IsOn;
            UpdateProcessSelectionControls();
        }

        private void UpdateProcessSelectionControls()
        {
            bool isDynamicCoordinates = radioButton_自动设置坐标.Checked;
            capsuleSwitch_自动识别进程.Enabled = isDynamicCoordinates;
            roundedButton_游戏进程窗口.Enabled = isDynamicCoordinates && !capsuleSwitch_自动识别进程.IsOn;
        }

        #endregion

        #region 快速设置坐标    

        /// <summary>
        /// 快速设置奕子截图坐标与大小按钮_被单击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void roundedButton1_Click(object sender, EventArgs e)
        {
            using (var setter = new FastSettingPositionService(targetScreen, _iLocalizationService, _iappConfigService.CurrentConfig.IsAllWindowsTopMost))
            {
                try
                {
                    // 第一张卡片
                    var rect1 = await setter.WaitForDrawAsync(
                        _iLocalizationService.Get("SettingForm.弈子坐标设置提示词1"));
                    _iappConfigService.CurrentConfig.HeroNameScreenshotRectangle_1 = rect1;

                    // 第二张卡片
                    var rect2 = await setter.WaitForDrawAsync(
                        _iLocalizationService.Get("SettingForm.弈子坐标设置提示词2"));
                    _iappConfigService.CurrentConfig.HeroNameScreenshotRectangle_2 = rect2;

                    // 第三张卡片
                    var rect3 = await setter.WaitForDrawAsync(
                        _iLocalizationService.Get("SettingForm.弈子坐标设置提示词3"));
                    _iappConfigService.CurrentConfig.HeroNameScreenshotRectangle_3 = rect3;

                    // 第四张卡片
                    var rect4 = await setter.WaitForDrawAsync(
                        _iLocalizationService.Get("SettingForm.弈子坐标设置提示词4"));
                    _iappConfigService.CurrentConfig.HeroNameScreenshotRectangle_4 = rect4;

                    // 第五张卡片
                    var rect5 = await setter.WaitForDrawAsync(
                        _iLocalizationService.Get("SettingForm.弈子坐标设置提示词5"));
                    _iappConfigService.CurrentConfig.HeroNameScreenshotRectangle_5 = rect5;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(_iLocalizationService.Get("SettingForm.Msg.错误", ex.Message));
                }
            }
            Update_AllComponents();
        }

        /// <summary>
        /// 快速设置商店刷新按钮坐标按钮_被单击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void roundedButton2_Click(object sender, EventArgs e)
        {
            using (var setter = new FastSettingPositionService(targetScreen, _iLocalizationService, _iappConfigService.CurrentConfig.IsAllWindowsTopMost))
            {
                try
                {
                    Rectangle rectangle = await setter.WaitForDrawAsync(_iLocalizationService.Get("SettingForm.商店刷新按钮坐标设置提示词"));
                    _iappConfigService.CurrentConfig.RefreshStoreButtonRectangle = rectangle;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(_iLocalizationService.Get("SettingForm.Msg.错误", ex.Message));
                }
            }
            Update_AllComponents();
        }
        /// <summary>
        /// 设置高亮提示框坐标按钮_被单击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void roundedButton3_Click(object sender, EventArgs e)
        {
            using (var setter = new FastSettingPositionService(targetScreen, _iLocalizationService, _iappConfigService.CurrentConfig.IsAllWindowsTopMost))
            {
                try
                {
                    // 第一张卡片
                    var rect1 = await setter.WaitForDrawAsync(
                        _iLocalizationService.Get("SettingForm.高亮提示框坐标设置提示词1"));
                    _iappConfigService.CurrentConfig.HighLightRectangle_1 = rect1;

                    // 第二张卡片
                    var rect2 = await setter.WaitForDrawAsync(
                        _iLocalizationService.Get("SettingForm.高亮提示框坐标设置提示词2"));
                    _iappConfigService.CurrentConfig.HighLightRectangle_2 = rect2;

                    // 第三张卡片
                    var rect3 = await setter.WaitForDrawAsync(
                        _iLocalizationService.Get("SettingForm.高亮提示框坐标设置提示词3"));
                    _iappConfigService.CurrentConfig.HighLightRectangle_3 = rect3;

                    // 第四张卡片
                    var rect4 = await setter.WaitForDrawAsync(
                        _iLocalizationService.Get("SettingForm.高亮提示框坐标设置提示词4"));
                    _iappConfigService.CurrentConfig.HighLightRectangle_4 = rect4;

                    // 第五张卡片
                    var rect5 = await setter.WaitForDrawAsync(
                        _iLocalizationService.Get("SettingForm.高亮提示框坐标设置提示词5"));
                    _iappConfigService.CurrentConfig.HighLightRectangle_5 = rect5;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(_iLocalizationService.Get("SettingForm.Msg.错误", ex.Message));
                }
            }
            Update_AllComponents();
        }
        #endregion

        #region 选择进程             
        /// <summary>
        /// 选择进程按钮被单击时触发的事件处理程序。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void roundedButton4_Click(object sender, EventArgs e)
        {
            // 1. 实时创建进程发现服务
            var discoveryService = new ProcessDiscoveryService();

            // 2. 创建并显示进程选择窗体
            using (var processForm = new ProcessSelectorForm(discoveryService, _iLocalizationService))
            {
                processForm.TopMost = _iappConfigService.CurrentConfig.IsAllWindowsTopMost;
                if (processForm.ShowDialog(this) == DialogResult.OK)
                {
                    var selectedProcess = processForm.SelectedProcess;
                    if (selectedProcess != null)
                    {
                        // 同时保存 Name 和 ID
                        _iappConfigService.CurrentConfig.TargetProcessName = selectedProcess.ProcessName;
                        _iappConfigService.CurrentConfig.TargetProcessId = selectedProcess.Id;
                        // 给用户反馈
                        string displayName = $"{selectedProcess.ProcessName} (ID: {selectedProcess.Id})";
                        MessageBox.Show(_iLocalizationService.Get("SettingForm.Msg.进程已选择", displayName), _iLocalizationService.Get("SettingForm.MsgTitle.提示"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }
        #endregion
        #endregion

        #region OCR相关设置
        #region 打开OCR纠正列表编辑器             
        /// <summary>
        /// OCR结果纠正列表编辑器按钮_被单击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void roundedButton5_Click(object sender, EventArgs e)
        {
            var form = new CorrectionEditorForm(_iappConfigService, _iLocalizationService);
            form.Owner = this;// 设置父窗口，这样配置窗口会显示在主窗口上方但不会阻止主窗口
            form.TopMost = _iappConfigService.CurrentConfig.IsAllWindowsTopMost;
            form.Show();// 显示窗口
        }
        #endregion

        #region 推理设备单选框改变
        private bool isUpdatingSwitch_CPU推理 = false;
        private bool isUpdatingSwitch_GPU推理 = false;
        /// <summary>
        /// 选择CPU推理时触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void capsuleSwitch10_IsOnChanged(object sender, EventArgs e)
        {
            _iappConfigService.CurrentConfig.IsUseCPUForInference = capsuleSwitch_CPU.IsOn;
            if (isUpdatingSwitch_CPU推理) return;
            推理方式变更_CPU();
        }

        /// <summary>
        /// 选择GPU推理时触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void capsuleSwitch9_IsOnChanged(object sender, EventArgs e)
        {
            _iappConfigService.CurrentConfig.IsUseGPUForInference = capsuleSwitch_GPU.IsOn;
            if (isUpdatingSwitch_GPU推理) return;
            推理方式变更_GPU();
        }

        private void 推理方式变更_CPU()
        {
            if (_iappConfigService.CurrentConfig.IsUseCPUForInference)
            {
                isUpdatingSwitch_GPU推理 = true;
                capsuleSwitch_GPU.IsOn = false;
                isUpdatingSwitch_GPU推理 = false;
            }
            else
            {
                isUpdatingSwitch_GPU推理 = true;
                capsuleSwitch_GPU.IsOn = true;
                isUpdatingSwitch_GPU推理 = false;
            }
        }

        private void 推理方式变更_GPU()
        {
            if (_iappConfigService.CurrentConfig.IsUseGPUForInference)
            {
                isUpdatingSwitch_CPU推理 = true;
                capsuleSwitch_CPU.IsOn = false;
                isUpdatingSwitch_CPU推理 = false;
            }
            else
            {
                isUpdatingSwitch_CPU推理 = true;
                capsuleSwitch_CPU.IsOn = true;
                isUpdatingSwitch_CPU推理 = false;
            }
        }
        #endregion

        #region 过滤字符
        private void capsuleSwitch16_IsOnChanged(object sender, EventArgs e)
        {
            _iappConfigService.CurrentConfig.IsFilterLetters = capsuleSwitch_排除字母.IsOn;
        }

        private void capsuleSwitch17_IsOnChanged(object sender, EventArgs e)
        {
            _iappConfigService.CurrentConfig.IsFilterNumbers = capsuleSwitch_排除数字.IsOn;
        }
        #endregion

        #region 严格匹配模式
        private void capsuleSwitch19_IsOnChanged(object sender, EventArgs e)
        {
            _iappConfigService.CurrentConfig.IsStrictMatching = capsuleSwitch_严格匹配模式.IsOn;
        }
        #endregion
        #endregion

        #region 窗口设置
        #region 英雄选择窗口
        /// <summary>
        /// 勾选或取消勾选“使用选择窗口位置”复选框时触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>      
        private void capsuleSwitch11_IsOnChanged(object sender, EventArgs e)
        {
            _iappConfigService.CurrentConfig.IsUseSelectForm = capsuleSwitch_启用英雄选择面板.IsOn;
        }

        /// <summary>
        /// 离开textBox_英雄头像框边长时触发，若用户输入为空，则显示文本从数据类读取；若用户输入合法，则更新数据类数据并更新显示文本。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_英雄头像框边长_Leave(object sender, EventArgs e)
        {
            //启用全局热键
            GlobalHotkeyTool.Enabled = true;
            if (string.IsNullOrWhiteSpace(textBox_英雄头像框边长.Text))
            {
                Update_AllComponents();
            }
            else
            {
                try
                {
                    int result = int.Parse(textBox_英雄头像框边长.Text);
                    if (result > 0)
                    {
                        _iappConfigService.CurrentConfig.SelectFormHeroPictureBoxSize = result;
                    }
                    Update_AllComponents();
                }
                catch
                {
                    Update_AllComponents();
                }
            }
        }

        /// <summary>
        /// 离开textBox_英雄头像框水平间隔时触发，若用户输入为空，则显示文本从数据类读取；若用户输入合法，则更新数据类数据并更新显示文本。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_英雄头像框水平间隔_Leave(object sender, EventArgs e)
        {
            //启用全局热键
            GlobalHotkeyTool.Enabled = true;
            if (string.IsNullOrWhiteSpace(textBox_英雄头像框水平间隔.Text))
            {
                Update_AllComponents();
            }
            else
            {
                try
                {
                    int result = int.Parse(textBox_英雄头像框水平间隔.Text);
                    if (result >= 0)
                    {
                        _iappConfigService.CurrentConfig.SelectFormHeroPictureBoxHorizontalSpacing = result;
                    }
                    Update_AllComponents();
                }
                catch
                {
                    Update_AllComponents();
                }
            }
        }

        /// <summary>
        /// 离开textBox_英雄头像框垂直间隔时触发，若用户输入为空，则显示文本从数据类读取；若用户输入合法，则更新数据类数据并更新显示文本。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_英雄头像框垂直间隔_Leave(object sender, EventArgs e)
        {
            //启用全局热键
            GlobalHotkeyTool.Enabled = true;
            if (string.IsNullOrWhiteSpace(textBox_英雄头像框垂直间隔.Text))
            {
                Update_AllComponents();
            }
            else
            {
                try
                {
                    int result = int.Parse(textBox_英雄头像框垂直间隔.Text);
                    if (result >= 0)
                    {
                        _iappConfigService.CurrentConfig.SelectFormHeroPanelsVerticalSpacing = result;
                    }
                    Update_AllComponents();
                }
                catch
                {
                    Update_AllComponents();
                }
            }
        }


        #endregion

        /// <summary>
        /// 勾选或取消勾选“使用阵容窗口位置”复选框时触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void capsuleSwitch12_IsOnChanged(object sender, EventArgs e)
        {
            _iappConfigService.CurrentConfig.IsUseLineUpForm = capsuleSwitch_启用阵容面板.IsOn;
        }

        /// <summary>
        /// 勾选或取消勾选“使用状态覆盖窗口位置”复选框时触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>      
        private void capsuleSwitch13_IsOnChanged(object sender, EventArgs e)
        {
            _iappConfigService.CurrentConfig.IsUseStatusOverlayForm = capsuleSwitch_启用状态面板.IsOn;
        }

        /// <summary>
        /// 勾选或取消勾选“使用错误输出窗口位置”复选框时触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>      
        private void capsuleSwitch14_IsOnChanged(object sender, EventArgs e)
        {
            _iappConfigService.CurrentConfig.IsUseOutputForm = capsuleSwitch_启用输出面板.IsOn;
        }

        #endregion

        #region 大数据推荐
        #region 自动更新推荐装备设置
        /// <summary>
        /// 勾选或取消勾选“定时更新推荐装备”复选框时触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void capsuleSwitch15_IsOnChanged(object sender, EventArgs e)
        {
            _iappConfigService.CurrentConfig.IsAutomaticUpdateEquipment = capsuleSwitch_程序启动时更新推荐装备.IsOn;
            if (_iappConfigService.CurrentConfig.IsAutomaticUpdateEquipment)
            {
                textBox_更新推荐装备间隔.Enabled = true;

            }
            else
            {
                textBox_更新推荐装备间隔.Enabled = false;
            }
        }

        /// <summary>
        /// 离开textBox_更新推荐装备间隔时触发，若用户输入为空，则显示文本从数据类读取；若用户输入合法，则更新数据类数据并更新显示文本。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_更新推荐装备间隔_Leave(object sender, EventArgs e)
        {
            //启用全局热键
            GlobalHotkeyTool.Enabled = true;
            if (string.IsNullOrWhiteSpace(textBox_更新推荐装备间隔.Text))
            {

                Update_AllComponents();
            }
            else
            {
                try
                {
                    int result = int.Parse(textBox_更新推荐装备间隔.Text);
                    if (result > 0)
                    {
                        _iappConfigService.CurrentConfig.UpdateEquipmentInterval = result;
                    }
                    Update_AllComponents();

                }
                catch
                {
                    Update_AllComponents();
                }
            }
        }
        #endregion

        #region 阵容推荐
        /// <summary>
        /// 程序启动时更新推荐阵容开关状态改变时触发
        /// </summary>
        private void capsuleSwitch_程序启动时更新推荐阵容_IsOnChanged(object sender, EventArgs e)
        {
            _iappConfigService.CurrentConfig.IsAutomaticUpdateLineup = capsuleSwitch_程序启动时更新推荐阵容.IsOn;
            if (_iappConfigService.CurrentConfig.IsAutomaticUpdateLineup)
            {
                textBox_更新推荐阵容间隔.Enabled = true;
            }
            else
            {
                textBox_更新推荐阵容间隔.Enabled = false;
            }
        }

        /// <summary>
        /// 更新推荐阵容间隔文本框失去焦点时触发
        /// </summary>
        private void textBox_更新推荐阵容间隔_Leave(object sender, EventArgs e)
        {
            if (int.TryParse(textBox_更新推荐阵容间隔.Text, out int interval))
            {
                if (interval >= 1 && interval <= 168)
                {
                    _iappConfigService.CurrentConfig.UpdateLineupInterval = interval;
                }
                else
                {
                    MessageBox.Show(
                        _iLocalizationService.Get("SettingForm.Msg.更新间隔范围错误"),
                        _iLocalizationService.Get("SettingForm.MsgTitle.设置错误"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    textBox_更新推荐阵容间隔.Text = _iappConfigService.CurrentConfig.UpdateLineupInterval.ToString();
                    Update_AllComponents();
                }
            }
        }
        #endregion
        #endregion

        #region 开发者选项
        /// <summary>
        /// 是否保存截图开关状态改变时触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void capsuleSwitch18_IsOnChanged(object sender, EventArgs e)
        {
            _iappConfigService.CurrentConfig.IsSaveCapturedImages = capsuleSwitch_保存截图.IsOn;
        }
        #endregion

        private void SettingForm_Load(object sender, EventArgs e)
        {

        }

        #region 设置存取相关

        /// <summary>
        /// /保存设置按钮_被单击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            // 保存语言设置
            if (comboBox_语言选择.SelectedItem is LanguageInfo selectedLanguage)
            {
                _iappConfigService.CurrentConfig.Language = selectedLanguage.LanguageCode;
            }

            // 保存阵容容量设置
            _iappConfigService.CurrentConfig.LineUpCapacity = (int)numericUpDown_阵容容量.Value;

            _iappConfigService.Save(true);
        }

        /// <summary>
        /// 还原默认设置按钮_被单击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            var confirmResult = MessageBox.Show(_iLocalizationService.Get("SettingForm.Msg.确认恢复默认设置"), _iLocalizationService.Get("SettingForm.MsgTitle.确认恢复默认设置"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirmResult != DialogResult.Yes)
            {
                return; // 用户取消操作
            }
            _iappConfigService.SetDefaultConfig();
            Update_AllComponents();
        }
        #endregion

        #region 圆角实现
        // GDI32 API - 用于创建圆角效果
        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        [DllImport("user32.dll")]
        private static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);

        // 圆角半径
        private const int CORNER_RADIUS = 16;

        /// <summary>
        /// 在窗口句柄创建后应用圆角效果
        /// </summary>
        /// <param name="e"></param>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // 应用 GDI Region 圆角效果（支持 Windows 10 和 Windows 11）
            ApplyRoundedCorners();
        }

        /// <summary>
        /// 应用 GDI Region 圆角效果
        /// </summary>
        private void ApplyRoundedCorners()
        {
            try
            {
                // 创建圆角矩形区域
                IntPtr region = CreateRoundRectRgn(0, 0, Width, Height, CORNER_RADIUS, CORNER_RADIUS);

                if (region != IntPtr.Zero)
                {
                    SetWindowRgn(Handle, region, true);
                    // 注意：SetWindowRgn 会接管 region 的所有权，不需要手动删除

                }
                else
                {

                }
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// 窗口大小改变时重新应用圆角
        /// </summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            // 调整大小时重新创建圆角区域
            if (Handle != IntPtr.Zero)
            {
                ApplyRoundedCorners();
            }
        }
        #endregion

        #region 标题栏按钮事件
        private void button_最小化_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void button_关闭_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion

        #region 语言选择
        /// <summary>
        /// 初始化语言选择下拉框
        /// </summary>
        private void InitializeLanguageSelector()
        {
            // 填充可用语言列表
            comboBox_语言选择.Items.Clear();
            foreach (var lang in _iLocalizationService.AvailableLanguages)
            {
                comboBox_语言选择.Items.Add(lang);
            }

            // 设置DisplayMember
            comboBox_语言选择.DisplayMember = "NativeName";

            // 选中当前语言
            string currentLanguage = _iappConfigService.CurrentConfig.Language;
            var selectedLang = _iLocalizationService.AvailableLanguages
                .FirstOrDefault(l => l.LanguageCode == currentLanguage);

            if (selectedLang != null)
            {
                comboBox_语言选择.SelectedItem = selectedLang;
            }
        }

        /// <summary>
        /// 应用本地化到所有控件
        /// </summary>
        private void ApplyLocalization()
        {
            // 标题栏
            label_标题.Text = _iLocalizationService.Get("SettingForm.标题");

            // 按钮
            roundedButton_保存设置.Text = _iLocalizationService.Get("SettingForm.Button.保存设置");
            roundedButton_默认设置.Text = _iLocalizationService.Get("SettingForm.Button.默认设置");

            // 选项卡
            tabPage_常规.Text = _iLocalizationService.Get("SettingForm.Tab.常规");
            tabPage_快捷键.Text = _iLocalizationService.Get("SettingForm.Tab.快捷键");
            tabPage_功能.Text = _iLocalizationService.Get("SettingForm.Tab.功能");
            tabPage_坐标设置.Text = _iLocalizationService.Get("SettingForm.Tab.坐标设置");
            tabPage_OCR相关.Text = _iLocalizationService.Get("SettingForm.Tab.OCR相关");
            tabPage_窗口.Text = _iLocalizationService.Get("SettingForm.Tab.窗口");
            tabPage_大数据推荐.Text = _iLocalizationService.Get("SettingForm.Tab.大数据推荐");
            tabPage_开发者选项.Text = _iLocalizationService.Get("SettingForm.Tab.开发者选项");
            tabPage_鼠标设备.Text = _iLocalizationService.Get("SettingForm.Tab.鼠标设备");
            tabPage_鼠标设备_常规.Text = _iLocalizationService.Get("SettingForm.Tab.鼠标设备.常规");
            tabPage_鼠标设备_WinApi.Text = _iLocalizationService.Get("SettingForm.Tab.鼠标设备.WinApi");
            tabPage_鼠标设备_Makcu.Text = _iLocalizationService.Get("SettingForm.Tab.鼠标设备.Makcu");
            tabPage_功能_常规.Text = _iLocalizationService.Get("SettingForm.Tab.功能.常规");
            tabPage_功能_自动拿牌.Text = _iLocalizationService.Get("SettingForm.Tab.功能.自动拿牌");
            tabPage_功能_自动刷新商店.Text = _iLocalizationService.Get("SettingForm.Tab.功能.自动刷新商店");
            tabPage_功能_高亮提示.Text = _iLocalizationService.Get("SettingForm.Tab.功能.高亮提示");
            tabPage_OCR相关_OCR设置.Text = _iLocalizationService.Get("SettingForm.Tab.OCR相关.OCR设置");
            tabPage_OCR相关_OCR设备.Text = _iLocalizationService.Get("SettingForm.Tab.OCR相关.OCR设备");
            tabPage_窗口_英雄选择窗口.Text = _iLocalizationService.Get("SettingForm.Tab.窗口.英雄选择窗口");
            tabPage_窗口_阵容窗口.Text = _iLocalizationService.Get("SettingForm.Tab.窗口.阵容窗口");
            tabPage_窗口_状态窗口.Text = _iLocalizationService.Get("SettingForm.Tab.窗口.状态窗口");
            tabPage_窗口_输出窗口.Text = _iLocalizationService.Get("SettingForm.Tab.窗口.输出窗口");
            tabPage_大数据推荐_装备推荐.Text = _iLocalizationService.Get("SettingForm.Tab.大数据推荐.装备推荐");
            tabPage_大数据推荐_阵容推荐.Text = _iLocalizationService.Get("SettingForm.Tab.大数据推荐.阵容推荐");
            tabPage_功能_自动拿牌_拿牌方式.Text = _iLocalizationService.Get("SettingForm.Tab.功能.自动拿牌.拿牌方式");
            tabPage_功能_自动拿牌_异常处理.Text = _iLocalizationService.Get("SettingForm.Tab.功能.自动拿牌.异常处理");
            tabPage_功能_自动刷新商店_刷新方式.Text = _iLocalizationService.Get("SettingForm.Tab.功能.自动刷新商店.刷新方式");
            tabPage_功能_自动刷新商店_异常处理.Text = _iLocalizationService.Get("SettingForm.Tab.功能.自动刷新商店.异常处理");
            tabPage_功能_自动刷新商店_延迟.Text = _iLocalizationService.Get("SettingForm.Tab.功能.自动刷新商店.延迟");

            // 鼠标设备选项卡
            label_鼠标移动方式.Text = _iLocalizationService.Get("SettingForm.Label.鼠标移动方式");
            label_鼠标移动方式描述.Text = _iLocalizationService.Get("SettingForm.Label.鼠标移动方式描述");
            label_Makcu串口.Text = _iLocalizationService.Get("SettingForm.Label.Makcu串口");
            label_Makcu串口描述.Text = _iLocalizationService.Get("SettingForm.Label.Makcu串口描述");
            label_Makcu波特率.Text = _iLocalizationService.Get("SettingForm.Label.Makcu波特率");
            label_Makcu波特率描述.Text = _iLocalizationService.Get("SettingForm.Label.Makcu波特率描述");
            label_Makcu连接状态.Text = _iLocalizationService.Get("SettingForm.Label.Makcu连接状态");
            label_测试鼠标移动.Text = _iLocalizationService.Get("SettingForm.Label.测试鼠标移动");
            label_测试鼠标移动描述.Text = _iLocalizationService.Get("SettingForm.Label.测试鼠标移动描述");
            label_测试鼠标点击.Text = _iLocalizationService.Get("SettingForm.Label.测试鼠标点击");
            label_测试鼠标点击描述.Text = _iLocalizationService.Get("SettingForm.Label.测试鼠标点击描述");
            roundedButton_刷新Makcu串口.Text = _iLocalizationService.Get("SettingForm.Button.刷新串口");
            roundedButton_测试Makcu连接.Text = _iLocalizationService.Get("SettingForm.Button.测试Makcu连接");
            roundedButton_测试鼠标移动.Text = _iLocalizationService.Get("SettingForm.Button.测试鼠标移动");
            roundedButton_测试鼠标点击.Text = _iLocalizationService.Get("SettingForm.Button.测试鼠标点击");

            // 切换语言后复用统一方法刷新下拉文本，并保持当前设备选择不变。
            UpdateMouseControlModeOptions();
            UpdateMakcuControlState();
            LayoutMouseDeviceSettings();

            // 常规选项卡
            label_界面语言.Text = _iLocalizationService.Get("SettingForm.Label.界面语言");
            label_界面语言描述.Text = _iLocalizationService.Get("SettingForm.Label.界面语言描述");
            label_所有窗口置顶.Text = _iLocalizationService.Get("SettingForm.Label.所有窗口置顶");
            label_所有窗口置顶描述.Text = _iLocalizationService.Get("SettingForm.Label.所有窗口置顶描述");
            label_阵容容量.Text = _iLocalizationService.Get("SettingForm.Label.阵容容量");
            label_阵容容量描述.Text = _iLocalizationService.Get("SettingForm.Label.阵容容量描述");

            // 快捷键标签
            label_召出隐藏窗口_快捷键.Text = _iLocalizationService.Get("SettingForm.Label.快捷键.召出隐藏窗口");
            label_高亮提示_快捷键.Text = _iLocalizationService.Get("SettingForm.Label.快捷键.高亮提示");
            label_自动拿牌_快捷键.Text = _iLocalizationService.Get("SettingForm.Label.快捷键.自动拿牌");
            label_自动刷新商店_快捷键.Text = _iLocalizationService.Get("SettingForm.Label.快捷键.自动刷新商店");
            label_长按自动D牌_快捷键.Text = _iLocalizationService.Get("SettingForm.Label.快捷键.长按自动D牌");
            label_英雄选择窗口_快捷键.Text = _iLocalizationService.Get("SettingForm.Label.快捷键.英雄选择窗口");
            label_阵容选择窗口_快捷键.Text = _iLocalizationService.Get("SettingForm.Label.快捷键.阵容选择窗口");
            label_状态窗口_快捷键.Text = _iLocalizationService.Get("SettingForm.Label.快捷键.状态窗口");
            label_输出窗口_快捷键.Text = _iLocalizationService.Get("SettingForm.Label.快捷键.输出窗口");

            // 功能选项卡-常规
            label_避免程序与用户争夺光标控制权.Text = _iLocalizationService.Get("SettingForm.Label.避免光标冲突");
            label_避免程序与用户争夺光标控制权描述.Text = _iLocalizationService.Get("SettingForm.Label.避免光标冲突描述");
            label_模拟操作间隔.Text = _iLocalizationService.Get("SettingForm.Label.模拟操作间隔");
            label_模拟操作间隔描述.Text = _iLocalizationService.Get("SettingForm.Label.模拟操作间隔描述");

            // 功能选项卡-自动拿牌
            label_模拟鼠标拿牌.Text = _iLocalizationService.Get("SettingForm.Label.模拟鼠标拿牌");
            label_模拟鼠标拿牌描述.Text = _iLocalizationService.Get("SettingForm.Label.模拟鼠标拿牌描述");
            label_模拟按键拿牌.Text = _iLocalizationService.Get("SettingForm.Label.模拟按键拿牌");
            label_模拟按键拿牌描述.Text = _iLocalizationService.Get("SettingForm.Label.模拟按键拿牌描述");
            label_拿牌按键1.Text = _iLocalizationService.Get("SettingForm.Label.拿牌按键1");
            label_拿牌按键2.Text = _iLocalizationService.Get("SettingForm.Label.拿牌按键2");
            label_拿牌按键3.Text = _iLocalizationService.Get("SettingForm.Label.拿牌按键3");
            label_拿牌按键4.Text = _iLocalizationService.Get("SettingForm.Label.拿牌按键4");
            label_拿牌按键5.Text = _iLocalizationService.Get("SettingForm.Label.拿牌按键5");            
            label_自动停止拿牌.Text = _iLocalizationService.Get("SettingForm.Label.自动停止拿牌");
            label_自动停止拿牌描述1.Text = _iLocalizationService.Get("SettingForm.Label.自动停止拿牌描述1");
            label_自动停止拿牌描述2.Text = _iLocalizationService.Get("SettingForm.Label.自动停止拿牌描述2");
            label_自动停止拿牌描述3.Text = _iLocalizationService.Get("SettingForm.Label.自动停止拿牌描述3");

            // 功能选项卡-自动刷新商店                      
            label_模拟鼠标刷新商店.Text = _iLocalizationService.Get("SettingForm.Label.模拟鼠标刷新商店");
            label_模拟鼠标刷新商店描述.Text = _iLocalizationService.Get("SettingForm.Label.模拟鼠标刷新商店描述");
            label_模拟按键刷新商店.Text = _iLocalizationService.Get("SettingForm.Label.模拟按键刷新商店");
            label_模拟按键刷新商店描述.Text = _iLocalizationService.Get("SettingForm.Label.模拟按键刷新商店描述");
            label_刷新商店按键.Text = _iLocalizationService.Get("SettingForm.Label.刷新商店按键");            
            label_刷新失败时自动停止刷新商店.Text = _iLocalizationService.Get("SettingForm.Label.刷新失败时自动停止");
            label_刷新失败时自动停止刷新商店描述1.Text = _iLocalizationService.Get("SettingForm.Label.刷新失败时自动停止描述1");
            label_刷新失败时自动停止刷新商店描述2.Text = _iLocalizationService.Get("SettingForm.Label.刷新失败时自动停止描述2");
            label_刷新失败时自动停止刷新商店描述3.Text = _iLocalizationService.Get("SettingForm.Label.刷新失败时自动停止描述3");
            label_识别错误时自动停止刷新商店.Text = _iLocalizationService.Get("SettingForm.Label.识别错误时自动停止");
            label_识别错误时自动停止刷新商店描述.Text = _iLocalizationService.Get("SettingForm.Label.识别错误时自动停止描述");            
            label_刷新商店间隔_CPU.Text = _iLocalizationService.Get("SettingForm.Label.刷新商店间隔CPU");
            label_刷新商店间隔_CPU_描述.Text = _iLocalizationService.Get("SettingForm.Label.刷新商店间隔CPU描述");
            label_刷新商店间隔_GPU.Text = _iLocalizationService.Get("SettingForm.Label.刷新商店间隔GPU");
            label_刷新商店间隔_GPU_描述.Text = _iLocalizationService.Get("SettingForm.Label.刷新商店间隔GPU描述");

            // 功能选项卡-高亮提示
            label_高亮边框渐变色1.Text = _iLocalizationService.Get("SettingForm.Label.高亮边框渐变色1");
            label_高亮边框渐变色1描述.Text = _iLocalizationService.Get("SettingForm.Label.高亮边框渐变色1描述");
            label_高亮边框渐变色2.Text = _iLocalizationService.Get("SettingForm.Label.高亮边框渐变色2");
            label_高亮边框渐变色2描述.Text = _iLocalizationService.Get("SettingForm.Label.高亮边框渐变色2描述");
            label_高亮边框粗细.Text = _iLocalizationService.Get("SettingForm.Label.高亮边框粗细");
            label_高亮边框粗细描述.Text = _iLocalizationService.Get("SettingForm.Label.高亮边框粗细描述");
            label_渐变流动速度.Text = _iLocalizationService.Get("SettingForm.Label.渐变流动速度");
            label_渐变流动速度描述.Text = _iLocalizationService.Get("SettingForm.Label.渐变流动速度描述");
            
            // 坐标设置
            radioButton_自动设置坐标.Text = _iLocalizationService.Get("SettingForm.Label.自动设置坐标");
            radioButton_手动设置坐标.Text = _iLocalizationService.Get("SettingForm.Label.手动设置坐标");
            label_自动设置坐标提示.Text = _iLocalizationService.Get("SettingForm.Label.自动设置坐标提示");
            label_目标显示器.Text = _iLocalizationService.Get("SettingForm.Label.目标显示器");
            label_目标显示器描述.Text = _iLocalizationService.Get("SettingForm.Label.目标显示器描述");
            label_弈子截图坐标与大小.Text = _iLocalizationService.Get("SettingForm.Label.弈子截图坐标与大小");
            label_弈子截图坐标与大小描述.Text = _iLocalizationService.Get("SettingForm.Label.弈子截图坐标与大小描述");
            roundedButton_弈子截图坐标与大小.Text = _iLocalizationService.Get("SettingForm.Button.弈子截图坐标与大小设置");
            label_商店刷新按钮坐标与大小.Text = _iLocalizationService.Get("SettingForm.Label.商店刷新按钮坐标与大小");
            label_商店刷新按钮坐标与大小描述.Text = _iLocalizationService.Get("SettingForm.Label.商店刷新按钮坐标与大小描述");
            roundedButton_商店刷新按钮坐标与大小.Text = _iLocalizationService.Get("SettingForm.Button.商店刷新按钮坐标与大小");
            label_高亮提示框坐标与大小.Text = _iLocalizationService.Get("SettingForm.Label.高亮提示框坐标与大小");
            label_高亮提示框坐标与大小描述.Text = _iLocalizationService.Get("SettingForm.Label.高亮提示框坐标与大小描述");
            roundedButton_高亮提示框坐标与大小.Text = _iLocalizationService.Get("SettingForm.Button.高亮提示框坐标与大小设置");
            label_自动识别进程.Text = _iLocalizationService.Get("SettingForm.Label.自动识别进程");
            label_自动识别进程描述.Text = _iLocalizationService.Get("SettingForm.Label.自动识别进程描述");
            label_游戏进程窗口.Text = _iLocalizationService.Get("SettingForm.Label.游戏进程窗口");
            label_游戏进程窗口描述.Text = _iLocalizationService.Get("SettingForm.Label.游戏进程窗口描述");
            roundedButton_游戏进程窗口.Text = _iLocalizationService.Get("SettingForm.Button.游戏进程窗口选择");

            // OCR相关
            label_CPU.Text = _iLocalizationService.Get("SettingForm.Label.CPU");
            label_CPU描述.Text = _iLocalizationService.Get("SettingForm.Label.CPU描述");
            label_GPU.Text = _iLocalizationService.Get("SettingForm.Label.GPU");
            label_GPU描述1.Text = _iLocalizationService.Get("SettingForm.Label.GPU描述1");
            label_GPU描述2.Text = _iLocalizationService.Get("SettingForm.Label.GPU描述2");
            label_OCR结果纠正列表.Text = _iLocalizationService.Get("SettingForm.Label.OCR结果纠正列表");
            label_OCR结果纠正列表描述.Text = _iLocalizationService.Get("SettingForm.Label.OCR结果纠正列表描述");
            roundedButton_OCR结果纠正列表.Text = _iLocalizationService.Get("SettingForm.Button.编辑纠正列表");                       
            label_排除字母.Text = _iLocalizationService.Get("SettingForm.Label.排除字母");
            label_排除字母描述.Text = _iLocalizationService.Get("SettingForm.Label.排除字母描述");
            label_排除数字.Text = _iLocalizationService.Get("SettingForm.Label.排除数字");
            label_排除数字描述.Text = _iLocalizationService.Get("SettingForm.Label.排除数字描述");
            label_严格匹配模式.Text = _iLocalizationService.Get("SettingForm.Label.严格匹配模式");
            label_严格匹配模式描述.Text = _iLocalizationService.Get("SettingForm.Label.严格匹配模式描述");
            
            // 窗口设置            
            label_启用英雄选择面板.Text = _iLocalizationService.Get("SettingForm.Label.启用英雄选择面板");
            label_启用英雄选择面板描述.Text = _iLocalizationService.Get("SettingForm.Label.启用英雄选择面板描述");
            label_英雄头像框边长.Text = _iLocalizationService.Get("SettingForm.Label.英雄头像框边长");
            label_英雄头像框边框描述.Text = _iLocalizationService.Get("SettingForm.Label.英雄头像框边长描述");
            label_英雄头像框水平间隔.Text = _iLocalizationService.Get("SettingForm.Label.英雄头像框水平间隔");
            label_英雄头像框水平间隔描述.Text = _iLocalizationService.Get("SettingForm.Label.英雄头像框水平间隔描述");
            label_英雄头像框垂直间隔.Text = _iLocalizationService.Get("SettingForm.Label.英雄头像框垂直间隔");
            label_英雄头像框垂直间隔描述.Text = _iLocalizationService.Get("SettingForm.Label.英雄头像框垂直间隔描述");            
            label_启用阵容面板.Text = _iLocalizationService.Get("SettingForm.Label.启用阵容面板");
            label_启用阵容面板描述.Text = _iLocalizationService.Get("SettingForm.Label.启用阵容面板描述");            
            label_启用状态面板.Text = _iLocalizationService.Get("SettingForm.Label.启用状态面板");
            label_启用状态面板描述.Text = _iLocalizationService.Get("SettingForm.Label.启用状态面板描述");            
            label_启用输出面板.Text = _iLocalizationService.Get("SettingForm.Label.启用输出面板");
            label_启用输出面板描述.Text = _iLocalizationService.Get("SettingForm.Label.启用输出面板描述");

            // 大数据推荐            
            label_程序启动时更新推荐装备.Text = _iLocalizationService.Get("SettingForm.Label.程序启动时更新推荐装备");
            label_程序启动时更新推荐装备描述1.Text = _iLocalizationService.Get("SettingForm.Label.程序启动时更新推荐装备描述1");
            label_程序启动时更新推荐装备描述2.Text = _iLocalizationService.Get("SettingForm.Label.程序启动时更新推荐装备描述2");
            label_程序启东时更新推荐装备描述3.Text = _iLocalizationService.Get("SettingForm.Label.程序启动时更新推荐装备描述3");            
            label_程序启动时更新推荐阵容.Text = _iLocalizationService.Get("SettingForm.Label.程序启动时更新推荐阵容");
            label_程序启动时更新推荐阵容描述1.Text = _iLocalizationService.Get("SettingForm.Label.程序启动时更新推荐阵容描述1");
            label_程序启动时更新推荐阵容描述2.Text = _iLocalizationService.Get("SettingForm.Label.程序启动时更新推荐阵容描述2");
            label_程序启动时更新推荐阵容描述3.Text = _iLocalizationService.Get("SettingForm.Label.程序启动时更新推荐阵容描述3");

            // 开发者选项
            label_保存截图.Text = _iLocalizationService.Get("SettingForm.Label.保存截图");
            label_保存截图描述.Text = _iLocalizationService.Get("SettingForm.Label.保存截图描述");
        }
        #endregion




        
    }
}
