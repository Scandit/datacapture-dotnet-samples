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

using Android;
using Android.Annotation;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using AndroidX.Activity.Result;
using AndroidX.Activity.Result.Contract;
using AndroidX.Core.Content;
using Fragment = AndroidX.Fragment.App.Fragment;

namespace SearchAndFindSample.Search;

public abstract class CameraPermissionFragment : Fragment, IActivityResultCallback
{
    private const string CameraPermission = Manifest.Permission.Camera;

    // The launcher to request the user permission to use their device's camera.
    private ActivityResultLauncher requestCameraPermission;

    protected CameraPermissionFragment()
    {
        this.requestCameraPermission =
            this.RegisterForActivityResult(new ActivityResultContracts.RequestPermission(), this);
    }

    protected void RequestCameraPermission()
    {
        // Check for camera permission and request it, if it hasn't yet been granted.
        // Once we have the permission start the capture process.
        if (ContextCompat.CheckSelfPermission(this.RequireContext(), CameraPermission) == Permission.Granted)
        {
            this.OnCameraPermissionGranted();
        }
        else
        {
            this.requestCameraPermission.Launch(CameraPermission);
        }
    }

    public void OnActivityResult(Java.Lang.Object? result)
    {
        if (this.IsResumed &&
            result != null &&
            result is Java.Lang.Boolean value &&
            value.BooleanValue() == true)
        {
            this.OnCameraPermissionGranted();
        }
    }

    protected abstract void OnCameraPermissionGranted();
}
