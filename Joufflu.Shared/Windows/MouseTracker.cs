using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using static Joufflu.Shared.Windows.MouseMoveNative;

namespace Joufflu.Shared.Windows
{
    public static class MouseMoveNative
    {
        public delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        // Win32 API constants and delegates
        public const int WH_MOUSE_LL = 14;
        public const int WM_MOUSEMOVE = 0x0200;

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public static implicit operator System.Drawing.Point(POINT point)
            {
                return new System.Drawing.Point(point.X, point.Y);
            }

            public static implicit operator Point(POINT point)
            {
                return new Point(point.X, point.Y);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEHOOKSTRUCT
        {
            public POINT Position;
            public IntPtr hwnd;
            public uint wHitTestCode;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);
    }

    public class MouseTracker : IDisposable
    {
        private IntPtr _hookID;
        private readonly Action<Point> _mouseMoveCallback;
        private readonly LowLevelMouseProc _proc;

        public MouseTracker(Action<Point> mouseMoveCallback) { 
            _mouseMoveCallback = mouseMoveCallback;
            _proc = HookCallback;
        }

        public IntPtr SetHook()
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule!)
            {
                _hookID = SetWindowsHookEx(WH_MOUSE_LL, _proc!,
                    GetModuleHandle(curModule.ModuleName!), 0);
            }
            return _hookID;
        }

        public void UnsetHook()
        {
            if (_hookID == IntPtr.Zero)
                return;
            UnhookWindowsHookEx(_hookID);
            _hookID = IntPtr.Zero;
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_MOUSEMOVE)
            {
                var hookStruct = Marshal.PtrToStructure<MOUSEHOOKSTRUCT>(lParam);

                // Convert screen coordinates to adorner layer coordinates
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    var screenPoint = new Point(hookStruct.Position.X, hookStruct.Position.Y);
                    _mouseMoveCallback.Invoke(screenPoint);
                }));
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        public void Dispose()
        {
            UnsetHook();
            GC.SuppressFinalize(this);
        }
    }
}
