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

namespace BarcodeCaptureRejectSample
{
    public partial class ViewController : UIViewController, IBarcodeCaptureListener
    {
        // There is a Scandit sample license key set below here.
        // This license key is enabled for sample evaluation only.
        // If you want to build your own application, get your license key by signing up for a trial at https://ssl.scandit.com/dashboard/sign-up?p=test
        public const string SCANDIT_LICENSE_KEY = "AQIzpSC5AyYeKA6KZgjthjEmMbJBFJEpiUUjkCJu72AUVSWyGjN0xNt0OVgASxKO6FwLejYDRFGraFReiUwL8wp3a8mgX0elHhmx0JhY/QYrbQHJjGIhQAhjcW1cYr+ogWCDUmhM2KuWPlJXBkSGmbwinMAqKusC5zQHGoY6JDKJXbzv97CRhGdjlfgjhTZErgfs+P/fLp0cCCAmP+TTZ6jiyA/my9Ojy7ugt7DKay2ZAkezAO8OwAtnl0GUIflPz6KI68hRPaAV18wwS030+riqfDIcFQ+3BAfqRMpJxrYfKZOvvwyTAbC+5ZzgFmwd9YR0vbFToSmHDemEyRVufdMw0s+jqCHsCY5ox8jBfV1RkmDQxCckkJoS3rhPmLgEyiTm+gI0y30swn2orZ4aaml+aoA55vhN4jY+ZAkMkmhipAXK/TMzyHo4iUDA4/v3TgiJbodw27iI/+f6YxIpA+/nAEItRH7C3vuxAdo8lmk5q0QeCkc6QA0FhQa6S/cu8yrehTi+Lb8khFmt3gkwEubowGdg3cg8KoBsDgY59lAKWy55rmVznq7REv6ugw1KwgW724K4s5ILfgQ2NcV/jFgeTReaTSVYUWKZGXdJmDrteX7tgmdfkpjaCrijgSGwYRaATxVKitCYIPyfuipsSHdC0iLqCoJ8CIc2UclvimPXDzDLk83uIRFjgspykVm+eIsKiMuxrW6OlB7o7NWPcJtEcyO74Mq6scB8+bWP5eJFIPazUcZEtxG2u3UpWz7+EoBADwbUI9G63HcTwt2bi8JZo16pfGxsWti3DJ1HWooGSIVvyZ2jePvhBcuu+EbtOucgdPDvDTCTpm/V";
        
        private DataCaptureContext dataCaptureContext;
        private Camera camera;
        private DataCaptureView dataCaptureView;
        private BarcodeCapture barcodeCapture;
        private BarcodeCaptureOverlay overlay;
        private readonly Feedback feedback = Feedback.DefaultFeedback;
        private readonly BarcodeCaptureOverlayStyle overlayStyle = BarcodeCaptureOverlayStyle.Frame;

        public ViewController()
        { }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();
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

        public override void DidReceiveMemoryWarning ()
        {
            base.DidReceiveMemoryWarning ();
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
            // For the purpose of this sample we enable the QR symbology. In your own app ensure
            // that you only enable the symbologies that your app requires as every additional
            // enabled symbology has an impact on processing times.
            barcodeCaptureSettings.EnableSymbology(Symbology.Qr, true);

            // Create new barcode capture mode with the settings from above.
            this.barcodeCapture = BarcodeCapture.Create(this.dataCaptureContext, barcodeCaptureSettings);

            // By default, every time a barcode is scanned, a sound (if not in silent mode) and a
            // vibration are played. In the following we are setting a success feedback without sound
            // and vibration.
            this.barcodeCapture.Feedback.Success = new Feedback(vibration: null, sound: null);

            // Register self as a listener to get informed whenever a new barcode got recognized.
            this.barcodeCapture.AddListener(this);

            // To visualize the on-going barcode capturing process on screen, setup a data capture view
            // that renders the camera preview. The view must be connected to the data capture context.
            this.dataCaptureView = DataCaptureView.Create(this.dataCaptureContext, this.View.Bounds);
            this.dataCaptureView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight |
                                                    UIViewAutoresizing.FlexibleWidth;
            this.View.AddSubview(this.dataCaptureView);

            this.overlay = BarcodeCaptureOverlay.Create(this.barcodeCapture, this.dataCaptureView, this.overlayStyle);
            this.overlay.Viewfinder = new RectangularViewfinder(RectangularViewfinderStyle.Square, RectangularViewfinderLineStyle.Light);
        }

        public void ShowResult(string result)
        {
            DispatchQueue.MainQueue.DispatchAsync(() => {
                UIAlertController alert = UIAlertController.Create(result, message: null, preferredStyle: UIAlertControllerStyle.Alert);
                var action = UIAlertAction.Create("OK", UIAlertActionStyle.Default, (UIAlertAction) => {
                    this.barcodeCapture.Enabled = true;
                });
                alert.AddAction(action);
                this.PresentViewController(alert, animated: true, completionHandler: () => { });
            });
        }

        #region IBarcodeCaptureListener

        public void OnBarcodeScanned(BarcodeCapture barcodeCapture, BarcodeCaptureSession session, IFrameData frameData)
        {
            if (session.NewlyRecognizedBarcodes.Count == 0)
            {
                return;
            }

            var barcode = session.NewlyRecognizedBarcodes[0];

            // If the code scanned doesn't start with "09:", we will just ignore it and continue
            // scanning.
            if (barcode.Data?.StartsWith("09:") == false)
            {
                // We temporarily change the brush, used to highlight recognized barcodes, to a
                // transparent brush.
                this.overlay.Brush = Brush.TransparentBrush;
                return;
            }

            // If the code is accepted, we want to make sure to use
            // a brush to highlight the code.
            this.overlay.Brush = BarcodeCaptureOverlay.DefaultBrushForStyle(this.overlayStyle);

            // We also want to emit a feedback (vibration and, if enabled, sound).
            this.feedback.Emit();

            // Stop recognizing barcodes for as long as we are displaying the result. There won't be any new
            // results until the capture mode is enabled again. Note that disabling the capture mode does
            // not stop the camera, the camera continues to stream frames until it is turned off.
            this.barcodeCapture.Enabled = false;

            // If you are not disabling barcode capture here and want to continue scanning, consider
            // setting the codeDuplicateFilter when creating the barcode capture settings to around 500
            // or even -1 if you do not want codes to be scanned more than once.

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
