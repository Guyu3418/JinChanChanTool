using JinChanChanTool.DataClass;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace JinChanChanTool.Tools.MouseTools
{
    /// <summary>
    /// WinAPI 鼠标设备实现，同时保留既有静态门面以兼容业务层和设置页调用。
    /// </summary>
    public sealed class MouseControlTool : IMouseOperationDevice
    {
        /// <summary>
        /// WinAPI 左键按下事件标记。
        /// </summary>
        private const int MouseEventLeftDown = 0x02;

        /// <summary>
        /// WinAPI 左键抬起事件标记。
        /// </summary>
        private const int MouseEventLeftUp = 0x04;

        /// <summary>
        /// 兼容旧版公开常量，表示鼠标左键按下事件。
        /// </summary>
        public const int MOUSEEVENTF_LEFTDOWN = MouseEventLeftDown;

        /// <summary>
        /// 兼容旧版公开常量，表示鼠标左键抬起事件。
        /// </summary>
        public const int MOUSEEVENTF_LEFTUP = MouseEventLeftUp;

        /// <summary>
        /// 仅供静态兼容门面使用的 WinAPI 设备实例。
        /// </summary>
        private static readonly MouseControlTool WinApiDevice = new MouseControlTool();

        /// <summary>
        /// 仅供静态兼容门面使用的 Makcu 设备实例。
        /// </summary>
        private static readonly MakcuMouseDevice MakcuDevice = new MakcuMouseDevice();

        /// <summary>
        /// 防止外部创建多余的 WinAPI 设备实例，统一由静态门面维护。
        /// </summary>
        private MouseControlTool()
        {
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern void mouse_event(int flags, int dx, int dy, int buttons, int extraInfo);

        /// <summary>
        /// 获取 Makcu 设备当前是否已经完成串口连接。
        /// </summary>
        public static bool IsMakcuConnected => MakcuDevice.IsConnected;

        /// <summary>
        /// 获取可供 Makcu 使用的串口名称。
        /// </summary>
        /// <returns>按端口编号排序的串口名称。</returns>
        public static string[] GetMakcuPortNames()
        {
            return MakcuMouseDevice.GetPortNames();
        }

        /// <summary>
        /// 按指定串口和波特率建立 Makcu 连接。
        /// </summary>
        /// <param name="portName">串口名称。</param>
        /// <param name="baudRate">串口波特率。</param>
        /// <param name="error">连接失败时的错误信息。</param>
        /// <returns>连接是否成功。</returns>
        public static bool TryConnectMakcu(string portName, int baudRate, out string error)
        {
            MakcuDevice.Disconnect();
            return MakcuDevice.TryConnect(portName, baudRate, out error);
        }

        /// <summary>
        /// 主动断开 Makcu 串口，供设置切换和应用退出时调用。
        /// </summary>
        public static void DisconnectMakcu()
        {
            MakcuDevice.Disconnect();
        }

        /// <summary>
        /// 按当前设置选择设备并将鼠标移动到绝对坐标。
        /// </summary>
        /// <param name="x">目标横坐标。</param>
        /// <param name="y">目标纵坐标。</param>
        /// <param name="settings">当前鼠标设备配置。</param>
        /// <param name="error">失败时的错误信息。</param>
        /// <returns>操作是否成功。</returns>
        public static bool TrySetMousePosition(int x, int y, ManualSettings settings, out string error)
        {
            if (!TryGetMouseOperationDevice(settings, out IMouseOperationDevice device, out error))
            {
                return false;
            }

            return device.TrySetMousePosition(x, y, out error);
        }

        /// <summary>
        /// 按当前设置选择设备并执行一次鼠标左键单击。
        /// </summary>
        /// <param name="settings">当前鼠标设备配置。</param>
        /// <param name="error">失败时的错误信息。</param>
        /// <returns>操作是否成功。</returns>
        public static bool TryClickLeftButton(ManualSettings settings, out string error)
        {
            if (!TryGetMouseOperationDevice(settings, out IMouseOperationDevice device, out error))
            {
                return false;
            }

            return device.TryClickLeftButton(out error);
        }

        /// <summary>
        /// 在 Makcu 模式下执行指定相对位移，供设置页连接测试使用。
        /// </summary>
        /// <param name="deltaX">横向位移。</param>
        /// <param name="deltaY">纵向位移。</param>
        /// <param name="settings">当前鼠标设备配置。</param>
        /// <param name="error">失败时的错误信息。</param>
        /// <returns>操作是否成功。</returns>
        public static bool TryMoveMakcu(int deltaX, int deltaY, ManualSettings settings, out string error)
        {
            error = string.Empty;
            if (settings == null || settings.MouseControlMode != MouseControlMode.Makcu)
            {
                error = "请先将鼠标移动方式切换为 Makcu。";
                return false;
            }

            if (!EnsureMakcuConnected(settings, out error))
            {
                return false;
            }

            IMouseOperationDevice makcuOperationDevice = MakcuDevice;
            return makcuOperationDevice.TryMove(deltaX, deltaY, out error);
        }

        /// <summary>
        /// 保留旧版 WinAPI 调用入口：移动至坐标后单击左键。
        /// </summary>
        /// <param name="x">目标横坐标。</param>
        /// <param name="y">目标纵坐标。</param>
        public static void SetMousePositionAndClickLeftButton(int x, int y)
        {
            IMouseOperationDevice winApiOperationDevice = WinApiDevice;
            winApiOperationDevice.TrySetMousePosition(x, y, out string ignoredMoveError);
            winApiOperationDevice.TryClickLeftButton(out string ignoredClickError);
        }

        /// <summary>
        /// 保留旧版 WinAPI 调用入口：移动至指定坐标。
        /// </summary>
        /// <param name="x">目标横坐标。</param>
        /// <param name="y">目标纵坐标。</param>
        public static void SetMousePosition(int x, int y)
        {
            IMouseOperationDevice winApiOperationDevice = WinApiDevice;
            winApiOperationDevice.TrySetMousePosition(x, y, out string ignoredError);
        }

        /// <summary>
        /// 保留旧版 WinAPI 调用入口：按下鼠标左键。
        /// </summary>
        public static void MakeMouseLeftButtonDown()
        {
            IMouseOperationDevice winApiOperationDevice = WinApiDevice;
            winApiOperationDevice.TryLeftButtonDown(out string ignoredError);
        }

        /// <summary>
        /// 保留旧版 WinAPI 调用入口：抬起鼠标左键。
        /// </summary>
        public static void MakeMouseLeftButtonUp()
        {
            IMouseOperationDevice winApiOperationDevice = WinApiDevice;
            winApiOperationDevice.TryLeftButtonUp(out string ignoredError);
        }

        /// <summary>
        /// 根据配置返回对应设备；Makcu 连接异常会原样上报，不会静默改用 WinAPI。
        /// </summary>
        /// <param name="settings">当前鼠标设备配置。</param>
        /// <param name="device">可执行操作的设备。</param>
        /// <param name="error">获取设备失败时的错误信息。</param>
        /// <returns>是否获得可用设备。</returns>
        private static bool TryGetMouseOperationDevice(
            ManualSettings? settings,
            out IMouseOperationDevice device,
            out string error)
        {
            device = WinApiDevice;
            error = string.Empty;
            if (settings == null)
            {
                error = "未提供鼠标设备配置。";
                return false;
            }

            if (settings.MouseControlMode == MouseControlMode.WinApi)
            {
                return true;
            }

            if (settings.MouseControlMode == MouseControlMode.Makcu)
            {
                if (!EnsureMakcuConnected(settings, out error))
                {
                    return false;
                }

                device = MakcuDevice;
                return true;
            }

            error = "未识别的鼠标移动方式。";
            return false;
        }

        /// <summary>
        /// 按当前配置确保 Makcu 串口处于可用连接状态。
        /// </summary>
        /// <param name="settings">当前鼠标设备配置。</param>
        /// <param name="error">连接失败时的错误信息。</param>
        /// <returns>Makcu 是否可用。</returns>
        private static bool EnsureMakcuConnected(ManualSettings settings, out string error)
        {
            return MakcuDevice.TryConnect(settings.MakcuPortName, settings.MakcuBaudRate, out error);
        }

        /// <summary>
        /// WinAPI 设备实现：将鼠标移动到绝对坐标。
        /// </summary>
        bool IMouseOperationDevice.TrySetMousePosition(int x, int y, out string error)
        {
            error = string.Empty;
            if (SetCursorPos(x, y))
            {
                return true;
            }

            error = $"WinAPI 设置鼠标位置失败：{new Win32Exception(Marshal.GetLastWin32Error()).Message}";
            return false;
        }

        /// <summary>
        /// WinAPI 设备实现：基于当前系统鼠标位置完成相对移动。
        /// </summary>
        bool IMouseOperationDevice.TryMove(int deltaX, int deltaY, out string error)
        {
            Point currentPosition = Cursor.Position;
            long targetX = (long)currentPosition.X + deltaX;
            long targetY = (long)currentPosition.Y + deltaY;
            if (targetX < int.MinValue || targetX > int.MaxValue ||
                targetY < int.MinValue || targetY > int.MaxValue)
            {
                error = "WinAPI 相对移动目标超出坐标范围。";
                return false;
            }

            IMouseOperationDevice winApiOperationDevice = WinApiDevice;
            return winApiOperationDevice.TrySetMousePosition((int)targetX, (int)targetY, out error);
        }

        /// <summary>
        /// WinAPI 设备实现：按下鼠标左键。
        /// </summary>
        bool IMouseOperationDevice.TryLeftButtonDown(out string error)
        {
            error = string.Empty;
            mouse_event(MouseEventLeftDown, 0, 0, 0, 0);
            return true;
        }

        /// <summary>
        /// WinAPI 设备实现：抬起鼠标左键。
        /// </summary>
        bool IMouseOperationDevice.TryLeftButtonUp(out string error)
        {
            error = string.Empty;
            mouse_event(MouseEventLeftUp, 0, 0, 0, 0);
            return true;
        }

        /// <summary>
        /// WinAPI 设备实现：按下并抬起鼠标左键。
        /// </summary>
        bool IMouseOperationDevice.TryClickLeftButton(out string error)
        {
            IMouseOperationDevice winApiOperationDevice = WinApiDevice;
            if (!winApiOperationDevice.TryLeftButtonDown(out error))
            {
                return false;
            }

            return winApiOperationDevice.TryLeftButtonUp(out error);
        }
    }
}
