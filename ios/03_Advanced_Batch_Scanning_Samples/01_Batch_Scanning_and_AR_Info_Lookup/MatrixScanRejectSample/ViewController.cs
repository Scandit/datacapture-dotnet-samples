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
using Foundation;
using UIKit;

using Scandit.DataCapture.Barcode.Data;
using Scandit.DataCapture.Barcode.Batch.Capture;
using Scandit.DataCapture.Barcode.Batch.Data;
using Scandit.DataCapture.Barcode.Batch.UI.Overlay;
using Scandit.DataCapture.Core.Capture;
using Scandit.DataCapture.Core.Data;
using Scandit.DataCapture.Core.Source;
using Scandit.DataCapture.Core.UI;
using Scandit.DataCapture.Core.UI.Style;

namespace MatrixScanRejectSample
{
    public partial class ViewController : UIViewController, IBarcodeBatchBasicOverlayListener
    {
        // Enter your Scandit License key here.
        // Your Scandit License key is available via your Scandit SDK web account.
        public const string SCANDIT_LICENSE_KEY = "-- ENTER YOUR SCANDIT LICENSE KEY HERE --";

        private DataCaptureContext? dataCaptureContext;
        private Camera? camera;
        private DataCaptureView? dataCaptureView;
        private BarcodeBatch? barcodeBatch;
        private BarcodeBatchBasicOverlay? overlay;

        private HashSet<ScanResult> scanResults = new HashSet<ScanResult>();

        private readonly Brush defaultBrush = new Brush(UIColor.Clear, UIColor.Green, 3);
        private readonly Brush rejectedBrush = new Brush(UIColor.Clear, UIColor.Red, 3);

        public ViewController(IntPtr handle) : base(handle)
        { }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (this.barcodeBatch != null)
            {
                // First, disable barcode batch to stop processing frames.
                this.barcodeBatch.Enabled = false;
            }

            // Switch the camera off to stop streaming frames. The camera is stopped asynchronously.
            this.camera?.SwitchToDesiredStateAsync(FrameSourceState.Off);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Remove the scanned barcodes everytime the barcode batch starts.
            this.scanResults.Clear();

            if (this.barcodeBatch != null)
            {
                // First, enable barcode batch to resume processing frames.
                this.barcodeBatch.Enabled = true;
            }

            // Switch camera on to start streaming frames. The camera is started asynchronously and will take some time to
            // completely turn on.
            this.camera?.SwitchToDesiredStateAsync(FrameSourceState.On);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            this.InitializeAndStartBarcodeScanning();
        }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject? sender)
        {
            base.PrepareForSegue(segue, sender);

            if (segue.DestinationViewController is ResultsViewController resultViewController)
            {
                resultViewController.Items = this.scanResults.ToList();
            }
        }

        #region IBarcodeBatchBasicOverlayListener
        public Brush BrushForTrackedBarcode(BarcodeBatchBasicOverlay overlay, TrackedBarcode trackedBarcode)
        {
            if (IsValidBarcode(trackedBarcode.Barcode))
            {
                return this.defaultBrush;
            }
            else
            {
                return this.rejectedBrush;
            }
        }

        public void OnTrackedBarcodeTapped(BarcodeBatchBasicOverlay overlay, TrackedBarcode trackedBarcode)
        {
            // Handle barcode click if necessary.
        }
        #endregion

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
                this.dataCaptureContext.SetFrameSourceAsync(this.camera);
            }

            // Use the recommended camera settings for the BarcodeBatch mode as default settings.
            // The preferred resolution is automatically chosen, which currently defaults to HD on all devices.
            CameraSettings cameraSettings = BarcodeBatch.RecommendedCameraSettings;

            // Setting the preferred resolution to full HD helps to get a better decode range.
            cameraSettings.PreferredResolution = VideoResolution.FullHd;
            this.camera?.ApplySettingsAsync(cameraSettings);
            BarcodeBatchSettings barcodeBatchSettings = BarcodeBatchSettings.Create();

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

            barcodeBatchSettings.EnableSymbologies(symbologies);

            // Create new barcode batch mode with the settings from above.
            this.barcodeBatch = BarcodeBatch.Create(this.dataCaptureContext, barcodeBatchSettings);

            // Subscribe to session updates to get informed whenever a new barcode got recognized.
            this.barcodeBatch.SessionUpdated += BarcodeBatchSessionUpdated;

            // To visualize the on-going barcode batch process on screen, setup a data capture view
            // that renders the camera preview. The view must be connected to the data capture context.
            this.dataCaptureView = DataCaptureView.Create(this.dataCaptureContext, this.View!.Bounds);
            this.dataCaptureView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight |
                                                    UIViewAutoresizing.FlexibleWidth;

            this.overlay = BarcodeBatchBasicOverlay.Create(this.barcodeBatch,
                                                              this.dataCaptureView,
                                                              BarcodeBatchBasicOverlayStyle.Frame);
            this.overlay.Listener = this;

            this.View.AddSubview(this.dataCaptureView);
            this.View.SendSubviewToBack(this.dataCaptureView);
        }

        private void BarcodeBatchSessionUpdated(object? sender, BarcodeBatchEventArgs args)
        {
            var barcodes = args.Session.TrackedBarcodes
                                       .Values
                                       .Where(item => IsValidBarcode(item.Barcode));

            this.scanResults = barcodes.Select(v =>
            {
                return new ScanResult
                {
                    Data = v.Barcode.Data ?? string.Empty,
                    Symbology = v.Barcode.Symbology.ReadableName()
                };
            }).ToHashSet();
        }

        private static bool IsValidBarcode(Barcode barcode)
        {
            // Reject invalid barcodes.
            if (string.IsNullOrEmpty(barcode.Data))
            {
                return false;
            }

            // Reject barcodes based on your logic.
            if (barcode.Data.StartsWith("7"))
            {
                return false;
            }

            return true;
        }
    }
}
