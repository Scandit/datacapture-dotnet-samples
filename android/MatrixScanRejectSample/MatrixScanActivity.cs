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
using Android.Graphics;
using Android.Runtime;
using MatrixScanRejectSample.Data;
using Scandit.DataCapture.Barcode.Data;
using Scandit.DataCapture.Barcode.Batch.Capture;
using Scandit.DataCapture.Barcode.Batch.Data;
using Scandit.DataCapture.Barcode.Batch.UI.Overlay;
using Scandit.DataCapture.Core.Capture;
using Scandit.DataCapture.Core.Data;
using Scandit.DataCapture.Core.Source;
using Scandit.DataCapture.Core.UI;
using Scandit.DataCapture.Core.UI.Style;

using Camera = Scandit.DataCapture.Core.Source.Camera;

namespace MatrixScanRejectSample
{
    [Activity(MainLauncher = true)]
    public class MatrixScanActivity : CameraPermissionActivity, IBarcodeBatchListener, IBarcodeBatchBasicOverlayListener
    {
        // Enter your Scandit License key here.
        // Your Scandit License key is available via your Scandit SDK web account.
        public const string SCANDIT_LICENSE_KEY = "-- ENTER YOUR SCANDIT LICENSE KEY HERE --";

        public const int REQUEST_CODE_SCAN_RESULTS = 1;

        private Camera camera;
        private BarcodeBatch barcodeBatch;
        private DataCaptureContext dataCaptureContext;

        private readonly HashSet<ScanResult> scanResults = new HashSet<ScanResult>();

        private Brush defaultBrush;
        private Brush rejectedBrush;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            this.SetContentView(Resource.Layout.activity_matrix_scan);
            this.SetTitle(Resource.String.app_title);
            this.Initialize();
            this.FindViewById<Button>(Resource.Id.done_button).Click += (sender, e) =>
            {
                var intent = ResultsActivity.GetIntent(
                            this, scanResults);
                StartActivityForResult(intent, REQUEST_CODE_SCAN_RESULTS);
            };
        }

        private void Initialize()
        {
            // Create data capture context using your license key.
            dataCaptureContext = DataCaptureContext.ForLicenseKey(SCANDIT_LICENSE_KEY);

            // Use the recommended camera settings for the BarcodeBatch mode.
            var cameraSettings = BarcodeBatch.RecommendedCameraSettings;
            // Adjust camera settings - set Full HD resolution.
            cameraSettings.PreferredResolution = VideoResolution.FullHd;
            // Use the default camera and set it as the frame source of the context.
            // The camera is off by default and must be turned on to start streaming frames to the data
            // capture context for recognition.
            // See resumeFrameSource and pauseFrameSource below.
            camera = Camera.GetDefaultCamera(cameraSettings);

            if (camera == null)
            {
                throw new NullReferenceException(
                   "Sample depends on a camera, which failed to initialize.");
            }

            dataCaptureContext.SetFrameSourceAsync(camera);

            // The barcode batch process is configured through barcode batch settings
            // which are then applied to the barcode batch instance that manages barcode batch.
            var barcodeBatchSettings = BarcodeBatchSettings.Create();

            // The settings instance initially has all types of barcodes (symbologies) disabled.
            // For the purpose of this sample we enable a very generous set of symbologies.
            // In your own app ensure that you only enable the symbologies that your app requires
            // as every additional enabled symbology has an impact on processing times.
            var symbologies = new HashSet<Symbology>
            {
                Symbology.Ean13Upca,
                Symbology.Ean8,
                Symbology.Upce,
                Symbology.Code39,
                Symbology.Code128
            };

            barcodeBatchSettings.EnableSymbologies(symbologies);

            // Create barcode batch and attach to context.
            barcodeBatch = BarcodeBatch.Create(dataCaptureContext, barcodeBatchSettings);

            // Register self as a listener to get informed of tracked barcodes.
            barcodeBatch.AddListener(this);

            // To visualize the on-going barcode batch process on screen, setup a data capture view
            // that renders the camera preview. The view must be connected to the data capture context.
            var dataCaptureView = DataCaptureView.Create(this, dataCaptureContext);

            // Add a barcode batch overlay to the data capture view to render the tracked barcodes on
            // top of the video preview. This is optional, but recommended for better visual feedback.
            var overlay =
                    BarcodeBatchBasicOverlay.Create(barcodeBatch, dataCaptureView, BarcodeBatchBasicOverlayStyle.Frame);

            // Configure how barcodes are highlighted - apply default brush or create your own.
            this.rejectedBrush = new Brush(Color.Transparent, Color.Red, 3f);
            this.defaultBrush = new Brush(Color.Transparent, Color.Green, 3f);
            overlay.Listener = this;

            // Add the DataCaptureView to the container.
            var container = FindViewById<FrameLayout>(Resource.Id.data_capture_view_container);
            container.AddView(dataCaptureView);
        }

        protected override void OnPause()
        {
            PauseFrameSource();
            base.OnPause();
        }

        private void PauseFrameSource()
        {
            // Switch camera off to stop streaming frames.
            // The camera is stopped asynchronously and will take some time to completely turn off.
            // Until it is completely stopped, it is still possible to receive further results, hence
            // it's a good idea to first disable barcode batch as well.
            barcodeBatch.Enabled = false;
            camera.SwitchToDesiredStateAsync(FrameSourceState.Off);
        }

        protected override void OnResume()
        {
            base.OnResume();

            // Check for camera permission and request it, if it hasn't yet been granted.
            // Once we have the permission the onCameraPermissionGranted() method will be called.
            RequestCameraPermission();
        }

        public void OnObservationStarted(BarcodeBatch barcodeBatch)
        {
            // NOP
        }

        public void OnObservationStopped(BarcodeBatch barcodeBatch)
        {
            // NOP
        }

        public void OnSessionUpdated(BarcodeBatch barcodeBatch, BarcodeBatchSession session, IFrameData frameData)
        {
            lock (scanResults)
            {
                foreach (var trackedBarcode in
                    session.AddedTrackedBarcodes.Where(trb => IsValidBarcode(trb.Barcode)))
                {
                    scanResults.Add(new ScanResult(trackedBarcode.Barcode));
                }
            }
        }

        protected override void OnCameraPermissionGranted()
        {
            ResumeFrameSource();
        }

        private void ResumeFrameSource()
        {
            // Switch camera on to start streaming frames.
            // The camera is started asynchronously and will take some time to completely turn on.
            barcodeBatch.Enabled = true;
            camera.SwitchToDesiredStateAsync(FrameSourceState.On);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if (requestCode == REQUEST_CODE_SCAN_RESULTS
                && (int)resultCode == ResultsActivity.RESULT_CODE_CLEAN)
            {
                lock (scanResults)
                {
                    scanResults.Clear();
                }
            }
            base.OnActivityResult(requestCode, resultCode, data);
        }

        protected override void OnDestroy()
        {
            dataCaptureContext.RemoveMode(barcodeBatch);
            base.OnDestroy();
        }

        public Brush BrushForTrackedBarcode(BarcodeBatchBasicOverlay overlay, TrackedBarcode trackedBarcode)
        {
            if (IsValidBarcode(trackedBarcode.Barcode))
            {
                return defaultBrush;
            }
            else
            {
                return rejectedBrush;
            }
        }

        private bool IsValidBarcode(Barcode barcode)
        {
            // Reject invalid barcodes.
            if (String.IsNullOrEmpty(barcode.Data)) return false;

            // Reject barcodes based on your logic.
            if (barcode.Data.StartsWith("7")) return false;

            return true;
        }

        public void OnTrackedBarcodeTapped(BarcodeBatchBasicOverlay overlay, TrackedBarcode trackedBarcode)
        {
            // Handle barcode click if necessary.
        }
    }
}
