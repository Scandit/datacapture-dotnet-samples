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
using LabelCaptureSimpleSample.Models;

namespace LabelCaptureSimpleSample.ViewModels;

public abstract class BaseViewModel : INotifyPropertyChanged, IRecipient<ApplicationMessage>
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected BaseViewModel()
    {
        WeakReferenceMessenger.Default.Register(this);
    }

    public virtual void Receive(ApplicationMessage message)
    {
        switch (message.Key)
        {
            case App.MessageKey.OnResume:
                _ = this.ResumeAsync();
                break;
            case App.MessageKey.OnSleep:
                _ = this.SleepAsync();
                break;
        }
    }

    public virtual Task SleepAsync() => Task.CompletedTask;

    public virtual Task ResumeAsync() => Task.CompletedTask;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        this.OnPropertyChanged(propertyName);
        return true;
    }
}

