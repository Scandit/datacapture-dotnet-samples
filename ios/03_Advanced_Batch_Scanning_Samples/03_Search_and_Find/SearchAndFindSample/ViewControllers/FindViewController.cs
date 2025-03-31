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

using System;
using System.Collections.Generic;
using Scandit.DataCapture.Barcode.Data;
using Scandit.DataCapture.Barcode.Find.Capture;
using Scandit.DataCapture.Barcode.Find.UI;
using Scandit.DataCapture.Core.Capture;
using SearchAndFindSample.Models;

namespace SearchAndFindSample.ViewControllers;

public partial class FindViewController : UIViewController
{
    private static string unWindToSearchSegueIdentifier = "unWindToSearchSegueIdentifier";

    private DataCaptureContext context = DataCaptureManager.Instance.DataCaptureContext;
    private BarcodeFind barcodeFind = FindDataCaptureManager.Instance.BarcodeFind;

    private BarcodeFindView barcodeFindView = null!;
    private IList<BarcodeFindItem>? foundItems;

    public Symbology? Symbology { get; set; }
    public string? ItemToFind { get; set; }

    public FindViewController(IntPtr handle) : base(handle)
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

        this.SetupRecognition();
    }

    public override void ViewWillAppear(bool animated)
    {
        base.ViewWillAppear(animated);
        this.barcodeFindView.StartSearching();
    }

    public override void ViewWillDisappear(bool animated)
    {
        base.ViewWillDisappear(animated);
        this.barcodeFindView.StopSearching();
    }

    private void SetupRecognition()
    {
        if (!this.Symbology.HasValue || string.IsNullOrEmpty(this.ItemToFind))
        {
            return;
        }

        // Make sure barcode find is the only mode associated with the context.
        this.context.RemoveAllModes();

        // The settings instance initially has all types of barcodes (symbologies) disabled. For the purpose of this
        // sample we enable just the symbology of the barcode to find. In your own app ensure that you only enable the
        // symbologies that your app requires as every additional enabled symbology has an impact on processing times.
        FindDataCaptureManager.Instance.SetupForSymbology(this.Symbology.Value);

        // Set the list of items to find.
        FindDataCaptureManager.Instance.SetupSearchedItems(this.ItemToFind);

        // To visualize the on-going barcode find process on screen, setup a barcode find view that renders the
        // camera preview and the barcode find UI. The view is automatically added to the parent view hierarchy.
        var viewSettings = new BarcodeFindViewSettings();
        this.barcodeFindView = BarcodeFindView.Create(
            parentView: this.View!,
            dataCaptureContext: this.context,
            barcodeFind: this.barcodeFind,
            settings: viewSettings);
        this.barcodeFindView.FinishButtonTapped += FinishButtonTapped;
        this.barcodeFindView.PrepareSearching();
    }

    private void FinishButtonTapped(object? sender, FinishButtonTappedEventArgs args)
    {
        // This method is called when the user presses the finish button.
        // The `args.FoundItems` parameter contains the list of items found in the entire session.
        this.PerformSegue(identifier: unWindToSearchSegueIdentifier, sender: this);
    }
}
