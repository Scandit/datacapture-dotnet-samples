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


using Android.OS;
using AndroidX.Lifecycle;
using Scandit.DataCapture.Barcode.Capture;
using Scandit.DataCapture.Barcode.Data;
using Scandit.DataCapture.Core.Capture;
using Scandit.DataCapture.Core.Source;

namespace SearchAndFindSample.Search;

public class SearchScanViewModel : ViewModel
{
    private readonly DataCaptureManager dataCaptureManager = DataCaptureManager.Instance;
    private Barcode? lastScannedBarcode;
    private Handler mainHandler = new Handler(Looper.MainLooper!);

    public BarcodeCapture BarcodeCapture { get; } = SearchDataCaptureManager.Instance.BarcodeCapture;
    public DataCaptureContext DataCaptureContext { get; } = DataCaptureManager.Instance.DataCaptureContext;
    public event EventHandler<BarcodeCodeEventArgs>? CodeScanned;
    public Action<Barcode>? FindBarcode;

    public SearchScanViewModel()
    {
        // Subscribe to BarcodeScanned event to get informed of captured barcodes.
        this.BarcodeCapture.BarcodeScanned += BarcodeScanned;
    }

    public void ResumeScanning()
    {
        this.dataCaptureManager.Camera?.ApplySettingsAsync(BarcodeCapture.RecommendedCameraSettings);
        this.dataCaptureManager.Camera?.SwitchToDesiredStateAsync(FrameSourceState.On);
        this.DataCaptureContext.SetMode(this.BarcodeCapture);
        this.BarcodeCapture.Enabled = true;
    }

    public void PauseScanning()
    {
        this.DataCaptureContext.RemoveCurrentMode();
        this.dataCaptureManager.Camera?.SwitchToDesiredStateAsync(FrameSourceState.Off);
        this.BarcodeCapture.Enabled = false;
    }

    public void OnSearchClicked()
    {
        lock (this)
        {
            if (this.lastScannedBarcode != null)
            {
                this.FindBarcode?.Invoke(lastScannedBarcode);
                this.ResetCode();
            }
        }
    }

    protected override void OnCleared()
    {
        base.OnCleared();

        // Unsubscribe from barcode caputre events when clearing the ViewModel.
        this.BarcodeCapture.BarcodeScanned -= BarcodeScanned;
    }

    private void BarcodeScanned(object? sender, BarcodeCaptureEventArgs args)
    {
        lock (this)
        {
            if (args.Session.NewlyRecognizedBarcode != null)
            {
                this.lastScannedBarcode = args.Session.NewlyRecognizedBarcode;
                string code = this.lastScannedBarcode.Data ?? string.Empty;

                // This method is invoked on a non-UI thread, so in order to perform UI work,
                // we have to switch to the main thread.
                this.NotifyListenerOnMainThread(code);
            }
        }
    }

    private void ResetCode()
    {
        lock (this)
        {
            this.lastScannedBarcode = null;
        }
    }

    private void NotifyListenerOnMainThread(string code)
    {
        this.mainHandler.Post(() => this.CodeScanned?.Invoke(this, new BarcodeCodeEventArgs(code)));
    }
}
