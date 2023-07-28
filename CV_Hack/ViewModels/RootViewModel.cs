using CV_Hack.Helper.WinAPI;
using CV_Hack.ViewModels.Test.OpenCV;
using HandyControl.Tools;
using Stylet;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static CV_Hack.Helper.WinAPI.TouchInjector;
using IContainer = StyletIoC.IContainer;

namespace CV_Hack.ViewModels;

public class RootViewModel : Screen
{
    // https://stackoverflow.com/questions/41075778/register-a-touch-event-on-windows-10-without-a-mouse-click-or-actual-touch
    // https://docs.microsoft.com/zh-cn/windows/win32/api/winuser/nf-winuser-injecttouchinput?redirectedfrom=MSDN
    // https://github.com/DuelCode/TouchSimulate/blob/master/InjectTouchInputTest/MainWindow.xaml.cs
    // https://github.com/furuya02/AlTouch
    // https://social.technet.microsoft.com/wiki/contents/articles/6460.simulating-touch-input-in-windows-8-using-touch-injection-api.aspx

    private HookProc _globalLlMouseHookCallback;
    private IntPtr _hGlobalLlMouseHook;

    public string Title { get; set; } = "HandyControl Application";
    public double Top { get; set; } = 200;
    public double Left { get; set; } = 200;

    private IWindowManager WindowManager { get; }
    private IContainer Container { get; }

    public RootViewModel(IWindowManager windowManager, IContainer container)
    {
        WindowManager = windowManager;
        Container = container;
        InitializeTouchInjection();
        SetUpHook();
    }

    public void AddSimple()
    {
        WindowManager.ShowWindow(new InRangeViewModel(WindowManager, Container));
    }

    #region 鼠标监控、模拟触屏

    private void SetUpHook()
    {
        Logger.Debug("Setting up global mouse hook");

        // Create an instance of HookProc.
        _globalLlMouseHookCallback = LowLevelMouseProc;

        _hGlobalLlMouseHook = NativeMethods.SetWindowsHookEx(
            HookType.WH_MOUSE_LL,
            _globalLlMouseHookCallback,
            Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]),
            0);

        if (_hGlobalLlMouseHook == IntPtr.Zero)
        {
            //Logger.Fatal("Unable to set global mouse hook");
            throw new Win32Exception("Unable to set MouseHook");
        }
    }

    private void ClearHook()
    {
        Logger.Debug("Deleting global mouse hook");

        if (_hGlobalLlMouseHook != IntPtr.Zero)
        {
            // Unhook the low-level mouse hook
            if (!NativeMethods.UnhookWindowsHookEx(_hGlobalLlMouseHook))
                throw new Win32Exception("Unable to clear MouseHoo;");

            _hGlobalLlMouseHook = IntPtr.Zero;
        }
    }

    private static MouseMessage LastMouseMessage { get; set; }
    public static int LastX { get; set; }
    public static int LastY { get; set; }

    //public int LowLevelMouseProc(int nCode, MouseMessage wParam, MSLLHOOKSTRUCT lParam)
    public int LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam)
    {
        MSLLHOOKSTRUCT hookData = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

        if (nCode >= 0)
        {
            MouseMessage wParam1 = (MouseMessage)wParam;
            switch (wParam1)
            {
                case MouseMessage.WM_LBUTTONDOWN:
                    {
                        if (hookData.pt.x + 1 == LastX && hookData.pt.y + 1 == LastY) { break; }
                        Debug.WriteLine($"Left Mouse down\tX:{hookData.pt.x}\tY:{hookData.pt.y}");
                        LastMouseMessage = wParam1;
                        SimulateTouch("Down", hookData.pt.x - 1, hookData.pt.y - 1, 1);
                        Task.Run(() =>
                        {
                            while (LastMouseMessage == MouseMessage.WM_LBUTTONDOWN)
                            {
                                Debug.WriteLine($"Left Mouse Long down\tX:{hookData.pt.x}\tY:{hookData.pt.y}");
                                SimulateTouch("Hold", hookData.pt.x - 1, hookData.pt.y - 1, 1);
                            }
                        });
                        LastX = hookData.pt.x;
                        LastY = hookData.pt.y;
                        break;
                    }
                case MouseMessage.WM_LBUTTONUP:
                    {
                        if (hookData.pt.x + 1 == LastX && hookData.pt.y + 1 == LastY) { break; }
                        Debug.WriteLine($"Left Mouse up\tX:{hookData.pt.x}\tY:{hookData.pt.y}");
                        LastMouseMessage = wParam1;
                        SimulateTouch("Up", hookData.pt.x - 1, hookData.pt.y - 1, 1);
                        LastX = hookData.pt.x;
                        LastY = hookData.pt.y;
                        break;
                    }
                case MouseMessage.WM_MOUSEMOVE:
                    {
                        if (LastMouseMessage == MouseMessage.WM_LBUTTONDOWN)
                        {
                            //LastMouseMessage = wParam1;
                            SimulateTouch("Move", hookData.pt.x - LastX, hookData.pt.y - LastY, 1);

                            LastX = hookData.pt.x;
                            LastY = hookData.pt.y;
                        }
                        break;
                    }
            }
        }

        // Pass the hook information to the next hook procedure in chain
        return NativeMethods.CallNextHookEx(_hGlobalLlMouseHook, nCode, wParam, lParam);
    }

    private static PointerTouchInfo Contact;

    private static void SimulateTouch(/*MouseMessage*/string touchType, int x, int y, uint unPointerId)
    {
        //PointerTouchInfo 
        //switch (touchType)
        switch (touchType.ToUpper())
        {
            //case MouseMessage.WM_LBUTTONDOWN:
            case "DOWN":
                {
                    Contact = MakePointerTouchInfo(x, y, 5, unPointerId, 1);
                    // Touch Down Simulate
                    Debug.WriteLine("Touch Down Simulate");
                    PointerFlags oFlags = PointerFlags.DOWN | PointerFlags.INRANGE | PointerFlags.INCONTACT;
                    Contact.PointerInfo.PointerFlags = oFlags;
                    break;
                }
            //case MouseMessage.WM_MOUSEMOVE:
            case "MOVE":
                {
                    // Touch Move Simulate
                    Debug.WriteLine("Touch Move Simulate");
                    //x,y is delta x,y
                    Contact.Move(x, y);
                    PointerFlags oFlags = PointerFlags.INRANGE | PointerFlags.INCONTACT | PointerFlags.UPDATE;
                    Contact.PointerInfo.PointerFlags = oFlags;
                    break;
                }
            //case MouseMessage.WM_LBUTTONUP:
            case "UP":
                {
                    // Touch Up Simulate
                    Debug.WriteLine("Touch Up Simulate");
                    Debug.WriteLine("========================================");
                    Contact.PointerInfo.PointerFlags = PointerFlags.UP;
                    break;
                }
            case "HOLD":
                {
                    // Touch Up Simulate
                    Debug.WriteLine("Touch Hold Simulate");
                    Debug.WriteLine("========================================");
                    Contact.PointerInfo.PointerFlags = PointerFlags.UPDATE | PointerFlags.INRANGE | PointerFlags.INCONTACT;
                    break;
                }
        }

        InjectTouchInput(1, new[] { Contact });
    }

    private static PointerTouchInfo MakePointerTouchInfo(
        int x,
        int y,
        int radius,
        uint unPointerId,
        uint orientation = 90,
        uint pressure = 32000)
    {
        PointerTouchInfo contact = new()
        {
            Pressure = pressure
        };
        contact.PointerInfo.pointerType = PointerInputType.TOUCH;
        contact.TouchFlags = TouchFlags.NONE;
        contact.Orientation = orientation;
        contact.TouchMasks = TouchMask.CONTACTAREA | TouchMask.ORIENTATION | TouchMask.PRESSURE;
        contact.PointerInfo.PtPixelLocation.X = x;
        contact.PointerInfo.PtPixelLocation.Y = y;
        contact.PointerInfo.PointerId = unPointerId;
        contact.ContactArea.left = x - radius;
        contact.ContactArea.right = x + radius;
        contact.ContactArea.top = y - radius;
        contact.ContactArea.bottom = y + radius;
        return contact;
    }

    #endregion

    protected override void OnClose()
    {
        ClearHook();
        base.OnClose();
    }
}