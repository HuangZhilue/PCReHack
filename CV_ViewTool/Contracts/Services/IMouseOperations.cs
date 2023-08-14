using CV_ViewTool.Services;
using static CV_ViewTool.Services.MouseOperations;

namespace CV_ViewTool.Contracts.Services
{
    internal interface IMouseOperations
    {
        void SetCursorPosition(int x, int y);
        void SetCursorPosition(MousePoint point);
        MousePoint GetCursorPosition();
        void MouseEvent(MouseEventFlags value);
    }
}
