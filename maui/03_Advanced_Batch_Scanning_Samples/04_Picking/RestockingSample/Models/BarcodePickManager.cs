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

using BarcodePickView = Scandit.DataCapture.Barcode.Pick.UI.Maui.BarcodePickView;

namespace RestockingSample.Models;

/// <summary>
/// Helper class to create BarcodePick and BarcodePickView.
/// </summary>
public class BarcodePickManager : IBarcodePickAsyncMapperProductProviderCallback
{
    private static readonly Lazy<BarcodePickManager> _instance = 
        new(valueFactory: () => new BarcodePickManager(), isThreadSafe: true);

    private readonly DataCaptureContext _dataCaptureContext;
    private BarcodePickViewSettings _viewSettings = null!;
    private BarcodePickSettings _settings = null!;

    public static BarcodePickManager Instance => _instance.Value;

    public DataCaptureContext DataCaptureContext => _dataCaptureContext;
    public BarcodePickViewSettings ViewSettings => _viewSettings;

    private BarcodePickManager() 
    { 
        _dataCaptureContext = DataCaptureContext.ForLicenseKey(App.SCANDIT_LICENSE_KEY);
    }


    public BarcodePickView CreateBarcodePickView(BarcodePick barcodePick) 
    {
        // Initialize the view settings. While we maintain the default settings here, you have the option to customize 
        // them according to your requirements, such as specifying hints to display, toggling guidelines, and more.
        _viewSettings = new BarcodePickViewSettings();

        // We finally create the view.
        return new BarcodePickView(_dataCaptureContext, barcodePick, _viewSettings);
    }

    public BarcodePick CreateBarcodePick() 
    {
        _dataCaptureContext.RemoveAllModes();

        // Create settings and enable the symbologies we want to scan.
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
