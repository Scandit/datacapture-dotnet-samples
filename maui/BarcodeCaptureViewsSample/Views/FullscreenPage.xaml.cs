﻿/*
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

#nullable enable

using Scandit.DataCapture.Barcode.UI.Overlay;

namespace BarcodeCaptureViewsSample.Views
{
    public partial class FullscreenPage : ContentPage
    {
        private BarcodeCaptureOverlay? overlay;

        public FullscreenPage()
        {
            this.InitializeComponent();
            this.Title = "Fullscreen";

            // Initialization of DataCaptureView happens on handler changed event.
            this.dataCaptureView.HandlerChanged += DataCaptureViewHandlerChanged;
        }

        private void DataCaptureViewHandlerChanged(object? sender, EventArgs e)
        {
            this.overlay = BarcodeCaptureOverlay.Create(this.viewModel.BarcodeCapture, BarcodeCaptureOverlayStyle.Frame);
            this.dataCaptureView.AddOverlay(overlay);
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
    }
}
