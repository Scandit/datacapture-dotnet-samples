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

namespace GS1ParserSample.Services
{
    public class MessageService : IMessageService
    {
        private static Application app => Application.Current;

        public Task ShowAsync(string message, string title)
        {
            if (app == null)
            {
                return Task.CompletedTask;
            }

            if (app.Dispatcher.IsDispatchRequired)
            {
                return app.Dispatcher.DispatchAsync(() => MessageService.ShowAction(message, title));
            }
            else
            {
                return MessageService.ShowAction(message, title);
            }
        }

        private static Task ShowAction(string message, string title)
        {
            return app?.MainPage?.DisplayAlert(title, message, "OK") ?? Task.CompletedTask;
        }
    }
}
