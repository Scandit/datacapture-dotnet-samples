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

namespace BarcodeSelectionSimpleSample;

[Register ("AppDelegate")]
public class AppDelegate : UIApplicationDelegate
{
	  public override UIWindow Window { get; set; }

    public override bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
    {
        // create a new window instance based on the screen size
        this.Window = new UIWindow(UIScreen.MainScreen.Bounds);
        this.Window.RootViewController = new ViewController();
        this.Window.MakeKeyAndVisible();

        return true;
    }
}
