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

using Scandit.DataCapture.Barcode.Data;
using Scandit.DataCapture.Barcode.Spark.Feedback;

namespace ListBuildingSample.Views;

public partial class MainPage : ContentPage, ISparkScanFeedbackDelegate
{
    private SparkScanBarcodeErrorFeedback errorFeedback;
    private SparkScanBarcodeSuccessFeedback successFeedback;
    
    public MainPage()
    {
        this.InitializeComponent();
        this.SubscribeToViewModelEvents();
        this.SetupSparkScanFeedback();
        this.SparkScanView.Loaded += SparkScanView_Loaded;
    }

    private void SparkScanView_Loaded(object sender, EventArgs e)
    {
        this.SparkScanView.Feedback = this;
    }

    private void SetupSparkScanFeedback()
    {
        this.errorFeedback = new SparkScanBarcodeErrorFeedback(
            message: "This code should not have been scanned",
            resumeCapturingDelay: TimeSpan.FromSeconds(60));

        this.successFeedback = new SparkScanBarcodeSuccessFeedback();
    }

    private void SubscribeToViewModelEvents()
    {
        this.viewModel.Sleep += (object sender, EventArgs args) =>
        {
            this.SparkScanView.PauseScanning();
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        this.SparkScanView.OnAppearing();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        this.SparkScanView.OnDisappearing();
    }

    private void ButtonClicked(object sender, EventArgs e)
    {
        this.viewModel.ClearScannedItems();
    }

    SparkScanBarcodeFeedback ISparkScanFeedbackDelegate.GetFeedbackForBarcode(Barcode barcode)
    {
        if (ViewModels.MainPageViewModel.IsBarcodeValid(barcode))
        {
            return this.successFeedback;
        }

        return this.errorFeedback;
    }
}
