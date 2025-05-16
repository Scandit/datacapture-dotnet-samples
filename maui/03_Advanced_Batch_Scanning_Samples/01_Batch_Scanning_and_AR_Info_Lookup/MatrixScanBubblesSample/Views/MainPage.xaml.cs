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

using MatrixScanBubblesSample.ViewModels;
using Scandit.DataCapture.Barcode.Batch.Data;
using Scandit.DataCapture.Barcode.Batch.UI.Overlay;
using Scandit.DataCapture.Core.Common.Geometry;

namespace MatrixScanBubblesSample.Views
{
    public partial class MainPage : ContentPage
    {
        private BarcodeBatchBasicOverlay basicOverlay;
        private BarcodeBatchAdvancedOverlay advancedOverlay;

        private readonly float barcodeToScreenTresholdRatio = 0.1f;
        private bool freezeEnabled = true;

        public MainPage()
        {
            this.InitializeComponent();

            // Initialization of DataCaptureView happens on handler changed event.
            this.dataCaptureView.HandlerChanged += DataCaptureViewHandlerChanged;
            this.viewModel.ShouldHideOverlay = (TrackedBarcode trackedBarcode) =>
            {
                double GetWidth(Quadrilateral barcodeLocation)
                {
                    return Math.Max(barcodeLocation.BottomRight.X - barcodeLocation.BottomLeft.X, barcodeLocation.TopRight.X - barcodeLocation.TopLeft.X);
                }

                // If the barcode is wider than the desired percent of the data capture view's width,
                // show it to the user.
                var width = GetWidth(this.dataCaptureView.MapFrameQuadrilateralToView(trackedBarcode.Location));
                return (width / this.dataCaptureView.Width) <= this.barcodeToScreenTresholdRatio;
            };
        }

        private void DataCaptureViewHandlerChanged(object sender, EventArgs e)
        {
            this.basicOverlay = BarcodeBatchBasicOverlay.Create(
                this.viewModel.BarcodeBatch,
                BarcodeBatchBasicOverlayStyle.Dot);
            this.advancedOverlay = BarcodeBatchAdvancedOverlay.Create(
                this.viewModel.BarcodeBatch);
            this.advancedOverlay.Listener = this.viewModel;
            this.dataCaptureView.AddOverlay(this.basicOverlay);
            this.dataCaptureView.AddOverlay(this.advancedOverlay);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = this.viewModel.ResumeAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _ = this.viewModel.SleepAsync();
        }

        private void FreezeButtonClicked(object sender, EventArgs e)
        {
            if (sender is ImageButton button)
            {
                if (this.freezeEnabled)
                {
                    button.Source = ImageSource.FromFile("freeze_disabled.png");
                }
                else
                {
                    button.Source = ImageSource.FromFile("freeze_enabled.png");
                }

                this.freezeEnabled = !this.freezeEnabled;
            }
        }
    }
}
