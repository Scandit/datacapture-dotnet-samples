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

using System;
using System.Collections.Generic;
using System.Linq;
using CoreFoundation;

using Scandit.DataCapture.Barcode.Capture;
using Scandit.DataCapture.Barcode.Data;
using Scandit.DataCapture.Barcode.UI.Overlay;
using Scandit.DataCapture.Core.Capture;
using Scandit.DataCapture.Core.Common.Feedback;
using Scandit.DataCapture.Core.Data;
using Scandit.DataCapture.Core.Source;
using Scandit.DataCapture.Core.UI;
using Scandit.DataCapture.Core.UI.Style;
using Scandit.DataCapture.Core.UI.Viewfinder;
using UIKit;

namespace BarcodeCaptureSimpleSample
{
    public partial class ViewController : UIViewController, IBarcodeCaptureListener
    {
        // Enter your Scandit License key here.
        // Your Scandit License key is available via your Scandit SDK web account.
        public const string SCANDIT_LICENSE_KEY = "-- ENTER YOUR SCANDIT LICENSE KEY HERE --";

        private DataCaptureContext dataCaptureContext;
        private Camera camera;
        private DataCaptureView dataCaptureView;
        private BarcodeCapture barcodeCapture;
        private BarcodeCaptureOverlay overlay;
        private readonly Feedback feedback = Feedback.DefaultFeedback;
        private readonly BarcodeCaptureOverlayStyle overlayStyle = BarcodeCaptureOverlayStyle.Frame;

        public ViewController(IntPtr handle) : base(handle)
        { }

        public ViewController()
        { }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            this.InitializeAndStartBarcodeScanning();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            this.barcodeCapture.Enabled = true;

            // Switch camera on to start streaming frames. The camera is started asynchronously and will
            // take some time to completely turn on.
            this.camera?.SwitchToDesiredStateAsync(FrameSourceState.On);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            // Switch camera off to stop streaming frames. The camera is stopped asynchronously and will
            // take some time to completely turn off. Until it is completely stopped, it is still possible
            // to receive further results, hence it's a good idea to first disable barcode capture as well.
            this.camera?.SwitchToDesiredStateAsync(FrameSourceState.Off);
        }

        protected void InitializeAndStartBarcodeScanning()
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

            BarcodeCaptureSettings barcodeCaptureSettings = BarcodeCaptureSettings.Create();

            // The settings instance initially has all types of barcodes (symbologies) disabled.
            // For the purpose of this sample we enable a very generous set of symbologies.
            // In your own app ensure that you only enable the symbologies that your app requires as
            // every additional enabled symbology has an impact on processing times.
            HashSet<Symbology> symbologies = new HashSet<Symbology>()
            {
                Symbology.Ean13Upca,
                Symbology.Ean8,
                Symbology.Upce,
                Symbology.Qr,
                Symbology.DataMatrix,
                Symbology.Code39,
                Symbology.Code128,
                Symbology.InterleavedTwoOfFive
            };

            barcodeCaptureSettings.EnableSymbologies(symbologies);

            // Some linear/1d barcode symbologies allow you to encode variable-length data.
            // By default, the Scandit Data Capture SDK only scans barcodes in a certain length range.
            // If your application requires scanning of one of these symbologies, and the length is
            // falling outside the default range, you may need to adjust the "active symbol counts"
            // for this symbology. This is shown in the following few lines of code for one of the
            // variable-length symbologies.
            SymbologySettings symbologySettings =
                    barcodeCaptureSettings.GetSymbologySettings(Symbology.Code39);

            HashSet<short> activeSymbolCounts = new HashSet<short>(
                new short[] { 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 });

            symbologySettings.ActiveSymbolCounts = activeSymbolCounts;

            // Create new barcode capture mode with the settings from above.
            this.barcodeCapture = BarcodeCapture.Create(this.dataCaptureContext, barcodeCaptureSettings);

            // By default, every time a barcode is scanned, a sound (if not in silent mode) and a vibration are played.
            // Uncomment the following lines to set a success feedback without sound and vibration.
            // this.barcodeCapture.Feedback.Success = new Feedback(vibration: null, sound: null);

            // Register self as a listener to get informed whenever a new barcode got recognized.
            this.barcodeCapture.AddListener(this);

            // To visualize the on-going barcode capturing process on screen, setup a data capture view
            // that renders the camera preview. The view must be connected to the data capture context.
            this.dataCaptureView = DataCaptureView.Create(this.dataCaptureContext, this.View.Bounds);
            this.dataCaptureView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight |
                                                    UIViewAutoresizing.FlexibleWidth;
            this.View.AddSubview(this.dataCaptureView);

            this.overlay = BarcodeCaptureOverlay.Create(this.barcodeCapture, this.dataCaptureView, BarcodeCaptureOverlayStyle.Frame);
            this.overlay.Viewfinder = new RectangularViewfinder(RectangularViewfinderStyle.Square, RectangularViewfinderLineStyle.Light);
        }

        public void ShowResult(string result)
        {
            DispatchQueue.MainQueue.DispatchAsync(() =>
            {
                UIAlertController alert = UIAlertController.Create(result, message: null, preferredStyle: UIAlertControllerStyle.Alert);
                var action = UIAlertAction.Create("OK", UIAlertActionStyle.Default, (UIAlertAction) =>
                {
                    this.barcodeCapture.Enabled = true;
                });
                alert.AddAction(action);
                this.PresentViewController(alert, animated: true, completionHandler: () => { });
            });
        }

        #region IBarcodeCaptureListener

        public void OnBarcodeScanned(BarcodeCapture barcodeCapture, BarcodeCaptureSession session, IFrameData frameData)
        {
            var barcode = session.NewlyRecognizedBarcode;

            if (barcode == null)
            {
                return;
            }

            // Use the following code to reject barcodes.
            // By uncommenting the following lines, barcodes not starting with 09: are ignored.
            // if (barcode.Data?.StartsWith("09:") == false)
            // {
            //     // We temporarily change the brush, used to highlight recognized barcodes, to a
            //     // transparent brush.
            //     this.overlay.Brush = Brush.TransparentBrush;
            //     return;
            // }

            // Otherwise, if the barcode is of interest, we want to use a brush to highlight it.
            // this.overlay.Brush = BarcodeCaptureOverlay.DefaultBrushForStyle(this.overlayStyle);

            // We also want to emit a feedback (vibration and, if enabled, sound).
            // By default, every time a barcode is scanned, a sound (if not in silent mode) and a vibration are played.
            // To emit a feedback only when necessary, it is necessary to set a success feedback without sound and
            // vibration when setting up Barcode Capture (in this case in the `InitializeAndStartBarcodeScanning` method).
            // this.feedback.Emit();

            // Stop recognizing barcodes for as long as we are displaying the result. There won't be any new
            // results until the capture mode is enabled again. Note that disabling the capture mode does
            // not stop the camera, the camera continues to stream frames until it is turned off.
            this.barcodeCapture.Enabled = false;

            // If you don't want the codes to be scanned more than once, consider setting the codeDuplicateFilter when
            // creating the barcode capture settings to -1.
            // You can set any other value (e.g. 500) to set a fixed timeout and override the smart behaviour enabled
            // by default.

            // Get the human readable name of the symbology and assemble the result to be shown.
            string symbology = new SymbologyDescription(barcode.Symbology).ReadableName;
            string result = string.Format("Scanned {0} ({1})", barcode.Data, symbology);
            this.ShowResult(result);

            // Dispose the frame when you have finished processing it. If the frame is not properly disposed,
            // different issues could arise, e.g. a frozen, non-responsive, or "severely stuttering" video feed.
            frameData.Dispose();
        }

        public void OnSessionUpdated(BarcodeCapture barcodeCapture, BarcodeCaptureSession session, IFrameData frameData)
        {
            // Dispose the frame when you have finished processing it. If the frame is not properly disposed,
            // different issues could arise, e.g. a frozen, non-responsive, or "severely stuttering" video feed.
            frameData.Dispose();
        }

        public void OnObservationStarted(BarcodeCapture barcodeCapture)
        {
        }

        public void OnObservationStopped(BarcodeCapture barcodeCapture)
        {
        }

        #endregion
    }
}
