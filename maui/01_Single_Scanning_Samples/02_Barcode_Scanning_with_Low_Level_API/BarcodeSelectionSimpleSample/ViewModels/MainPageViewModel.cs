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

using BarcodeSelectionSimpleSample.Models;
using BarcodeSelectionSimpleSample.Services;

using Scandit.DataCapture.Barcode.Data;
using Scandit.DataCapture.Barcode.Selection.Capture;
using Scandit.DataCapture.Core.Capture;
using Scandit.DataCapture.Core.Source;

namespace BarcodeSelectionSimpleSample.ViewModels
{
    public class MainPageViewModel : BaseViewModel
    {
        public Camera? Camera => DataCaptureManager.Instance.CurrentCamera;
        public DataCaptureContext DataCaptureContext => DataCaptureManager.Instance.DataCaptureContext;
        public BarcodeSelection BarcodeSelection => DataCaptureManager.Instance.BarcodeSelection;
        public BarcodeSelectionSettings BarcodeSelectionSettings => DataCaptureManager.Instance.BarcodeSelectionSettings;

        public MainPageViewModel()
        {
            // Subscribe to get informed whenever a new barcode got recognized.
            this.BarcodeSelection.SelectionUpdated += this.OnSelectionUpdated;
        }

        public override Task SleepAsync()
        {
            return this.Camera?.SwitchToDesiredStateAsync(FrameSourceState.Off) ?? Task.CompletedTask;
        }

        public override async Task ResumeAsync()
        {
            var permissionStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();

            if (permissionStatus != PermissionStatus.Granted)
            {
                permissionStatus = await Permissions.RequestAsync<Permissions.Camera>();
                if (permissionStatus == PermissionStatus.Granted)
                {
                    await this.ResumeFrameSource();
                }
            }
            else
            {
                await this.ResumeFrameSource();
            }
        }

        public bool SwitchToAimToSelectMode()
        {
            if (this.BarcodeSelectionSettings.SelectionType is BarcodeSelectionTapSelection)
            {
                this.BarcodeSelectionSettings.SelectionType = BarcodeSelectionAimerSelection.Create();
                this.BarcodeSelection
                    .ApplySettingsAsync(this.BarcodeSelectionSettings)
                    .ContinueWith((task) =>
                        // Switch the camera to On state in case it froze through
                        // double tap while on TapToSelect selection type.
                        this.Camera?.SwitchToDesiredStateAsync(FrameSourceState.On));

                return true;
            }

            return false;
        }

        public bool SwitchToTapToSelectMode()
        {
            if (this.BarcodeSelectionSettings.SelectionType is BarcodeSelectionAimerSelection)
            {
                this.BarcodeSelectionSettings.SelectionType = BarcodeSelectionTapSelection.Create();
                this.BarcodeSelection.ApplySettingsAsync(this.BarcodeSelectionSettings);

                return true;
            }

            return false;
        }

        private Task ResumeFrameSource()
        {
            // Switch camera on to start streaming frames.
            // The camera is started asynchronously and will take some time to completely turn on.
            return this.Camera?.SwitchToDesiredStateAsync(FrameSourceState.On) ?? Task.CompletedTask;
        }

        public void OnSelectionUpdated(object? sender, BarcodeSelectionEventArgs e)
        {
            if (!e.Session.NewlySelectedBarcodes.Any())
            {
                return;
            }

            Barcode barcode = e.Session.NewlySelectedBarcodes.First();

            // Get barcode selection count.
            int selectionCount = e.Session.GetCount(barcode);

            // Get the human readable name of the symbology and assemble the result to be shown.
            var description = new SymbologyDescription(barcode.Symbology);
            string result = barcode.Data + " (" + description.ReadableName + ")" + "\nTimes: " + selectionCount;

            DependencyService.Get<IMessageService>().ShowAsync(result);
        }
    }
}
