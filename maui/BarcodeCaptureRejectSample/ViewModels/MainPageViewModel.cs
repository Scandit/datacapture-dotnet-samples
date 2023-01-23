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

#nullable enable

using BarcodeCaptureRejectSample.Models;
using BarcodeCaptureRejectSample.Services;

using Scandit.DataCapture.Barcode.Capture;
using Scandit.DataCapture.Barcode.Data;
using Scandit.DataCapture.Core.Capture;
using Scandit.DataCapture.Core.Common.Feedback;
using Scandit.DataCapture.Core.Source;
using Scandit.DataCapture.Core.UI.Viewfinder;
using Vibration = Scandit.DataCapture.Core.Common.Feedback.Vibration;

namespace BarcodeCaptureRejectSample.ViewModels
{
    public class MainPageViewModel : BaseViewModel
    {
        public Camera? Camera => DataCaptureManager.Instance.CurrentCamera;
        public DataCaptureContext DataCaptureContext => DataCaptureManager.Instance.DataCaptureContext;
        public BarcodeCapture BarcodeCapture => DataCaptureManager.Instance.BarcodeCapture;
        public Feedback Feedback => DataCaptureManager.Instance.Feedback;
        public IViewfinder Viewfinder { get; }

        public event EventHandler? RejectedCode;
        public event EventHandler? AcceptedCode;

        public MainPageViewModel()
        {
            this.SubscribeToAppMessages();

            this.BarcodeCapture.BarcodeScanned += OnBarcodeScanned;

            // Rectangular viewfinder with an embedded Scandit logo.
            // The rectangular viewfinder is displayed when the recognition is active and hidden when it is not.
            this.Viewfinder = new RectangularViewfinder(RectangularViewfinderStyle.Square, RectangularViewfinderLineStyle.Light);
        }

        public Task OnSleep()
        {
            return this.Camera?.SwitchToDesiredStateAsync(FrameSourceState.Off) ?? Task.CompletedTask;
        }

        public async Task OnResumeAsync()
        {
            var permissionStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();

            if (permissionStatus != PermissionStatus.Granted)
            {
                permissionStatus = await Permissions.RequestAsync<Permissions.Camera>();
                if (permissionStatus == PermissionStatus.Granted)
                {
                    await this.ResumeFrameSource();
                }
            }
            else
            {
                await this.ResumeFrameSource();
            }
        }

        private void SubscribeToAppMessages()
        {
            MessagingCenter.Subscribe(this, App.MessageKeys.OnResume, callback: async (App? app) => await this.OnResumeAsync());
            MessagingCenter.Subscribe(this, App.MessageKeys.OnSleep, callback: async (App? app) => await this.OnSleep());
        }

        private Task ResumeFrameSource()
        {
            // Switch camera on to start streaming frames.
            // The camera is started asynchronously and will take some time to completely turn on.
            return this.Camera?.SwitchToDesiredStateAsync(FrameSourceState.On) ?? Task.CompletedTask;
        }

        private void OnBarcodeScanned(object? sender, BarcodeCaptureEventArgs args)
        {
            if (!args.Session.NewlyRecognizedBarcodes.Any())
            {
                return;
            }

            Barcode barcode = args.Session.NewlyRecognizedBarcodes[0];

            // If the code scanned doesn't start with "09:", we will just ignore it and continue
            // scanning.
            if (barcode.Data?.StartsWith("09:") == false)
            {
                this.RejectedCode?.Invoke(this, EventArgs.Empty);
                return;
            }

            this.AcceptedCode?.Invoke(this, EventArgs.Empty);

            // We also want to emit a feedback (vibration and, if enabled, sound).
            this.Feedback.Emit();

            // Stop recognizing barcodes for as long as we are displaying the result. There won't be any new results until
            // the capture mode is enabled again. Note that disabling the capture mode does not stop the camera, the camera
            // continues to stream frames until it is turned off.
            this.BarcodeCapture.Enabled = false;

            // If you are not disabling barcode capture here and want to continue scanning, consider
            // setting the codeDuplicateFilter when creating the barcode capture settings to around 500
            // or even -1 if you do not want codes to be scanned more than once.

            // Get the human readable name of the symbology and assemble the result to be shown.
            SymbologyDescription description = new SymbologyDescription(barcode.Symbology);
            string result = "Scanned: " + barcode.Data + " (" + description.ReadableName + ")";


            DependencyService.Get<IMessageService>().Show(result)
                                                    .ContinueWith(task => this.BarcodeCapture.Enabled = true);
        }
    }
}
