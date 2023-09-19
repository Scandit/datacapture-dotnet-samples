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

using UIKit;
using CoreFoundation;
using CoreGraphics;

using Scandit.DataCapture.Barcode.Data;
using Scandit.DataCapture.Barcode.Selection.Capture;
using Scandit.DataCapture.Barcode.Selection.UI.Overlay;
using Scandit.DataCapture.Core.Capture;
using Scandit.DataCapture.Core.Source;
using Scandit.DataCapture.Core.UI;
using Scandit.DataCapture.Core.Data;

namespace BarcodeSelectionSimpleSample
{
    public partial class ViewController : UIViewController, IBarcodeSelectionListener
    {
        // There is a Scandit sample license key set below here.
        // This license key is enabled for sample evaluation only.
        // If you want to build your own application, get your license key by signing up for a trial at https://ssl.scandit.com/dashboard/sign-up?p=test
        public const string SCANDIT_LICENSE_KEY = "AYjTKgwFKLhZGtmHmyNAawklGVUpLfmaJ2JN39hPFcbHRdb8Sh3UX45m7PRkJtORsQzsAeBZw7aAZ/VBZlp5ykVZZOOYUI8ZAxAsZ3tOrh5HXX2CzFyh2yNzGtUXQuR5eFHqhXNx8+mfbsvN2zErPt0+TW4TESKXSx4764U8HnIF/01crbTR4/qxeWvIgdmGJkoV2YZc4wfZjpQI2Uvd3/J2jFcv/WrVHgWZ/VAC2lHTzC3JdwtTNJKxxDpsqKp1sDlARxGjw4hlebrAUbft3aWMjbtpVn2T4D+tBN3GVuwlD9Uo7MN3Sto17fSVSD1JLymYPHP7zxsnByy9mCBhKqTf3YKCh8DughdNJpIIWaaoY6t6OTof+TxY25XAboYM1Ii3FdaK1MjK2x9bVujInqaIYzPRYRwQj6lPyVaYSiRRJTsR6l3RLXyorSeqM6Mjyspyb9Gl3ht1grXe8TzMwVUFLYwBlV1zYcKfCVxHIaPo8irO1X7+sImu0166pNeK962FxzUx+rJMsvEIhy8mzF//yRI8WBLZvuBS5AH8EJHBb5p6DcdLgNVf3AwQWw6S5ENIw1Nu+eS2p+nm7msRRWP5jbqo8TfwgoellmtHaljlvmQ47kXfZvo9feDd7qZtGvWuX22yZkb+3k0OEfNKZaBKLrfzKU6X5TlmMvyhU7mF6mMdkBwex+NuKhRl1fYVjzD1hk75j70/QgXyjMv9nJpSEIXEt//AVHZTG4lGvAT0l3hPOie/zS0ixEH11+LJvbzsZQXYngggsJ40oCbajRxnvrMEcJQ5Lcxnp/Ov8qTmApOqK+XmLAV/s+MdeeIatFNTk6o9xGar+cB8";

        private DataCaptureContext dataCaptureContext;
        private Camera camera;
        private BarcodeSelection barcodeSelection;
        private BarcodeSelectionSettings barcodeSelectionSettings;

        private const int dialogAutoDissmissInterval = 500;
        private UIButton aimToSelectButton;
        private UIButton tapToSelectButton;

        public ViewController()
        { }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillAppear(animated);
            // First, disable barcode selection to stop processing frames.
            this.barcodeSelection.Enabled = false;
            // Switch the camera off to stop streaming frames. The camera is stopped asynchronously.
            this.camera?.SwitchToDesiredStateAsync(FrameSourceState.Off);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // First, enable barcode selection to resume processing frames.
            this.barcodeSelection.Enabled = true;
            // Switch camera on to start streaming frames. The camera is started asynchronously and will take some time to
            // completely turn on.
            this.camera?.SwitchToDesiredStateAsync(FrameSourceState.On);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            this.View.BackgroundColor = UIColor.Black;
            this.InitializeAndStartBarcodeSelection();
            this.AddAimToSelectButton();
            this.AddTapToSelectButton();
            this.View.LayoutIfNeeded();
        }

        protected void InitializeAndStartBarcodeSelection()
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
                this.dataCaptureContext.SetFrameSourceAsync(this.camera);
            }

            // Use the recommended camera settings for the BarcodeSelection mode as default settings.
            // The preferred resolution is automatically chosen, which currently defaults to HD on all devices.
            CameraSettings cameraSettings = BarcodeSelection.RecommendedCameraSettings;

            // Setting the preferred resolution to full HD helps to get a better decode range.
            cameraSettings.PreferredResolution = VideoResolution.FullHd;
            camera?.ApplySettingsAsync(cameraSettings);
            this.barcodeSelectionSettings = BarcodeSelectionSettings.Create();

            // The settings instance initially has all types of barcodes (symbologies) disabled.
            // For the purpose of this sample we enable a very generous set of symbologies.
            // In your own app ensure that you only enable the symbologies that your app requires as
            // every additional enabled symbology has an impact on processing times.
            HashSet<Symbology> symbologies = new HashSet<Symbology>()
            {
                Symbology.Ean13Upca,
                Symbology.Ean8,
                Symbology.Upce,
                Symbology.Code39,
                Symbology.Code128
            };

            this.barcodeSelectionSettings.EnableSymbologies(symbologies);

            // Create new barcode selection mode with the settings from above.
            this.barcodeSelection = BarcodeSelection.Create(this.dataCaptureContext, this.barcodeSelectionSettings);

            // Register self as a listener to get informed whenever a new barcode got selected.
            this.barcodeSelection.AddListener(this);

            // To visualize the on-going barcode selection process on screen, setup a data capture view
            // that renders the camera preview. The view must be connected to the data capture context.
            var dataCaptureView = DataCaptureView.Create(this.dataCaptureContext,
                                                         new CGRect(
                                                             this.View.Bounds.Location,
                                                             new CGSize(
                                                                 this.View.Bounds.Width,
                                                                 this.View.Bounds.Height - 48)));
            dataCaptureView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight |
                                               UIViewAutoresizing.FlexibleWidth;
            this.View.AddSubview(dataCaptureView);
            this.View.SendSubviewToBack(dataCaptureView);

            // Create barcode selection overlay to the data capture view to render the selected barcodes on
            // top of the video preview. This is optional, but recommended for better visual feedback.
            BarcodeSelectionBasicOverlay.Create(this.barcodeSelection, dataCaptureView);
        }

        protected void AddAimToSelectButton()
        {
            this.aimToSelectButton = new UIButton(UIButtonType.RoundedRect);
            this.aimToSelectButton.SetTitle("Aim to Select", UIControlState.Normal);
            this.aimToSelectButton.SetTitleColor(UIColor.White, UIControlState.Normal);
            this.aimToSelectButton.BackgroundColor = UIColor.Black;
            this.aimToSelectButton.TranslatesAutoresizingMaskIntoConstraints = false;
            this.aimToSelectButton.TitleLabel.Font = UIFont.SystemFontOfSize(18);

            this.View.AddSubview(aimToSelectButton);
            this.View.AddConstraints(new[]
            {
                this.aimToSelectButton.LeadingAnchor.ConstraintEqualTo(this.View.LeadingAnchor, 32),
                this.aimToSelectButton.BottomAnchor.ConstraintEqualTo(this.View.BottomAnchor, -8)
            });
            this.aimToSelectButton.TouchUpInside += AimToSelectButtonClick;
        }

        protected void AddTapToSelectButton()
        {
            this.tapToSelectButton = new UIButton(UIButtonType.RoundedRect);
            this.tapToSelectButton.SetTitle("Tap to Select", UIControlState.Normal);
            this.tapToSelectButton.SetTitleColor(UIColor.White, UIControlState.Normal);
            this.tapToSelectButton.BackgroundColor = UIColor.Black;
            this.tapToSelectButton.TranslatesAutoresizingMaskIntoConstraints = false;
            this.tapToSelectButton.TitleLabel.Font = UIFont.SystemFontOfSize(18);

            this.View.AddSubview(tapToSelectButton);
            this.View.AddConstraints(new[]
            {
                this.tapToSelectButton.TrailingAnchor.ConstraintEqualTo(this.View.TrailingAnchor, -32),
                this.tapToSelectButton.BottomAnchor.ConstraintEqualTo(this.View.BottomAnchor, -8)
            });
            this.tapToSelectButton.TouchUpInside += TapToSelectButtonClick;
        }

        private void AimToSelectButtonClick(object sender, System.EventArgs e)
        {
            if (this.barcodeSelectionSettings.SelectionType is BarcodeSelectionTapSelection)
            {
                this.barcodeSelectionSettings.SelectionType = BarcodeSelectionAimerSelection.Create();
                this.barcodeSelection.ApplySettingsAsync(this.barcodeSelectionSettings)
                                     .ContinueWith((task) =>
                                         // Switch the camera to On state in case it froze through
                                         // double tap while on TapToSelect selection type.
                                         this.camera.SwitchToDesiredStateAsync(FrameSourceState.On));
            }
        }

        private void TapToSelectButtonClick(object sender, System.EventArgs e)
        {
            if (this.barcodeSelectionSettings.SelectionType is BarcodeSelectionAimerSelection)
            {
                this.barcodeSelectionSettings.SelectionType = BarcodeSelectionTapSelection.Create();
                this.barcodeSelection.ApplySettingsAsync(this.barcodeSelectionSettings);
            }
        }

        #region IBarcodeSelectionListener
        public void OnSelectionUpdated(BarcodeSelection barcodeSelection, BarcodeSelectionSession session, IFrameData frameData)
        {
            if (!session.NewlySelectedBarcodes.Any())
            {
                return;
            }

            Barcode barcode = session.NewlySelectedBarcodes.First();

            // Get barcode selection count.
            int selectionCount = session.GetCount(barcode);

            // Get the human readable name of the symbology and assemble the result to be shown.
            SymbologyDescription description = new SymbologyDescription(barcode.Symbology);
            string result = barcode.Data + " (" + description.ReadableName + ")" + "\nTimes: " + selectionCount; ;

            this.ShowResult(result);

            // Dispose the frame when you have finished processing it. If the frame is not properly disposed,
            // different issues could arise, e.g. a frozen, non-responsive, or "severely stuttering" video feed.
            frameData?.Dispose();
        }

        public void OnSessionUpdated(BarcodeSelection barcodeSelection, BarcodeSelectionSession session, IFrameData frameData)
        {
            // Dispose the frame when you have finished processing it. If the frame is not properly disposed,
            // different issues could arise, e.g. a frozen, non-responsive, or "severely stuttering" video feed.
            frameData?.Dispose();
        }

        public void OnObservationStarted(BarcodeSelection barcodeSelection)
        { }

        public void OnObservationStopped(BarcodeSelection barcodeSelection)
        { }
        #endregion

        private void ShowResult(string result)
        {
            DispatchQueue.MainQueue.DispatchAsync(() =>
            {
                var alert = UIAlertController.Create(result, message: null, UIAlertControllerStyle.Alert);
                this.PresentViewController(alert, true, () =>
                {
                    var delta = new TimeSpan(0, 0, 0, 0, dialogAutoDissmissInterval);
                    var dispatchTime = new DispatchTime(DispatchTime.Now, delta);
                    DispatchQueue.MainQueue.DispatchAfter(dispatchTime, () =>
                    {
                        this.DismissViewController(true, completionHandler: null);
                    });
                });
            });
        }
    }
}
