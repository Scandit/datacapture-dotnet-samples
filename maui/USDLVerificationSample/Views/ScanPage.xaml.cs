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

using Scandit.DataCapture.ID.UI.Overlay;
using USDLVerificationSample.ViewModels;

namespace USDLVerificationSample.Views
{
    public partial class ScanPage : ContentPage
    {
        private IdCaptureOverlay overlay;
        public readonly string AlignFrontText = "Align front of document";
        public readonly string AlignBackText = "Align back of document";

        public ScanPage()
        {
            this.InitializeComponent();
            this.viewModel.AlignBack += AlignBack;

            // Initialization of DataCaptureView happens on handler changed event.
            this.dataCaptureView.HandlerChanged += DataCaptureViewHandlerChanged;
        }

        private void DataCaptureViewHandlerChanged(object sender, EventArgs e)
        {
            this.overlay = IdCaptureOverlay.Create(this.viewModel.IdCapture);
            this.overlay.IdLayoutStyle = IdLayoutStyle.Square;
            this.dataCaptureView.AddOverlay(this.overlay);
        }

        public event EventHandler<CapturedIdEventArgs> IdCaptured
        {
            add { this.viewModel.IdCaptured += value; }
            remove { this.viewModel.IdCaptured -= value; }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            this.ScanLabel.Text = AlignFrontText;
            this.viewModel.IdCapture.Reset();
            _ = this.viewModel.OnResumeAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            this.viewModel.OnSleepAsync();
        }

        private void AlignBack(object sender, EventArgs e)
        {
            App.Current.Dispatcher.DispatchAsync(() =>
            {
                this.ScanLabel.Text = AlignBackText;
            });
        }
    }
}
