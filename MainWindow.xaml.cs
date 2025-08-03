using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Threading;

namespace PieOverlay
{
    public partial class MainWindow : Window
    {
        // Keeps the 8 labels; defaulted here but will be overridden in Settings
        public string[] ItemNames { get; } =
            { "Item1","Item2","Item3","Item4","Item5","Item6","Item7","Item8" };

        // Hotkeys for each item; editable via settings
        public string[] ItemHotkeys { get; } =
            { "Ctrl+A","Ctrl+N","Delete","Esc","","","","" };

        // Radius of the pie menu; default 100 but configurable in settings
        public double Radius { get; set; } = 100;

        // Low-level hook constants
        private const int WH_KEYBOARD_LL   = 13;
        private const int WM_KEYDOWN       = 0x0100;
        private const int WM_KEYUP         = 0x0101;
        private const int WM_SYSKEYDOWN    = 0x0104;
        private const int WM_SYSKEYUP      = 0x0105;
        private const int VK_1             = 0x31;
        private const int VK_F12           = 0x7B;

        private static IntPtr _hookID = IntPtr.Zero;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private bool _visible;

        public MainWindow()
        {
            InitializeComponent();

            // install the global low-level keyboard hook
            _hookID = SetHook(_proc);

            Topmost    = true;
            Visibility = Visibility.Hidden;
        }

        // Open the settings dialog
        public void OpenSettings()
        {
            var dlg = new SettingsWindow(this);
            dlg.Owner = this;
            dlg.ShowDialog();
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using var curProcess = Process.GetCurrentProcess();
            using var curModule  = curProcess.MainModule!;
            return SetWindowsHookEx(
                WH_KEYBOARD_LL,
                proc,
                GetModuleHandle(curModule.ModuleName!),
                0
            );
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                bool down  = wParam == (IntPtr)WM_KEYDOWN  || wParam == (IntPtr)WM_SYSKEYDOWN;
                bool up    = wParam == (IntPtr)WM_KEYUP    || wParam == (IntPtr)WM_SYSKEYUP;
                int vkCode = Marshal.ReadInt32(lParam);

                // dispatch back to UI thread
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var wnd = (MainWindow)Application.Current.MainWindow!;
                    // F12 opens settings
                    if (vkCode == VK_F12 && down)
                    {
                        wnd.OpenSettings();
                    }
                    // "1" down shows pie
                    else if (vkCode == VK_1 && down && !wnd._visible)
                    {
                        wnd.DrawEightRects();
                        wnd.Visibility = Visibility.Visible;
                        wnd._visible   = true;
                    }
                    // "1" up selects hovered slice & hides
                    else if (vkCode == VK_1 && up && wnd._visible)
                    {
                        // hit‐test; VisualHit might be the TextBlock inside the Border,
                        // so walk up the tree until we find the enclosing Border element
                        Point rel = Mouse.GetPosition(wnd.MainCanvas);
                        var hit   = VisualTreeHelper.HitTest(wnd.MainCanvas, rel);

                        Border? border = null;
                        DependencyObject? cur = hit?.VisualHit;
                        while (cur != null && border == null)
                        {
                            if (cur is Border b)
                                border = b;
                            else
                                cur = VisualTreeHelper.GetParent(cur);
                        }

                        if (border != null)
                        {
                            int idx = wnd.MainCanvas.Children.IndexOf(border);
                            if (idx >= 0 && idx < wnd.ItemHotkeys.Length)
                                SendHotkey(wnd.ItemHotkeys[idx]);
                        }

                        wnd.MainCanvas.Children.Clear();
                        wnd.Visibility = Visibility.Hidden;
                        wnd._visible   = false;
                    }
                }, DispatcherPriority.Send);

                // swallow the key so other apps don't see it
                if (vkCode == VK_1 || vkCode == VK_F12)
                    return (IntPtr)1;
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private static void SendHotkey(string hotkey)
        {
            if (string.IsNullOrWhiteSpace(hotkey))
                return;

            const byte VK_CONTROL = 0x11;
            const byte VK_SHIFT   = 0x10;
            const byte VK_MENU    = 0x12; // Alt
            const uint KEYDOWN = 0x0000, KEYUP = 0x0002;

            try
            {
                var converter = new KeyGestureConverter();
                if (converter.ConvertFromString(hotkey) is not KeyGesture gesture)
                    return;

                var modifiers = new List<byte>();
                if (gesture.Modifiers.HasFlag(ModifierKeys.Control)) modifiers.Add(VK_CONTROL);
                if (gesture.Modifiers.HasFlag(ModifierKeys.Shift))   modifiers.Add(VK_SHIFT);
                if (gesture.Modifiers.HasFlag(ModifierKeys.Alt))     modifiers.Add(VK_MENU);

                byte mainKey = (byte)KeyInterop.VirtualKeyFromKey(gesture.Key);

                foreach (var m in modifiers)
                    keybd_event(m, 0, KEYDOWN, UIntPtr.Zero);

                keybd_event(mainKey, 0, KEYDOWN, UIntPtr.Zero);
                keybd_event(mainKey, 0, KEYUP,   UIntPtr.Zero);

                for (int i = modifiers.Count - 1; i >= 0; i--)
                    keybd_event(modifiers[i], 0, KEYUP, UIntPtr.Zero);
            }
            catch (FormatException)
            {
                // ignore invalid hotkey strings
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            UnhookWindowsHookEx(_hookID);
            base.OnClosed(e);
        }

        #region DrawEightRects (use ItemNames[])
        private void DrawEightRects()
        {
            GetCursorPos(out POINT p);
            var pt = new Point(p.X, p.Y);
            if (PresentationSource.FromVisual(this) is { CompositionTarget: var ct })
                pt = ct.TransformFromDevice.Transform(pt);

            const int count = 8;
            double radius = Radius, w = 80, h = 30;
            double cw = radius * 2 + w, ch = radius * 2 + h;
            Width  = cw;  Height = ch;
            MainCanvas.Width  = cw;
            MainCanvas.Height = ch;
            Left = pt.X - cw/2;  Top = pt.Y - ch/2;

            var shadow = new DropShadowEffect {
                BlurRadius  = 8,
                ShadowDepth = 2,
                Opacity     = 0.4,
                Color       = Colors.Black
            };

            for (int i = 0; i < count; i++)
            {
                double θ = 2*Math.PI*i/count;
                double x = cw/2 + radius*Math.Cos(θ) - w/2;
                double y = ch/2 + radius*Math.Sin(θ) - h/2;

                var border = new Border {
                    Width        = w,
                    Height       = h,
                    CornerRadius = new CornerRadius(6),
                    Background   = Brushes.LightBlue,
                    Effect       = shadow
                };

                var label = new TextBlock {
                    Text                = ItemNames[i],
                    FontSize            = 14,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment   = VerticalAlignment.Center,
                    Foreground          = Brushes.DarkBlue
                };

                border.Child = label;
                Canvas.SetLeft(border, x);
                Canvas.SetTop(border, y);
                MainCanvas.Children.Add(border);
            }
        }
        #endregion

        #region Win32 + Hook P/Invoke
        [DllImport("user32.dll")] private static extern bool GetCursorPos(out POINT lpPoint);
        [StructLayout(LayoutKind.Sequential)] private struct POINT { public int X, Y; }
        [DllImport("user32.dll", SetLastError=true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll", SetLastError=true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll")] private static extern IntPtr CallNextHookEx(IntPtr hhk,int nCode,IntPtr wParam,IntPtr lParam);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto,SetLastError=true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        [DllImport("user32.dll",SetLastError=true)]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
        #endregion
    }
}
