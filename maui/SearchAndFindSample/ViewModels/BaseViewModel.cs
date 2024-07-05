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
using SearchAndFindSample.Models;

namespace SearchAndFindSample.ViewModels;

/// <summary>
/// Provides an abstract base class for a view model that supports property change notification and handling
/// incoming ApplicationMessage events.
/// </summary>
public abstract class BaseViewModel : INotifyPropertyChanged, IRecipient<ApplicationMessage>
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected BaseViewModel()
    {
        this.SubscribeToMessages();
    }

    /// <summary>
    /// Notifies subscribers about property changes.
    /// </summary>
    public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Gets or sets a value indicating whether the mode implemented by derived class is active.
    /// </summary>
    /// <remarks>
    /// This property is used for responding to application lifecycle events, ensuring that only the active mode is
    /// enabled or disabled as needed.
    /// </remarks>
    protected bool IsActive { get; set; }

    /// <summary>
    /// When overridden in a derived class, handles the application's resume event.
    /// </summary>
    public abstract Task ResumeAsync();

    /// <summary>
    /// When overridden in a derived class, handles the application's sleep event.
    /// </summary>
    public abstract Task SleepAsync();

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
                    if (this.IsActive)
                    {
                        MainThread.InvokeOnMainThreadAsync(this.ResumeAsync);
                    }
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
