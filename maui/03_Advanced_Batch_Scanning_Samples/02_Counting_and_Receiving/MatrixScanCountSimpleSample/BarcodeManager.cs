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

using MatrixScanCountSimpleSample.Models;
using Scandit.DataCapture.Barcode.Count.Capture;
using Scandit.DataCapture.Barcode.Data;

namespace MatrixScanCountSimpleSample;

public sealed class BarcodeManager
{
    private static readonly Lazy<BarcodeManager> instance = new(() =>
        new BarcodeManager(), LazyThreadSafetyMode.PublicationOnly);

    private List<Barcode>? scannedBarcodes;
    private List<Barcode>? additionalBarcodes;
    private BarcodeCount? barcodeCount;

    public static BarcodeManager Instance => instance.Value;

    private BarcodeManager()
    { }

    public void Initialize(BarcodeCount barcodeCount)
    {
        this.barcodeCount = barcodeCount ?? throw new ArgumentNullException(nameof(barcodeCount));
    }

    public void UpdateWithSession(BarcodeCountSession session)
    {
        // Update lists of barcodes with the contents of the current session.
        this.scannedBarcodes = [.. session.RecognizedBarcodes];
        this.additionalBarcodes = [.. session.AdditionalBarcodes];
    }

    public void SaveCurrentBarcodesAsAdditionalBarcodes()
    {
        if (this.scannedBarcodes != null && this.scannedBarcodes.Count != 0)
        {
            // Save any scanned barcodes as additional barcodes, so they're still scanned
            // after coming back from background.
            List<Barcode> barcodesToSave = [.. this.scannedBarcodes, .. additionalBarcodes ?? []];
            this.barcodeCount?.SetAdditionalBarcodes(barcodesToSave);
        }
    }

    public IDictionary<string, ScanItem> GetScanResults()
    {
        // Create a map of barcodes to be passed to the scan results page.
        var scanResults = new Dictionary<string, ScanItem>();

        void copyToResult(Barcode barcode)
        {
            if (barcode.Data == null)
            {
                return;
            }

            if (scanResults.TryGetValue(barcode.Data, out ScanItem? value))
            {
                value.IncreaseQuantity();
            }
            else
            {
                var description = new SymbologyDescription(barcode.Symbology);

                scanResults.Add(
                    barcode.Data,
                    new ScanItem(
                        barcode.Data,
                        description.ReadableName));
            }
        }

        if (this.scannedBarcodes != null && this.scannedBarcodes.Count != 0)
        {
            // Add the inner Barcode objects of each scanned Barcode to the results map.
            foreach (Barcode barcode in this.scannedBarcodes)
            {
                copyToResult(barcode);
            }
        }

        if (this.additionalBarcodes != null && this.additionalBarcodes.Count != 0)
        {
            // Add the previously saved Barcode objects to the results map.
            foreach (Barcode barcode in this.additionalBarcodes)
            {
                copyToResult(barcode);
            }
        }

        return scanResults;
    }

    public void Reset()
    {
        // Reset the barcodes lists.
        this.scannedBarcodes?.Clear();
        this.additionalBarcodes?.Clear();
    }
}
