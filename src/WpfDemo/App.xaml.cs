using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace WpfDemo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            DisableWPFTabletSupport();
            
            var mw = new MainWindow();
            mw.Show();
        }
        
        public static void DisableWPFTabletSupport()
        {
            // Get a collection of the tablet devices for this window.    
            TabletDeviceCollection devices = System.Windows.Input.Tablet.TabletDevices;

            if (devices.Count > 0)
            {
                // Get the Type of InputManager.  
                Type inputManagerType = typeof(System.Windows.Input.InputManager);


                // Call the StylusLogic method on the InputManager.Current instance.  
                object stylusLogic = inputManagerType.InvokeMember("StylusLogic",
                    BindingFlags.GetProperty | BindingFlags.Instance | 
                    BindingFlags.NonPublic,
                    null, InputManager.Current, null);


                if (stylusLogic != null)
                {
                    // Get the type of the stylusLogic returned 
                    // from the call to StylusLogic.  
                    Type stylusLogicType = stylusLogic.GetType();


                    // Loop until there are no more devices to remove.  
                    while (devices.Count > 0)
                    {
                        // Remove the first tablet device in the devices collection.  
                        stylusLogicType.InvokeMember("OnTabletRemoved",
                            BindingFlags.InvokeMethod | 
                            BindingFlags.Instance | BindingFlags.NonPublic,
                            null, stylusLogic, new object[] { (uint)0 });
                    }
                }
            }
        }
    }
}