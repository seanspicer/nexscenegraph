using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace Veldrid.SceneGraph.Wpf.Controls
{
    /// <summary>
    /// A control that enables graphics rendering inside a WPF control through
    /// the use of a hosted child Hwnd.
    /// </summary>
    public abstract class HwndWrapper : HwndHost
    {
        #region Fields

        // The name of our window class
        private const string WindowClass = "GraphicsDeviceControlHostWindowClass";

        // The HWND we present to when rendering
        private IntPtr _hWnd;

        // For holding previous hWnd focus
        private IntPtr _hWndPrev;

        // Track if the application has focus
        private bool _applicationHasFocus;

        // Track if the mouse is in the window
        private bool _mouseInWindow;

        // Track the previous mouse position
        private System.Windows.Point _previousPosition;

        // Track the mouse state
        private readonly HwndMouseState _mouseState = new HwndMouseState();

        // Tracking whether we've "capture" the mouse
        private bool _isMouseCaptured;

        #endregion

        #region Events

        /// <summary>
        /// Invoked when the control receives a left mouse down message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndLButtonDown;

        /// <summary>
        /// Invoked when the control receives a left mouse up message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndLButtonUp;

        /// <summary>
        /// Invoked when the control receives a left mouse double click message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndLButtonDblClick;

        /// <summary>
        /// Invoked when the control receives a right mouse down message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndRButtonDown;

        /// <summary>
        /// Invoked when the control receives a right mouse up message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndRButtonUp;

        /// <summary>
        /// Invoked when the control receives a rigt mouse double click message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndRButtonDblClick;

        /// <summary>
        /// Invoked when the control receives a middle mouse down message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndMButtonDown;

        /// <summary>
        /// Invoked when the control receives a middle mouse up message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndMButtonUp;

        /// <summary>
        /// Invoked when the control receives a middle mouse double click message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndMButtonDblClick;

        /// <summary>
        /// Invoked when the control receives a mouse down message for the first extra button.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndX1ButtonDown;

        /// <summary>
        /// Invoked when the control receives a mouse up message for the first extra button.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndX1ButtonUp;

        /// <summary>
        /// Invoked when the control receives a double click message for the first extra mouse button.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndX1ButtonDblClick;

        /// <summary>
        /// Invoked when the control receives a mouse down message for the second extra button.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndX2ButtonDown;

        /// <summary>
        /// Invoked when the control receives a mouse up message for the second extra button.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndX2ButtonUp;

        /// <summary>
        /// Invoked when the control receives a double click message for the first extra mouse button.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndX2ButtonDblClick;

        /// <summary>
        /// Invoked when the control receives a mouse move message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndMouseMove;

        /// <summary>
        /// Invoked when the control first gets a mouse move message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndMouseEnter;

        /// <summary>
        /// Invoked when the control gets a mouse leave message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndMouseLeave;

        /// <summary>
        /// Invoked when the control recieves a mouse wheel delta.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndMouseWheel;

        #endregion

        #region Properties

        public new bool IsMouseCaptured
        {
            get { return _isMouseCaptured; }
        }

        #endregion

        #region Construction and Disposal

        protected IntPtr Hwnd
        {
            get => _hWnd;
        }
        protected bool HwndInitialized { get; private set; }
        
        protected HwndWrapper()
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            
            // We must be notified of the application foreground status for our mouse input events
            Application.Current.Activated += OnApplicationActivated;
            Application.Current.Deactivated += OnApplicationDeactivated;

            // We use the CompositionTarget.Rendering event to trigger the control to draw itself
            CompositionTarget.Rendering += OnCompositionTargetRendering;

            // Check whether the application is active (it almost certainly is, at this point).
            if (Application.Current.Windows.Cast<Window>().Any(x => x.IsActive))
                _applicationHasFocus = true;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Initialize();
            HwndInitialized = true;

            Loaded -= OnLoaded;
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Uninitialize();
            HwndInitialized = false;

            Unloaded -= OnUnloaded;

            Dispose();
        }
        
        protected abstract void Initialize();
        protected abstract void Uninitialize();
        protected abstract void Resized();
        
        protected override void Dispose(bool disposing)
        {
            // Unhook all events.
            CompositionTarget.Rendering -= OnCompositionTargetRendering;
            if (Application.Current != null)
            {
                Application.Current.Activated -= OnApplicationActivated;
                Application.Current.Deactivated -= OnApplicationDeactivated;
            }

            base.Dispose(disposing);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Captures the mouse, hiding it and trapping it inside the window bounds.
        /// </summary>
        /// <remarks>
        /// This method is useful for tooling scenarios where you only care about the mouse deltas
        /// and want the user to be able to continue interacting with the window while they move
        /// the mouse. A good example of this is rotating an object based on the mouse deltas where
        /// through capturing you can spin and spin without having the cursor leave the window.
        /// </remarks>
        public new void CaptureMouse()
        {
            // Don't do anything if the mouse is already captured
            if (_isMouseCaptured)
                return;

            Win32.NativeMethods.SetCapture(_hWnd);
            _isMouseCaptured = true;
        }

        /// <summary>
        /// Releases the capture of the mouse which makes it visible and allows it to leave the window bounds.
        /// </summary>
        public new void ReleaseMouseCapture()
        {
            // Don't do anything if the mouse is not captured
            if (!_isMouseCaptured)
                return;

            Win32.NativeMethods.ReleaseCapture();
            _isMouseCaptured = false;
        }

        #endregion

        #region Graphics Device Control Implementation

        private void OnCompositionTargetRendering(object sender, EventArgs e)
        {
            // Get the current width and height of the control
            var width = (int) ActualWidth;
            var height = (int) ActualHeight;

            // If the control has no width or no height, skip drawing since it's not visible
            if (width < 1 || height < 1)
                return;

            Render(_hWnd);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            UpdateWindowPos();

            base.OnRenderSizeChanged(sizeInfo);

            if (HwndInitialized)
                Resized();
        }
        
        protected abstract void Render(IntPtr windowHandle);

        private void OnApplicationActivated(object sender, EventArgs e)
        {
            _applicationHasFocus = true;
        }

        private void OnApplicationDeactivated(object sender, EventArgs e)
        {
            _applicationHasFocus = false;
            ResetMouseState();

            if (_mouseInWindow)
            {
                _mouseInWindow = false;
                RaiseHwndMouseLeave(new HwndMouseEventArgs(_mouseState));
            }

            ReleaseMouseCapture();
        }

        private void ResetMouseState()
        {
            // We need to invoke events for any buttons that were pressed
            bool fireL = _mouseState.LeftButton == MouseButtonState.Pressed;
            bool fireM = _mouseState.MiddleButton == MouseButtonState.Pressed;
            bool fireR = _mouseState.RightButton == MouseButtonState.Pressed;
            bool fireX1 = _mouseState.X1Button == MouseButtonState.Pressed;
            bool fireX2 = _mouseState.X2Button == MouseButtonState.Pressed;

            // Update the state of all of the buttons
            _mouseState.LeftButton = MouseButtonState.Released;
            _mouseState.MiddleButton = MouseButtonState.Released;
            _mouseState.RightButton = MouseButtonState.Released;
            _mouseState.X1Button = MouseButtonState.Released;
            _mouseState.X2Button = MouseButtonState.Released;

            // Fire any events
            var args = new HwndMouseEventArgs(_mouseState);
            if (fireL)
                RaiseHwndLButtonUp(args);
            if (fireM)
                RaiseHwndMButtonUp(args);
            if (fireR)
                RaiseHwndRButtonUp(args);
            if (fireX1)
                RaiseHwndX1ButtonUp(args);
            if (fireX2)
                RaiseHwndX2ButtonUp(args);

            // The mouse is no longer considered to be in our window
            _mouseInWindow = false;
        }

        #endregion

        #region HWND Management

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            // Create the host window as a child of the parent
            _hWnd = CreateHostWindow(hwndParent.Handle);
            return new HandleRef(this, _hWnd);
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            // Destroy the window and reset our hWnd value
            Win32.NativeMethods.DestroyWindow(hwnd.Handle);
            _hWnd = IntPtr.Zero;
        }

        /// <summary>
        /// Creates the host window as a child of the parent window.
        /// </summary>
        private IntPtr CreateHostWindow(IntPtr hWndParent)
        {
            // Register our window class
            RegisterWindowClass();

            // Create the window
            return Win32.NativeMethods.CreateWindowEx(0, WindowClass, "",
               Win32.NativeMethods.WS_CHILD | Win32.NativeMethods.WS_VISIBLE,
               0, 0, (int) Width, (int) Height, hWndParent, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        }

        /// <summary>
        /// Registers the window class.
        /// </summary>
        private void RegisterWindowClass()
        {
            var wndClass = new Win32.NativeMethods.WNDCLASSEX();
            wndClass.cbSize = (uint) Marshal.SizeOf(wndClass);
            wndClass.hInstance = Win32.NativeMethods.GetModuleHandle(null);
            wndClass.lpfnWndProc = Win32.NativeMethods.DefaultWindowProc;
            wndClass.lpszClassName = WindowClass;
            wndClass.hCursor = Win32.NativeMethods.LoadCursor(IntPtr.Zero, Win32.NativeMethods.IDC_ARROW);

            Win32.NativeMethods.RegisterClassEx(ref wndClass);
        }

        #endregion

        #region WndProc Implementation

        protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case Win32.NativeMethods.WM_MOUSEWHEEL:
                    if (_mouseInWindow)
                    {
                        var param = wParam.ToInt64();
                        int delta = Win32.NativeMethods.GetWheelDeltaWParam((int)param) / 10;
                        RaiseHwndMouseWheel(new HwndMouseEventArgs(_mouseState, delta, 0));
                    }
                    break;
                case Win32.NativeMethods.WM_LBUTTONDOWN:
                    _mouseState.LeftButton = MouseButtonState.Pressed;
                    RaiseHwndLButtonDown(new HwndMouseEventArgs(_mouseState));
                    break;
                case Win32.NativeMethods.WM_LBUTTONUP:
                    _mouseState.LeftButton = MouseButtonState.Released;
                    RaiseHwndLButtonUp(new HwndMouseEventArgs(_mouseState));
                    break;
                case Win32.NativeMethods.WM_LBUTTONDBLCLK:
                    RaiseHwndLButtonDblClick(new HwndMouseEventArgs(_mouseState, System.Windows.Input.MouseButton.Left));
                    break;
                case Win32.NativeMethods.WM_RBUTTONDOWN:
                    _mouseState.RightButton = MouseButtonState.Pressed;
                    RaiseHwndRButtonDown(new HwndMouseEventArgs(_mouseState));
                    break;
                case Win32.NativeMethods.WM_RBUTTONUP:
                    _mouseState.RightButton = MouseButtonState.Released;
                    RaiseHwndRButtonUp(new HwndMouseEventArgs(_mouseState));
                    break;
                case Win32.NativeMethods.WM_RBUTTONDBLCLK:
                    RaiseHwndRButtonDblClick(new HwndMouseEventArgs(_mouseState, System.Windows.Input.MouseButton.Right));
                    break;
                case Win32.NativeMethods.WM_MBUTTONDOWN:
                    _mouseState.MiddleButton = MouseButtonState.Pressed;
                    RaiseHwndMButtonDown(new HwndMouseEventArgs(_mouseState));
                    break;
                case Win32.NativeMethods.WM_MBUTTONUP:
                    _mouseState.MiddleButton = MouseButtonState.Released;
                    RaiseHwndMButtonUp(new HwndMouseEventArgs(_mouseState));
                    break;
                case Win32.NativeMethods.WM_MBUTTONDBLCLK:
                    RaiseHwndMButtonDblClick(new HwndMouseEventArgs(_mouseState, System.Windows.Input.MouseButton.Middle));
                    break;
                case Win32.NativeMethods.WM_XBUTTONDOWN:
                    if (((int) wParam & Win32.NativeMethods.MK_XBUTTON1) != 0)
                    {
                        _mouseState.X1Button = MouseButtonState.Pressed;
                        RaiseHwndX1ButtonDown(new HwndMouseEventArgs(_mouseState));
                    }
                    else if (((int) wParam & Win32.NativeMethods.MK_XBUTTON2) != 0)
                    {
                        _mouseState.X2Button = MouseButtonState.Pressed;
                        RaiseHwndX2ButtonDown(new HwndMouseEventArgs(_mouseState));
                    }
                    break;
                case Win32.NativeMethods.WM_XBUTTONUP:
                    if (((int) wParam & Win32.NativeMethods.MK_XBUTTON1) != 0)
                    {
                        _mouseState.X1Button = MouseButtonState.Released;
                        RaiseHwndX1ButtonUp(new HwndMouseEventArgs(_mouseState));
                    }
                    else if (((int) wParam & Win32.NativeMethods.MK_XBUTTON2) != 0)
                    {
                        _mouseState.X2Button = MouseButtonState.Released;
                        RaiseHwndX2ButtonUp(new HwndMouseEventArgs(_mouseState));
                    }
                    break;
                case Win32.NativeMethods.WM_XBUTTONDBLCLK:
                    if (((int) wParam & Win32.NativeMethods.MK_XBUTTON1) != 0)
                        RaiseHwndX1ButtonDblClick(new HwndMouseEventArgs(_mouseState, System.Windows.Input.MouseButton.XButton1));
                    else if (((int) wParam & Win32.NativeMethods.MK_XBUTTON2) != 0)
                        RaiseHwndX2ButtonDblClick(new HwndMouseEventArgs(_mouseState, System.Windows.Input.MouseButton.XButton2));
                    break;
                case Win32.NativeMethods.WM_MOUSEMOVE:
                    // If the application isn't in focus, we don't handle this message
                    if (!_applicationHasFocus)
                        break;

                    // record the prevous and new position of the mouse
                    _mouseState.ScreenPosition = PointToScreen(new System.Windows.Point(
                        Win32.NativeMethods.GetXLParam((int) lParam),
                        Win32.NativeMethods.GetYLParam((int) lParam)));

                    if (!_mouseInWindow)
                    {
                        _mouseInWindow = true;

                        RaiseHwndMouseEnter(new HwndMouseEventArgs(_mouseState));

                        // Track the previously focused window, and set focus to this window.
                        _hWndPrev = Win32.NativeMethods.GetFocus();
                        Win32.NativeMethods.SetFocus(_hWnd);

                        // send the track mouse event so that we get the WM_MOUSELEAVE message
                        var tme = new Win32.NativeMethods.TRACKMOUSEEVENT
                        {
                            cbSize = Marshal.SizeOf(typeof (Win32.NativeMethods.TRACKMOUSEEVENT)),
                            dwFlags = Win32.NativeMethods.TME_LEAVE,
                            hWnd = hwnd
                        };
                        Win32.NativeMethods.TrackMouseEvent(ref tme);
                    }

                    if (_mouseState.ScreenPosition != _previousPosition)
                        RaiseHwndMouseMove(new HwndMouseEventArgs(_mouseState));

                    _previousPosition = _mouseState.ScreenPosition;

                    break;
                case Win32.NativeMethods.WM_MOUSELEAVE:

                    // If we have capture, we ignore this message because we're just
                    // going to reset the cursor position back into the window
                    if (_isMouseCaptured)
                        break;

                    // Reset the state which releases all buttons and 
                    // marks the mouse as not being in the window.
                    ResetMouseState();

                    RaiseHwndMouseLeave(new HwndMouseEventArgs(_mouseState));

                    Win32.NativeMethods.SetFocus(_hWndPrev);

                    break;
            }

            return base.WndProc(hwnd, msg, wParam, lParam, ref handled);
        }

        protected virtual void RaiseHwndLButtonDown(HwndMouseEventArgs args)
        {
            var handler = HwndLButtonDown;
            if (handler != null)
                handler(this, args);
        }

        protected virtual void RaiseHwndLButtonUp(HwndMouseEventArgs args)
        {
            var handler = HwndLButtonUp;
            if (handler != null)
                handler(this, args);
        }

        protected virtual void RaiseHwndRButtonDown(HwndMouseEventArgs args)
        {
            var handler = HwndRButtonDown;
            if (handler != null)
                handler(this, args);
        }

        protected virtual void RaiseHwndRButtonUp(HwndMouseEventArgs args)
        {
            var handler = HwndRButtonUp;
            if (handler != null)
                handler(this, args);
        }

        protected virtual void RaiseHwndMButtonDown(HwndMouseEventArgs args)
        {
            var handler = HwndMButtonDown;
            if (handler != null)
                handler(this, args);
        }

        protected virtual void RaiseHwndMButtonUp(HwndMouseEventArgs args)
        {
            var handler = HwndMButtonUp;
            if (handler != null)
                handler(this, args);
        }

        protected virtual void RaiseHwndLButtonDblClick(HwndMouseEventArgs args)
        {
            var handler = HwndLButtonDblClick;
            if (handler != null)
                handler(this, args);
        }

        protected virtual void RaiseHwndRButtonDblClick(HwndMouseEventArgs args)
        {
            var handler = HwndRButtonDblClick;
            if (handler != null)
                handler(this, args);
        }

        protected virtual void RaiseHwndMButtonDblClick(HwndMouseEventArgs args)
        {
            var handler = HwndMButtonDblClick;
            if (handler != null)
                handler(this, args);
        }

        protected virtual void RaiseHwndMouseEnter(HwndMouseEventArgs args)
        {
            var handler = HwndMouseEnter;
            if (handler != null)
                handler(this, args);
        }

        protected virtual void RaiseHwndX1ButtonDown(HwndMouseEventArgs args)
        {
            var handler = HwndX1ButtonDown;
            if (handler != null)
                handler(this, args);
        }

        protected virtual void RaiseHwndX1ButtonUp(HwndMouseEventArgs args)
        {
            var handler = HwndX1ButtonUp;
            if (handler != null)
                handler(this, args);
        }

        protected virtual void RaiseHwndX2ButtonDown(HwndMouseEventArgs args)
        {
            var handler = HwndX2ButtonDown;
            if (handler != null)
                handler(this, args);
        }

        protected virtual void RaiseHwndX2ButtonUp(HwndMouseEventArgs args)
        {
            var handler = HwndX2ButtonUp;
            if (handler != null)
                handler(this, args);
        }

        protected virtual void RaiseHwndX1ButtonDblClick(HwndMouseEventArgs args)
        {
            var handler = HwndX1ButtonDblClick;
            if (handler != null)
                handler(this, args);
        }

        protected virtual void RaiseHwndX2ButtonDblClick(HwndMouseEventArgs args)
        {
            var handler = HwndX2ButtonDblClick;
            if (handler != null)
                handler(this, args);
        }

        protected virtual void RaiseHwndMouseLeave(HwndMouseEventArgs args)
        {
            var handler = HwndMouseLeave;
            if (handler != null)
                handler(this, args);
        }

        protected virtual void RaiseHwndMouseMove(HwndMouseEventArgs args)
        {
            var handler = HwndMouseMove;
            if (handler != null)
                handler(this, args);
        }

        protected virtual void RaiseHwndMouseWheel(HwndMouseEventArgs args)
        {
            var handler = HwndMouseWheel;
            if (handler != null)
                handler(this, args);
        }

        #endregion
    }
}