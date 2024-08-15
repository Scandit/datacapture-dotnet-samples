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

using Android.Views;
using Scandit.DataCapture.Barcode.Pick.Capture;
using Scandit.DataCapture.Barcode.Pick.UI;
using Fragment = AndroidX.Fragment.App.Fragment;
using ActionBar = AndroidX.AppCompat.App.ActionBar;
using RestockingSample.Result;
using RestockingSample.Products;

namespace RestockingSample.Pick;

public class PickFragment : Fragment, IBarcodePickActionListener, IBarcodePickScanningListener
{
    private BarcodePickView _barcodePickView = null!;
    private BarcodePick _barcodePick = null!;

    public static PickFragment Create()
    {
        return new PickFragment();
    }

    public override View? OnCreateView(
        LayoutInflater inflater,
        ViewGroup? container,
        Bundle? savedInstanceState)
    {
        return inflater.Inflate(Resource.Layout.fragment_pick, container, false);
    }

    public override void OnViewCreated(View view, Bundle? savedInstanceState)
    {
        base.OnViewCreated(view, savedInstanceState);

        _barcodePick = BarcodePickManager.Instance.CreateBarcodePick();
        _barcodePickView = BarcodePickManager.Instance.CreateBarcodePickView((ViewGroup)view, _barcodePick);

        // Sets the listener that gets called when the user interacts with one of the
        // items on screen to pick or unpick them.
        _barcodePickView.AddActionListener(this);

        // Sets the listener to be notified for updates in scanning and picking.
        _barcodePick.AddScanningListener(this);

        // Assigns an event handler to the Finish button that navigates the user 
        // to the product list upon clicking.
        _barcodePickView.FinishButtonTapped +=
            (sender, args) => GoToItemList();

        // Start the scanning flow.
        // This will be automatically paused and restored when onResume and onPause are called.
        _barcodePickView.Start();
    }

    public override void OnResume()
    {
        base.OnResume();
        _barcodePickView.OnResume();
    }

    public override void OnPause()
    {
        base.OnPause();
        _barcodePickView.OnPause();
    }

    public override void OnDestroyView()
    {
        base.OnDestroyView();
        _barcodePickView.OnDestroy();
    }

    public override void OnHiddenChanged(bool hidden)
    {
        if (hidden)
        {
            _barcodePickView.Stop();
        }
        else
        {
            _barcodePickView.Start();
            SetupActionBar();
        }
    }

    #region IBarcodePickActionListener
    public void OnPick(string itemData, BarcodePickActionCallback callback)
    {
        // Invokes the callback's OnFinish method to report the outcome of the pick action.
        ProductManager.Instance.PickItemAsync(itemData, callback);
    }

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
        Fragment listFragment = ProductListFragment.Create();

        ParentFragmentManager.BeginTransaction()
            .Add(Resource.Id.container, listFragment, nameof(ProductListFragment))
            .Hide(this)
            .AddToBackStack(nameof(PickFragment))
            .Commit();
    }

    private void SetupActionBar()
    {
        ActionBar? actionBar = ((MainActivity)RequireActivity()).SupportActionBar;

        if (actionBar != null)
        {
            actionBar.SetTitle(Resource.String.app_name);
            actionBar.SetDisplayHomeAsUpEnabled(false);
        }
    }
}
