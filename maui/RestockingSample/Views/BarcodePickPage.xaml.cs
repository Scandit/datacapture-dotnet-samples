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

using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Layouts;
using RestockingSample.Models;
using RestockingSample.Models.Products;
using Scandit.DataCapture.Barcode.Pick.Capture;
using Scandit.DataCapture.Barcode.Pick.UI.Maui;

namespace RestockingSample.Views;

public partial class BarcodePickPage : ContentPage, IRecipient<ApplicationMessage>
{
    private BarcodePickView _barcodePickView;

    public BarcodePickPage()
    {
        this.Title = "Restocking";
        this.SubscribeToMessages();
        this.InitializeComponent();

        _viewModel.SetupRecognition();
        _barcodePickView = CreateBarcodePickView(_viewModel.BarcodePick);
    }

    private BarcodePickView CreateBarcodePickView(BarcodePick barcodePick)
    {
        var view = BarcodePickManager.Instance.CreateBarcodePickView(barcodePick);
        view.HandlerChanged += BarcodePickViewHandlerChanged;
        Layout.SetLayoutBounds(view, new Rect(0, 0, 1, 1));
        Layout.SetLayoutFlags(view, AbsoluteLayoutFlags.All);
        Layout.Add(view);

        return view;
    }

    private void BarcodePickViewHandlerChanged(object? sender, EventArgs e)
    {
        _barcodePickView.Start();

        // Sets the listener that gets called when the user interacts with one of the
        // items on screen to pick or unpick them.
        _barcodePickView.AddActionListener(_viewModel);

        // Assigns an event handler to the Finish button that navigates the user 
        // to the product list upon clicking.
        _barcodePickView.FinishButtonTapped += (sender, args) => FinishButtonClicked();
    }

    public void Receive(ApplicationMessage message)
    {
        switch (message.Value)
        {
            case App.MessageKey.OnResume:
            case App.MessageKey.OnStart:
                {
                    MainThread.InvokeOnMainThreadAsync(OnResumeAsync);
                    break;
                }
            case App.MessageKey.OnSleep:
                {
                    MainThread.InvokeOnMainThreadAsync(_barcodePickView.OnPause);
                    break;
                }

            default:
                break;
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _barcodePickView.Start();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _barcodePickView.Stop();
    }

    private async Task OnResumeAsync()
    {
        var permissionStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();

        if (permissionStatus != PermissionStatus.Granted)
        {
            permissionStatus = await Permissions.RequestAsync<Permissions.Camera>();
            if (permissionStatus == PermissionStatus.Granted)
            {
                _barcodePickView.OnResume();
            }
        }
        else
        {
            _barcodePickView.OnResume();
        }
    }

    /// <summary>
    /// Subscribes to application messages to handle lifecycle events.
    /// </summary>
    private void SubscribeToMessages()
    {
        WeakReferenceMessenger.Default.Register(recipient: this);
    }

    private void FinishButtonClicked()
    {
        if (Application.Current?.MainPage is NavigationPage navigation)
        {
            navigation.PushAsync(
                new ResultListPage(
                    pickedItems: ProductManager.Instance.BuildPickedProductsListAndSetPickedProductsCount(),
                    inventoryList: ProductManager.Instance.GetInventoryList(),
                    onExitAction: ResetResults));
        }
    }

    private void ResetResults()
    {
        _barcodePickView.Stop();
        _barcodePickView.OnDestroy();
        ProductManager.Instance.ClearPickedAndScanned();
        Layout.Remove(_barcodePickView);
        Layout.ChildAdded += async (sender, args) => await OnResumeAsync();
        _viewModel.SetupRecognition();
        _barcodePickView = CreateBarcodePickView(_viewModel.BarcodePick);
    }
}
