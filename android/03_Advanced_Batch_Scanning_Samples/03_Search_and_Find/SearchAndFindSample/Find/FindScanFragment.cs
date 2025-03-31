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
using AndroidX.Lifecycle;
using Java.Lang;
using Scandit.DataCapture.Barcode.Data;
using Scandit.DataCapture.Barcode.Find.UI;
using SearchAndFindSample.Find;
using SearchAndFindSample.Search;
using Fragment = AndroidX.Fragment.App.Fragment;

namespace SearchAndFindSample.Find;

public class FindScanFragment : Fragment
{
    private const string FieldSymbology = "symbology";
    private const string FieldData = "data";

    public FindScanFragment(Barcode barcode)
    {
        Bundle arguments = new Bundle();
        arguments.PutInt(FieldSymbology, (int)barcode.Symbology);
        arguments.PutString(FieldData, barcode.Data);
        this.Arguments = arguments;
    }

    private FindScanViewModelFactory viewModelFactory = null!;
    private FindScanViewModel viewModel = null!;
    private BarcodeFindView barcodeFindView = null!;

    public override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        this.HasOptionsMenu = true;

        if (this.viewModelFactory == null)
        {
            Bundle arguments = this.Arguments ?? throw new ArgumentNullException(nameof(savedInstanceState));
            Symbology symbology = (Symbology)arguments.GetInt(FieldSymbology);
            string data = arguments.GetString(FieldData) ?? string.Empty;
            this.viewModelFactory = new FindScanViewModelFactory(symbology, data);
        }

        this.viewModel = (FindScanViewModel)new ViewModelProvider(this, this.viewModelFactory)
            .Get(Class.FromType(typeof(FindScanViewModel)));
    }

    public override View? OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState)
    {
        ViewGroup? rootView = (ViewGroup?)inflater.Inflate(Resource.Layout.fragment_find, container, false);

        if (rootView == null)
        {
            return null;
        }

        // The BarcodeFindView will automatically be added to the root view when created.
        this.barcodeFindView = BarcodeFindView.Create(
                rootView,
                this.viewModel.DataCaptureContext,
                this.viewModel.BarcodeFind,
                // With the BarcodeFindViewSettings, we can defined haptic and sound feedback,
                // as well as change the visual feedback for found barcodes.
                new BarcodeFindViewSettings()
        );

        return rootView;
    }

    public override void OnViewCreated(View view, Bundle? savedInstanceState)
    {
        base.OnViewCreated(view, savedInstanceState);

        this.barcodeFindView.StartSearching();
        this.barcodeFindView.FinishButtonTapped += FinishButtonTapped;
    }

    private void FinishButtonTapped(object? sender, FinishButtonTappedEventArgs e)
    {
        // Called from the UI thread, this method is called when the user presses the
        // finish button. It returns the list of all items that were found during
        // the session.
        this.RequireActivity().OnBackPressed();
    }

    public override void OnResume()
    {
        base.OnResume();

        // Resume finding by calling the BarcodeFindView OnResume method.
        // Under the hood, it re-enables the BarcodeFind mode and makes sure the view is properly
        // setup.
        this.barcodeFindView.OnResume();
    }

    public override void OnPause()
    {
        base.OnPause();

        // Pause finding by calling the BarcodeFindView OnPause method.
        // Under the hood, it will disable the mode and free resources that are not needed in a
        // paused state.
        this.barcodeFindView.OnPause();
    }

    [Obsolete]
    public override bool OnOptionsItemSelected(IMenuItem item)
    {
        if (item.ItemId == Android.Resource.Id.Home)
        {
            this.RequireActivity().OnBackPressed();
            return true;
        }
        else
        {
            return base.OnOptionsItemSelected(item);
        }
    }
}
