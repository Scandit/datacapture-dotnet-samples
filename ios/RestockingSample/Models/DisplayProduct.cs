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

namespace RestockingSample.Models;

/// <summary>
/// Represents a product to be displayed, including its details and state.
/// </summary>
public class DisplayProduct
{
    public const string UnknownProduct = "Unknown";

    public string Identifier { get; init; }

    public int QuantityToPick { get; init; }

    public string BarcodeData { get; init; }

    public bool Picked { get; init; }

    public bool InList { get; init; }

    public DisplayProduct(string identifier, int quantityToPick, string barcodeData, bool picked)
    {
        ArgumentNullException.ThrowIfNull(identifier, nameof(identifier));
        ArgumentNullException.ThrowIfNull(barcodeData, nameof(barcodeData));

        Identifier = identifier;
        QuantityToPick = quantityToPick;
        BarcodeData = barcodeData;
        Picked = picked;
        InList = ProductManager.Instance.VerifyProduct(barcodeData);
    }
}
