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

namespace LabelCaptureSimpleSample.Services.Internals;

internal sealed class CameraService : ICameraService
{
    private readonly Camera? camera;

    public CameraService(Camera camera, DataCaptureContext dataCaptureContext)
    {
        this.camera = camera;
        // The camera is off by default and must be turned on to start streaming frames to the data
        // capture context for recognition.
        // See ResumeFrameSourceAsync and PauseFrameSourceAsync below.
        if (this.camera != null)
        {
            dataCaptureContext.SetFrameSourceAsync(this.camera);
        }
    }

    public Task PauseFrameSourceAsync()
    {
        // Switch the camera off to stop streaming frames.
        // The camera is stopped asynchronously and will take some time to completely turn off.
        return this.camera?.SwitchToDesiredStateAsync(FrameSourceState.Off) ?? Task.CompletedTask;
    }

    public Task ResumeFrameSourceAsync()
    {
        // Switch the camera on to start streaming frames.
        // The camera is started asynchronously and will take some time to completely turn on.
        return this.camera?.SwitchToDesiredStateAsync(FrameSourceState.On) ?? Task.CompletedTask;
    }
}

