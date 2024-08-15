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

using RestockingSample.Models.Products;
using Scandit.DataCapture.Barcode.Data;
using Scandit.DataCapture.Barcode.Pick.Capture;
using Scandit.DataCapture.Barcode.Pick.Data;
using Scandit.DataCapture.Barcode.Pick.UI;
using Scandit.DataCapture.Core.Capture;

namespace RestockingSample;

/// <summary>
/// Helper class to create our BarcodePick and BarcodePickView.
/// </summary>
public class BarcodePickManager : IBarcodePickAsyncMapperProductProviderCallback
{
    private static readonly Lazy<BarcodePickManager> _instance = 
        new(valueFactory: () => new BarcodePickManager(), isThreadSafe: true);

    private readonly DataCaptureContext _dataCaptureContext;
    private BarcodePickViewSettings _viewSettings = null!;
    private BarcodePickSettings _settings = null!;

    public static BarcodePickManager Instance => _instance.Value;

    private BarcodePickManager() 
    { 
        _dataCaptureContext = DataCaptureContext.ForLicenseKey(Application.SCANDIT_LICENSE_KEY);
    }

    public BarcodePickView CreateBarcodePickView(CGRect frame, BarcodePick barcodePick) 
    {
        // We create the view settings.
        // We keep the default here, but you can use them to specify your own hints to display,
        // whether to show guidelines or not and so on.
        _viewSettings = new BarcodePickViewSettings();

        // We finally create the view.
        return BarcodePickView.Create(frame, _dataCaptureContext, barcodePick, _viewSettings);
    }

    public BarcodePick CreateBarcodePick() 
    {
        _dataCaptureContext.RemoveAllModes();

        // We first create settings and enable the symbologies we want to scan.
        _settings = new BarcodePickSettings();
        HashSet<Symbology> enabledSymbologies = new()
        {
            Symbology.Ean8,
            Symbology.Ean13Upca,
            Symbology.Upce,
            Symbology.Code128,
            Symbology.Code39
        };
        _settings.EnableSymbologies(enabledSymbologies);

        // Retrieve the list of desired products to pick.
        ICollection<BarcodePickProduct> products = ProductManager.Instance.GetBarcodePickProducts();

        // Initialize the product provider to correlate barcodes with their respective products.
        IBarcodePickProductProvider provider = new BarcodePickAsyncMapperProductProvider(products, this);

        // And finally create BarcodePick
        return new BarcodePick(_dataCaptureContext, _settings, provider);
    }

    #region IBarcodePickAsyncMapperProductProviderCallback
    /// <summary>
    /// Performs an asynchronous comparison of scanned barcode data against product records, updating the UI to reflect 
    /// whether the item is being searched for or not.
    /// </summary>
    /// <param name="itemsData">The list of barcode to match against.</param>
    /// <param name="callback">The callback to call with the results.</param>
    public void ProductIdentifierForItems(IList<string> itemsData, BarcodePickProductProviderCallback callback)
    {
        ProductManager.Instance.ConvertBarcodesToCallbackItemsAsync(itemsData, callback);
    }
    #endregion // IBarcodePickAsyncMapperProductProviderCallback
}
