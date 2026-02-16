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

using LabelCaptureSimpleSample.Services;
using Scandit.DataCapture.Core.Capture;
using Scandit.DataCapture.Label.UI.Overlay;
using Scandit.DataCapture.Label.UI.Overlay.Validation;

namespace LabelCaptureSimpleSample.ViewModels;

public class MainPageViewModel(
    DataCaptureContext dataCaptureContext,
    ICameraService cameraService,
    ILabelCaptureService labelCaptureService,
    IMessageService messageService)
    : BaseViewModel
{
    private LabelCaptureBasicOverlay? labelCaptureOverlay;
    private LabelCaptureValidationFlowOverlay? validationFlowOverlay;

    public DataCaptureContext DataCaptureContext { get; } = dataCaptureContext;

    public LabelCaptureBasicOverlay BuildOverlay()
    {
        return this.labelCaptureOverlay ??= labelCaptureService.BuildOverlay();
    }

    public LabelCaptureValidationFlowOverlay GetValidationFlowOverlay()
    {
        return this.validationFlowOverlay ??= labelCaptureService.BuildValidationFlowOverlay(
            onLabelScanned: async label =>
            {
                // Pause scanning when a label is captured
                await this.PauseScanningAsync();
                // Show the alert
                await this.ShowLabelCapturedAlertAsync(label);
            });
    }

    private async Task PauseScanningAsync()
    {
        await cameraService.PauseFrameSourceAsync();
        labelCaptureService.Disable();
    }

    private async Task ResumeScanningAsync()
    {
        await cameraService.ResumeFrameSourceAsync();
        labelCaptureService.Enable();
    }

    private async Task ShowLabelCapturedAlertAsync(string message)
    {
        await messageService.ShowAsync(
            title: "Label captured",
            message: message,
            buttonText: "Continue scanning",
            onDismiss: async () => await this.ResumeScanningAsync());
    }

    public override async Task ResumeAsync()
    {
        var permissionStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();

        if (permissionStatus != PermissionStatus.Granted)
        {
            permissionStatus = await Permissions.RequestAsync<Permissions.Camera>();
            if (permissionStatus != PermissionStatus.Granted)
            {
                await messageService.ShowAsync(
                    title: "Camera permission denied",
                    message: "You need to grant camera access in order to scan labels.");
                return;
            }
        }

        await cameraService.ResumeFrameSourceAsync();
        labelCaptureService.Enable();

        this.validationFlowOverlay?.OnResume();
    }

    public override async Task SleepAsync()
    {
        this.validationFlowOverlay?.OnPause();

        await cameraService.PauseFrameSourceAsync();
        labelCaptureService.Disable();
    }
}
