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

using System.Windows.Input;
using MatrixScanBubblesSample.Models;
using MatrixScanBubblesSample.Views;

using Scandit.DataCapture.Barcode.Tracking.Capture;
using Scandit.DataCapture.Barcode.Tracking.Data;
using Scandit.DataCapture.Barcode.Tracking.UI.Overlay;
using Scandit.DataCapture.Core.Capture;
using Scandit.DataCapture.Core.Common.Geometry;
using Scandit.DataCapture.Core.Data;
using Scandit.DataCapture.Core.Source;

namespace MatrixScanBubblesSample.ViewModels
{
    public partial class MainPageViewModel : IBarcodeTrackingListener, IBarcodeTrackingAdvancedOverlayListener
    {
        private readonly IDictionary<int, View> overlays = new Dictionary<int, View>();
        private readonly int shelfCount = 4;
        private readonly int backRoom = 8;
        private bool cameraPaused = false;

        public Camera Camera { get; private set; } = ScannerModel.Instance.CurrentCamera;
        public DataCaptureContext DataCaptureContext { get; private set; } = ScannerModel.Instance.DataCaptureContext;
        public BarcodeTracking BarcodeTracking { get; private set; } = ScannerModel.Instance.BarcodeTracking;
        public Func<TrackedBarcode, bool> ShouldHideOverlay;

        public MainPageViewModel()
        {
            this.InitializeScanner();
            this.SubscribeToAppMessages();

            this.ToggleFreezeButton = new Command(() =>
            {
                if (this.BarcodeTracking.Enabled)
                {
                    this.BarcodeTracking.Enabled = false;
                    this.Camera.SwitchToDesiredStateAsync(FrameSourceState.Off);
                    this.cameraPaused = true;
                }
                else
                {
                    this.BarcodeTracking.Enabled = true;
                    this.Camera.SwitchToDesiredStateAsync(FrameSourceState.On);
                    this.cameraPaused = false;
                }
            });
        }

        public Task OnSleep()
        {
            return this.Camera?.SwitchToDesiredStateAsync(FrameSourceState.Off);
        }

        public async Task OnResumeAsync()
        {
            var permissionStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();

            if (permissionStatus != PermissionStatus.Granted)
            {
                permissionStatus = await Permissions.RequestAsync<Permissions.Camera>();
                if (permissionStatus == PermissionStatus.Granted)
                {
                    await this.ResumeFrameSource();
                }
            }
            else
            {
                await this.ResumeFrameSource();
            }
        }

        public ICommand ToggleFreezeButton { get; private set; }

        private void SubscribeToAppMessages()
        {
            MessagingCenter.Subscribe(this, App.MessageKeys.OnResume, callback: async (App app) => await this.OnResumeAsync());
            MessagingCenter.Subscribe(this, App.MessageKeys.OnSleep, callback: async (App app) => await this.OnSleep());
        }

        private void InitializeScanner()
        {
            // Register self as a listener to get informed whenever a new barcode got recognized.
            this.BarcodeTracking.AddListener(this);
        }

        private Task ResumeFrameSource()
        {
            if (!this.cameraPaused)
            {
                // Switch camera on to start streaming frames.
                // The camera is started asynchronously and will take some time to completely turn on.
                return this.Camera?.SwitchToDesiredStateAsync(FrameSourceState.On);
            }

            return Task.FromResult(false);
        }

        #region IBarcodeTrackingListener
        public void OnObservationStarted(BarcodeTracking barcodeTracking)
        { }

        public void OnObservationStopped(BarcodeTracking barcodeTracking)
        { }

        public void OnSessionUpdated(BarcodeTracking barcodeTracking, BarcodeTrackingSession session, IFrameData frameData)
        {
            // This method is called whenever objects are updated and it's the right place to react to the tracking results.

            Application.Current.Dispatcher.Dispatch(() =>
            {
                if (!this.BarcodeTracking.Enabled)
                {
                    return;
                }

                foreach (var identifier in session.RemovedTrackedBarcodes)
                {
                    this.overlays.Remove(identifier);
                }

                var filteredTrackedCodes = session.TrackedBarcodes.Values.Where(code => code != null && code.Barcode != null);
                foreach (var trackedCode in filteredTrackedCodes)
                {
                    if (this.overlays.TryGetValue(trackedCode.Identifier, out View stockOverlay))
                    {
                        stockOverlay.IsVisible = !this.ShouldHideOverlay?.Invoke(trackedCode) ?? true;
                    }
                }
            });
        }
        #endregion

        #region IBarcodeTrackingAdvancedOverlay
        // Please check Platform specific implementation for the IBarcodeTrackingAdvancedOverlayListener.ViewForTrackedBarcode
        // implementation under ./Platforms/Android and ./Platforms/iOS folders.

        public Anchor AnchorForTrackedBarcode(BarcodeTrackingAdvancedOverlay overlay, TrackedBarcode trackedBarcode)
        {
            // The offset of our overlay will be calculated from the top center anchoring point.
            return Anchor.TopCenter;
        }

        public PointWithUnit OffsetForTrackedBarcode(BarcodeTrackingAdvancedOverlay overlay, TrackedBarcode trackedBarcode)
        {
            // We set the offset's height to be equal of the 100 percent of our overlay.
            // The minus sign means that the overlay will be above the barcode.
            return new PointWithUnit(
                x: new FloatWithUnit(value: 0, unit: MeasureUnit.Fraction),
                y: new FloatWithUnit(value: -1, unit: MeasureUnit.Fraction));
        }
        #endregion
    }
}
