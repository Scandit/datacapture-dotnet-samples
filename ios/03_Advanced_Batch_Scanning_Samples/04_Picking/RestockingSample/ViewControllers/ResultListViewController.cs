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
using RestockingSample.Views;

namespace RestockingSample.ViewControllers;

[Register("ResultListViewController")]
public class ResultListViewController : UIViewController
{
    private const int FooterHeight = 150;

    private UITableView _tableView = null!;
    private UIView _footerView = null!;
    private UIButton _continueButton = null!;
    private UIButton _finishButton = null!;
    private readonly Action? _finishButtonAction;

    public ResultListViewController(Action? finishButtonAction = null)
    {
        _finishButtonAction = finishButtonAction;
    }

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();
        this.SetupTableView();
        this.SetupFooterView();
        this.Title = "Result List";
    }

    private void SetupTableView()
    {
        ArgumentNullException.ThrowIfNull(View, nameof(View));

        var pickedItems = ProductManager.Instance.BuildPickedProductsListAndSetPickedProductsCount();
        var inventoryItems = ProductManager.Instance.GetInventoryList();
        var tableSource = new TableSource(pickedItems, inventoryItems);
        var tableDelegate = new TableDelegate(tableSource);

        _tableView = new UITableView
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Source = tableSource,
            Delegate = tableDelegate,
            BackgroundColor = UIColor.White
        };

        _tableView.RegisterClassForCellReuse(typeof(ResultListCell), ResultListCell.Identifier);
        _tableView.RegisterClassForHeaderFooterViewReuse(typeof(ResultListSectionHeaderView), ResultListSectionHeaderView.Identifier);
        _tableView.RowHeight = ResultListCell.CellHeight;
        _tableView.SeparatorStyle = UITableViewCellSeparatorStyle.SingleLine;

        View.AddSubview(_tableView);
        View.AddConstraints(new[]
        {
            _tableView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
            _tableView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
            _tableView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
            _tableView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor, -FooterHeight),
        });
    }

    private void SetupFooterView()
    {
        ArgumentNullException.ThrowIfNull(View, nameof(View));

        _footerView = new UIView
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            BackgroundColor = UIColor.White
        };
        _footerView.Layer.ShadowColor = new UIColor(red: 0.11f, green: 0.13f, blue: 0.15f, alpha: 0.2f).CGColor;
        _footerView.Layer.ShadowOpacity = 1;
        _footerView.Layer.ShadowRadius = 16;
        _footerView.Layer.ShadowOffset = CGSize.Empty;

        _continueButton = CreateContinueButton(_footerView);
        _finishButton = CreateFinishButton(_footerView);

        View.AddSubview(_footerView);
        View.AddConstraints(new []
        {
            _footerView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
            _footerView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
            _footerView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
            _footerView.HeightAnchor.ConstraintEqualTo(FooterHeight),
        });
    }

    private UIButton CreateContinueButton(UIView parentView)
    {
        var continueButton = new UIButton(UIButtonType.RoundedRect);
        continueButton.SetTitle("CONTINUE SCANNING", UIControlState.Normal);
        continueButton.SetTitleColor(UIColor.White, UIControlState.Normal);
        continueButton.BackgroundColor = UIColor.Black;
        continueButton.TranslatesAutoresizingMaskIntoConstraints = false;
        continueButton.TitleLabel.Font = UIFont.SystemFontOfSize(18);

        parentView.AddSubview(continueButton);
        parentView.AddConstraints(new []
        {
            continueButton.LeadingAnchor.ConstraintEqualTo(parentView.LeadingAnchor, 25),
            continueButton.TrailingAnchor.ConstraintEqualTo(parentView.TrailingAnchor, -25),
            continueButton.TopAnchor.ConstraintEqualTo(parentView.TopAnchor, 15),
            continueButton.HeightAnchor.ConstraintEqualTo(52),
        });
        continueButton.TouchUpInside += ContinueScanningPressed;

        return continueButton;
    }

    private UIButton CreateFinishButton(UIView parentView)
    {
        var finishButton = new UIButton(UIButtonType.RoundedRect);
        finishButton.SetTitle("FINISH", UIControlState.Normal);
        finishButton.SetTitleColor(UIColor.Black, UIControlState.Normal);
        finishButton.BackgroundColor = UIColor.White;
        finishButton.TranslatesAutoresizingMaskIntoConstraints = false;
        finishButton.TitleLabel.Font = UIFont.SystemFontOfSize(18);
        finishButton.Layer.BorderWidth = 2;
        finishButton.Layer.BorderColor = UIColor.Black.CGColor;

        parentView.AddSubview(finishButton);
        parentView.AddConstraints(new []
        {
            finishButton.LeadingAnchor.ConstraintEqualTo(parentView.LeadingAnchor, 25),
            finishButton.TrailingAnchor.ConstraintEqualTo(parentView.TrailingAnchor, -25),
            finishButton.BottomAnchor.ConstraintEqualTo(parentView.BottomAnchor, -15),
            finishButton.HeightAnchor.ConstraintEqualTo(52),
        });
        finishButton.TouchUpInside += FinishPressed;

        return finishButton;
    }

    private void ContinueScanningPressed(object? sender, EventArgs args) 
    {
        NavigationController?.PopViewController(animated: true);
    }

    private void FinishPressed(object? sender, EventArgs args)
    {
        _finishButtonAction?.Invoke();
        NavigationController?.PopViewController(animated: true);
    }
}
