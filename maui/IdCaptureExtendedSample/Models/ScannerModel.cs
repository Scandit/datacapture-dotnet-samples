﻿/*
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
        public const string SCANDIT_LICENSE_KEY = "-- ENTER YOUR SCANDIT LICENSE KEY HERE --";

        private static readonly Lazy<ScannerModel> instance = new Lazy<ScannerModel>(() => new ScannerModel(), LazyThreadSafetyMode.PublicationOnly);

        public static ScannerModel Instance => instance.Value;

        public Mode Mode { get; private set; } = Mode.Barcode;

        private ScannerModel()
        {
            this.CurrentCamera?.ApplySettingsAsync(this.CameraSettings);

            // Create data capture context using your license key and set the camera as the frame source.
            this.DataCaptureContext = DataCaptureContext.ForLicenseKey(SCANDIT_LICENSE_KEY);
            this.DataCaptureContext.SetFrameSourceAsync(this.CurrentCamera);

            this.ConfigureIdCapture(this.Mode);
        }

        #region DataCaptureContext
        public DataCaptureContext DataCaptureContext { get; private set; }
        #endregion

        #region CamerSettings
        public Camera CurrentCamera { get; set; } = Camera.GetCamera(CameraPosition.WorldFacing);
        public CameraSettings CameraSettings { get; set; } = IdCapture.RecommendedCameraSettings;
        #endregion

        #region IdCapture
        public IdCapture IdCapture { get; private set; }
        public IdCaptureSettings IdCaptureSettings { get; private set; }
        #endregion

        public void ConfigureIdCapture(Mode mode)
        {
            this.Mode = mode;
            this.DataCaptureContext.RemoveAllModes();

            // Create a mode responsible for recognizing documents. This mode is automatically added
            // to the passed DataCaptureContext.
            IdCaptureSettings settings = new IdCaptureSettings();

            switch (this.Mode)
            {
                case Mode.VIZ:
                    this.ConfigureVIZMode(settings);
                    break;
                case Mode.MRZ:
                    this.ConfigureMRZMode(settings);
                    break;
                case Mode.Barcode:
                default:
                    this.ConfigureBarcodeMode(settings);
                    break;
            }

            this.IdCapture = IdCapture.Create(this.DataCaptureContext, settings);
        }

        private void ConfigureBarcodeMode(IdCaptureSettings settings)
        {
            settings.SupportedDocuments = IdDocumentType.AamvaBarcode |
                                          IdDocumentType.ArgentinaIdBarcode |
                                          IdDocumentType.ColombiaIdBarcode |
                                          IdDocumentType.SouthAfricaDlBarcode |
                                          IdDocumentType.SouthAfricaIdBarcode |
                                          IdDocumentType.UsUsIdBarcode;
        }

        private void ConfigureVIZMode(IdCaptureSettings settings)
        {
            settings.SupportedDocuments = IdDocumentType.DlViz | IdDocumentType.IdCardViz;
            settings.SetShouldPassImageTypeToResult(IdImageType.Face, true);
            settings.SetShouldPassImageTypeToResult(IdImageType.IdBack, true);
            settings.SetShouldPassImageTypeToResult(IdImageType.IdFront, true);
            settings.SupportedSides = SupportedSides.FrontAndBack;
        }

        private void ConfigureMRZMode(IdCaptureSettings settings)
        {
            settings.SupportedDocuments = IdDocumentType.VisaMrz |
                                          IdDocumentType.PassportMrz |
                                          IdDocumentType.IdCardMrz |
                                          IdDocumentType.SwissDlMrz;
        }
    }
}
