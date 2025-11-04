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

using Scandit.DataCapture.ID.UI.Overlay;
using USDLVerificationSample.ViewModels;

namespace USDLVerificationSample.Views
{
    public partial class ScanPage : ContentPage
    {
        private IdCaptureOverlay? overlay;

        public ScanPage()
        {
            this.InitializeComponent();

            // Initialization of DataCaptureView happens on handler ready event.
            this.dataCaptureView.HandlerReady += DataCaptureViewHandlerReady;
        }

        private void DataCaptureViewHandlerReady(object? sender, EventArgs e)
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
        
        public event EventHandler<RejectedIdEventArgs> IdRejected
        {
            add { this.viewModel.IdRejected += value; }
            remove { this.viewModel.IdRejected -= value; }
        }

        public void VerificationChecksRunning()
        {
            this.VerificationCheckLabel.IsVisible = true;
        }

        public void VerificationChecksCompleted()
        {
            this.VerificationCheckLabel.IsVisible = false;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = this.viewModel.ResumeAsync();

            if (this.overlay != null)
            {
                this.dataCaptureView.AddOverlay(this.overlay);
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            if (this.overlay != null)
            {
                this.dataCaptureView.RemoveOverlay(this.overlay);
            }
            _ = this.viewModel.SleepAsync();
        }
    }
}
