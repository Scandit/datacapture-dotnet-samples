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


using Scandit.DataCapture.Barcode.Count.UI;

namespace MatrixScanCountSimpleSample;

public partial class BarcodeCountPage : ContentPage
{
    public BarcodeCountPage()
    {
        this.InitializeComponent();

        // Initialization of BarcodeCountView happens on handler changed event.
        this.barcodeCountView.HandlerChanged += DataCaptureViewHandlerChanged;
    }

    private void DataCaptureViewHandlerChanged(object? sender, EventArgs e)
    {
        this.barcodeCountView.ListButtonTapped += BarcodeCountViewListButtonTapped;
        this.barcodeCountView.ExitButtonTapped += BarcodeCountViewExitButtonTapped;
    }

    public void RestartScanning()
    {
        this.viewModel.ResetSession();
    }

    protected override void OnAppearing()
    {
        _ = this.viewModel.ResumeAsync();
        base.OnAppearing();
    }

    protected override void OnDisappearing()
    {
        this.viewModel.PauseScanning(navigatingInternally: true);
        base.OnDisappearing();
    }

    private void BarcodeCountViewExitButtonTapped(object? sender, ExitButtonTappedEventArgs e)
    {
        var app = Application.Current as App;
        app?.ShowScanResults(isOrderCompleted: true);
    }

    private void BarcodeCountViewListButtonTapped(object? sender, ListButtonTappedEventArgs e)
    {
        var app = Application.Current as App;
        app?.ShowScanResults(isOrderCompleted: false);
    }
}
