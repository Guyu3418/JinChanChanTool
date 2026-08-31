namespace JinChanChanTool.Tools.MouseTools
{
    /// <summary>
    /// 规范不同鼠标输入设备可执行的基础操作，供 WinAPI 和 Makcu 实现共享调用契约。
    /// </summary>
    public interface IMouseOperationDevice
    {
        /// <summary>
        /// 将鼠标移动到屏幕绝对坐标。
        /// </summary>
        /// <param name="x">目标横坐标。</param>
        /// <param name="y">目标纵坐标。</param>
        /// <param name="error">失败时的错误信息。</param>
        /// <returns>操作是否成功。</returns>
        bool TrySetMousePosition(int x, int y, out string error);

        /// <summary>
        /// 按相对位移移动鼠标。
        /// </summary>
        /// <param name="deltaX">横向位移。</param>
        /// <param name="deltaY">纵向位移。</param>
        /// <param name="error">失败时的错误信息。</param>
        /// <returns>操作是否成功。</returns>
        bool TryMove(int deltaX, int deltaY, out string error);

        /// <summary>
        /// 按下鼠标左键。
        /// </summary>
        /// <param name="error">失败时的错误信息。</param>
        /// <returns>操作是否成功。</returns>
        bool TryLeftButtonDown(out string error);

        /// <summary>
        /// 抬起鼠标左键。
        /// </summary>
        /// <param name="error">失败时的错误信息。</param>
        /// <returns>操作是否成功。</returns>
        bool TryLeftButtonUp(out string error);

        /// <summary>
        /// 完成一次鼠标左键单击。
        /// </summary>
        /// <param name="error">失败时的错误信息。</param>
        /// <returns>操作是否成功。</returns>
        bool TryClickLeftButton(out string error);
    }
}
