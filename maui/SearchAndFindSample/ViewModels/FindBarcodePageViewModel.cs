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

using Scandit.DataCapture.Barcode.Data;
using Scandit.DataCapture.Barcode.Find.Capture;
using Scandit.DataCapture.Barcode.Find.UI;
using Scandit.DataCapture.Core.Capture;

namespace SearchAndFindSample.ViewModels;

public class FindBarcodePageViewModel : BaseViewModel
{
    private readonly DataCaptureManager dataCaptureManager = DataCaptureManager.Instance;
    private readonly FindDataCaptureManager findCaptureManager = FindDataCaptureManager.Instance;

    public DataCaptureContext DataCaptureContext => this.dataCaptureManager.DataCaptureContext;
    public BarcodeFind BarcodeFind => this.findCaptureManager.BarcodeFind;

    /// <summary>
    /// With the BarcodeFindViewSettings, we can defined haptic and sound feedback,
    /// as well as change the visual feedback for found barcodes.
    /// </summary>
    public BarcodeFindViewSettings ViewSettings { get; private set; }

    public FindBarcodePageViewModel(Symbology symbology, string data)
    {
        this.ViewSettings = new BarcodeFindViewSettings();

        // We change the barcode batch settings to enable only the previously scanned symbology.
        this.findCaptureManager.SetupForSymbology(symbology);

        // We setup the list of searched items.
        this.findCaptureManager.SetupSearchedItems(data);
    }

    public void DisableMode()
    {
        this.IsActive = false;
    }

    public void EnableMode()
    {
        this.IsActive = true;
    }

    public override Task ResumeAsync()
    {
        this.BarcodeFind.Start();
        return Task.CompletedTask;
    }

    public override Task SleepAsync()
    {
        this.BarcodeFind.Pause();
        return Task.CompletedTask;
    }
}
