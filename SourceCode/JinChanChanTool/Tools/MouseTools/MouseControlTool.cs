using System.Runtime.InteropServices;

namespace JinChanChanTool.Tools.MouseTools
{
    public static class MouseControlTool
    {
        private const int MouseEventLeftDown = 0x02;
        private const int MouseEventLeftUp = 0x04;
        private static readonly MakcuMouseDevice MakcuDevice = new();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern void mouse_event(int flags, int dx, int dy, int buttons, int extraInfo);

        public static bool IsMakcuConnected => MakcuDevice.IsConnected;

        public static string[] GetMakcuPortNames()
        {
            return MakcuMouseDevice.GetPortNames();
        }

        public static bool TryConnectMakcu(string portName, int baudRate, out string error)
        {
            MakcuDevice.Disconnect();
            return MakcuDevice.TryConnect(portName, baudRate, out error);
        }

        public static void DisconnectMakcu()
        {
            MakcuDevice.Disconnect();
        }

        public static bool TrySetMousePosition(
            int x,
            int y,
            DataClass.ManualSettings settings,
            out string error)
        {
            error = string.Empty;
            if (settings.MouseControlMode == DataClass.MouseControlMode.WinApi)
            {
                if (SetCursorPos(x, y))
                {
                    return true;
                }

                error = $"WinAPI 设置鼠标位置失败：{new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error()).Message}";
                return false;
            }

            if (!EnsureMakcuConnected(settings, out error))
            {
                return false;
            }

            Point currentPosition = Cursor.Position;
            return MakcuDevice.TryMove(x - currentPosition.X, y - currentPosition.Y, out error);
        }

        public static bool TryClickLeftButton(DataClass.ManualSettings settings, out string error)
        {
            error = string.Empty;
            if (settings.MouseControlMode == DataClass.MouseControlMode.WinApi)
            {
                mouse_event(MouseEventLeftDown, 0, 0, 0, 0);
                mouse_event(MouseEventLeftUp, 0, 0, 0, 0);
                return true;
            }

            return EnsureMakcuConnected(settings, out error) &&
                   MakcuDevice.TryLeftClick(out error);
        }

        public static bool TryMoveMakcu(
            int deltaX,
            int deltaY,
            DataClass.ManualSettings settings,
            out string error)
        {
            error = string.Empty;
            if (settings.MouseControlMode != DataClass.MouseControlMode.Makcu)
            {
                error = "请先将鼠标移动方式切换为 Makcu。";
                return false;
            }

            return EnsureMakcuConnected(settings, out error) &&
                   MakcuDevice.TryMove(deltaX, deltaY, out error);
        }

        private static bool EnsureMakcuConnected(DataClass.ManualSettings settings, out string error)
        {
            return MakcuDevice.TryConnect(settings.MakcuPortName, settings.MakcuBaudRate, out error);
        }
    }
}
