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

using LabelCaptureSimpleSample.Services;
using LabelCaptureSimpleSample.Services.Internals;
using LabelCaptureSimpleSample.ViewModels;
using LabelCaptureSimpleSample.Views;
using Scandit.DataCapture.Barcode;
using Scandit.DataCapture.Core;
using Scandit.DataCapture.Core.Source;
using Scandit.DataCapture.Label;
using Scandit.DataCapture.Label.Capture;

namespace LabelCaptureSimpleSample;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        ScanditLabelCapture.Initialize();
        
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>()
               .ConfigureFonts(fonts =>
               {
                   fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
               })
               .UseScanditCore(configure =>
               {
                   configure.AddDataCaptureView();
               });

        // Register services
        builder.Services.AddDataCaptureContext(App.SCANDIT_LICENSE_KEY);
        builder.Services.AddCamera(configure =>
        {
            configure.Position = CameraPosition.WorldFacing;
            configure.Settings = LabelCapture.RecommendedCameraSettings;
        });
        builder.Services.AddSingleton<ICameraService, CameraService>();
        builder.Services.AddSingleton<ILabelCaptureService, LabelCaptureService>();
        builder.Services.AddSingleton<IMessageService, MessageService>();

        // Register view models
        builder.Services.AddTransient<MainPageViewModel>();

        // Register pages
        builder.Services.AddTransient<MainPage>();

        return builder.Build();
    }
}
