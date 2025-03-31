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
 
using Microsoft.Maui.Platform;
using Scandit.DataCapture.Barcode.Data;
using Scandit.DataCapture.Barcode.UI.Overlay;
using Scandit.DataCapture.Core.UI.Viewfinder;
using SearchAndFindSample.ViewModels;
using Brush = Scandit.DataCapture.Core.UI.Style.Brush;

namespace SearchAndFindSample.Views;

public partial class SearchScanPage
{
    private const int FindButtonSpaceWidth = 85;
    private BarcodeCaptureOverlay overlay = null!;

    public SearchScanPage()
    {
        // Make sure barcode capture is the only mode associated with the context.
        DataCaptureManager.Instance.DataCaptureContext.RemoveAllModes();
        
        this.InitializeComponent();
        this.SearchButton.Clicked += async (object? _, EventArgs _) => await SearchClickedAsync();
        this.ScannedCodeLabel.WidthRequest = GetScreenWidth - FindButtonSpaceWidth;

        // Initialization of DataCaptureView happens on handler changed event.
        this.DataCaptureView.HandlerChanged += SetupDataCaptureView;

        // Subscribe to view-model event
        this.ViewModel.CodeScanned += CodeScanned;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        this.ScannedCodeView.IsVisible = false;
        this.ViewModel.EnableMode();
        _ = this.ViewModel.ResumeAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        this.ViewModel.DisableMode();
        _ = this.ViewModel.SleepAsync();
    }

    private void SetupDataCaptureView(object? sender, EventArgs args)
    {
        // Add a barcode capture overlay to the data capture view to set a viewfinder UI.
        this.overlay = BarcodeCaptureOverlay.Create(
            this.ViewModel.BarcodeCapture, BarcodeCaptureOverlayStyle.Frame);
        this.DataCaptureView.AddOverlay(overlay);

        // We add the aim viewfinder to the overlay.
        this.overlay.Viewfinder = new AimerViewfinder();

        // Adjust the overlay's barcode highlighting to display a light green rectangle.
        this.overlay.Brush = new Brush(
            fillColor: Color.FromArgb("#8028D380").ToPlatform(),
            strokeColor: Colors.Transparent.ToPlatform(),
            strokeWidth: 0f);
    }

    private void CodeScanned(object? sender, BarcodeCodeEventArgs args)
    {
        this.ScannedCodeView.IsVisible = true;
        this.ScannedCodeLabel.Text = args.Code;
    }

    private async Task SearchClickedAsync()
    {
        if (this.ViewModel.LastScannedBarcode == null)
        {
            return;
        }
        
        await FindBarcodeAsync(this.ViewModel.LastScannedBarcode);
        this.ResetBarcode();
    }

    private static async Task FindBarcodeAsync(Barcode barcode)
    {
        if (Application.Current?.MainPage is not NavigationPage navigation)
        {
            return;
        }
        
        await navigation.PushAsync(new FindBarcodePage(barcode));
    }

    private void ResetBarcode()
    {
        this.ViewModel.ResetBarcode();
        this.ScannedCodeView.IsVisible = false;
    }

    private static int GetScreenWidth =>
        (int)(DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density);
}
