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

using MatrixScanCountSimpleSample.Models;

namespace MatrixScanCountSimpleSample.Views
{
    public class ViewCellData
    {
        private int index;
        private ScanItem scanItem;

        public ViewCellData(ScanItem scanItem, int index)
        {
            this.scanItem = scanItem;
            this.index = index;
        }

        public string Detail => $"{this.scanItem.Symbology}: {this.scanItem.Barcode}";

        public bool ShouldShowQuantity => this.scanItem.Quantity > 1;

        public string Label
        {
            get
            {
                if (this.scanItem.Quantity > 1)
                {
                    return $"Non-unique item {this.index}";
                }

                return $"Item {this.index}";
            }
        }

        public string QuantityText => $"Qty: {this.scanItem.Quantity}";
    }
}
