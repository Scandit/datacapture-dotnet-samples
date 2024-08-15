/*
 * This file is part of the Scandit Data Capture SDK
 *
 * Copyright (C) 2023- Scandit AG. All rights reserved.
 */

using Scandit.DataCapture.Barcode.Capture;
using Scandit.DataCapture.Barcode.Data;
using Scandit.DataCapture.Core.Capture;
using Scandit.DataCapture.Core.Source;

namespace SearchAndFindSample.ViewModels;

public class SearchScanPageViewModel : BaseViewModel
{
    private readonly DataCaptureManager dataCaptureManager = DataCaptureManager.Instance;
    private readonly DataCaptureContext dataCaptureContext = DataCaptureManager.Instance.DataCaptureContext;
    private readonly BarcodeCapture barcodeCapture = SearchDataCaptureManager.Instance.BarcodeCapture;

    public DataCaptureContext DataCaptureContext => this.dataCaptureContext;
    public BarcodeCapture BarcodeCapture => this.barcodeCapture;

    public event EventHandler<BarcodeCodeEventArgs>? CodeScanned;
    public Barcode? LastScannedBarcode { get; private set; }

    public SearchScanPageViewModel()
    {
        // Subscribe to BarcodeScanned event to get informed of captured barcodes.
        this.barcodeCapture.BarcodeScanned += BarcodeScanned;
    }

    public void ResetBarcode()
    {
        lock (this)
        {
            this.LastScannedBarcode = null;
        }
    }

    public void DisableMode()
    {
        this.IsActive = false;
    }

    public void EnableMode()
    {
        this.IsActive = true;
    }

    public async override Task ResumeAsync()
    {
        var permissionStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();

        if (permissionStatus != PermissionStatus.Granted)
        {
            permissionStatus = await Permissions.RequestAsync<Permissions.Camera>();

            if (permissionStatus == PermissionStatus.Granted)
            {
                this.ResumeScanning();
            }
        }
        else
        {
            this.ResumeScanning();
        }
    }

    public async override Task SleepAsync()
    {
        this.dataCaptureContext.RemoveMode(this.barcodeCapture);
        this.barcodeCapture.Enabled = false;

        if (this.dataCaptureManager.Camera != null)
        {
            await this.dataCaptureManager.Camera.SwitchToDesiredStateAsync(FrameSourceState.Off);
        }
    }

    private void ResumeScanning()
    {
        // Make sure barcode capture is the only mode associated with the context.
        this.dataCaptureContext.RemoveAllModes();
        this.dataCaptureContext.AddMode(this.barcodeCapture);

        this.dataCaptureManager.Camera?.ApplySettingsAsync(BarcodeCapture.RecommendedCameraSettings);
        this.dataCaptureManager.Camera?.SwitchToDesiredStateAsync(FrameSourceState.On);

        this.barcodeCapture.Enabled = true;
    }

    private void BarcodeScanned(object? sender, BarcodeCaptureEventArgs args)
    {
        lock (this)
        {
            if (args.Session.NewlyRecognizedBarcode != null)
            {
                this.LastScannedBarcode = args.Session.NewlyRecognizedBarcode;

                // This method is invoked on a non-UI thread, so in order to perform UI work,
                // we have to switch to the main thread.
                Application.Current?.Dispatcher.DispatchAsync(() =>
                {
                    this.CodeScanned?.Invoke(this, new BarcodeCodeEventArgs(this.LastScannedBarcode));
                });
            }
        }
    }
}
