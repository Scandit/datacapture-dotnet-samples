﻿/*
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
using ListBuildingSample.Extensions;
using ListBuildingSample.Models;
using ListBuildingSample.Views;
using Scandit.DataCapture.Barcode.Data;
using Scandit.DataCapture.Barcode.Spark.Capture;
using Scandit.DataCapture.Barcode.Spark.Feedback;
using Scandit.DataCapture.Barcode.Spark.UI;
using Scandit.DataCapture.Core.Capture;

namespace ListBuildingSample
{
    public partial class ViewController : UIViewController, ISparkScanFeedbackDelegate
    {
        public static readonly int HeaderMarginTop = 20;
        public static readonly int CellHeight = 70;

        private SparkScanBarcodeErrorFeedback errorFeedback = null!;
        private SparkScanBarcodeSuccessFeedback successFeedback = null!;

        private DataCaptureContext dataCaptureContext = null!;
        private SparkScan sparkScan = null!;
        private SparkScanView sparkScanView = null!;

        private ItemsTableHeaderView headerView = null!;
        private ItemsTableClearView clearView = null!;
        private UITableView tableView = null!;

        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            this.SetupHeaderView();
            this.SetupTableView();
            this.SetupClearView();
            this.SetupSparkScan();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            this.sparkScanView.PrepareScanning();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            this.sparkScanView.StopScanning();
        }

        private void SetupSparkScan()
        {
            // Create data capture context using your license key.
            this.dataCaptureContext = DataCaptureContext.ForLicenseKey(Application.SCANDIT_LICENSE_KEY);

            SparkScanSettings settings = new();

            // The settings instance initially has all types of barcodes (symbologies) disabled.
            // For the purpose of this sample we enable a very generous set of symbologies.
            // In your own app ensure that you only enable the symbologies that your app requires as
            // every additional enabled symbology has an impact on processing times.
            HashSet<Symbology> symbologies = new()
            {
                Symbology.Ean13Upca,
                Symbology.Ean8,
                Symbology.Upce,
                Symbology.Code39,
                Symbology.Code128,
                Symbology.InterleavedTwoOfFive
            };

            settings.EnableSymbologies(symbologies);

            // Some linear/1d barcode symbologies allow you to encode variable-length data.
            // By default, the Scandit Data Capture SDK only scans barcodes in a certain length range.
            // If your application requires scanning of one of these symbologies, and the length is
            // falling outside the default range, you may need to adjust the "active symbol counts"
            // for this symbology. This is shown in the following few lines of code for one of the
            // variable-length symbologies.
            settings.GetSymbologySettings(Symbology.Code39).ActiveSymbolCounts =
                new short[] { 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 };

            // Create new spark scan mode with the settings from above.
            this.sparkScan = new SparkScan(settings);

            // Subsribe to BarcodeScanned event to get informed whenever a new barcode got recognized.
            this.sparkScan.BarcodeScanned += this.BarcodeScanned;

            // You can customize the SparkScanView using SparkScanViewSettings.
            SparkScanViewSettings viewSettings = new();

            if (this.View == null)
            {
                throw new InvalidOperationException("Cannot initialize view");
            }

            // To visualize the on-going barcode capturing process on screen, setup a spark scan view
            // that renders the camera preview. The view must be connected to the data capture context.
            this.sparkScanView = SparkScanView.Create(parentView: this.View,
                                                      context: dataCaptureContext,
                                                      sparkScan: sparkScan,
                                                      settings: viewSettings);
            this.SetupSparkScanFeedback();
        }

        private void SetupSparkScanFeedback()
        {
            this.errorFeedback = new SparkScanBarcodeErrorFeedback(
                message: "Wrong barcode",
                resumeCapturingDelay: TimeSpan.FromSeconds(60));

            this.successFeedback = new SparkScanBarcodeSuccessFeedback();
            this.sparkScanView.Feedback = this;
        }

        private void SetupHeaderView()
        {
            this.headerView = new ItemsTableHeaderView
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            if (this.View == null)
            {
                throw new InvalidOperationException("Cannot initialize view");
            }

            this.View.AddSubview(this.headerView);
            this.View.AddConstraints(new[] {
                headerView.LeadingAnchor.ConstraintEqualTo(this.View.LeadingAnchor),
                headerView.TopAnchor.ConstraintEqualTo(this.View.TopAnchor, HeaderMarginTop),
                headerView.TrailingAnchor.ConstraintEqualTo(this.View.TrailingAnchor),
                headerView.HeightAnchor.ConstraintEqualTo(this.View.HeightAnchor, ItemsTableHeaderView.HeaderHeight),
            });
        }

        private void SetupTableView()
        {
            this.tableView = new UITableView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Source = new TableSource(ListItemManager.Instance.Inventory)
            };

            this.tableView.RegisterClassForCellReuse(typeof(ItemTableViewCell), ItemTableViewCell.Key);
            this.tableView.RowHeight = CellHeight;
            this.tableView.SeparatorStyle = UITableViewCellSeparatorStyle.SingleLine;
            ListItemManager.Instance.ListsChanged += ListsChanged;

            if (this.View == null)
            {
                throw new InvalidOperationException("Cannot initialize view");
            }

            this.View.AddSubview(this.tableView);
            this.View.AddConstraints(new[]{
                tableView.LeadingAnchor.ConstraintEqualTo(this.View.LeadingAnchor),
                tableView.TopAnchor.ConstraintEqualTo(this.View.TopAnchor, ItemsTableHeaderView.HeaderHeight),
                tableView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                tableView.BottomAnchor.ConstraintEqualTo(this.View.BottomAnchor, -ItemsTableClearView.TableClearViewHeight),
            });
        }

        private void SetupClearView()
        {
            this.clearView = new ItemsTableClearView
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            if (this.View == null)
            {
                throw new InvalidOperationException("Cannot initialize view");
            }

            this.View.AddSubview(this.clearView);
            this.View.AddConstraints(new[]{
                clearView.LeadingAnchor.ConstraintEqualTo(this.View.LeadingAnchor),
                clearView.TrailingAnchor.ConstraintEqualTo(this.View.TrailingAnchor),
                clearView.BottomAnchor.ConstraintEqualTo(this.View.BottomAnchor),
                clearView.HeightAnchor.ConstraintEqualTo(ItemsTableClearView.TableClearViewHeight),
            });
        }

        private void ListsChanged(object? sender, EventArgs e)
        {
            DispatchQueue.MainQueue.DispatchAsync(this.tableView.ReloadData);
        }

        private void BarcodeScanned(object? sender, SparkScanEventArgs args)
        {
            if (args.Session.NewlyRecognizedBarcode == null)
            {
                return;
            }

            var barcode = args.Session.NewlyRecognizedBarcode;

            using var imageBuffer = args.FrameData?.ImageBuffers.LastOrDefault();
            using var frame = imageBuffer?.ToImage();
            var location = barcode.GetBarcodeLocation(frame);
            var thumbnail = frame?.CropImage(
                (int)location.X, (int)location.Y, (int)location.Width, (int)location.Height);

            DispatchQueue.MainQueue.DispatchAsync(() =>
            {
                if (IsBarcodeValid(barcode))
                {
                    var itemNumber = ListItemManager.Instance.TotalItemsCount + 1;
                    ListItemManager.Instance.AddItem(
                        new ListItem(
                            thumbnail,
                            itemNumber,
                            barcode.Symbology,
                            barcode.Data));
                }
            });
        }

        SparkScanBarcodeFeedback ISparkScanFeedbackDelegate.GetFeedbackForBarcode(Barcode barcode)
        {
            return IsBarcodeValid(barcode) ? this.successFeedback : this.errorFeedback;
        }

        private static bool IsBarcodeValid(Barcode barcode)
        {
            return barcode.Data != "123456789";
        }
    }
}
