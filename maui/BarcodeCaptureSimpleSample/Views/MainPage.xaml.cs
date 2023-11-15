/*
 * This file is part of the Scandit Data Capture SDK
 *
 * Copyright (C) 2022- Scandit AG. All rights reserved.
 */

#nullable enable

using Scandit.DataCapture.Barcode.UI.Overlay;

namespace DebugAppMaui.Views;

public partial class MainPage : ContentPage
{
    private BarcodeCaptureOverlay? overlay;

    public MainPage()
    {
        this.InitializeComponent();

        // Initialization of DataCaptureView happens on handler changed event.
        this.dataCaptureView.HandlerChanged += DataCaptureViewHandlerChanged;
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
