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

using Microsoft.Maui.Platform;
using MatrixScanBubblesSample.Views;

using Scandit.DataCapture.Barcode.Tracking.Data;
using Scandit.DataCapture.Barcode.Tracking.UI.Overlay;

namespace MatrixScanBubblesSample.ViewModels
{
    public partial class MainPageViewModel
    {
        #region IBarcodeTrackingAdvancedOverlay

        public Android.Views.View ViewForTrackedBarcode(BarcodeTrackingAdvancedOverlay overlay, TrackedBarcode trackedBarcode)
        {
            var identifier = trackedBarcode.Identifier;

            if (!this.overlays.TryGetValue(identifier, out var stockOverlay))
            {
                // Get the information you want to show from your back end system/database.
                stockOverlay = new StockOverlay(trackedBarcode.Barcode.Data, this.shelfCount, this.backRoom);
                this.overlays[identifier] = stockOverlay;
            }

            stockOverlay.IsVisible = !this.ShouldHideOverlay?.Invoke(trackedBarcode) ?? true;

            return stockOverlay.ToPlatform(new MauiContext(MainApplication.Current.Services, MainApplication.Context));
        }
        #endregion
    }
}
