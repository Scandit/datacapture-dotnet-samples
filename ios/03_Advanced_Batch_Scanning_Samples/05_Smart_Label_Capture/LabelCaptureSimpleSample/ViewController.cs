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

using System.Reactive.Linq;
using LabelCaptureSimpleSample.ViewModels;
using Scandit.DataCapture.Core.UI;
using Scandit.DataCapture.Core.UI.Control;
using Scandit.DataCapture.Label.UI.Overlay;
using Scandit.DataCapture.Label.UI.Overlay.Validation;

namespace LabelCaptureSimpleSample;

public partial class ViewController(IntPtr handle) : UIViewController(handle)
{
    // Enter your Scandit License key here.
    // Your Scandit License key is available via your Scandit SDK web account.
    public const string SCANDIT_LICENSE_KEY = "-- ENTER YOUR SCANDIT LICENSE KEY HERE --";

    private ScanViewModel? viewModel;
    private IDisposable? subscription;
    private DataCaptureView? dataCaptureView;
    private LabelCaptureBasicOverlay? labelCaptureOverlay;
    private LabelCaptureValidationFlowOverlay? validationFlowOverlay;

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();

        this.viewModel = new ScanViewModel(SCANDIT_LICENSE_KEY);

        this.SetupDataCaptureView();
        this.SubscribeToMessages();
    }

    public override void ViewWillAppear(bool animated)
    {
        base.ViewWillAppear(animated);
        this.viewModel?.StartCamera();
    }

    public override void ViewWillDisappear(bool animated)
    {
        base.ViewWillDisappear(animated);
        this.viewModel?.StopCamera();
    }

    private void SetupDataCaptureView()
    {
        if (this.viewModel == null || this.View == null)
        {
            return;
        }

        // Create DataCaptureView first
        this.dataCaptureView = DataCaptureView.Create(this.viewModel.DataCaptureContext, this.View.Bounds);

        UIView platformView = this.dataCaptureView;
        platformView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight |
                                        UIViewAutoresizing.FlexibleWidth;

        // Now create overlays with the DataCaptureView reference
        this.labelCaptureOverlay = this.viewModel.BuildOverlay();
        this.validationFlowOverlay = this.viewModel.GetValidationFlowOverlay(this.dataCaptureView);

        this.dataCaptureView.AddOverlay(this.labelCaptureOverlay);
        this.dataCaptureView.AddOverlay(this.validationFlowOverlay);

        this.View.AddSubview(this.dataCaptureView);
        this.View.SendSubviewToBack(this.dataCaptureView);
    }

    private void SubscribeToMessages()
    {
        if (this.viewModel == null)
        {
            return;
        }

        var context = SynchronizationContext.Current ?? new SynchronizationContext();
        this.subscription = this.viewModel.Messages
            .ObserveOn(context)
            .Subscribe(this.ShowLabelCapturedAlert);
    }

    private void ShowLabelCapturedAlert(string message)
    {
        var alertController = UIAlertController.Create(
            title: "Label captured",
            message: message,
            preferredStyle: UIAlertControllerStyle.Alert);

        var continueAction = UIAlertAction.Create(
            title: "Continue scanning",
            style: UIAlertActionStyle.Default,
            handler: _ => this.OnDialogDismissed());

        alertController.AddAction(continueAction);

        this.PresentViewController(alertController, animated: true, completionHandler: null);
    }

    private void OnDialogDismissed()
    {
        this.viewModel?.OnDialogDismissed();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.subscription?.Dispose();
        }

        base.Dispose(disposing);
    }
}
