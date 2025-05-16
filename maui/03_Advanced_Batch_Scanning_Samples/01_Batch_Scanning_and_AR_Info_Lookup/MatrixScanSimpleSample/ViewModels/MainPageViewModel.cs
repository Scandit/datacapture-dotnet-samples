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

using Microsoft.Maui.Platform;
using MatrixScanSimpleSample.Models;

using Scandit.DataCapture.Barcode.Data;
using Scandit.DataCapture.Barcode.Batch.Capture;
using Scandit.DataCapture.Barcode.Batch.Data;
using Scandit.DataCapture.Barcode.Batch.UI.Overlay;
using Scandit.DataCapture.Core.Capture;
using Scandit.DataCapture.Core.Data;
using Scandit.DataCapture.Core.Source;
using Brush = Scandit.DataCapture.Core.UI.Style.Brush;

namespace MatrixScanSimpleSample.ViewModels;

public class MainPageViewModel : BaseViewModel, IBarcodeBatchListener, IBarcodeBatchBasicOverlayListener
{
    private readonly Brush highlightedBrush;
    private readonly HashSet<ScanResult> scanResults = [];

    public Camera Camera { get; private set; } = DataCaptureManager.Instance.CurrentCamera;
    public DataCaptureContext DataCaptureContext { get; private set; } = DataCaptureManager.Instance.DataCaptureContext;
    public BarcodeBatch BarcodeBatch { get; private set; } = DataCaptureManager.Instance.BarcodeBatch;

    public IEnumerable<ScanResult> ScanResults { get => this.scanResults; }

    public MainPageViewModel()
    {
        this.InitializeScanner();
        this.highlightedBrush = new Brush(Colors.Transparent.ToPlatform(), Colors.White.ToPlatform(), 2.0f);
    }

    public override async Task SleepAsync()
    {
        // Switch camera off to stop streaming frames.
        // The camera is stopped asynchronously and will take some time to completely turn off.
        // Until it is completely stopped, it is still possible to receive further results, hence
        // it's a good idea to first disable barcode batch as well.
        this.BarcodeBatch.Enabled = false;

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

    private void InitializeScanner()
    {
        // Register self as a listener to get informed whenever a new barcode got recognized.
        this.BarcodeBatch.AddListener(this);
    }

    private async Task<bool> ResumeFrameSourceAsync()
    {
        this.scanResults.Clear();
        this.BarcodeBatch.Enabled = true;

        if (this.Camera != null)
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
        lock (this.scanResults)
        {
            foreach (var trackedBarcode in session.AddedTrackedBarcodes)
            {
                var description = new SymbologyDescription(trackedBarcode.Barcode.Symbology);
                this.scanResults.Add(new ScanResult(trackedBarcode.Identifier)
                {
                    Data = trackedBarcode.Barcode.Data ?? string.Empty,
                    Symbology = description.ReadableName
                });
            }

            foreach (int removedTrackedBarcodeId in session.RemovedTrackedBarcodes)
            {
                this.scanResults.RemoveWhere(t => t.Id == removedTrackedBarcodeId);
            }
        }
    }
    #endregion

    #region IBarcodeBatchBasicOverlayListener
    public Brush BrushForTrackedBarcode(BarcodeBatchBasicOverlay overlay, TrackedBarcode trackedBarcode)
    {
        return this.highlightedBrush;
    }

    public void OnTrackedBarcodeTapped(BarcodeBatchBasicOverlay overlay, TrackedBarcode trackedBarcode)
    { }
    #endregion
}
