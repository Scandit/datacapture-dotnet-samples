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

using RestockingSample.Models;
using RestockingSample.Models.Products;
using Scandit.DataCapture.Barcode.Pick.Capture;
using Scandit.DataCapture.Core.Capture;

namespace RestockingSample.ViewModels;

public class BarcodePickViewModel : BaseViewModel, IBarcodePickActionListener, IBarcodePickScanningListener
{
    private readonly BarcodePickManager _manager = BarcodePickManager.Instance;

    public DataCaptureContext DataCaptureContext => _manager.DataCaptureContext;
    public BarcodePick BarcodePick { get; private set; } = null!;

    public void SetupRecognition()
    {
        this.BarcodePick = _manager.CreateBarcodePick();

        // Sets the listener to be notified for updates in scanning and picking.
        this.BarcodePick.AddScanningListener(this);
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
}
