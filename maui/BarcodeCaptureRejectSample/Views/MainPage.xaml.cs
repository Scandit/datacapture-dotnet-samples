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

#nullable enable

using BarcodeCaptureRejectSample.ViewModels;
using Scandit.DataCapture.Barcode.UI.Overlay;
using Brush = Scandit.DataCapture.Core.UI.Style.Brush;

namespace BarcodeCaptureRejectSample;

public partial class MainPage : ContentPage
{
    private BarcodeCaptureOverlay? overlay;
    private readonly BarcodeCaptureOverlayStyle overlayStyle = BarcodeCaptureOverlayStyle.Frame;

    public MainPage()
    {
        this.InitializeComponent();

        // Initialization of DataCaptureView happens on handler changed event.
        this.dataCaptureView.HandlerChanged += DataCaptureViewHandlerChanged;

        this.viewModel.AcceptedCode += (object? sender, EventArgs e) =>
        {
            if (this.overlay != null)
            {
                // If the code is accepted, we want to make sure to use
                // a brush to highlight the code.
                this.overlay.Brush = BarcodeCaptureOverlay.DefaultBrushForStyle(this.overlayStyle);
            }
        };

        this.viewModel.RejectedCode += (object? sender, EventArgs e) =>
        {
            if (this.overlay != null)
            {
                // If the code is rejected we temporarily change the brush
                // used to highlight recognized barcodes, to a transparent brush.
                this.overlay.Brush = Brush.TransparentBrush;
            }
        };
    }

    private void DataCaptureViewHandlerChanged(object? sender, EventArgs e)
    {
        this.overlay = BarcodeCaptureOverlay.Create(this.viewModel.BarcodeCapture, BarcodeCaptureOverlayStyle.Frame);
        this.overlay.Viewfinder = this.viewModel.Viewfinder;

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
