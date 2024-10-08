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

using Scandit.DataCapture.Barcode.Data;
using SearchAndFindSample.ViewModels;

namespace SearchAndFindSample.Views;

public partial class FindBarcodePage : ContentPage
{
    private FindBarcodePageViewModel viewModel;

    public FindBarcodePage(Barcode barcode)
    {
        this.viewModel = new FindBarcodePageViewModel(barcode.Symbology, barcode.Data ?? string.Empty);
        this.BindingContext = this.viewModel;
        this.Title = "Search & Find";

        this.InitializeComponent();

        // Initialization of DataCaptureView happens on handler changed event.
        this.barcodeFindView.HandlerChanged += SetupBarcodeFindView;
        this.barcodeFindView.Loaded += BarcodeFindViewLoaded;
    }

    private void BarcodeFindViewLoaded(object? sender, EventArgs e)
    {
        // Start the searching process. Call this method if you want to trigger
        // the searching process without any user interaction.
        this.barcodeFindView.StartSearching();
    }

    private void SetupBarcodeFindView(object? sender, EventArgs e)
    {
#if __IOS__
        this.barcodeFindView.PrepareSearching();
#endif
#if __ANDROID__
        this.barcodeFindView.OnResume();
#endif
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        this.viewModel.EnableMode();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        this.viewModel.DisableMode();
        this.barcodeFindView.OnPause();
        this.barcodeFindView.StopSearching();
    }

    private void FinishButtonClicked(object? sender, EventArgs args)
    {
        if (Application.Current?.MainPage is NavigationPage navigation)
        {
            navigation.PopToRootAsync(animated: true);
        }
    }
}
