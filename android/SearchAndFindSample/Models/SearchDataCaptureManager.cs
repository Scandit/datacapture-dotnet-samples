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
using Scandit.DataCapture.Core.Area;
using Scandit.DataCapture.Core.Capture;
using Scandit.DataCapture.Core.Common.Geometry;

namespace SearchAndFindSample;

public class SearchDataCaptureManager
{
    private static readonly Lazy<SearchDataCaptureManager> instance = new Lazy<SearchDataCaptureManager>(
        () => new SearchDataCaptureManager(), LazyThreadSafetyMode.PublicationOnly);

    public static SearchDataCaptureManager Instance => instance.Value;

    public BarcodeCapture BarcodeCapture { get; init; }

    private SearchDataCaptureManager()
    {
        // The barcode capturing process is configured through barcode capture settings
        // which are then applied to the barcode capture instance that manages barcode recognition.
        BarcodeCaptureSettings settings = BarcodeCaptureSettings.Create();
        HashSet<Symbology> enabledSymbologies = new HashSet<Symbology>()
        {
            Symbology.Ean8,
            Symbology.Ean13Upca,
            Symbology.Upce,
            Symbology.Code128,
            Symbology.Code39,
            Symbology.DataMatrix
        };
        settings.EnableSymbologies(enabledSymbologies);

        // In order not to pick up barcodes outside of the view finder,
        // restrict the code location selection to match the laser line's center.
        settings.LocationSelection = RadiusLocationSelection.Create(new FloatWithUnit(0f, MeasureUnit.Fraction));

        // Setting the code duplicate filter to one second means that the scanner won't report
        // the same code as recognized for one second once it's recognized.
        settings.CodeDuplicateFilter = TimeSpan.FromSeconds(1.0d);

        var dataCaptureContext = DataCaptureManager.Instance.DataCaptureContext;

        // Create new barcode capture mode with the settings from above.
        this.BarcodeCapture = BarcodeCapture.Create(dataCaptureContext, settings);
        dataCaptureContext.RemoveMode(this.BarcodeCapture);
    }
}
