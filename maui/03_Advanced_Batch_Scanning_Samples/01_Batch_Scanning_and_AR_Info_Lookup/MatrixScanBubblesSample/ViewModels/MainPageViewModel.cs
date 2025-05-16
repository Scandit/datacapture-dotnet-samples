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

using System.Windows.Input;
using MatrixScanBubblesSample.Models;

using Scandit.DataCapture.Barcode.Batch.Capture;
using Scandit.DataCapture.Barcode.Batch.Data;
using Scandit.DataCapture.Barcode.Batch.UI.Overlay;
using Scandit.DataCapture.Core.Capture;
using Scandit.DataCapture.Core.Common.Geometry;
using Scandit.DataCapture.Core.Data;
using Scandit.DataCapture.Core.Source;

namespace MatrixScanBubblesSample.ViewModels
{
    public partial class MainPageViewModel : BaseViewModel, IBarcodeBatchListener, IBarcodeBatchAdvancedOverlayListener
    {
        private readonly Dictionary<int, View> overlays = [];
        private readonly int shelfCount = 4;
        private readonly int backRoom = 8;
        private bool cameraPaused = false;

        public Camera Camera { get; private set; } = DataCaptureManager.Instance.CurrentCamera;
        public DataCaptureContext DataCaptureContext { get; private set; } = DataCaptureManager.Instance.DataCaptureContext;
        public BarcodeBatch BarcodeBatch { get; private set; } = DataCaptureManager.Instance.BarcodeBatch;
        public Func<TrackedBarcode, bool>? ShouldHideOverlay;

        public MainPageViewModel()
        {
            this.InitializeScanner();

            this.ToggleFreezeButton = new Command(() =>
            {
                if (this.BarcodeBatch.Enabled)
                {
                    this.BarcodeBatch.Enabled = false;
                    this.Camera.SwitchToDesiredStateAsync(FrameSourceState.Off);
                    this.cameraPaused = true;
                }
                else
                {
                    this.BarcodeBatch.Enabled = true;
                    this.Camera.SwitchToDesiredStateAsync(FrameSourceState.On);
                    this.cameraPaused = false;
                }
            });
        }

        public override async Task SleepAsync()
        {
            if (this.Camera != null)
            {
                await this.Camera.SwitchToDesiredStateAsync(FrameSourceState.Off);
            }
        }

        public override async Task ResumeAsync()
        {
            var permissionStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();

            if (permissionStatus != PermissionStatus.Granted)
            {
                permissionStatus = await Permissions.RequestAsync<Permissions.Camera>();
                if (permissionStatus == PermissionStatus.Granted)
                {
                    await this.ResumeFrameSourceAsync();
                }
            }
            else
            {
                await this.ResumeFrameSourceAsync();
            }
        }

        public ICommand ToggleFreezeButton { get; private set; }

        private void InitializeScanner()
        {
            // Register self as a listener to get informed whenever a new barcode got recognized.
            this.BarcodeBatch.AddListener(this);
        }

        private async Task<bool> ResumeFrameSourceAsync()
        {
            if (!this.cameraPaused && this.Camera != null)
            {
                // Switch camera on to start streaming frames.
                // The camera is started asynchronously and will take some time to completely turn on.
                return await this.Camera.SwitchToDesiredStateAsync(FrameSourceState.On);
            }

            return false;
        }

        #region IBarcodeBatchListener
        public void OnObservationStarted(BarcodeBatch barcodeBatch)
        { }

        public void OnObservationStopped(BarcodeBatch barcodeBatch)
        { }

        public void OnSessionUpdated(BarcodeBatch barcodeBatch, BarcodeBatchSession session, IFrameData frameData)
        {
            // This method is called whenever objects are updated and it's the right place to react to the batch results.
            MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (!this.BarcodeBatch.Enabled)
                {
                    return;
                }

                foreach (var identifier in session.RemovedTrackedBarcodes)
                {
                    this.overlays.Remove(identifier);
                }

                var filteredTrackedCodes = session
                    .TrackedBarcodes
                    .Values
                    .Where(code => code != null && code.Barcode != null);

                foreach (var trackedCode in filteredTrackedCodes)
                {
                    var id = trackedCode.Identifier;

                    if (this.overlays.TryGetValue(id, out View? stockOverlay) && stockOverlay != null)
                    {
                        stockOverlay.IsVisible = !this.ShouldHideOverlay?.Invoke(trackedCode) ?? true;
                    }
                }
            });
        }
        #endregion

        #region IBarcodeBatchAdvancedOverlay
        // Please check Platform specific implementation for the IBarcodeBatchAdvancedOverlayListener.ViewForTrackedBarcode
        // implementation under ./Platforms/Android and ./Platforms/iOS folders.

        public Anchor AnchorForTrackedBarcode(BarcodeBatchAdvancedOverlay overlay, TrackedBarcode trackedBarcode)
        {
            // The offset of our overlay will be calculated from the top center anchoring point.
            return Anchor.TopCenter;
        }

        public PointWithUnit OffsetForTrackedBarcode(BarcodeBatchAdvancedOverlay overlay, TrackedBarcode trackedBarcode)
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
