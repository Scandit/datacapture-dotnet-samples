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

using Scandit.DataCapture.Barcode.Count.Capture;
using Scandit.DataCapture.Barcode.Data;
using Scandit.DataCapture.Core.Capture;

namespace MatrixScanCountSimpleSample.ViewModels;

public class BarcodeCountPageViewModel : BaseViewModel
{
    private DataCaptureContext dataCaptureContext;
    private BarcodeCount barcodeCount;
    private BarcodeCountSettings barcodeCountSettings;

    public BarcodeCountPageViewModel()
    {
        this.InitializeScanner();
    }

    public DataCaptureContext DataCaptureContext => this.dataCaptureContext;
    public BarcodeCount BarcodeCount => this.barcodeCount;

    public void PauseScanning(bool navigatingInternally)
    {
        // Pause camera if the app is going to background,
        // but keep it on if it goes to result screen.
        // That way the session is not lost when coming back from results.
        if (!navigatingInternally)
        {
            CameraManager.Instance.PauseFrameSource();

            // Save current barcodes as additional barcodes.
            BarcodeManager.Instance.SaveCurrentBarcodesAsAdditionalBarcodes();
        }
    }

    public override Task SleepAsync()
    {
        this.PauseScanning(navigatingInternally: false);
        return Task.CompletedTask;
    }

    public override async Task ResumeAsync()
    {
        var permissionStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();

        if (permissionStatus != PermissionStatus.Granted)
        {
            permissionStatus = await Permissions.RequestAsync<Permissions.Camera>();
            if (permissionStatus == PermissionStatus.Granted)
            {
                this.ResumeFrameSource();
            }
        }
        else
        {
            this.ResumeFrameSource();
        }
    }

    public void ResetSession()
    {
        BarcodeManager.Instance.Reset();
        this.barcodeCount.ClearAdditionalBarcodes();
        this.barcodeCount.Reset();
    }

    private void InitializeScanner()
    {
        // Create data capture context using your license key.
        this.dataCaptureContext = DataCaptureContext.ForLicenseKey(App.SCANDIT_LICENSE_KEY);

        // Initialize the shared camera manager.
        CameraManager.Instance.Initialize(this.dataCaptureContext);

        // The barcode count process is configured through barcode count settings
        // which are then applied to the barcode count instance that manages barcode count.
        this.barcodeCountSettings = new BarcodeCountSettings();

        // The settings instance initially has all types of barcodes (symbologies) disabled.
        // For the purpose of this sample we enable a very generous set of symbologies.
        // In your own app ensure that you only enable the symbologies that your app requires
        // as every additional enabled symbology has an impact on processing times.
        HashSet<Symbology> symbologies =
        [
            Symbology.Ean13Upca,
            Symbology.Ean8,
            Symbology.Upce,
            Symbology.Code39,
            Symbology.Code128,
        ];
        this.barcodeCountSettings.EnableSymbologies(symbologies);

        // Create barcode count and attach to context.
        this.barcodeCount = BarcodeCount.Create(this.dataCaptureContext, this.barcodeCountSettings);

        // Subscribe for barcode count events.
        this.barcodeCount.Scanned += this.BarcodeCountScanned;

        // Initialize the shared barcode manager.
        BarcodeManager.Instance.Initialize(this.barcodeCount);
    }

    private void ResumeFrameSource()
    {
        // Enable the mode to start processing frames.
        this.barcodeCount.Enabled = true;

        CameraManager.Instance.ResumeFrameSource();
    }

    private void BarcodeCountScanned(object? sender, BarcodeCountEventArgs args)
    {
        BarcodeManager.Instance.UpdateWithSession(args.Session);
    }
}
