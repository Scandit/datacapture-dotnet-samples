using Microsoft.Maui.Platform;
using Scandit.DataCapture.Barcode.Data;
using Scandit.DataCapture.Barcode.UI.Overlay;
using Scandit.DataCapture.Core.UI.Viewfinder;
using SearchAndFindSample.ViewModels;
using Brush = Scandit.DataCapture.Core.UI.Style.Brush;

namespace SearchAndFindSample.Views;

public partial class SearchScanPage : ContentPage
{
    private const int FindButtonSpaceWidth = 85;
    private BarcodeCaptureOverlay overlay = null!;

    public Action<Barcode>? FindBarcode;

    public SearchScanPage()
    {
        this.InitializeComponent();

        this.scannedCodeLabel.WidthRequest = GetScreenWidth - FindButtonSpaceWidth;

        // Initialization of DataCaptureView happens on handler changed event.
        this.dataCaptureView.HandlerChanged += SetupDataCaptureView;

        // Subscribe to view-model event
        this.viewModel.CodeScanned += CodeScanned;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        this.scannedCodeView.IsVisible = false;
        _ = this.viewModel.ResumeScanningAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        this.viewModel.PauseScanning();
    }

    private void SetupDataCaptureView(object? sender, EventArgs args)
    {
        // Add a barcode capture overlay to the data capture view to set a viewfinder UI.
        BarcodeCaptureOverlay overlay = BarcodeCaptureOverlay.Create(
            this.viewModel.BarcodeCapture, BarcodeCaptureOverlayStyle.Frame);
        this.dataCaptureView.AddOverlay(overlay);

        // We add the aim viewfinder to the overlay.
        overlay.Viewfinder = new AimerViewfinder();

        // Adjust the overlay's barcode highlighting to display a light green rectangle.
        overlay.Brush = new Brush(
            fillColor: Color.FromArgb("#8028D380").ToPlatform(),
            strokeColor: Colors.Transparent.ToPlatform(),
            strokeWidth: 0f);
    }

    private void CodeScanned(object? sender, BarcodeCodeEventArgs args)
    {
        this.scannedCodeView.IsVisible = true;
        this.scannedCodeLabel.Text = args.Code;
    }

    private void SearchClicked(object? sender, EventArgs args)
    {
        if (this.viewModel.LastScannedBarcode != null)
        {
            this.FindBarcode?.Invoke(this.viewModel.LastScannedBarcode);
            this.viewModel.ResetBarcode();
            this.scannedCodeView.IsVisible = false;
        }
    }

    private static int GetScreenWidth =>
        (int)(DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density);
}
