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

using Android.Content.PM;
using RestockingSample.Pick;

namespace RestockingSample;

[Activity(Label = "@string/app_name", MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait)]
public class MainActivity : CameraPermissionActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        this.SetContentView(Resource.Layout.activity_main);
        this.Initialize(savedInstanceState);
    }

    protected override void OnResume() 
    {
        base.OnResume();

        // Check for camera permission and request it, if it hasn't yet been granted.
        // Once we have the permission the OnCameraPermissionGranted() method will be called.
        this.RequestCameraPermission();
    }

    protected override void OnCameraPermissionGranted() 
    { }

    private void Initialize(Bundle? savedInstanceState) 
    {
        if (savedInstanceState == null) 
        {
            // Create the MainActivity with the PickFragment visible.
            this.SupportFragmentManager.BeginTransaction()
                .Replace(Resource.Id.container, PickFragment.Create(), nameof(PickFragment))
                .Commit();
        }
    }
}