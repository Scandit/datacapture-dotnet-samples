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

using Scandit.DataCapture.Barcode.Data;
using Scandit.DataCapture.Barcode.Selection.Capture;
using Scandit.DataCapture.Core.Capture;
using Scandit.DataCapture.Core.Source;

namespace BarcodeSelectionSimpleSample.Models;

public class DataCaptureManager
{
    // Enter your Scandit License key here.
    // Your Scandit License key is available via your Scandit SDK web account.
    public const string SCANDIT_LICENSE_KEY = "-- ENTER YOUR SCANDIT LICENSE KEY HERE --";

    private static readonly Lazy<DataCaptureManager> instance = new(
        () => new DataCaptureManager(), LazyThreadSafetyMode.PublicationOnly);

    public static DataCaptureManager Instance => instance.Value;

    private DataCaptureManager()
    {
        this.CurrentCamera?.ApplySettingsAsync(this.CameraSettings);

        // Create data capture context using your license key and set the camera as the frame source.
        this.DataCaptureContext = DataCaptureContext.ForLicenseKey(SCANDIT_LICENSE_KEY);
        this.DataCaptureContext.SetFrameSourceAsync(this.CurrentCamera);

        // The barcode selection process is configured through barcode selection settings
        // which are then applied to the barcode selection instance that manages barcode recognition.
        this.BarcodeSelectionSettings = BarcodeSelectionSettings.Create();

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
            Symbology.Code128
        ];

        this.BarcodeSelectionSettings.EnableSymbologies(symbologies);
        this.BarcodeSelection = BarcodeSelection.Create(this.DataCaptureContext, this.BarcodeSelectionSettings);
    }

    #region DataCaptureContext
    public DataCaptureContext DataCaptureContext { get; private set; }
    #endregion

    #region CamerSettings
    public Camera? CurrentCamera { get; } = Camera.GetCamera(CameraPosition.WorldFacing);
    public CameraSettings CameraSettings { get; } = BarcodeSelection.RecommendedCameraSettings;
    #endregion

    #region BarcodeSelection
    public BarcodeSelection BarcodeSelection { get; private set; }
    public BarcodeSelectionSettings BarcodeSelectionSettings { get; private set; }
    #endregion
}
