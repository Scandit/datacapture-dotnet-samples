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

using USDLVerificationSample.ViewModels;
using Scandit.DataCapture.ID.Data;
using USDLVerificationSample.Models;

namespace USDLVerificationSample.Views
{
    public partial class ResultPage : ContentPage
    {
        public ResultPage(CapturedId capturedId, DriverLicenseVerificationResult verificationResult)
        {
            this.InitializeComponent();
            var resultViewModel = new ResultViewModel(capturedId, verificationResult);
            this.ExpirationLabel.TextColor = !resultViewModel.IsExpired ? Colors.Green : Colors.Red;
            this.BarcodeVerificationLabel.TextColor = resultViewModel.BarcodeVerificationPass ? Colors.Green : Colors.Red;
            this.BindingContext = resultViewModel;
        }
    }
}
