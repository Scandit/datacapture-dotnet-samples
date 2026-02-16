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

namespace LabelCaptureSimpleSample.Services.Internals;

internal class MessageService : IMessageService
{
    public async Task ShowAsync(string title, string message, string buttonText = "OK", Action? onDismiss = null)
    {
        var mainPage = Application.Current?.Windows[0].Page;
        if (mainPage != null)
        {
            await mainPage.DisplayAlert(title, message, buttonText);
            onDismiss?.Invoke();
        }
    }
}

