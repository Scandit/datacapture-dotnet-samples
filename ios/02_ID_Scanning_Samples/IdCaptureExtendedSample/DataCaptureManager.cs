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

using System;
using System.Threading;
using Scandit.DataCapture.Core.Capture;
using Scandit.DataCapture.Core.Source;
using Scandit.DataCapture.ID.Capture;
using Scandit.DataCapture.ID.Data;

namespace IdCaptureExtendedSample
{
    public class DataCaptureManager
    {
        // Enter your Scandit License key here.
        // Your Scandit License key is available via your Scandit SDK web account.
        private static readonly string SCANDIT_LICENSE_KEY = "-- ENTER YOUR SCANDIT LICENSE KEY HERE --";

        private static readonly Lazy<DataCaptureManager> INSTANCE = new(
            valueFactory: () => new DataCaptureManager(), LazyThreadSafetyMode.PublicationOnly);
        private Mode Mode { get; set; } = Mode.Barcode;
        
        private readonly List<IIdCaptureDocument> acceptedDocuments = [
            new IdCard(IdCaptureRegion.Any),
            new DriverLicense(IdCaptureRegion.Any),
            new Passport(IdCaptureRegion.Any),
        ];

        public DataCaptureContext DataCaptureContext { get; private set; }
        public Camera Camera { get; private set; }
        public IdCapture IdCapture { get; private set; }

        public static DataCaptureManager Instance => INSTANCE.Value;

        private DataCaptureManager()
        {
            // Create DataCaptureContext using your license key.
            DataCaptureContext = DataCaptureContext.ForLicenseKey(SCANDIT_LICENSE_KEY);
        }

        public void InitializeCamera()
        {
            // Set the device's default camera as DataCaptureContext's FrameSource. DataCaptureContext
            // passes the frames from it's FrameSource to the added modes to perform capture.
            //
            // Since we are going to perform IdCapture in this sample, we initiate the camera with
            // the recommended settings for this mode.
            Camera = Camera.GetDefaultCamera();

            if (Camera != null)
            {
                // Use the settings recommended by IdCapture.
                Camera.ApplySettingsAsync(IdCapture.RecommendedCameraSettings);
                DataCaptureContext.SetFrameSourceAsync(Camera);
            }
        }

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
            settings.Scanner = new IdCaptureScanner(
                physicalDocument: new SingleSideScanner(
                    barcode: true, machineReadableZone: false, visualInspectionZone: false),
                mobileDocument: null);
            settings.SetShouldPassImageTypeToResult(IdImageType.Face, true);
        }

        private void ConfigureVizMode(IdCaptureSettings settings)
        {
            settings.AcceptedDocuments = acceptedDocuments;
            settings.Scanner = new IdCaptureScanner(
                physicalDocument: new SingleSideScanner(
                    barcode: false, machineReadableZone: false, visualInspectionZone: true),
                mobileDocument: null);
            settings.SetShouldPassImageTypeToResult(IdImageType.Face, true);
            settings.SetShouldPassImageTypeToResult(IdImageType.CroppedDocument, true);
        }

        private void ConfigureMrzMode(IdCaptureSettings settings)
        {
            settings.AcceptedDocuments = acceptedDocuments;
            settings.Scanner = new IdCaptureScanner( 
                physicalDocument: new SingleSideScanner(
                    barcode: false, machineReadableZone: true, visualInspectionZone: false),
                mobileDocument: null);
        }
    }
}
