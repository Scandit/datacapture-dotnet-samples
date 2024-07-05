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

using CommunityToolkit.Mvvm.Messaging;
using Scandit.DataCapture.Barcode.Data;
using SearchAndFindSample.Models;
using SearchAndFindSample.Views;

namespace SearchAndFindSample;

public partial class App : Application
{
    // Enter your Scandit License key here.
    // Your Scandit License key is available via your Scandit SDK web account.
    public const string SCANDIT_LICENSE_KEY = "-- ENTER YOUR SCANDIT LICENSE KEY HERE --";

    public class MessageKey
    {
        public const string OnStart = nameof(OnStart);
        public const string OnSleep = nameof(OnSleep);
        public const string OnResume = nameof(OnResume);
    }

    public App()
    {
        this.InitializeComponent();

        var root = new SearchScanPage()
        {
            FindBarcode = (Barcode barcode) =>
            {
                if (this.MainPage is NavigationPage navigation)
                {
                    navigation.PushAsync(new FindBarcodePage(barcode));
                }
            }
        };

        this.MainPage = new NavigationPage(root);
    }

    protected override void OnStart()
    {
        WeakReferenceMessenger.Default.Send(new ApplicationMessage(MessageKey.OnStart));
    }

    protected override void OnSleep()
    {
        WeakReferenceMessenger.Default.Send(new ApplicationMessage(MessageKey.OnSleep));
    }

    protected override void OnResume()
    {
        WeakReferenceMessenger.Default.Send(new ApplicationMessage(MessageKey.OnResume));
    }
}
