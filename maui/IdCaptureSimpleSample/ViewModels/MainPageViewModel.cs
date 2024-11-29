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

using System.Text;
using System.Text.RegularExpressions;
using IdCaptureSimpleSample.Models;
using IdCaptureSimpleSample.Services;

using Scandit.DataCapture.Core.Capture;
using Scandit.DataCapture.Core.Data;
using Scandit.DataCapture.Core.Source;
using Scandit.DataCapture.ID.Capture;
using Scandit.DataCapture.ID.Data;

namespace IdCaptureSimpleSample.ViewModels
{
    public class MainPageViewModel : BaseViewModel, IIdCaptureListener
    {
        private readonly ScannerModel model = ScannerModel.Instance;

        public DataCaptureContext DataCaptureContext => this.model.DataCaptureContext;
        public IdCapture IdCapture => this.model.IdCapture;

        public MainPageViewModel()
        {
            this.InitializeScanner();
            this.SubscribeToAppMessages();
        }

        #region IIdCaptureListener
        public void OnIdCaptured(IdCapture capture, CapturedId capturedId)
        {
            // Don't capture unnecessarily when the result is displayed.
            this.IdCapture.Enabled = false;
            
            string message = GetDescriptionForCapturedId(capturedId);

            DependencyService.Get<IMessageService>()
                             .ShowAsync(message)
                             .ContinueWith((task) => this.IdCapture.Enabled = true);
        }

        public void OnIdRejected(IdCapture mode, CapturedId capturedId, RejectionReason reason)
        {
            this.IdCapture.Enabled = false;
            
            String message;
            
            switch (reason) {
                case RejectionReason.NotAcceptedDocumentType:
                    message = "Document not supported. Try scanning another document.";
                    break;
                default:
                    message = $"Document capture was rejected. Reason={reason}.";
                    break;
            }

            DependencyService.Get<IMessageService>()
                             .ShowAsync(message)
                             .ContinueWith((task) =>
                             {
                                 // On alert dialog completion resume the IdCapture.
                                 this.IdCapture.Enabled = true;
                             });
        }
        #endregion

        public Task OnSleepAsync()
        {
            return this.model.CurrentCamera?.SwitchToDesiredStateAsync(FrameSourceState.Off);
        }

        public async Task OnResumeAsync()
        {
            var permissionStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();

            if (permissionStatus != PermissionStatus.Granted)
            {
                permissionStatus = await Permissions.RequestAsync<Permissions.Camera>();
                if (permissionStatus == PermissionStatus.Granted)
                {
                    await this.ResumeFrameSourceAsync();
                }
            }
            else
            {
                await this.ResumeFrameSourceAsync();
            }
        }

        private void InitializeScanner()
        {
            // Start listening on IdCapture events.
            this.model.IdCapture.AddListener(this);
            this.model.IdCapture.Enabled = true;
        }

        private void SubscribeToAppMessages()
        {
            MessagingCenter.Subscribe(this, App.MessageKeys.OnResume, callback: async (App app) => await this.OnResumeAsync());
            MessagingCenter.Subscribe(this, App.MessageKeys.OnSleep, callback: async (App app) => await this.OnSleepAsync());
        }

        private Task ResumeFrameSourceAsync()
        {
            // Switch camera on to start streaming frames.
            // The camera is started asynchronously and will take some time to completely turn on.
            return this.model.CurrentCamera?.SwitchToDesiredStateAsync(FrameSourceState.On);
        }

        private static string GetDescriptionForCapturedId(CapturedId result)
        {
            StringBuilder builder = new StringBuilder();
            AppendDescriptionForCapturedId(result, builder);

            return builder.ToString();
        }
        
        private static void AppendDescriptionForCapturedId(CapturedId result, StringBuilder builder)
        {
            AppendField(builder, "Full Name: ", result.FullName);
            AppendField(builder, "Date of Birth: ", result.DateOfBirth);
            AppendField(builder, "Date of Expiry: ", result.DateOfExpiry);
            AppendField(builder, "Document Number: ", result.DocumentNumber);
            AppendField(builder, "Nationality: ", result.Nationality);
        }

        private static void AppendField(StringBuilder builder, string name, string value)
        {
            builder.Append(name);

            if (string.IsNullOrEmpty(value))
            {
                builder.Append("<empty>");
            }
            else
            {
                builder.Append(value);
            }

            builder.Append(System.Environment.NewLine);
        }

        private static void AppendField(StringBuilder builder, string name, DateResult value)
        {
            builder.Append(name);

            if (value == null)
            {
                builder.Append("<empty>");
            }
            else
            {
                builder.Append(value.Date.ToString());
            }

            builder.Append(System.Environment.NewLine);
        }
    }
}
