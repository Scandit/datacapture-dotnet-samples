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

using MatrixScanSimpleSample.ViewModels;
using Scandit.DataCapture.Barcode.Tracking.UI.Overlay;

namespace MatrixScanSimpleSample.Views
{
    public partial class MainPage : ContentPage
    {
        private BarcodeTrackingBasicOverlay overlay;
        private readonly MainPageViewModel viewModel;

        public MainPage()
        {
            this.InitializeComponent();
            this.viewModel = this.BindingContext as MainPageViewModel;

            // Initialization of DataCaptureView happens on handler changed event.
            this.dataCaptureView.HandlerChanged += DataCaptureViewHandlerChanged;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = this.viewModel.OnResumeAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            this.viewModel.OnSleep();
        }

        private void DataCaptureViewHandlerChanged(object? sender, EventArgs e)
        {
            this.overlay = BarcodeTrackingBasicOverlay.Create(this.viewModel.BarcodeTracking, BarcodeTrackingBasicOverlayStyle.Frame);
            this.overlay.Listener = this.viewModel;

            this.dataCaptureView.AddOverlay(overlay);
        }

        private void ButtonClicked(object sender, System.EventArgs e)
        {
            ((NavigationPage)Application.Current.MainPage).PushAsync(new ResultsPage());
        }
    }
}
