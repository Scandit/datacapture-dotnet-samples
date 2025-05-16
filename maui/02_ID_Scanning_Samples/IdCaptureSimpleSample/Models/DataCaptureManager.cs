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

using Scandit.DataCapture.ID.Capture;
using Scandit.DataCapture.ID.Data;
using Scandit.DataCapture.Core.Capture;
using Scandit.DataCapture.Core.Source;

namespace IdCaptureSimpleSample.Models
{
    public class DataCaptureManager
    {
        public const string SCANDIT_LICENSE_KEY = "-- ENTER YOUR SCANDIT LICENSE KEY HERE --";

        private static readonly Lazy<DataCaptureManager> instance = new(
            () => new DataCaptureManager(), LazyThreadSafetyMode.PublicationOnly);

        public static DataCaptureManager Instance => instance.Value;

        #region DataCaptureContext
        public DataCaptureContext DataCaptureContext { get; }
        #endregion

        #region CamerSettings
        public Camera? CurrentCamera { get; } = Camera.GetCamera(CameraPosition.WorldFacing);
        public CameraSettings CameraSettings { get; } = IdCapture.RecommendedCameraSettings;
        #endregion

        #region IdCapture
        public IdCapture IdCapture { get; }
        public IdCaptureSettings IdCaptureSettings { get; }
        #endregion

        private DataCaptureManager()
        {
            this.CurrentCamera?.ApplySettingsAsync(this.CameraSettings);

            // Create data capture context using your license key and set the camera as the frame source.
            this.DataCaptureContext = DataCaptureContext.ForLicenseKey(SCANDIT_LICENSE_KEY);
            this.DataCaptureContext.SetFrameSourceAsync(this.CurrentCamera);

            // Create a mode responsible for recognizing documents. This mode is automatically added
            // to the passed DataCaptureContext.
            this.IdCaptureSettings = new IdCaptureSettings
            {
                AcceptedDocuments = [
                    new IdCard(IdCaptureRegion.Any),
                    new DriverLicense(IdCaptureRegion.Any),
                    new Passport(IdCaptureRegion.Any)
                ],
                ScannerType = new FullDocumentScanner()
            };

            this.IdCapture = IdCapture.Create(this.DataCaptureContext, this.IdCaptureSettings);
        }
    }
}
