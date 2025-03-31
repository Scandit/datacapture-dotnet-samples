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

using Android.Graphics;
using Android.Views;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Lifecycle;
using Google.Android.Material.BottomSheet;
using Java.Lang;
using Scandit.DataCapture.Barcode.Data;
using Scandit.DataCapture.Barcode.UI.Overlay;
using Scandit.DataCapture.Core.UI;
using Scandit.DataCapture.Core.UI.Style;
using Scandit.DataCapture.Core.UI.Viewfinder;
using SearchAndFindSample.Find;

namespace SearchAndFindSample.Search;

public class SearchScanFragment : CameraPermissionFragment, View.IOnClickListener
{
    private SearchScanViewModel viewModel = null!;
    private BottomSheetBehavior? behavior;
    private TextView? textBarcode;
    private ImageButton? buttonSearch;

    public override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        this.viewModel = this.GetViewModelFromProvider();
        this.SubscribeToViewModelEvents(this.viewModel);
    }

    public override View? OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState)
    {
        CoordinatorLayout? root =
                (CoordinatorLayout?)inflater.Inflate(Resource.Layout.fragment_search, container, false);
        DataCaptureView dataCaptureView = this.CreateAndSetupDataCaptureView();

        // We put the dataCaptureView in its container.
        ViewGroup? scannerContainer = root?.FindViewById<ViewGroup>(Resource.Id.scanner_container);
        scannerContainer?.AddView(
            dataCaptureView,
            new ViewGroup.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.MatchParent));

        View? containerScannedCode = root?.FindViewById(Resource.Id.container_scanned_code);
        this.behavior = BottomSheetBehavior.From(containerScannedCode!);
        this.behavior.State = BottomSheetBehavior.StateHidden;
        this.behavior.AddBottomSheetCallback(new SheetCallback(this));
        this.textBarcode = root?.FindViewById<TextView>(Resource.Id.text_barcode);
        this.buttonSearch = root?.FindViewById<ImageButton>(Resource.Id.button_search);

        return root;
    }

    private SearchScanViewModel GetViewModelFromProvider() =>
        (SearchScanViewModel)new ViewModelProvider(this).Get(Class.FromType(typeof(SearchScanViewModel)));

    private void SubscribeToViewModelEvents(SearchScanViewModel viewModel)
    {
        viewModel.CodeScanned += CodeScanned;
        viewModel.FindBarcode = this.FindBarcode;
    }

    private void UnsubsribeFromViewModelEvents(SearchScanViewModel viewModel)
    {
        viewModel.CodeScanned -= CodeScanned;
        viewModel.FindBarcode = null;
    }

    private void CodeScanned(object? sender, BarcodeCodeEventArgs args)
    {
        this.ShowResult(args.Code);
    }

    private DataCaptureView CreateAndSetupDataCaptureView()
    {
        // To visualize the on-going barcode capturing process on screen,
        // setup a data capture view that renders the camera preview.
        // The view must be connected to the data capture context.
        DataCaptureView view = DataCaptureView.Create(this.RequireContext(), this.viewModel.DataCaptureContext);

        // Add a barcode capture overlay to the data capture view to set a viewfinder UI.
        BarcodeCaptureOverlay overlay = BarcodeCaptureOverlay.Create(
            this.viewModel.BarcodeCapture, view, BarcodeCaptureOverlayStyle.Frame);
        view.AddOverlay(overlay);

        // We add the aim viewfinder to the overlay.
        overlay.Viewfinder = new AimerViewfinder();

        // Adjust the overlay's barcode highlighting to display a light green rectangle.
        overlay.Brush = new Brush(Color.ParseColor("#8028D380"), Color.Transparent, 0f);
        return view;
    }

    public override void OnViewCreated(View view, Bundle? savedInstanceState)
    {
        base.OnViewCreated(view, savedInstanceState);

        this.buttonSearch?.SetOnClickListener(this);
    }

    void View.IOnClickListener.OnClick(View? v)
    {
        this.viewModel.OnSearchClicked();
    }

    public override void OnResume()
    {
        base.OnResume();

        // Check for camera permission and request it, if it hasn't yet been granted.
        // Once we have the permission the onCameraPermissionGranted() method will be called.
        this.RequestCameraPermission();
    }

    public override void OnPause()
    {
        this.PauseFrameSource();
        base.OnPause();
    }

    protected override void OnCameraPermissionGranted()
    {
        this.ResumeFrameSource();
    }

    private void FindBarcode(Barcode barcodeToFind)
    {
        this.HideResult();
        this.RequireActivity()
            .SupportFragmentManager
            .BeginTransaction()
            .Replace(Resource.Id.fragment_container, new FindScanFragment(barcodeToFind))
            .SetTransition((int)FragmentTransit.FragmentOpen)
            .AddToBackStack(null)
            .Commit();
    }

    private void ResumeFrameSource()
    {
        this.SubscribeToViewModelEvents(this.viewModel);

        // Resume scanning by enabling and re-adding the barcode capture mode.
        // Since barcode batch and barcode capture modes are not compatible (meaning they cannot
        // be added at the same time to the same data capture context) we're also removing and
        // adding them back whenever the scanning is paused/resumed.
        this.viewModel.ResumeScanning();
    }

    private void PauseFrameSource()
    {
        this.UnsubsribeFromViewModelEvents(this.viewModel);

        // Pause scanning by disabling the barcode capture mode.
        // Since barcode batch and barcode capture modes are not compatible (meaning they cannot
        // be added at the same time to the same data capture context) we're also removing and
        // adding them back whenever the scanning is paused/resumed.
        this.viewModel.PauseScanning();
    }

    private void ShowResult(string code)
    {
        if (this.textBarcode != null)
        {
            this.textBarcode.Text = code;
        }

        if (this.behavior != null)
        {
            this.behavior.State = BottomSheetBehavior.StateExpanded;
        }
    }

    private void HideResult()
    {
        if (this.behavior != null)
        {
            this.behavior.State = BottomSheetBehavior.StateHidden;
        }
    }

    private class SheetCallback : BottomSheetBehavior.BottomSheetCallback
    {
        private readonly SearchScanFragment scanFragment;

        public SheetCallback(SearchScanFragment scanFragment)
        {
            this.scanFragment = scanFragment;
        }

        public override void OnSlide(View bottomSheet, float newState)
        {
            // Nothing to do.
        }

        public override void OnStateChanged(View bottomSheet, int newState)
        {
            if (this.scanFragment.buttonSearch != null)
            {
                if (newState == BottomSheetBehavior.StateExpanded)
                {
                    this.scanFragment.buttonSearch.Clickable = true;
                }
                else if (newState == BottomSheetBehavior.StateHidden)
                {
                    this.scanFragment.buttonSearch.Clickable = false;
                }
            }
        }
    }
}
