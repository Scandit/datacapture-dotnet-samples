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

using Scandit.DataCapture.Core.Capture;
using Scandit.DataCapture.Core.Source;
using Scandit.DataCapture.Label.Capture;

namespace LabelCaptureSimpleSample.Services.Internals;

internal sealed class CameraService : ICameraService
{
    private Camera? camera;

    public CameraSettings CameraSettings { get; } = LabelCapture.RecommendedCameraSettings;

    public void Initialize(DataCaptureContext dataCaptureContext)
    {
        ArgumentNullException.ThrowIfNull(dataCaptureContext);

        // Use the default camera and set it as the frame source of the context.
        // The camera is off by default and must be turned on to start streaming frames to the data
        // capture context for recognition.
        // See ResumeFrameSource and PauseFrameSource below.
        this.camera = Camera.GetDefaultCamera(this.CameraSettings);

        if (this.camera != null)
        {
            dataCaptureContext.SetFrameSourceAsync(this.camera);
        }
    }

    public void PauseFrameSource()
    {
        // Switch the camera off to stop streaming frames.
        // The camera is stopped asynchronously and will take some time to completely turn off.
        this.camera?.SwitchToDesiredStateAsync(FrameSourceState.Off);
    }

    public void ResumeFrameSource()
    {
        // Switch the camera on to start streaming frames.
        // The camera is started asynchronously and will take some time to completely turn on.
        this.camera?.SwitchToDesiredStateAsync(FrameSourceState.On);
    }
}