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

using RestockingSample.Result;
using Scandit.DataCapture.Barcode.Pick.Capture;
using Scandit.DataCapture.Barcode.Pick.Data;

namespace RestockingSample.Products;

/// <summary>
/// Helper class to query the list of products and to maintain the list of items that are picked and scanned.
/// A real application would implement a database and network call, and we simulate it here by introducing some 
/// artificial delay.
/// </summary>
public class ProductManager
{
    private static readonly Lazy<ProductManager> _instance =
        new(valueFactory: () => new ProductManager(), isThreadSafe: true);

    private readonly ProductDatabase _database;
    private readonly HashSet<string> _allScannedCodes = new();
    private readonly HashSet<string> _allPickedCodes = new();

    public static ProductManager Instance => _instance.Value;

    private ProductManager()
    {
        _database = new ProductDatabase(
            // Add your Products here:
            new Product("Item A", 1, "8414792869912", "3714711193285", "4951107312342", "1520070879331"),
            new Product("Item B", 1, "1318890267144", "9866064348233", "4782150677788", "2371266523793"),
            new Product("Item C", 1, "5984430889778", "7611879254123"));
    }

    /// <summary>
    /// Creates the list of BarcodePickProduct that we need to pick.
    /// </summary>
    /// <returns>A list of items that can directly be ingested when creating our BarcodePick object.</returns>
    public ICollection<BarcodePickProduct> GetBarcodePickProducts()
    {
        HashSet<BarcodePickProduct> pickProducts = new();

        foreach (Product product in _database.Products)
        {
            pickProducts.Add(new BarcodePickProduct(product.Identifier, product.QuantityToPick));
        }

        return pickProducts;
    }

    /// <summary>
    /// Performs an asynchronous comparison of scanned barcode data against product records, updating the UI to reflect 
    /// whether the item is being searched for or not.
    /// </summary>
    /// <param name="itemsData">The list of barcode to match against.</param>
    /// <param name="callback">The callback to call with the results.</param>
    public Task ConvertBarcodesToCallbackItemsAsync(
        IList<string> itemsData, BarcodePickProductProviderCallback callback)
    {
        return Task.Run(() =>
        {
            callback.OnData(ConvertBarcodesToCallbackItems(itemsData));
        });
    }

    /// <summary>
    /// Flags the product as picked and introduces an artificial delay to emulate network latency.
    /// </summary>
    /// <param name="itemData">The barcode content of the item that was tapped in the BarcodePick UI.</param>
    /// <param name="callback">The callback to call with the results.</param>
    public Task PickItemAsync(string itemData, BarcodePickActionCallback callback)
    {
        return Task.Run(async () =>
        {
            try
            {
                var rnd = new Random();
                int delay = rnd.Next(250, 751); // Generates a random number between 250 and 750
                await Task.Delay(delay);
                callback.OnFinish(result: PickItem(itemData));
            }
            catch (Exception)
            {
                callback.OnFinish(result: false);
            }
        });
    }

    /// <summary>
    /// Unflags the product as picked and introduces an artificial delay to emulate network latency.
    /// </summary>
    /// <param name="itemData">The barcode content of the item that was tapped in the BarcodePick UI.</param>
    /// <param name="callback">The callback to call with the results.</param>
    public Task UnpickItemAsync(string itemData, BarcodePickActionCallback callback)
    {
        return Task.Run(async () =>
        {
            try
            {
                var rnd = new Random();
                int delay = rnd.Next(250, 751); // Generates a random number between 250 and 750
                await Task.Delay(delay);
                callback.OnFinish(result: UnpickItem(itemData));
            }
            catch (Exception)
            {
                callback.OnFinish(result: false);
            }
        });
    }

    /// <summary>
    /// Returns the list of what was picked and scanned with section titles.
    /// </summary>
    public IList<DisplayItem> GetPickResultContent()
    {
        IList<DisplayItem> pickList = BuildPickedProductsListAndSetPickedProductsCount();
        IList<DisplayItem> inventoryList = GetInventoryList();

        var returnList = pickList.Concat(inventoryList).ToList();

        return returnList;
    }

    /// <summary>
    /// Returns a list of all products that are picked.
    /// </summary>
    private IList<DisplayItem> BuildPickedProductsListAndSetPickedProductsCount()
    {
        List<DisplayItem> displayItems = new() { new DisplayHeader(true, _database.Products.Count) };
        List<Product> pickedProducts = new();

        lock (_allPickedCodes)
        {
            foreach (string itemData in _allPickedCodes)
            {
                Product? product = _database.GetProductForItemData(itemData);

                if (product != null)
                {
                    pickedProducts.Add(product);
                    int numberOfProductsInList = pickedProducts.Count(p => p.Equals(product));

                    // Display a checkmark next to products in the result list only if their count 
                    // does not exceed the required quantity to pick.
                    bool picked = numberOfProductsInList <= product.QuantityToPick;
                    displayItems.Add(product.GetDisplayProduct(itemData, picked));
                }
                else
                {
                    // No product matching itemData was found.
                    displayItems.Add(new DisplayProduct("Unknown", 0, itemData, true));
                }
            }
        }

        return displayItems;
    }

    /// <summary>
    /// Returns a list of scanned and not picked items.
    /// </summary>
    private IList<DisplayItem> GetInventoryList()
    {
        // Inventory (all the barcodes scanned, without the ones picked)
        var inventory = _allScannedCodes.Except(_allPickedCodes);

        List<DisplayItem> scannedItems = new() { new DisplayHeader(false, inventory.Count()) };

        foreach (string itemData in inventory)
        {
            Product? product = _database.GetProductForItemData(itemData);
            DisplayProduct displayProduct =
                product?.GetDisplayProduct(itemData, false) ?? new DisplayProduct("Unknown", 0, itemData, false);

            scannedItems.Add(displayProduct);
        }

        return scannedItems;
    }

    public void SetAllScannedCodes(ICollection<string> scannedCodes)
    {
        _allScannedCodes.Clear();

        foreach (var scannedCode in scannedCodes)
        {
            _allScannedCodes.Add(scannedCode);
        }
    }

    public void SetAllPickedCodes(ICollection<string> pickedCodes)
    {
        _allPickedCodes.Clear();

        foreach (var pickedCode in pickedCodes)
        {
            _allPickedCodes.Add(pickedCode);
        }
    }

    /// <summary>
    /// Clear all picked and scanned codes so we can start the sample over again.
    /// </summary>
    public void ClearPickedAndScanned()
    {
        _allPickedCodes.Clear();
        _allScannedCodes.Clear();
    }

    private IList<BarcodePickProductProviderCallbackItem> ConvertBarcodesToCallbackItems(IList<string> itemsData)
    {
        List<BarcodePickProductProviderCallbackItem> callbackItems = new();

        foreach (string itemData in itemsData)
        {
            Product? product = _database.GetProductForItemData(itemData);

            if (product != null)
            {
                callbackItems.Add(new BarcodePickProductProviderCallbackItem(itemData, product.Identifier));
            }
            else
            {
                callbackItems.Add(new BarcodePickProductProviderCallbackItem(itemData, null));
            }
        }

        return callbackItems;
    }

    private bool PickItem(string itemData)
    {
        // In a real-world application, consider invoking a database method
        // to verify if the product represented by 'itemData' is valid.
        // Example: return _database.VerifyProductInDatabase(itemData);

        // For now, let's assume the product is always valid.
        return true;
    }

    private bool UnpickItem(string itemData)
    {
        // In a real-world application, consider invoking a database method
        // to verify if the product represented by 'itemData' is valid.
        // Example: return _database.VerifyProductInDatabase(itemData);

        // For now, let's assume the product is always valid.
        return true;
    }
}
