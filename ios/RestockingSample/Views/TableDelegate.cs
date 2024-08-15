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

public class TableDelegate : UITableViewDelegate
{
    private readonly TableSource _source;

    public TableDelegate(TableSource source)
    {
        _source = source;
    }

    public override UIView GetViewForHeader(UITableView tableView, nint section)
    {
        var view = tableView.DequeueReusableHeaderFooterView(ResultListSectionHeaderView.Identifier)
            as ResultListSectionHeaderView ?? throw new InvalidOperationException("Cannot retrieve view header");
        var items = _source.GetItemsForSection((int)section);
        view.Configure(section, items ?? new List<DisplayProduct>());

        return view;
    }

    public override nfloat GetHeightForHeader(UITableView tableView, nint section)
    {
        return UITableView.AutomaticDimension;
    }
}
