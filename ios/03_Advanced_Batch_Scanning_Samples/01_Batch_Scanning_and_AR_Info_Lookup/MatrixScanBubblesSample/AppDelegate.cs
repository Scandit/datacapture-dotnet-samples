namespace MatrixScanBubblesSample;

[Register("AppDelegate")]
public class AppDelegate : UIResponder, IUIApplicationDelegate
{
    [Export("window")]
    public UIWindow? Window { get; set; }

    [Export("application:didFinishLaunchingWithOptions:")]
    public bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        return true;
    }
}
