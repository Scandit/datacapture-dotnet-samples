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

using RestockingSample.Models.Products;
using Scandit.DataCapture.Barcode.Pick.Capture;
using Scandit.DataCapture.Barcode.Pick.UI;

namespace RestockingSample.ViewControllers;

[Register("PickViewController")]
public class PickViewController : UIViewController, IBarcodePickActionListener, IBarcodePickScanningListener
{
    private BarcodePick? _barcodePick;
    private BarcodePickView? _barcodePickView;

    public override void ViewWillAppear(bool animated)
    {
        base.ViewWillAppear(animated);
        _barcodePickView?.Start();
    }

    public override void ViewDidDisappear(bool animated)
    {
        base.ViewDidDisappear(animated);
        _barcodePickView?.Stop();
    }

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();
        this.SetupRecognition();
        this.Title = "Restocking";
    }

    public void SetupRecognition()
    {
        ArgumentNullException.ThrowIfNull(View, nameof(View));

        if (_barcodePickView != null)
        {
            _barcodePickView.Stop();
            ((UIView)_barcodePickView).RemoveFromSuperview();
            _barcodePickView.Dispose();
        }

        _barcodePick = BarcodePickManager.Instance.CreateBarcodePick();
        _barcodePickView = BarcodePickManager.Instance.CreateBarcodePickView(View.Bounds, _barcodePick);
        var platformView = (UIView)_barcodePickView;
        platformView.TranslatesAutoresizingMaskIntoConstraints = false;

        // Sets the listener that gets called when the user interacts with one of the
        // items on screen to pick or unpick them.
        _barcodePickView.AddActionListener(this);

        // Sets the listener to be notified for updates in scanning and picking.
        _barcodePick.AddScanningListener(this);

        // Assigns an event handler to the Finish button that navigates the user 
        // to the product list upon clicking.
        _barcodePickView.FinishButtonTapped +=
            (object? sender, FinishButtonTappedEventArgs args) => GoToItemList();

        View.AddSubview(_barcodePickView);
        View.SendSubviewToBack(_barcodePickView);
        NSLayoutConstraint.ActivateConstraints(new []
        {
            platformView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
            platformView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
            platformView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
            platformView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
        });
    }

    #region IBarcodePickActionListener
    // This method will be called when the user taps on a code that is not picked.
    public void OnPick(string itemData, BarcodePickActionCallback callback)
    {
        // Invokes the callback's OnFinish method to report the outcome of the pick action.
        ProductManager.Instance.PickItemAsync(itemData, callback);
    }
    // This method will be called when the user taps on a code that is already picked.
    public void OnUnpick(string itemData, BarcodePickActionCallback callback)
    {
        // Invokes the callback's OnFinish method to report the outcome of the unpick action.
        ProductManager.Instance.UnpickItemAsync(itemData, callback);
    }
    #endregion // IBarcodePickActionListener
    #region IBarcodePickScanningListener
    public void OnScanningSessionUpdated(BarcodePick barcodePick, BarcodePickScanningSession session)
    {
        ProductManager.Instance.SetAllScannedCodes(session.ScannedItems);
        ProductManager.Instance.SetAllPickedCodes(session.PickedItems);
    }

    public void OnScanningSessionCompleted(BarcodePick barcodePick, BarcodePickScanningSession session)
    {
        // Not relevant in this sample.
    }
    #endregion // IBarcodePickScanningListener

    private void GoToItemList()
    {
        void resetResults()
        {
            ProductManager.Instance.ClearPickedAndScanned();
            SetupRecognition();
        }

        this.NavigationController?.PushViewController(
            new ResultListViewController(finishButtonAction: resetResults), animated: true);
    }
}
