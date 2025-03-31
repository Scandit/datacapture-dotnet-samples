/*
 * This file is part of the Scandit Data Capture SDK
 *
 * Copyright (C) 2022- Scandit AG. All rights reserved.
 */

#nullable enable

namespace DebugAppMaui.Services
{
    public class MessageService : IMessageService
    {
        private Application? app => Application.Current;

        public Task Show(string message)
        {
            if (app == null)
            {
                return Task.CompletedTask;
            }

            if (app.Dispatcher.IsDispatchRequired)
            {
                return app.Dispatcher.DispatchAsync(() => ShowAction(message));
            }
            else
            {
                return ShowAction(message);
            }
        }

        private Task ShowAction(string message)
        {
            return app?.MainPage?.DisplayAlert("Scandit", message, "OK") ?? Task.CompletedTask;
        }
    }
}
