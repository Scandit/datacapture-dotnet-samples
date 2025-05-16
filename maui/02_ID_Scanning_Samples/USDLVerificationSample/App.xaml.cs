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
using USDLVerificationSample.Models;
using USDLVerificationSample.Services;
using USDLVerificationSample.ViewModels;
using USDLVerificationSample.Views;

namespace USDLVerificationSample;

public partial class App : Application
{
    private readonly NavigationPage navigationPage;

    public abstract class MessageKey
    {
        public const string OnStart = nameof(OnStart);
        public const string OnSleep = nameof(OnSleep);
        public const string OnResume = nameof(OnResume);
    }

    public App()
    {
        this.InitializeComponent();
        
        var scanPage = new ScanPage();
        scanPage.IdCaptured += this.IdCaptured;
        this.navigationPage = new NavigationPage(scanPage);

        DependencyService.Register<IMessageService, MessageService>();
        DependencyService.Register<IDriverLicenseVerificationService, DriverLicenseVerificationService>();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(page: this.navigationPage);
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

    private void IdCaptured(object? sender, CapturedIdEventArgs args)
    {
        MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var scanPage = this.navigationPage.CurrentPage as ScanPage;
            scanPage?.VerificationChecksRunning();

            DriverLicenseVerificationResult verificationResult =
                await DependencyService.Get<IDriverLicenseVerificationService>().VerifyAsync(args.CapturedId);

            scanPage?.VerificationChecksCompleted();

            bool barcodeError = verificationResult.BarcodeVerificationError.HasValue &&
                              verificationResult.BarcodeVerificationError.Value;

            if (barcodeError)
            {
                await DependencyService.Get<IMessageService>()
                    .ShowAlertAsync(
                        "An error was encountered. " +
                        "Please make sure that " +
                        "your Scandit license key permits barcode verification.")
                    .ContinueWith((t) =>
                    {
                        // On alert dialog completion resume the IdCapture.
                        DataCaptureManager.Instance.IdCapture.Reset();
                        DataCaptureManager.Instance.IdCapture.Enabled = true;
                    });
            }
            else
            {
                await this.navigationPage.PushAsync(new ResultPage(args.CapturedId, verificationResult));
            }
        });
    }
}
