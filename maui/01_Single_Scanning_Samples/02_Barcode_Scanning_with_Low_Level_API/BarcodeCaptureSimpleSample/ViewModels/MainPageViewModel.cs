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

using BarcodeCaptureSimpleSample.Services;
using Scandit.DataCapture.Barcode.Capture;
using Scandit.DataCapture.Barcode.Data;
using Scandit.DataCapture.Core.Capture;
using Scandit.DataCapture.Core.Common.Feedback;
using Scandit.DataCapture.Core.Source;
using Scandit.DataCapture.Core.UI.Viewfinder;
using Vibration = Scandit.DataCapture.Core.Common.Feedback.Vibration;

namespace BarcodeCaptureSimpleSample.ViewModels;

public class MainPageViewModel : BaseViewModel
{
    private readonly IDataCaptureManager dataCaptureManager;
    private readonly IMessageService messageService;
    private IViewfinder viewfinder;
    private readonly Camera? camera;

    public DataCaptureContext DataCaptureContext { get; }
    public BarcodeCapture BarcodeCapture { get; }
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

    public MainPageViewModel(IDataCaptureManager dataCaptureManager, IMessageService messageService)
    {
        this.dataCaptureManager = dataCaptureManager;
        this.messageService = messageService;
        
        this.camera = dataCaptureManager.CurrentCamera;
        this.DataCaptureContext = dataCaptureManager.DataCaptureContext;
        this.BarcodeCapture = dataCaptureManager.BarcodeCapture;
        
        // The rectangular viewfinder is the recommended viewfinder for the Barcode capture mode.
        this.viewfinder = new RectangularViewfinder(RectangularViewfinderStyle.Square, RectangularViewfinderLineStyle.Light);

        // Register self as a listener to get informed whenever a new barcode got recognized.
        this.BarcodeCapture.BarcodeScanned += this.OnBarcodeScanned;
    }

    public override Task SleepAsync()
    {
        return this.camera?.SwitchToDesiredStateAsync(FrameSourceState.Off) ?? Task.CompletedTask;
    }

    public override async Task ResumeAsync()
    {
        var permissionStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();

        if (permissionStatus != PermissionStatus.Granted)
        {
            permissionStatus = await Permissions.RequestAsync<Permissions.Camera>();
            if (permissionStatus != PermissionStatus.Granted)
            {
                await this.messageService.ShowAsync(
                    title: "Camera permission denied",
                    message: "You need to grant camera access in order to scan barcodes.");
                return;
            }
        }

        // Switch camera on to start streaming frames.
        // The camera is started asynchronously and will take some time to completely turn on.
        await this.ResumeFrameSource();
    }

    private Task ResumeFrameSource()
    {
        // Switch camera on to start streaming frames.
        // The camera is started asynchronously and will take some time to completely turn on.
        return this.camera?.SwitchToDesiredStateAsync(FrameSourceState.On) ?? Task.CompletedTask;
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

        // Get the human-readable name of the symbology and assemble the result to be shown.
        var description = new SymbologyDescription(barcode.Symbology);
        var result = "Scanned: " + barcode.Data + " (" + description.ReadableName + ")";

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            // Emit feedback
            this.Feedback.Emit();
            
            await this.messageService.ShowAsync(
                title: result,
                message: "Continue scanning?",
                buttonText: "OK",
                // Since we've disabled barcode capture above, we need to reverse it
                onDismiss: () => this.BarcodeCapture.Enabled = true); 
        });
    }
}
