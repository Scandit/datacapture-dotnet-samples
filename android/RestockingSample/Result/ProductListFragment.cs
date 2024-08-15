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

using Android.Content;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using RestockingSample.Pick;
using RestockingSample.Products;
using Fragment = AndroidX.Fragment.App.Fragment;

namespace RestockingSample.Result;

public class ProductListFragment : Fragment
{
    private RecyclerView resultList = null!;

    public static ProductListFragment Create()
    {
        return new ProductListFragment();
    }

    public override void OnCreate(Bundle? savedInstanceState)
    {
        HasOptionsMenu = true;
        base.OnCreate(savedInstanceState);
    }

    public override View? OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState)
    {
        View view = inflater.Inflate(Resource.Layout.fragment_item_list, container, false) ??
            throw new InvalidOperationException(
                "LayoutInflater failed to inflate the view. 'fragment_item_list' layout could not be found.");

        ActionBar? actionBar = ((MainActivity)RequireActivity()).ActionBar;

        if (actionBar != null)
        {
            actionBar.SetTitle(Resource.String.result_list);
            actionBar.SetDisplayHomeAsUpEnabled(true);
        }

        resultList = view.FindViewById<RecyclerView>(Resource.Id.result_list) ??
            throw new InvalidOperationException(
                "Unable to find RecycleView with the specified Id. Ensure the `Resource.Id.result_list` is correct " +
                "and the view is properly initialized.");

        Button scanButton = view.FindViewById<Button>(Resource.Id.continueScanningButton) ??
            throw new InvalidOperationException(
                "Unable to find Button with the specified Id. Ensure the `Resource.Id.continueScanningButton` is " +
                "correct and the view is properly initialized.");

        scanButton.Click += (sender, args) => ContinueScanning();

        Button finishButton = view.FindViewById<Button>(Resource.Id.finishScanningButton) ??
            throw new InvalidOperationException(
                "Unable to find Button with the specified Id. Ensure the `Resource.Id.finishScanningButton` is " +
                "correct and the view is properly initialized.");

        finishButton.Click += (sender, args) => FinishPickup();
        return view;
    }

    public override void OnViewCreated(View view, Bundle? savedInstanceState)
    {
        Context context = view.Context ?? throw new InvalidOperationException(
            "Failed to retrieve the view context. Ensure 'view.Context' is not null.");

        resultList.SetLayoutManager(new LinearLayoutManager(view.Context));
        resultList.SetAdapter(new ProductListAdapter(ProductManager.Instance.GetPickResultContent()));
        resultList.AddItemDecoration(new SeparatorDecorator(view.Context));
    }

    public override bool OnContextItemSelected(IMenuItem item)
    {
        if (item.ItemId == Android.Resource.Id.Home)
        {
            RequireActivity().OnBackPressedDispatcher.OnBackPressed();
            return true;
        }

        return base.OnContextItemSelected(item);
    }

    private void ContinueScanning()
    {
        RequireActivity().OnBackPressedDispatcher.OnBackPressed();
    }

    private void FinishPickup()
    {
        // Remove all previously scanned and picked products to reset the process.
        ProductManager.Instance.ClearPickedAndScanned();
        ParentFragmentManager.PopBackStack(nameof(PickFragment), (int)PopBackStackFlags.Inclusive);
        PickFragment pickFragment = PickFragment.Create();
        ParentFragmentManager.BeginTransaction()
            .Replace(Resource.Id.container, pickFragment, nameof(PickFragment))
            .Commit();
    }
}
