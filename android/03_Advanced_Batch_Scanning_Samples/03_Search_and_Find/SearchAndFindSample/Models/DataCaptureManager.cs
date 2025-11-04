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

namespace SearchAndFindSample;

public class DataCaptureManager
{
    // Enter your Scandit License key here.
    // Your Scandit License key is available via your Scandit SDK web account.
    private const string SCANDIT_LICENSE_KEY = "-- ENTER YOUR SCANDIT LICENSE KEY HERE --";

    private static readonly Lazy<DataCaptureManager> instance = new Lazy<DataCaptureManager>(
        () => new DataCaptureManager(), LazyThreadSafetyMode.PublicationOnly);

    public static DataCaptureManager Instance => instance.Value;
    public DataCaptureContext DataCaptureContext { get; init; }
    public Camera? Camera { get; init; }

    private DataCaptureManager()
    {
        this.Camera = Camera.GetDefaultCamera();

        this.DataCaptureContext = DataCaptureContext.ForLicenseKey(SCANDIT_LICENSE_KEY);
        this.DataCaptureContext.SetFrameSourceAsync(this.Camera);
    }
}
