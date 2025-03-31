/*
 * This file is part of the Scandit Data Capture SDK
 *
 * Copyright (C) 2022- Scandit AG. All rights reserved.
 */

#nullable enable
using DebugAppMaui.Models;
using DebugAppMaui.Services;

using Scandit.DataCapture.Barcode.Capture;
using Scandit.DataCapture.Barcode.Data;
using Scandit.DataCapture.Core.Capture;
using Scandit.DataCapture.Core.Common.Feedback;
using Scandit.DataCapture.Core.Source;
using Scandit.DataCapture.Core.UI.Viewfinder;
using Vibration = Scandit.DataCapture.Core.Common.Feedback.Vibration;

namespace DebugAppMaui.ViewModels
{
    public class MainPageViewModel : BaseViewModel
    {
        private IViewfinder viewfinder;

        public Camera? Camera { get; } = DataCaptureManager.Instance.CurrentCamera;
        public DataCaptureContext DataCaptureContext { get; } = DataCaptureManager.Instance.DataCaptureContext;
        public BarcodeCapture BarcodeCapture { get; } = DataCaptureManager.Instance.BarcodeCapture;
        public Feedback Feedback { get; } = Feedback.DefaultFeedback;

        public IViewfinder Viewfinder
        {
            get { return this.viewfinder; }
            set
            {
                this.viewfinder = value;
                this.OnPropertyChanged(nameof(Viewfinder));
            }
        }

        public event EventHandler? RejectedCode;
        public event EventHandler? AcceptedCode;

        public MainPageViewModel()
        {
            this.SubscribeToAppMessages();

            this.BarcodeCapture.BarcodeScanned += OnBarcodeScanned;
            this.BarcodeCapture.Feedback.Success = new Feedback(Vibration.DefaultVibration);

            // Rectangular viewfinder with an embedded Scandit logo.
            // The rectangular viewfinder is displayed when the recognition is active and hidden when it is not.
            this.viewfinder = new RectangularViewfinder(RectangularViewfinderStyle.Square, RectangularViewfinderLineStyle.Light);
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
            if (args.Session.NewlyRecognizedBarcode == null)
            {
                return;
            }

            Barcode barcode = args.Session.NewlyRecognizedBarcode;

            // Use the following code to reject barcodes.
            // By uncommenting the following lines, barcodes not starting with 09: are ignored.
            // if (barcode.Data?.StartsWith("09:") == false)
            // {
            //     this.RejectedCode?.Invoke(this, EventArgs.Empty);
            //     return;
            // }
            // this.AcceptedCode?.Invoke(this, EventArgs.Empty);

            // We also want to emit a feedback (vibration and, if enabled, sound).
            // By default, every time a barcode is scanned, a sound (if not in silent mode) and a vibration are played.
            // To emit a feedback only when necessary, it is necessary to set a success feedback without sound and vibration
            // when setting up Barcode Capture (in this case in the constructor of the `DataCaptureManager` class).
            // this.Feedback.Emit();

            // Stop recognizing barcodes for as long as we are displaying the result. There won't be any new results until
            // the capture mode is enabled again. Note that disabling the capture mode does not stop the camera, the camera
            // continues to stream frames until it is turned off.
            this.BarcodeCapture.Enabled = false;

            // If you don't want the codes to be scanned more than once, consider setting the codeDuplicateFilter when
            // creating the barcode capture settings to -1.
            // You can set any other value (e.g. 500) to set a fixed timeout and override the smart behaviour enabled
            // by default.

            // Get the human readable name of the symbology and assemble the result to be shown.
            SymbologyDescription description = new SymbologyDescription(barcode.Symbology);
            string result = "Scanned: " + barcode.Data + " (" + description.ReadableName + ")";

            // We also want to emit a feedback (vibration and, if enabled, sound).
            this.Feedback.Emit();

            DependencyService.Get<IMessageService>().Show(result)
                                                    .ContinueWith(task => this.BarcodeCapture.Enabled = true);
        }
    }
}
