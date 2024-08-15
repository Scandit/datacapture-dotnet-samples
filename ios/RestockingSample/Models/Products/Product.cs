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
/// A simple product representation for demo purposes.
/// </summary>
public class Product : IEquatable<Product>
{
    /// <summary>
    /// Identifier, must be unique per product.
    /// </summary>
    public string Identifier { get; init; }

    public int QuantityToPick { get; init; }

    /// <summary>
    /// The content of the barcode that matches the product. Multiple barcode can point to the same product.
    /// </summary>
    public IList<string> BarcodeData { get; init; }

    public Product(string identifier, int quantityToPick, params string[] barcodeData)
    {
        ArgumentNullException.ThrowIfNull(identifier, nameof(identifier));

        Identifier = identifier;
        QuantityToPick = quantityToPick;
        BarcodeData = barcodeData?.ToList() ?? new List<string>();
    }

    public DisplayProduct GetDisplayProduct(string itemData, bool picked)
    {
        return new DisplayProduct(Identifier, QuantityToPick, itemData, picked);
    }

    public bool Equals(Product? other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (other == null)
        {
            return false;
        }

        return Identifier == other.Identifier && QuantityToPick == other.QuantityToPick;
    }

    public override bool Equals(object? obj)
    {
        return base.Equals(obj as Product);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Identifier, QuantityToPick);
    }
}
