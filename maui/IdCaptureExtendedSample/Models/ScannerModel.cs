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
using Scandit.DataCapture.ID.Capture;
using Scandit.DataCapture.ID.Data;

namespace IdCaptureExtendedSample.Models
{
    public class ScannerModel
    {
        private const string SCANDIT_LICENSE_KEY = "-- ENTER YOUR SCANDIT LICENSE KEY HERE --";

        private static readonly Lazy<ScannerModel> INSTANCE = new Lazy<ScannerModel>(() => new ScannerModel(), LazyThreadSafetyMode.PublicationOnly);
        private readonly List<IIdCaptureDocument> acceptedDocuments = [
            new IdCard(IdCaptureRegion.Any),
            new DriverLicense(IdCaptureRegion.Any),
            new Passport(IdCaptureRegion.Any),
        ];

        public static ScannerModel Instance => INSTANCE.Value;

        public Mode Mode { get; private set; } = Mode.Barcode;

        private ScannerModel()
        {
            CurrentCamera?.ApplySettingsAsync(CameraSettings);

            // Create data capture context using your license key and set the camera as the frame source.
            DataCaptureContext = DataCaptureContext.ForLicenseKey(SCANDIT_LICENSE_KEY);
            DataCaptureContext.SetFrameSourceAsync(CurrentCamera);

            ConfigureIdCapture(Mode);
        }

        #region DataCaptureContext
        public DataCaptureContext DataCaptureContext { get; }
        #endregion

        #region CamerSettings
        public Camera CurrentCamera { get; } = Camera.GetCamera(CameraPosition.WorldFacing);
        private CameraSettings CameraSettings { get; } = IdCapture.RecommendedCameraSettings;
        #endregion

        #region IdCapture
        public IdCapture IdCapture { get; private set; }
        public IdCaptureSettings IdCaptureSettings { get; private set; }
        #endregion

        public void ConfigureIdCapture(Mode mode)
        {
            Mode = mode;

            // Create a mode responsible for recognizing documents. This mode is automatically added
            // to the passed DataCaptureContext.
            var settings = new IdCaptureSettings();

            switch (Mode)
            {
                case Mode.VIZ:
                    ConfigureVizMode(settings);
                    break;
                case Mode.MRZ:
                    ConfigureMrzMode(settings);
                    break;
                case Mode.Barcode:
                default:
                    ConfigureBarcodeMode(settings);
                    break;
            }

            if (IdCapture == null)
            {
                IdCapture = IdCapture.Create(DataCaptureContext, settings);
            }
            else
            {
                IdCapture.ApplySettings(settings);
            }
        }

        private void ConfigureBarcodeMode(IdCaptureSettings settings)
        {
            settings.AcceptedDocuments = acceptedDocuments;
            settings.ScannerType = 
                new SingleSideScanner(barcode: true, machineReadableZone: false, visualInspectionZone: false);
            settings.SetShouldPassImageTypeToResult(IdImageType.Face, shouldPass: true);
        }

        private void ConfigureVizMode(IdCaptureSettings settings)
        {
            settings.AcceptedDocuments = acceptedDocuments;
            settings.ScannerType = 
                new SingleSideScanner(barcode: false, machineReadableZone: false, visualInspectionZone: true);
            settings.SetShouldPassImageTypeToResult(IdImageType.Face, shouldPass: true);
            settings.SetShouldPassImageTypeToResult(IdImageType.CroppedDocument, shouldPass: true);
        }

        private void ConfigureMrzMode(IdCaptureSettings settings)
        {
            settings.AcceptedDocuments = acceptedDocuments;
            settings.ScannerType = 
                new SingleSideScanner(barcode: false, machineReadableZone: true, visualInspectionZone: false);
        }
    }
}
