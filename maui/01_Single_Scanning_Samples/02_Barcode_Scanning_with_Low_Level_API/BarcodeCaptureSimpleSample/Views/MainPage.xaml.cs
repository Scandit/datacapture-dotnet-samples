/*
 * This file is part of the Scandit Data Capture SDK
 *
 * Copyright (C) 2022- Scandit AG. All rights reserved.
 */

#nullable enable

using Scandit.DataCapture.Barcode.UI.Overlay;
using Brush = Scandit.DataCapture.Core.UI.Style.Brush;

namespace DebugAppMaui.Views;

public partial class MainPage : ContentPage
{
    private BarcodeCaptureOverlay? overlay;

    private readonly BarcodeCaptureOverlayStyle overlayStyle = BarcodeCaptureOverlayStyle.Frame;

    public MainPage()
    {
        this.InitializeComponent();

        // Initialization of DataCaptureView happens on handler changed event.
        this.dataCaptureView.HandlerChanged += DataCaptureViewHandlerChanged;

        // Enable the following lines in case you want to handle rejected barcodes
        // this.viewModel.AcceptedCode += (object? sender, EventArgs e) =>
        // {
        //     if (this.overlay != null)
        //     {
        //         // If the code is accepted, we want to make sure to use
        //         // a brush to highlight the code.
        //         this.overlay.Brush = BarcodeCaptureOverlay.DefaultBrushForStyle(this.overlayStyle);
        //     }
        // };

        // this.viewModel.RejectedCode += (object? sender, EventArgs e) =>
        // {
        //     if (this.overlay != null)
        //     {
        //         // If the code is rejected we temporarily change the brush
        //         // used to highlight recognized barcodes, to a transparent brush.
        //         this.overlay.Brush = Brush.TransparentBrush;
        //     }
        // };
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
