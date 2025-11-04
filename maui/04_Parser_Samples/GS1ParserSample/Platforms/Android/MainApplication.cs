using Android.App;
using Android.Runtime;
using Scandit.DataCapture.Parser;

namespace GS1ParserSample;

[Application]
public class MainApplication(IntPtr handle, JniHandleOwnership ownership) : MauiApplication(handle, ownership)
{
    public override void OnCreate()
    {
        ScanditParser.Initialize();
        
        base.OnCreate();
    }
    
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}

