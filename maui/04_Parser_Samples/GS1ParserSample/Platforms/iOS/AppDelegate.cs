using Foundation;
using Scandit.DataCapture.Parser;
using UIKit;

namespace GS1ParserSample;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    public override bool FinishedLaunching(UIApplication app, NSDictionary options)
    {
        ScanditParser.Initialize();
        
        return base.FinishedLaunching(app, options);
    }
    
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}

