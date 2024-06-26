﻿/*
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

using Android.Content;
using Android.OS;

using Scandit.DataCapture.Barcode.Capture;
using Scandit.DataCapture.Barcode.Data;
using Scandit.DataCapture.Barcode.UI.Overlay;
using Scandit.DataCapture.Core.Capture;
using Scandit.DataCapture.Core.Data;
using Scandit.DataCapture.Core.Source;
using Scandit.DataCapture.Core.UI;
using Scandit.DataCapture.Core.UI.Viewfinder;

namespace BarcodeCaptureSimpleSample
{
    [Activity(MainLauncher = true, Label = "@string/app_name")]
    public class BarcodeScanActivity : CameraPermissionActivity, IBarcodeCaptureListener
    {
        // There is a Scandit sample license key set below here.
        // This license key is enabled for sample evaluation only.
        // If you want to build your own application, get your license key by signing up for a trial at https://ssl.scandit.com/dashboard/sign-up?p=test
        public const string SCANDIT_LICENSE_KEY = "AZ707AsCLmJWHbYO4RjqcVAEgAxmNGYcF3Ytg4RiKa/lWTQ3IXkfVZhSSi0yOzuabn9STRdnzTLybIiJVkVZU2QK5jeqbn1HGCGXQ+9lqsN8VUaTw1IeuHJo+9mYVdv3I1DhedtSy89aKA4QugKI5d9ykKaXGohXjlI+PB5ju8Tyc80FPAC3WP9D8oKBcWyemTLQjoUu0Nl3T7mVyFIXMPshQeYddkjMQ1sVV9Jcuf1CbI9riUJWzbNUb4NcB4MoV0BHuyALUPtuM2+cBkX3bPN0AxjD9WC7KflL2UrsZeenvl/aDx2yU4t5vsa2BImNTyEqdVs+rmrGUzRdbYvSUFzKBeiBncLAASqnexTuSzh9KfEm/cKrVlWekP+zOkrilICkn3KVNY6g9RQd8FrsHTBI9OBbMpC79BTwuzHcnlFUG5S3ru/viJ2+f9JEEejxDbdJ7u4JohfBuUYBSEBQ/XzEPMdpqWcmxHGWF4j7jQy83B9Wlgrhd8xNWKjgAViI0bcebjnB7o6yuKacXICH/lo787RhnXSjqjQhJBCbEvwxHQZiEfWPdVKtY7EM+x8HFr6j3orKllKOMJ9asZ5bJYz9aIHlOWeRGm90guQn0KWiPwuKbUOQIMxFAOem2zcSTt4OfqS6Ci0Y6lk7FIrgpbaz8L1PW64kkjrZB6FtQ8OppmsyZ/QTvrHYFQFTH7MpamDviRjEKMyiD2ID6ypl+Meeme6cZYRJVujr6b4tweQCsfNEYhuDiMJaWQ57R0n2XdF0zkepLVc0yA2Q3wWhxSIASLnv6GTCYYVnDJnkrr6VaTv8RVUOp8h8U34wGDanamQ+39+rESMD59E288OKgFvZZWN9Ltu/VQCcjYCYT1RTDcA9co3Y18aGpDxvtLVEGJ8QDPv1E//IYAYEhXqu8r9xbsx/hTwZmLpNKyXGPRr9+hpufTAcAj908f2kuQ==";

        private DataCaptureContext dataCaptureContext;
        private BarcodeCapture barcodeCapture;
        private Camera camera;
        private DataCaptureView dataCaptureView;

        private AlertDialog dialog;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            this.SetContentView(Resource.Layout.activity_main);
            this.SetTitle(Resource.String.app_title);
            this.InitializeAndStartBarcodeScanning();
        }

        protected override void OnPause()
        {
            base.OnPause();

            this.camera?.SwitchToDesiredStateAsync(FrameSourceState.Off);
        }

        protected override void OnResume()
        {
            base.OnResume();

            // Handle permissions for Marshmallow and onwards...
            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.M)
            {
                this.RequestCameraPermission();
            }
            else
            {
                // Once the activity is in the foreground again, restart scanning.
                this.ResumeFrameSource();
            }
        }

        protected override void OnCameraPermissionGranted()
        {
            this.ResumeFrameSource();
        }

        private void ResumeFrameSource()
        {
            this.DismissScannedCodesDialog();

            // Switch camera on to start streaming frames.
            // The camera is started asynchronously and will take some time to completely turn on.
            this.barcodeCapture.Enabled = true;
            this.camera?.SwitchToDesiredStateAsync(FrameSourceState.On);
        }

        private void InitializeAndStartBarcodeScanning()
        {
            // Create data capture context using your license key.
            this.dataCaptureContext = DataCaptureContext.ForLicenseKey(SCANDIT_LICENSE_KEY);

            // Use the default camera and set it as the frame source of the context.
            // The camera is off by default and must be turned on to start streaming frames to the data
            // capture context for recognition.
            // See resumeFrameSource and pauseFrameSource below.
            this.camera = Camera.GetDefaultCamera();
            if (this.camera != null)
            {
                // Use the settings recommended by barcode capture.
                this.camera.ApplySettingsAsync(BarcodeCapture.RecommendedCameraSettings);
                this.dataCaptureContext.SetFrameSourceAsync(this.camera);
            }

            // The barcode capturing process is configured through barcode capture settings
            // which are then applied to the barcode capture instance that manages barcode recognition.
            BarcodeCaptureSettings barcodeCaptureSettings = BarcodeCaptureSettings.Create();

            // The settings instance initially has all types of barcodes (symbologies) disabled.
            // For the purpose of this sample we enable a very generous set of symbologies.
            // In your own app ensure that you only enable the symbologies that your app requires as
            // every additional enabled symbology has an impact on processing times.
            HashSet<Symbology> symbologies = new HashSet<Symbology>();
            symbologies.Add(Symbology.Ean13Upca);
            symbologies.Add(Symbology.Ean8);
            symbologies.Add(Symbology.Upce);
            symbologies.Add(Symbology.Qr);
            symbologies.Add(Symbology.DataMatrix);
            symbologies.Add(Symbology.Code39);
            symbologies.Add(Symbology.Code128);
            symbologies.Add(Symbology.InterleavedTwoOfFive);

            barcodeCaptureSettings.EnableSymbologies(symbologies);

            // Some linear/1d barcode symbologies allow you to encode variable-length data.
            // By default, the Scandit Data Capture SDK only scans barcodes in a certain length range.
            // If your application requires scanning of one of these symbologies, and the length is
            // falling outside the default range, you may need to adjust the "active symbol counts"
            // for this symbology. This is shown in the following few lines of code for one of the
            // variable-length symbologies.
            SymbologySettings symbologySettings =
                    barcodeCaptureSettings.GetSymbologySettings(Symbology.Code39);

            ICollection<short> activeSymbolCounts = new HashSet<short>(
                new short[] { 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 });

            symbologySettings.ActiveSymbolCounts = activeSymbolCounts;

            // Create new barcode capture mode with the settings from above.
            this.barcodeCapture = BarcodeCapture.Create(this.dataCaptureContext, barcodeCaptureSettings);
            // Register self as a listener to get informed whenever a new barcode got recognized.
            barcodeCapture.AddListener(this);

            // To visualize the on-going barcode capturing process on screen, setup a data capture view
            // that renders the camera preview. The view must be connected to the data capture context.
            this.dataCaptureView = DataCaptureView.Create(this, this.dataCaptureContext);

            // Add a barcode capture overlay to the data capture view to render the location of captured
            // barcodes on top of the video preview.
            // This is optional, but recommended for better visual feedback.
            BarcodeCaptureOverlay overlay = BarcodeCaptureOverlay.Create(this.barcodeCapture, this.dataCaptureView, BarcodeCaptureOverlayStyle.Frame);
            overlay.Viewfinder = new RectangularViewfinder(RectangularViewfinderStyle.Square, RectangularViewfinderLineStyle.Light);

            // Add the DataCaptureView to the container.
            var container = FindViewById<FrameLayout>(Resource.Id.data_capture_view_container);
            container?.AddView(dataCaptureView);
        }

        private void DismissScannedCodesDialog()
        {
            if (this.dialog != null)
            {
                this.dialog.Dismiss();
                this.dialog = null;
            }
        }

        public void OnBarcodeScanned(BarcodeCapture barcodeCapture, BarcodeCaptureSession session, IFrameData frameData)
        {
            if (!session.NewlyRecognizedBarcodes.Any())
            {
                return;
            }

            Barcode barcode = session.NewlyRecognizedBarcodes[0];

            // Stop recognizing barcodes for as long as we are displaying the result. There won't be any new results until
            // the capture mode is enabled again. Note that disabling the capture mode does not stop the camera, the camera
            // continues to stream frames until it is turned off.
            barcodeCapture.Enabled = false;

            // If you are not disabling barcode capture here and want to continue scanning, consider
            // setting the codeDuplicateFilter when creating the barcode capture settings to around 500
            // or even -1 if you do not want codes to be scanned more than once.

            // Get the human readable name of the symbology and assemble the result to be shown.
            SymbologyDescription description = new SymbologyDescription(barcode.Symbology);

            string result = "Scanned: " + barcode.Data + " (" + description.ReadableName + ")";
            RunOnUiThread(() => ShowResults(result));
        }

        public void OnObservationStarted(BarcodeCapture barcodeCapture)
        {
        }

        public void OnObservationStopped(BarcodeCapture barcodeCapture)
        {
        }

        public void OnSessionUpdated(BarcodeCapture barcodeCapture, BarcodeCaptureSession session, IFrameData frameData)
        {
        }

        private void ShowResults(string result)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            this.dialog = builder.SetCancelable(false)
                                 .SetTitle(result)
                                 .SetPositiveButton(Android.Resource.String.Ok, (Object sender, DialogClickEventArgs args) =>
                                 {
                                     this.barcodeCapture.Enabled = true;
                                 })
                                 .Create();
            this.dialog.Show();
        }
    }
}
