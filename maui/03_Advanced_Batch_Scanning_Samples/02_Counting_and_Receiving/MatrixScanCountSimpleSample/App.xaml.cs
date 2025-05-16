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

using CommunityToolkit.Mvvm.Messaging;
using MatrixScanCountSimpleSample.Models;
using MatrixScanCountSimpleSample.Views;

namespace MatrixScanCountSimpleSample;

public partial class App : Application, IScanResultsPageListener
{
    private readonly BarcodeCountPage barcodeCountPage;
    private readonly NavigationPage navigationPage;

    // Enter your Scandit License key here.
    // Your Scandit License key is available via your Scandit SDK web account.
    public const string SCANDIT_LICENSE_KEY = "-- ENTER YOUR SCANDIT LICENSE KEY HERE --";

    public abstract class MessageKey
    {
        public const string OnStart = nameof(OnStart);
        public const string OnSleep = nameof(OnSleep);
        public const string OnResume = nameof(OnResume);
    }

    public App()
    {
        this.InitializeComponent();

        // Set BarcodeCountPage as the main page.
        this.barcodeCountPage = new BarcodeCountPage();
        this.navigationPage = new NavigationPage(this.barcodeCountPage);
    }

    public void ShowScanResults(bool isOrderCompleted)
    {
        // Get a list of ScannedItem to display
        var scannedItems = BarcodeManager.Instance.GetScanResults().Values.ToList();
        this.navigationPage.PushAsync(new ScanResultsPage(this, scannedItems, isOrderCompleted));
    }

    public void RestartScanning()
    {
        this.barcodeCountPage.RestartScanning();
    }
    
    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(this.navigationPage);
    }

    protected override void OnStart()
    {
        WeakReferenceMessenger.Default.Send(new ApplicationMessage(MessageKey.OnStart));
    }

    protected override void OnSleep()
    {
        WeakReferenceMessenger.Default.Send(new ApplicationMessage(MessageKey.OnSleep));
    }

    protected override void OnResume()
    {
        WeakReferenceMessenger.Default.Send(new ApplicationMessage(MessageKey.OnResume));
    }
}
