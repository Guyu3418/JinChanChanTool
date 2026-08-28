using System.Diagnostics;
using System.IO.Ports;
using System.Text;

namespace JinChanChanTool.Tools.MouseTools
{
    internal sealed class MakcuMouseDevice : IDisposable
    {
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

        public bool TryMove(int deltaX, int deltaY, out string error)
        {
            var commands = new StringBuilder();
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

        public bool TryLeftClick(out string error)
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
                    serialPort.Write("km.left(1)\r\n");
                    Thread.Sleep(2);
                    serialPort.Write("km.left(0)\r\n");
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

            var response = new StringBuilder();
            var timer = Stopwatch.StartNew();
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
