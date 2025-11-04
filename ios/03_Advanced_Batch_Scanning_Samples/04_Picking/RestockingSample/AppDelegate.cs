/*
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using RestockingSample.ViewControllers;
using Scandit.DataCapture.Barcode;
using Scandit.DataCapture.Core;

namespace RestockingSample;

[Register("AppDelegate")]
public class AppDelegate : UIApplicationDelegate
{
    public override UIWindow? Window { get; set; }

    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        // Initialize Scandit libraries
        ScanditCaptureCore.Initialize();
        ScanditBarcodeCapture.Initialize();
        
        // Create a new window instance based on the screen size
        Window = new UIWindow(UIScreen.MainScreen.Bounds);

        // Create a UIViewController with a single UILabel
        var vc = new PickViewController();
        var navigation = new UINavigationController(vc);
        navigation.NavigationBar.TitleTextAttributes = new UIStringAttributes
        {
            ForegroundColor = UIColor.White
        };
        Window.RootViewController = navigation;

        // Make the window visible
        Window.MakeKeyAndVisible();

        return true;
    }
}
