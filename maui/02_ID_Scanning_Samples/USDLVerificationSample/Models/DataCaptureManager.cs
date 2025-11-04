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

namespace USDLVerificationSample.Models
{
    public class DataCaptureManager
    {
        // Enter your Scandit License key here.
        // Your Scandit License key is available via your Scandit SDK web account.
        private const string SCANDIT_LICENSE_KEY = "-- ENTER YOUR SCANDIT LICENSE KEY HERE --";

        private static readonly Lazy<DataCaptureManager> instance = new(
            valueFactory: () => new DataCaptureManager(), LazyThreadSafetyMode.PublicationOnly);

        public static DataCaptureManager Instance => instance.Value;

        private DataCaptureManager()
        {
            this.CurrentCamera.ApplySettingsAsync(this.CameraSettings);

            this.DataCaptureContext = DataCaptureContext.ForLicenseKey(SCANDIT_LICENSE_KEY);
            this.DataCaptureContext.SetFrameSourceAsync(this.CurrentCamera);

            this.ConfigureIdCapture();
        }

        #region DataCaptureContext
        public DataCaptureContext DataCaptureContext { get; private set; }
        #endregion

        #region CamerSettings
        public Camera CurrentCamera { get; } = Camera.GetCamera(CameraPosition.WorldFacing);
        public CameraSettings CameraSettings { get; } = IdCapture.RecommendedCameraSettings;
        #endregion

        #region IdCapture
        public IdCapture IdCapture { get; private set; }
        #endregion

        private void ConfigureIdCapture()
        {
            this.DataCaptureContext.RemoveAllModes();

            // Create a mode responsible for recognizing documents. This mode is automatically added
            // to the passed DataCaptureContext. Enable built-in verification - documents with inconsistent data,
            // forged AAMVA barcodes, or expired documents will be rejected.
            IdCaptureSettings settings = new()
            {
                AcceptedDocuments = [new DriverLicense(IdCaptureRegion.Us)],
                RejectForgedAamvaBarcodes = true,
                RejectInconsistentData = true,
                RejectExpiredIds = true,
            };
            settings.SetShouldPassImageTypeToResult(IdImageType.Face, true);
            settings.SetShouldPassImageTypeToResult(IdImageType.CroppedDocument, true);
            settings.Scanner = new IdCaptureScanner(physicalDocument: new FullDocumentScanner(), mobileDocument: null);

            this.IdCapture = IdCapture.Create(this.DataCaptureContext, settings);
        }
    }
}
