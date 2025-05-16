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

using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.Messaging;
using USDLVerificationSample.Models;

namespace USDLVerificationSample.ViewModels;

public class BaseViewModel : INotifyPropertyChanged, IRecipient<ApplicationMessage>
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public BaseViewModel()
    {
        this.SubscribeToMessages();
    }

    public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// When overridden in a derived class, handles the application's resume event.
    /// </summary>
    public virtual Task ResumeAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// When overridden in a derived class, handles the application's sleep event.
    /// </summary>
    public virtual Task SleepAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Receives a message of type ApplicationMessage.
    /// </summary>
    /// <param name="message">The message to be received and processed.</param>
    public void Receive(ApplicationMessage message)
    {
        switch (message.Value)
        {
            case App.MessageKey.OnResume:
                {
                    MainThread.InvokeOnMainThreadAsync(this.ResumeAsync);
                    break;
                }
            case App.MessageKey.OnSleep:
                {
                    MainThread.InvokeOnMainThreadAsync(this.SleepAsync);
                    break;
                }

            default:
                break;
        }
    }

    /// <summary>
    /// Subscribes to application messages to handle lifecycle events.
    /// </summary>
    private void SubscribeToMessages()
    {
        WeakReferenceMessenger.Default.Register(recipient: this);
    }
}
