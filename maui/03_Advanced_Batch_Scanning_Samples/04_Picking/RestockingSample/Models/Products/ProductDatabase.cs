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
 
namespace RestockingSample.Models.Products;

/// <summary>
/// Simulated product repository. In a production environment, this would typically be replaced with an API call to 
/// retrieve product listings, supplemented by a local database for resilience and offline access.
/// </summary>
public class ProductDatabase
{
    private readonly List<Product> _products = new();

    public IReadOnlyCollection<Product> Products => _products;

    public ProductDatabase(params Product[] products)
    {
        if (products != null)
        {
            foreach (var p in products)
            {
                _products.Add(p);
            }
        }
    }

    public Product? GetProductForItemData(string itemData)
    {
        // Verify data against the local database or API.
        return _products.Find(p => p.BarcodeData.Contains(itemData));
    }

    public bool VerifyProductInDatabase(string itemData)
    {
        // Verify data against the local database or API.
        return _products.Any(p => p.BarcodeData.Contains(itemData)); ;
    }
}
