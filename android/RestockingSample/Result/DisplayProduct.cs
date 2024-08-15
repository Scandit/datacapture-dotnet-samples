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

namespace RestockingSample.Result;

/// <summary>
/// Serves as the base class for items to be displayed.
/// </summary>
public abstract class DisplayItem
{ }

/// <summary>
/// Represents a header to separate results between picked and not yet picked items.
/// </summary>
public class DisplayHeader : DisplayItem
{
    public bool PickList { get; init; }
    public int Total { get; init; }

    public DisplayHeader(bool pickList, int total)
    {
        PickList = pickList;
        Total = total;
    }
}

/// <summary>
/// Represents a product to be displayed, including its details and state.
/// </summary>
public class DisplayProduct : DisplayItem
{
    public string Identifier { get; init; }

    public int QuantityToPick { get; init; }

    public string BarcodeData { get; init; }

    public bool Picked { get; init; }

    public DisplayProduct(string identifier, int quantityToPick, string barcodeData, bool picked)
    {
        ArgumentNullException.ThrowIfNull(identifier, nameof(identifier));
        ArgumentNullException.ThrowIfNull(barcodeData, nameof(barcodeData));

        Identifier = identifier;
        QuantityToPick = quantityToPick;
        BarcodeData = barcodeData;
        Picked = picked;
    }
}
