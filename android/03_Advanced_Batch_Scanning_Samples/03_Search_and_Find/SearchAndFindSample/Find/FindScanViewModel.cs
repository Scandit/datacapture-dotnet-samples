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

using AndroidX.Lifecycle;
using Scandit.DataCapture.Barcode.Data;
using Scandit.DataCapture.Barcode.Find.Capture;
using Scandit.DataCapture.Core.Capture;

namespace SearchAndFindSample.Find;

public class FindScanViewModel : ViewModel
{
    private readonly FindDataCaptureManager findCaptureManager = FindDataCaptureManager.Instance;

    public DataCaptureContext DataCaptureContext { get; } = DataCaptureManager.Instance.DataCaptureContext;
    public BarcodeFind BarcodeFind { get; } = FindDataCaptureManager.Instance.BarcodeFind;

    public FindScanViewModel(Symbology symbology, string data)
    {
        // We change the barcode batch settings to enable only the previously scanned symbology.
        this.findCaptureManager.SetupForSymbology(symbology);

        // We setup the list of searched items.
        this.findCaptureManager.SetupSearchedItems(data);
    }
}
