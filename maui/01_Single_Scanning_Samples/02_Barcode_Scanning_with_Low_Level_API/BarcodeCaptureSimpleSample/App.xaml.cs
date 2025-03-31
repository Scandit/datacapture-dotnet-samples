/*
 * This file is part of the Scandit Data Capture SDK
 *
 * Copyright (C) 2022- Scandit AG. All rights reserved.
 */

#nullable enable

using DebugAppMaui.Services;
using DebugAppMaui.Views;

namespace DebugAppMaui;

public partial class App : Application
{
    public class MessageKeys
    {
        public const string OnStart = nameof(OnStart);
        public const string OnSleep = nameof(OnSleep);
        public const string OnResume = nameof(OnResume);
    }

    public App()
    {
        this.InitializeComponent();
        this.MainPage = new MainPage();

        DependencyService.RegisterSingleton<IMessageService>(new MessageService());
    }

    protected override void OnStart()
    {
        MessagingCenter.Send(this, MessageKeys.OnStart);
    }

    protected override void OnSleep()
    {
        MessagingCenter.Send(this, MessageKeys.OnSleep);
    }

    protected override void OnResume()
    {
        MessagingCenter.Send(this, MessageKeys.OnResume);
    }
}
