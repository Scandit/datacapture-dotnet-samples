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

using RestockingSample.Models;

namespace RestockingSample.Views;

public class TableSource : UITableViewSource
{
    private const int numberOfSections = 2;

    private readonly IList<DisplayProduct> _pickedItems;
    private readonly IList<DisplayProduct> _inventoryItems;

    public TableSource(IList<DisplayProduct> pickedItems, IList<DisplayProduct> inventoryItems)
    {
        ArgumentNullException.ThrowIfNull(pickedItems, nameof(pickedItems));
        ArgumentNullException.ThrowIfNull(inventoryItems, nameof(inventoryItems));

        _pickedItems = pickedItems;
        _inventoryItems = inventoryItems;
    }

    public override nint NumberOfSections(UITableView tableView)
    {
        return numberOfSections;
    }

    public override nint RowsInSection(UITableView tableView, nint section)
    {
        return section switch
        {
            0 => _pickedItems.Count,
            1 => _inventoryItems.Count,
            _ => 0
        };
    }

    public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
    {
        ResultListCell cell =
            tableView.DequeueReusableCell(ResultListCell.Identifier, indexPath) as ResultListCell ??
            throw new InvalidOperationException("Cannot retrieve view cell");

        if (GetItemAtIndexPath(indexPath) is not DisplayProduct item)
        {
            return cell;
        }

        cell.Configure(item, indexPath);
        return cell;
    }

    public IList<DisplayProduct>? GetItemsForSection(int section)
    {
        return section switch
        {
            0 => _pickedItems,
            1 => _inventoryItems,
            _ => null,
        };
    }

    public DisplayProduct? GetItemAtIndexPath(NSIndexPath indexPath)
    {
        var itemArray = GetItemsForSection(indexPath.Section) ?? null;
        return itemArray?[indexPath.Row];
    }
}
