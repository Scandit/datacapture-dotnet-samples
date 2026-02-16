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

using LabelCaptureSimpleSample.ViewModels;
using Scandit.DataCapture.Core.UI.Control;
using Scandit.DataCapture.Label.UI.Overlay;
using Scandit.DataCapture.Label.UI.Overlay.Validation;

namespace LabelCaptureSimpleSample.Views;

public partial class MainPage
{
    private LabelCaptureBasicOverlay? labelCaptureOverlay;
    private LabelCaptureValidationFlowOverlay? validationFlowOverlay;
    private readonly MainPageViewModel viewModel;

    public MainPage(MainPageViewModel viewModel)
    {
        this.viewModel = viewModel;
        this.InitializeComponent();
        this.BindingContext = viewModel;

        // Initialization of DataCaptureView happens on handler changed event.
        this.dataCaptureView.HandlerChanged += this.DataCaptureViewHandlerChanged;
    }

    private void DataCaptureViewHandlerChanged(object? sender, EventArgs e)
    {
        // Create and add overlays
        this.labelCaptureOverlay = this.viewModel.BuildOverlay();
        this.validationFlowOverlay = this.viewModel.GetValidationFlowOverlay();

        this.dataCaptureView.AddOverlay(this.labelCaptureOverlay);
        this.dataCaptureView.AddOverlay(this.validationFlowOverlay);
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

