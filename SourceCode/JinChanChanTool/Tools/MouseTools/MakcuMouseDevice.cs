using System.Diagnostics;
using System.IO.Ports;
using System.Text;

namespace JinChanChanTool.Tools.MouseTools
{
    /// <summary>
    /// 通过 Makcu 串口协议执行鼠标操作的设备实现。
    /// </summary>
    internal sealed class MakcuMouseDevice : IMouseOperationDevice, IDisposable
    {
        /// <summary>
        /// 单击时按下与抬起之间的最小等待时间（毫秒）。
        /// </summary>
        private const int LeftClickReleaseDelayMilliseconds = 2;

        private const int ConnectionTimeoutMilliseconds = 500;
        private readonly object syncRoot = new();
        private SerialPort? serialPort;

        public bool IsConnected
        {
            get
            {
                lock (syncRoot)
                {
                    return serialPort?.IsOpen == true;
                }
            }
        }

        public static string[] GetPortNames()
        {
            return SerialPort.GetPortNames()
                .OrderBy(GetPortNumber)
                .ThenBy(port => port, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        public bool TryConnect(string portName, int baudRate, out string error)
        {
            lock (syncRoot)
            {
                error = string.Empty;

                if (string.IsNullOrWhiteSpace(portName))
                {
                    error = "尚未选择 Makcu 串口。";
                    return false;
                }

                if (baudRate <= 0)
                {
                    error = "Makcu 波特率无效。";
                    return false;
                }

                if (serialPort?.IsOpen == true &&
                    string.Equals(serialPort.PortName, portName, StringComparison.OrdinalIgnoreCase) &&
                    serialPort.BaudRate == baudRate)
                {
                    return true;
                }

                DisconnectNoLock();

                try
                {
                    serialPort = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One)
                    {
                        Encoding = Encoding.ASCII,
                        ReadTimeout = 50,
                        WriteTimeout = ConnectionTimeoutMilliseconds,
                        DtrEnable = false,
                        RtsEnable = false,
                        NewLine = "\r\n"
                    };
                    serialPort.Open();
                    serialPort.DiscardInBuffer();
                    serialPort.DiscardOutBuffer();

                    if (!VerifyDeviceNoLock(out error))
                    {
                        DisconnectNoLock();
                        return false;
                    }

                    // 关闭命令回显，避免长时间自动操作时串口接收缓冲区堆积。
                    serialPort.Write("km.echo(0)\r\n");
                    return true;
                }
                catch (Exception ex)
                {
                    error = $"打开 Makcu 串口 {portName} 失败：{ex.Message}";
                    DisconnectNoLock();
                    return false;
                }
            }
        }

        /// <summary>
        /// 将相对位移拆分为 Makcu 协议支持的短整型分段移动命令。
        /// </summary>
        /// <param name="deltaX">横向位移。</param>
        /// <param name="deltaY">纵向位移。</param>
        /// <param name="error">发送失败时的错误信息。</param>
        /// <returns>命令是否成功发送。</returns>
        public bool TryMove(int deltaX, int deltaY, out string error)
        {
            StringBuilder commands = new StringBuilder();
            int remainingX = deltaX;
            int remainingY = deltaY;

            do
            {
                int stepX = Math.Clamp(remainingX, short.MinValue, short.MaxValue);
                int stepY = Math.Clamp(remainingY, short.MinValue, short.MaxValue);
                commands.Append("km.move(")
                    .Append(stepX)
                    .Append(',')
                    .Append(stepY)
                    .Append(")\r\n");
                remainingX -= stepX;
                remainingY -= stepY;
            }
            while (remainingX != 0 || remainingY != 0);

            return TrySend(commands.ToString(), out error);
        }

        /// <summary>
        /// 将 Makcu 的相对移动能力转换为统一设备接口要求的绝对坐标移动。
        /// </summary>
        public bool TrySetMousePosition(int x, int y, out string error)
        {
            Point currentPosition = Cursor.Position;
            long deltaX = (long)x - currentPosition.X;
            long deltaY = (long)y - currentPosition.Y;
            if (deltaX < int.MinValue || deltaX > int.MaxValue ||
                deltaY < int.MinValue || deltaY > int.MaxValue)
            {
                error = "Makcu 相对移动距离超出坐标范围。";
                return false;
            }

            return TryMove((int)deltaX, (int)deltaY, out error);
        }

        /// <summary>
        /// 按下 Makcu 控制的鼠标左键。
        /// </summary>
        public bool TryLeftButtonDown(out string error)
        {
            return TrySetLeftButtonState(true, out error);
        }

        /// <summary>
        /// 抬起 Makcu 控制的鼠标左键。
        /// </summary>
        public bool TryLeftButtonUp(out string error)
        {
            return TrySetLeftButtonState(false, out error);
        }

        /// <summary>
        /// 使用按下和抬起的统一接口完成一次 Makcu 左键单击。
        /// </summary>
        public bool TryLeftClick(out string error)
        {
            return TryClickLeftButton(out error);
        }

        /// <summary>
        /// 按下并抬起 Makcu 控制的鼠标左键。
        /// </summary>
        public bool TryClickLeftButton(out string error)
        {
            if (!TryLeftButtonDown(out error))
            {
                return false;
            }

            Thread.Sleep(LeftClickReleaseDelayMilliseconds);
            return TryLeftButtonUp(out error);
        }

        /// <summary>
        /// 向 Makcu 发送鼠标左键状态命令，并在串口异常时尽力释放按键。
        /// </summary>
        /// <param name="isPressed">是否按下鼠标左键。</param>
        /// <param name="error">发送失败时的错误信息。</param>
        /// <returns>命令是否成功发送。</returns>
        private bool TrySetLeftButtonState(bool isPressed, out string error)
        {
            lock (syncRoot)
            {
                error = string.Empty;
                if (serialPort?.IsOpen != true)
                {
                    error = "Makcu 尚未连接。";
                    return false;
                }

                try
                {
                    DiscardPendingInputNoLock();
                    string command = isPressed ? "km.left(1)\r\n" : "km.left(0)\r\n";
                    serialPort.Write(command);
                    return true;
                }
                catch (Exception ex)
                {
                    TryReleaseLeftButtonNoLock();
                    error = $"Makcu 串口写入失败：{ex.Message}";
                    DisconnectNoLock();
                    return false;
                }
            }
        }

        public void Disconnect()
        {
            lock (syncRoot)
            {
                DisconnectNoLock();
            }
        }

        public void Dispose()
        {
            Disconnect();
        }

        private bool TrySend(string commands, out string error)
        {
            lock (syncRoot)
            {
                error = string.Empty;
                if (serialPort?.IsOpen != true)
                {
                    error = "Makcu 尚未连接。";
                    return false;
                }

                try
                {
                    DiscardPendingInputNoLock();
                    serialPort.Write(commands);
                    return true;
                }
                catch (Exception ex)
                {
                    error = $"Makcu 串口写入失败：{ex.Message}";
                    DisconnectNoLock();
                    return false;
                }
            }
        }

        private bool VerifyDeviceNoLock(out string error)
        {
            error = string.Empty;
            serialPort!.DiscardInBuffer();
            serialPort.Write("km.version()\r\n");

            StringBuilder response = new StringBuilder();
            Stopwatch timer = Stopwatch.StartNew();
            while (timer.ElapsedMilliseconds < ConnectionTimeoutMilliseconds)
            {
                if (serialPort.BytesToRead > 0)
                {
                    response.Append(serialPort.ReadExisting());
                    if (response.ToString().Contains("km.MAKCU", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

                Thread.Sleep(10);
            }

            error = $"串口 {serialPort.PortName} 已打开，但未收到 Makcu 设备响应。请检查串口和波特率。";
            return false;
        }

        private void DiscardPendingInputNoLock()
        {
            if (serialPort?.IsOpen == true && serialPort.BytesToRead > 0)
            {
                serialPort.DiscardInBuffer();
            }
        }

        private void TryReleaseLeftButtonNoLock()
        {
            try
            {
                if (serialPort?.IsOpen == true)
                {
                    serialPort.Write("km.left(0)\r\n");
                }
            }
            catch
            {
            }
        }

        private void DisconnectNoLock()
        {
            if (serialPort == null)
            {
                return;
            }

            try
            {
                if (serialPort.IsOpen)
                {
                    serialPort.Close();
                }
            }
            catch
            {
            }
            finally
            {
                serialPort.Dispose();
                serialPort = null;
            }
        }

        private static int GetPortNumber(string portName)
        {
            return portName.Length > 3 && int.TryParse(portName.AsSpan(3), out int number)
                ? number
                : int.MaxValue;
        }
    }
}
