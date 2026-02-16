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

using System.Reactive.Linq;
using System.Reactive.Subjects;
using LabelCaptureSimpleSample.Services;
using LabelCaptureSimpleSample.Services.Internals;
using Scandit.DataCapture.Core.Capture;
using Scandit.DataCapture.Core.UI;
using Scandit.DataCapture.Label.UI.Overlay;
using Scandit.DataCapture.Label.UI.Overlay.Validation;

namespace LabelCaptureSimpleSample.ViewModels;

public class ScanViewModel
{
    private readonly ICameraService cameraService;
    private readonly ILabelCaptureService labelCaptureService;
    private readonly Subject<string> messageSubject = new();

    private LabelCaptureValidationFlowOverlay? validationFlowOverlay;

    public IObservable<string> Messages { get { return this.messageSubject.AsObservable(); } }

    public DataCaptureContext DataCaptureContext { get; }

    public ScanViewModel(string licenseKey)
    {
        this.DataCaptureContext = DataCaptureContext.ForLicenseKey(licenseKey);

        this.cameraService = new CameraService();
        this.cameraService.Initialize(this.DataCaptureContext);

        this.labelCaptureService = new LabelCaptureService();
        this.labelCaptureService.Initialize(this.DataCaptureContext);
    }

    public LabelCaptureBasicOverlay BuildOverlay()
    {
        return this.labelCaptureService.BuildOverlay();
    }

    public LabelCaptureValidationFlowOverlay GetValidationFlowOverlay(DataCaptureView view)
    {
        if (this.validationFlowOverlay == null)
        {
            this.validationFlowOverlay =
                this.labelCaptureService.BuildValidationFlowOverlay(
                    view,
                    onLabelScanned: label =>
                    {
                        // Pause scanning when a label is captured
                        this.PauseScanning();
                        // Send a message to show the alert
                        this.messageSubject.OnNext(label);
                    });
        }

        return this.validationFlowOverlay;
    }

    private void PauseScanning()
    {
        this.cameraService.PauseFrameSource();
        this.labelCaptureService.Disable();
    }

    public void OnDialogDismissed()
    {
        this.cameraService.ResumeFrameSource();
        this.labelCaptureService.Enable();
    }

    public void StartCamera()
    {
        this.cameraService.ResumeFrameSource();
        this.labelCaptureService.Enable();
    }

    public void StopCamera()
    {
        this.cameraService.PauseFrameSource();
        this.labelCaptureService.Disable();
    }
}