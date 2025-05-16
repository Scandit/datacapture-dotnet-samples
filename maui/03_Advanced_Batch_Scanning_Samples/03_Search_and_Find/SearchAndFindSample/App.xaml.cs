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
using SearchAndFindSample.Models;
using SearchAndFindSample.Views;

namespace SearchAndFindSample;

public partial class App
{
    // Enter your Scandit License key here.
    // Your Scandit License key is available via your Scandit SDK web account.
    public const string SCANDIT_LICENSE_KEY = "-- ENTER YOUR SCANDIT LICENSE KEY HERE --";
    private readonly NavigationPage navigationPage;

    public static class MessageKey
    {
        public const string OnStart = nameof(OnStart);
        public const string OnSleep = nameof(OnSleep);
        public const string OnResume = nameof(OnResume);
    }

    public App()
    {
        this.InitializeComponent();
        this.navigationPage = new NavigationPage(new SearchScanPage());
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(page: this.navigationPage);
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
