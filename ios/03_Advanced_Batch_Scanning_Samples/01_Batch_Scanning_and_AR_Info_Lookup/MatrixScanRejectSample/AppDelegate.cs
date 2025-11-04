using Scandit.DataCapture.Barcode;
using Scandit.DataCapture.Core;

namespace MatrixScanRejectSample;

[Register("AppDelegate")]
public class AppDelegate : UIResponder, IUIApplicationDelegate
{
    [Export("window")]
    public UIWindow? Window { get; set; }

    [Export("application:didFinishLaunchingWithOptions:")]
    public bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        // Initialize Scandit libraries
        ScanditCaptureCore.Initialize();
        ScanditBarcodeCapture.Initialize();
        
        return true;
    }
}
