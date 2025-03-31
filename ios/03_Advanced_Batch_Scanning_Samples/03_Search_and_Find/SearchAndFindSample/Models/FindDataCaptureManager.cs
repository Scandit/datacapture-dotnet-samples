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

namespace SearchAndFindSample.Models;

public class FindDataCaptureManager
{
    private static readonly Lazy<FindDataCaptureManager> instance = new Lazy<FindDataCaptureManager>(
        () => new FindDataCaptureManager(), LazyThreadSafetyMode.PublicationOnly);

    public static FindDataCaptureManager Instance => instance.Value;

    public BarcodeFind BarcodeFind { get; init; }

    private FindDataCaptureManager()
    {
        // Create new barcode find mode with default settings.
        this.BarcodeFind = new BarcodeFind(new BarcodeFindSettings());
    }

    public void SetupForSymbology(Symbology symbology)
    {
        // The barcode find process is configured through barcode find settings
        // which are then applied to the barcode find instance.
        var settings = new BarcodeFindSettings();

        // We enable only the given symbology.
        settings.EnableSymbology(symbology, true);

        // We apply the new settings to the barcode find.
        this.BarcodeFind.ApplySettingsAsync(settings);
    }

    public void SetupSearchedItems(string data)
    {
        // Create the Barcode Find Item with the scanned barcode data.
        // If you have more information about the product (such as name or image) that you want
        // to display, you can pass a BarcodeFindItemContent object to the content parameter below.

        ICollection<BarcodeFindItem> items = new HashSet<BarcodeFindItem>();
        items.Add(new BarcodeFindItem(new BarcodeFindItemSearchOptions(data), content: null));

        // The BarcodeFind can search a set of items. In this simplified sample, we set only
        // one items, but for real case we can set several at once.
        this.BarcodeFind.SetItemList(items);
    }
}
