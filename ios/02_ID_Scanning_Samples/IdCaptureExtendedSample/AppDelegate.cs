using Scandit.DataCapture.Core;
using Scandit.DataCapture.ID;

namespace IdCaptureExtendedSample;

[Register("AppDelegate")]
public class AppDelegate : UIResponder, IUIApplicationDelegate
{
    [Export("window")]
    public UIWindow Window { get; set; }

    [Export("application:didFinishLaunchingWithOptions:")]
    public bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        // Initialize Scandit libraries
        ScanditCaptureCore.Initialize();
        ScanditIdCapture.Initialize();
        
        Window.RootViewController = new UINavigationController(Window.RootViewController);
        return true;
    }
}
