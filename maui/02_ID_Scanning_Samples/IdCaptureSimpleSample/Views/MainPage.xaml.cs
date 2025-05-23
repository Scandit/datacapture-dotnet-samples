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

using IdCaptureSimpleSample.ViewModels;
using Scandit.DataCapture.ID.UI.Overlay;

namespace IdCaptureSimpleSample.Views
{
    public partial class MainPage : ContentPage
    {
        private IdCaptureOverlay overlay;

        public MainPage()
        {
            this.InitializeComponent();

            // Initialization of DataCaptureView happens on handler changed event.
            this.dataCaptureView.HandlerChanged += DataCaptureViewHandlerChanged;
        }

        private void DataCaptureViewHandlerChanged(object sender, EventArgs e)
        {
            this.overlay = IdCaptureOverlay.Create(this.viewModel.IdCapture);
            this.dataCaptureView.AddOverlay(this.overlay);
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
    }
}
