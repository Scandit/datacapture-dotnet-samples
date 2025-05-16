﻿/*
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

using BarcodeCaptureSimpleSample.Services;
using BarcodeCaptureSimpleSample.Services.Internals;
using BarcodeCaptureSimpleSample.ViewModels;
using BarcodeCaptureSimpleSample.Views;
using Scandit.DataCapture.Core.UI.Maui;

namespace BarcodeCaptureSimpleSample;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>()
               .ConfigureFonts(fonts =>
               {
                   fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
               })
               .ConfigureMauiHandlers(handler =>
               {
                   handler.AddHandler<DataCaptureView, DataCaptureViewHandler>();
               });

        // Register services
        builder.Services.AddSingleton<IDataCaptureManager, DataCaptureManager>();
        builder.Services.AddSingleton<IMessageService, MessageService>();
        
        // Register view models
        builder.Services.AddTransient<MainPageViewModel>();
        
        // Register pages
        builder.Services.AddTransient<MainPage>();

        return builder.Build();
    }
}
