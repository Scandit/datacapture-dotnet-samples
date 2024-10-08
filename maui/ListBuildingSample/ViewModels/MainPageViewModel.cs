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

using System.Collections.ObjectModel;

using ListBuildingSample.Models;

using Scandit.DataCapture.Barcode.Data;
using Scandit.DataCapture.Barcode.Spark.Capture;
using Scandit.DataCapture.Barcode.Spark.UI;
using Scandit.DataCapture.Core.Capture;

namespace ListBuildingSample.ViewModels;

public class MainPageViewModel : BaseViewModel
{
    public DataCaptureContext DataCaptureContext { get; } = ScannerModel.Instance.DataCaptureContext;
    public SparkScan SparkScan { get; } = ScannerModel.Instance.SparkScan;
    public SparkScanViewSettings ViewSettings { get; private set; } = new SparkScanViewSettings();

    public event EventHandler Sleep;
    public event EventHandler Resume;

    public ObservableCollection<ListItem> ScanResults { get; set; } = new ObservableCollection<ListItem>();

    public MainPageViewModel()
    {
        this.Initialize();
        this.SubscribeToAppMessages();
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

    public void OnSleep()
    {
        this.Sleep?.Invoke(this, EventArgs.Empty);
    }

    public void OnResume()
    {
        this.Resume?.Invoke(this, EventArgs.Empty);
    }

    public void ClearScannedItems()
    {
        this.ScanResults.Clear();
        this.OnPropertyChanged(nameof(ItemCount));
    }

    public async Task CheckCameraPermissionAsync()
    {
        var permissionStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();

        if (permissionStatus != PermissionStatus.Granted)
        {
            permissionStatus = await Permissions.RequestAsync<Permissions.Camera>();
            if (permissionStatus == PermissionStatus.Granted)
            {
                this.OnResume();
            }
        }
        else
        {
            this.OnResume();
        }
    }

    public static bool IsBarcodeValid(Barcode barcode)
    {
        return barcode.Data != "123456789";
    }

    private void Initialize()
    {
        this.SparkScan.BarcodeScanned += BarcodeScanned;
    }

    private void BarcodeScanned(object sender, SparkScanEventArgs args)
    {
        if (args.Session.NewlyRecognizedBarcode == null)
        {
            return;
        }

        var barcode = args.Session.NewlyRecognizedBarcode;

        Application.Current.Dispatcher.DispatchAsync(() =>
        {
            if (IsBarcodeValid(barcode))
            {
                this.ScanResults.Add(
                    new ListItem(
                        index: this.ScanResults.Count + 1,
                        symbology: new SymbologyDescription(barcode.Symbology).ReadableName,
                        data: barcode.Data));
                this.OnPropertyChanged(nameof(ItemCount));
            }
        });
    }

    private void SubscribeToAppMessages()
    {
        MessagingCenter.Subscribe(
            subscriber: this,
            message: App.MessageKeys.OnResume,
            callback: async (App app) => await this.CheckCameraPermissionAsync());

        MessagingCenter.Subscribe(
            subscriber: this,
            message: App.MessageKeys.OnStart,
            callback: async (App app) => await this.CheckCameraPermissionAsync());

        MessagingCenter.Subscribe(
            subscriber: this,
            message: App.MessageKeys.OnSleep,
            callback: (App app) => this.OnSleep());
    }
}
