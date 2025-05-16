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

using System.Collections.ObjectModel;
using ListBuildingSample.Models;
using Microsoft.Maui.ApplicationModel;
using Scandit.DataCapture.Barcode.Data;
using Scandit.DataCapture.Barcode.Spark.Capture;
using Scandit.DataCapture.Barcode.Spark.UI;
using Scandit.DataCapture.Core.Capture;

namespace ListBuildingSample.ViewModels;

public class MainPageViewModel : BaseViewModel
{
    public DataCaptureContext DataCaptureContext { get; } = ScannerModel.Instance.DataCaptureContext;
    public SparkScan SparkScan { get; } = ScannerModel.Instance.SparkScan;
    public SparkScanViewSettings ViewSettings { get; } = new();

    public event EventHandler? PauseScanning;

    public ObservableCollection<ListItem> ScanResults { get; set; } = [];

    public MainPageViewModel()
    {
        this.SparkScan.BarcodeScanned += BarcodeScanned;
    }

    public string ItemCount
    {
        get
        {
            var itemCount = this.ScanResults.Count;
            var text = itemCount == 1 ? "Item" : "Items";
            var label = $"{itemCount} {text}";
            return label;
        }
    }

    protected override async Task StartAsync()
    {
        await CheckCameraPermissionAsync();
    }

    protected override async Task ResumeAsync()
    {
        await CheckCameraPermissionAsync();
    }

    protected override Task SleepAsync()
    {
        this.PauseScanning?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    public void ClearScannedItems()
    {
        this.ScanResults.Clear();
        this.OnPropertyChanged(nameof(ItemCount));
    }

    public static bool IsBarcodeValid(Barcode barcode)
    {
        return barcode.Data != "123456789";
    }

    private static async Task CheckCameraPermissionAsync()
    {
        var permissionStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();

        if (permissionStatus != PermissionStatus.Granted)
        {
            await Permissions.RequestAsync<Permissions.Camera>();
        }
    }

    private void BarcodeScanned(object? sender, SparkScanEventArgs args)
    {
        if (args.Session.NewlyRecognizedBarcode == null)
        {
            return;
        }

        var barcode = args.Session.NewlyRecognizedBarcode;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (!IsBarcodeValid(barcode))
            {
                return;
            }

            this.ScanResults.Add(
                new ListItem(
                    index: this.ScanResults.Count + 1,
                    symbology: new SymbologyDescription(barcode.Symbology).ReadableName,
                    data: barcode.Data ?? string.Empty));
            this.OnPropertyChanged(nameof(ItemCount));
        });
    }
}