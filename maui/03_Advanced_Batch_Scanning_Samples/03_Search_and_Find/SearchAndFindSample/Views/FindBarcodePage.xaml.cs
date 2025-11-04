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
using Scandit.DataCapture.Barcode.Find.UI;
using Scandit.DataCapture.Core.Source;
using SearchAndFindSample.ViewModels;

namespace SearchAndFindSample.Views;

public partial class FindBarcodePage
{
    private readonly FindBarcodePageViewModel viewModel;

    public FindBarcodePage(Barcode barcode)
    {
        // Make sure barcode find is the only mode associated with the context.
        DataCaptureManager.Instance.DataCaptureContext.RemoveAllModes();
        
        this.viewModel = new FindBarcodePageViewModel(barcode.Symbology, barcode.Data ?? string.Empty);
        this.BindingContext = this.viewModel;
        this.Title = "Search & Find";

        this.InitializeComponent();

        // Initialization of DataCaptureView happens on handler changed event.
        this.BarcodeFindView.HandlerChanged += SetupBarcodeFindView;
        this.BarcodeFindView.Loaded += BarcodeFindViewLoaded;
    }

    private void BarcodeFindViewLoaded(object? sender, EventArgs e)
    {
        // Start the searching process. Call this method if you want to trigger
        // the searching process without any user interaction.
        this.BarcodeFindView.StartSearching();
    }

    private void SetupBarcodeFindView(object? sender, EventArgs e)
    {
#if __IOS__
        this.BarcodeFindView.PrepareSearching();
#endif
#if __ANDROID__
        this.BarcodeFindView.OnResume();
#endif

        this.BarcodeFindView.FinishButtonTapped += 
            async (object? _, FinishButtonTappedEventArgs _) => await FinishButtonClickedAsync();
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
    }

    protected override bool OnBackButtonPressed()
    {
        this.StopBarcodeFind();
        return base.OnBackButtonPressed();
    }

    private async Task FinishButtonClickedAsync()
    {
        this.StopBarcodeFind();
        await this.Navigation.PopToRootAsync(animated: true);
    }

    private void StopBarcodeFind()
    {
        this.BarcodeFindView.OnPause();
        this.BarcodeFindView.StopSearching();
    }
}
