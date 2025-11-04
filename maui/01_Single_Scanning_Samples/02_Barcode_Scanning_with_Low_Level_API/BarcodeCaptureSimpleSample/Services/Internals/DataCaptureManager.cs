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

namespace BarcodeCaptureSimpleSample.Services.Internals;

internal class DataCaptureManager : IDataCaptureManager
{
    public DataCaptureContext DataCaptureContext { get; }

    // Use the recommended camera settings for the BarcodeCapture mode.
    public Camera? CurrentCamera { get; } = Camera.GetCamera(CameraPosition.WorldFacing);
    public CameraSettings CameraSettings { get; } = BarcodeCapture.RecommendedCameraSettings;

    // Configure the barcode capture process.
    public BarcodeCapture BarcodeCapture { get; }
    public BarcodeCaptureSettings BarcodeCaptureSettings { get; }

    public DataCaptureManager()
    {
        this.CurrentCamera?.ApplySettingsAsync(this.CameraSettings);

        this.DataCaptureContext = DataCaptureContext.ForLicenseKey(App.SCANDIT_LICENSE_KEY);
        this.DataCaptureContext.SetFrameSourceAsync(this.CurrentCamera);

        // The barcode capturing process is configured through barcode capture settings
        // which are then applied to the barcode capture instance that manages barcode recognition.
        this.BarcodeCaptureSettings = BarcodeCaptureSettings.Create();

        // The settings instance initially has all types of barcodes (symbologies) disabled.
        // For the purpose of this sample we enable a very generous set of symbologies.
        // In your own app ensure that you only enable the symbologies that your app requires as
        // every additional enabled symbology has an impact on processing times.
        HashSet<Symbology> symbologies =
        [
            Symbology.Ean13Upca,
            Symbology.Ean8,
            Symbology.Upce,
            Symbology.Qr,
            Symbology.DataMatrix,
            Symbology.Code39,
            Symbology.Code128,
            Symbology.InterleavedTwoOfFive
        ];

        this.BarcodeCaptureSettings.EnableSymbologies(symbologies);
        this.BarcodeCapture = BarcodeCapture.Create(this.DataCaptureContext, this.BarcodeCaptureSettings);

        // By default, every time a barcode is scanned, a sound (if not in silent mode) and a vibration are played.
        // Uncomment the following line to set a success feedback without sound and vibration.
        // this.BarcodeCapture.Feedback.Success = new Feedback(vibration: null, sound: null);
    }
}
