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

namespace RestockingSample.ViewModels;

public class ResultListViewModel
{
    public IList<DisplayProduct> ScanResults { get; }
    public IList<DisplayProduct> PickedResults { get; }

    public bool IsPicklistVisible => this.PickedResults.Count > 0;
    public bool IsScanResultsVisible => this.ScanResults.Count > 0;
    public string InventoryLabel => $"Inventory ({this.ScanResults.Count})";

    public ResultListViewModel(IList<DisplayProduct> pickedResults, IList<DisplayProduct> scanResults)
    {
        PickedResults = pickedResults ?? new List<DisplayProduct>();
        ScanResults = scanResults ?? new List<DisplayProduct>();

        foreach (var pickedItem in PickedResults)
        {
            if (!pickedItem.InList)
            {
                pickedItem.ItemImageSource = ImageSource.FromFile(file: "not_in_list");
            }
            else if (pickedItem.Picked)
            {
                pickedItem.ItemImageSource = ImageSource.FromFile(file: "picked");
            }
        }
    }
}
