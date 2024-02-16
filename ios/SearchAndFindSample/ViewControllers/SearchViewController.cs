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

using CoreFoundation;
using System;
using Scandit.DataCapture.Barcode.Capture;
using Scandit.DataCapture.Barcode.Data;
using Scandit.DataCapture.Barcode.Find.Capture;
using Scandit.DataCapture.Barcode.UI.Overlay;
using Scandit.DataCapture.Core.Capture;
using Scandit.DataCapture.Core.Data;
using Scandit.DataCapture.Core.Source;
using Scandit.DataCapture.Core.UI;
using Scandit.DataCapture.Core.UI.Style;
using Scandit.DataCapture.Core.UI.Viewfinder;
using SearchAndFindSample.Models;

namespace SearchAndFindSample.ViewControllers;

public partial class SearchViewController : UIViewController
{
    private static string presentFindViewControllerSegue = "presentFindViewControllerSegue";

    private DataCaptureContext context = DataCaptureManager.Instance.DataCaptureContext;
    private Camera? camera = DataCaptureManager.Instance.Camera;
    private BarcodeCapture barcodeCapture = SearchDataCaptureManager.Instance.BarcodeCapture;

    private DataCaptureView captureView = null!;
    private BarcodeCaptureOverlay overlay = null!;

    private string? displayedData;
    private Symbology? symbology;

    public SearchViewController(IntPtr handle) : base(handle)
    { }

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();

        this.Title = "SEARCH & FIND";
        this.NavigationItem.BackBarButtonItem = new UIBarButtonItem(
            title: "",
            style: UIBarButtonItemStyle.Plain,
            target: null,
            action: null);

        this.scannedBarcodeOverlay.Layer.CornerRadius = 16;
        this.scannedBarcodeOverlay.Layer.MaskedCorners =
            CoreAnimation.CACornerMask.MinXMinYCorner |
            CoreAnimation.CACornerMask.MaxXMinYCorner;

        this.SetupRecognition();
    }

    public override void ViewDidAppear(bool animated)
    {
        base.ViewDidAppear(animated);

        // Make sure barcode capture is the only mode associated with the context.
        this.context.RemoveAllModes();
        this.context.AddMode(this.barcodeCapture);

        // Enable barcode capture to resume processing frames.
        this.barcodeCapture.Enabled = true;

        // Switch camera on to start streaming frames. The camera is started asynchronously and will take some time to
        // completely turn on.
        this.camera?.SwitchToDesiredStateAsync(FrameSourceState.On);
    }

    public override void ViewWillDisappear(bool animated)
    {
        base.ViewWillDisappear(animated);

        // Disable barcode capture to stop processing frames.
        this.barcodeCapture.Enabled = false;
    }

    public override void PrepareForSegue(UIStoryboardSegue segue, NSObject? sender)
    {
        if (segue.DestinationViewController is FindViewController destination &&
            !string.IsNullOrEmpty(this.displayedData))
        {
            lock (this.displayedData)
            {
                destination.ItemToFind = this.displayedData;
                destination.Symbology = this.symbology;
            }
        }
    }

    [Action("searchButtonDidTouchUpInside:")]
    public void SearchButtonDidTouchUpInside(NSObject? sender)
    {
        if (!string.IsNullOrEmpty(this.displayedData))
        {
            lock (this.displayedData)
            {
                this.scannedBarcodeLabel.Text = null;
                this.scannedBarcodeOverlay.Hidden = true;
                this.dismissOverlayButton.Hidden = true;
                this.PerformSegue(identifier: presentFindViewControllerSegue, sender: this);
            }
        }
    }

    [Action("dismissOverlayButtonDidTouchUpInside:")]
    public void DismissOverlayButtonDidTouchUpInside(NSObject? sender)
    {
        this.scannedBarcodeOverlay.Hidden = true;
        this.dismissOverlayButton.Hidden = true;
        this.displayedData = null;
        this.symbology = null;
    }

    [Action("unwindFromSearchViewControllerWithSegue:")]
    public void UnwindFromSearchViewController(UIStoryboardSegue segue)
    { }

    private void SetupRecognition()
    {
        // Use the recommended camera settings for the BarcodeCapture mode as default settings.
        // The preferred resolution is automatically chosen, which currently defaults to HD on all devices.
        // Setting the preferred resolution to full HD helps to get a better decode range.
        var cameraSettings = BarcodeCapture.RecommendedCameraSettings;
        cameraSettings.PreferredResolution = VideoResolution.FullHd;

        this.camera?.ApplySettingsAsync(cameraSettings);

        // Subscribe to BarcodeScanned event to get informed of captured barcodes.
        this.barcodeCapture.BarcodeScanned += BarcodeScanned;

        // To visualize the on-going barcode tracking process on screen, setup a data capture view that renders the
        // camera preview. The view must be connected to the data capture context.
        this.captureView = DataCaptureView.Create(context: context, frame: this.View?.Bounds ?? CGRect.Empty);
        this.captureView.AutoresizingMask =
            UIViewAutoresizing.FlexibleWidth |
            UIViewAutoresizing.FlexibleHeight;

        this.View?.AddSubview(this.captureView);

        // Add a barcode capture overlay to the data capture view to render the location of captured barcodes on top of
        // the video preview. This is optional, but recommended for better visual feedback.
        this.overlay = BarcodeCaptureOverlay.Create(
            barcodeCapture: barcodeCapture,
            view: this.captureView,
            style: BarcodeCaptureOverlayStyle.Frame);

        // We add the aim viewfinder to the overlay.
        this.overlay.Viewfinder = new AimerViewfinder();

        // Adjust the overlay's barcode highlighting to display a light green rectangle.
        var matchColor = new UIColor(red: 40.0f / 255.0f, green: 211.0f / 255.0f, blue: 128.0f / 255.0f, alpha: 0.5f);
        this.overlay.Brush = new Brush(fillColor: matchColor, strokeColor: matchColor, strokeWidth: 2);
    }

    private void BarcodeScanned(object? sender, BarcodeCaptureEventArgs args)
    {
        lock (this)
        {
            if (args.Session.NewlyRecognizedBarcodes.Any())
            {
                var barcode = args.Session.NewlyRecognizedBarcodes.FirstOrDefault();
                this.displayedData = barcode?.Data;
                this.symbology = barcode?.Symbology;

                // This method is invoked on a non-UI thread, so in order to perform UI work,
                // we have to switch to the main thread.
                DispatchQueue.MainQueue.DispatchAsync(() =>
                {
                    this.scannedBarcodeLabel.Text = this.displayedData;

                    if (this.scannedBarcodeOverlay.Hidden)
                    {
                        this.scannedBarcodeOverlay.Hidden = false;
                        this.View?.BringSubviewToFront(this.scannedBarcodeOverlay);
                        this.dismissOverlayButton.Hidden = false;
                        this.View?.BringSubviewToFront(this.dismissOverlayButton);
                    }
                });
            }
        }
    }
}
