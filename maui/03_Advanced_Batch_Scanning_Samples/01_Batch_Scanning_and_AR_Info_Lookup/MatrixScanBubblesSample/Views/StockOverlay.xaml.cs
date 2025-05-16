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

namespace MatrixScanBubblesSample.Views
{
    public partial class StockOverlay
    {
        public StockOverlay(string barcode, int shelfCount, int backRoom)
        {
            this.InitializeComponent();
            this.Barcode.Text = barcode;
            this.Description.Text = $"Shelf: {shelfCount} Back room: {backRoom}";
        }

        private void OnTapped(object sender, TappedEventArgs e)
        {
            this.Title.IsVisible = !this.Title.IsVisible;
            this.Description.IsVisible = !this.Description.IsVisible;
            this.Barcode.IsVisible = !this.Barcode.IsVisible;
        }
    }
}
