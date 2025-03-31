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

using System.Timers;
using Timer = System.Timers.Timer;
using BarcodeSelectionSimpleSample.Views;

namespace BarcodeSelectionSimpleSample.Services
{
    public class MessageService : IMessageService
    {
        private const int messageAutoDissmissInterval = 500;
        private readonly Timer messageAutoDismissTimer;

        public MessageService()
        {
            this.messageAutoDismissTimer = new Timer(messageAutoDissmissInterval)
            {
                AutoReset = false,
                Enabled = false
            };
            this.messageAutoDismissTimer.Elapsed += (object sender, ElapsedEventArgs e) =>
            {
                DismissScanResults();
            };
        }

        public Task ShowAsync(string message)
        {
            return Application.Current.Dispatcher.DispatchAsync(() =>
            {
                if (App.Current.MainPage is MainPage mainPage)
                {
                    this.messageAutoDismissTimer.Stop();
                    this.messageAutoDismissTimer.Start();

                    mainPage.ShowScanResults(message);
                }
            });
        }

        private static void DismissScanResults()
        {
            Application.Current.Dispatcher.Dispatch(() =>
            {
                if (Application.Current.MainPage is MainPage mainPage)
                {
                    mainPage.HideScanResults();
                }
            });
        }
    }
}
