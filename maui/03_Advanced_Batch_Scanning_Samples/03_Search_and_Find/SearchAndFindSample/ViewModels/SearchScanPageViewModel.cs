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

using Scandit.DataCapture.Barcode.Capture;
using Scandit.DataCapture.Barcode.Data;
using Scandit.DataCapture.Core.Capture;
using Scandit.DataCapture.Core.Source;

namespace SearchAndFindSample.ViewModels;

public class SearchScanPageViewModel : BaseViewModel
{
    private readonly DataCaptureManager dataCaptureManager = DataCaptureManager.Instance;

    public DataCaptureContext DataCaptureContext { get; } = DataCaptureManager.Instance.DataCaptureContext;

    public BarcodeCapture BarcodeCapture { get; } = SearchDataCaptureManager.Instance.BarcodeCapture;

    public event EventHandler<BarcodeCodeEventArgs>? CodeScanned;
    public Barcode? LastScannedBarcode { get; private set; }

    public SearchScanPageViewModel()
    {
        // Subscribe to BarcodeScanned event to get informed of captured barcodes.
        this.BarcodeCapture.BarcodeScanned += BarcodeScanned;
    }

    public void ResetBarcode()
    {
        this.LastScannedBarcode = null;
    }

    public void DisableMode()
    {
        this.IsActive = false;
    }

    public void EnableMode()
    {
        this.IsActive = true;
    }

    public override async Task ResumeAsync()
    {
        var permissionStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();

        if (permissionStatus != PermissionStatus.Granted)
        {
            permissionStatus = await Permissions.RequestAsync<Permissions.Camera>();

            if (permissionStatus == PermissionStatus.Granted)
            {
                this.ResumeScanning();
            }
        }
        else
        {
            this.ResumeScanning();
        }
    }

    public override async Task SleepAsync()
    {
        this.DataCaptureContext.RemoveMode(this.BarcodeCapture);
        this.BarcodeCapture.Enabled = false;

        if (this.dataCaptureManager.Camera != null)
        {
            await this.dataCaptureManager.Camera.SwitchToDesiredStateAsync(FrameSourceState.Off);
        }
    }

    private void ResumeScanning()
    {
        // Make sure barcode capture is the only mode associated with the context.
        this.DataCaptureContext.RemoveAllModes();
        this.DataCaptureContext.AddMode(this.BarcodeCapture);
        this.dataCaptureManager.Camera?.ApplySettingsAsync(BarcodeCapture.RecommendedCameraSettings);
        this.dataCaptureManager.Camera?.SwitchToDesiredStateAsync(FrameSourceState.On);
        this.BarcodeCapture.Enabled = true;
    }

    private void BarcodeScanned(object? sender, BarcodeCaptureEventArgs args)
    {
        if (args.Session.NewlyRecognizedBarcode == null)
        {
            return;
        }
        
        this.LastScannedBarcode = args.Session.NewlyRecognizedBarcode;

        // This method is invoked on a non-UI thread, so in order to perform UI work,
        // we have to switch to the main thread.
        Application.Current?.Dispatcher.DispatchAsync(() =>
        {
            this.CodeScanned?.Invoke(this, new BarcodeCodeEventArgs(this.LastScannedBarcode));
        });
    }
}
