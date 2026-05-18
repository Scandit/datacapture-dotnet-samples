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

using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using AndroidX.Activity;
using AndroidX.Core.View;

namespace LabelCaptureSimpleSample;

[Activity(Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTop,
    ScreenOrientation = ScreenOrientation.Portrait,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        EdgeToEdge.Enable(this);

        var rootView = FindViewById(Android.Resource.Id.Content);
        ViewCompat.SetOnApplyWindowInsetsListener(rootView, new SystemBarsInsetsListener());
    }

    private sealed class SystemBarsInsetsListener : Java.Lang.Object, IOnApplyWindowInsetsListener
    {
        public WindowInsetsCompat? OnApplyWindowInsets(Android.Views.View? v, WindowInsetsCompat? insets)
        {
            if (v == null || insets == null)
            {
                return insets;
            }

            var bottomInsets = insets.GetInsets(WindowInsetsCompat.Type.NavigationBars() | WindowInsetsCompat.Type.Ime());
            if (bottomInsets != null)
            {
                v.SetPadding(
                    left: 0,
                    top: 0,
                    right: 0,
                    bottom: bottomInsets.Bottom
                );
            }
            return insets;
        }
    }
}
