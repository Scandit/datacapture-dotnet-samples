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

using ObjCRuntime;
using RestockingSample.Models;

namespace RestockingSample.Views;

public class ResultListCell : UITableViewCell
{
    private const int IdentifierLeadingMargin = 75;
    private const int IdentifierTopMargin = 10;
    private const int IdentifierHeight = 20;
    private const int BarcodeImageLeadingMargin = 15;
    private const int BarcodeImageTopMargin = 10;
    private const int BarcodeImageSize = 50;
    private const int SubtitleLeadingMargin = 75;
    private const int SubtitleTopMargin = 35;
    private const int SubtitleHeight = 15;
    private const int NotInListLeadingMargin = 75;
    private const int NotInListTopMargin = 50;
    private const int NotInListHeight = 15;

    private UILabel? _identifierLabel;    
    private UILabel? _dataLabel;
    private UILabel? _notInListLabel;
    private UIImageView? _iconImageView;

    public const string Identifier = nameof(ResultListCell);
    public const int CellHeight = 70;

    public ResultListCell(NativeHandle handle) : base(handle)
    {
        // Note: this .ctor should not contain any initialization logic.
    }

    public void Configure(DisplayProduct product, NSIndexPath indexPath)
    {
        ArgumentNullException.ThrowIfNull(product, nameof(product));

        _identifierLabel ??= CreateIdentifierLabel(product);
        _identifierLabel.Text = product.Identifier;
        _dataLabel ??= CreateDataLabel(product);
        _dataLabel.Text = product.BarcodeData;
        _notInListLabel ??= CreateNotInListLabel();
        _iconImageView ??= CreateIconImage();

        switch (indexPath.Section)
        {
            case 0:
                if (!product.InList)
                {
                    _iconImageView.Image = UIImage.FromBundle(name: "NotInList");
                }
                else if (product.Picked)
                {
                    _iconImageView.Image = UIImage.FromBundle(name: "Picked");
                }
                _notInListLabel.Hidden = product.InList;
                break;
            case 1:
                _notInListLabel.Hidden = true;
                break;
        }
    }

    private UIImageView CreateIconImage()
    {
        var iconImageView = new UIImageView
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Image = UIImage.FromBundle(name: "ProductImage")
        };

        this.ContentView.AddSubview(iconImageView);
        this.ContentView.AddConstraints(new[]
        {
            iconImageView.LeadingAnchor.ConstraintEqualTo(this.ContentView.LeadingAnchor, BarcodeImageLeadingMargin),
            iconImageView.TopAnchor.ConstraintEqualTo(this.ContentView.TopAnchor, BarcodeImageTopMargin),
            iconImageView.HeightAnchor.ConstraintEqualTo(BarcodeImageSize),
            iconImageView.WidthAnchor.ConstraintEqualTo(BarcodeImageSize)
        });

        return iconImageView;
    }

    private UILabel CreateIdentifierLabel(DisplayProduct product)
    {
        var identifierLabel = new UILabel
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            TextAlignment = UITextAlignment.Left,
            Font = UIFont.BoldSystemFontOfSize(18),
            TextColor = UIColor.Black,
            Text = product.Identifier
        };
        this.ContentView.AddSubview(identifierLabel);
        this.ContentView.AddConstraints(new[]
        {
            identifierLabel.LeadingAnchor.ConstraintEqualTo(this.ContentView.LeadingAnchor, IdentifierLeadingMargin),
            identifierLabel.TopAnchor.ConstraintEqualTo(this.ContentView.TopAnchor, IdentifierTopMargin),
            identifierLabel.HeightAnchor.ConstraintEqualTo(IdentifierHeight),
        });

        return identifierLabel;
    }

    private UILabel CreateDataLabel(DisplayProduct product)
    {
        var dataLabel = new UILabel
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            TextAlignment = UITextAlignment.Left,
            Font = UIFont.SystemFontOfSize(13),
            TextColor = UIColor.Gray,
            Text = product.BarcodeData,
        };
        this.ContentView.AddSubview(dataLabel);
        this.ContentView.AddConstraints(new[]
        {
            dataLabel.LeadingAnchor.ConstraintEqualTo(this.ContentView.LeadingAnchor, SubtitleLeadingMargin),
            dataLabel.TopAnchor.ConstraintEqualTo(this.ContentView.TopAnchor, SubtitleTopMargin),
            dataLabel.HeightAnchor.ConstraintEqualTo(SubtitleHeight)
        });

        return dataLabel;
    }

    private UILabel CreateNotInListLabel()
    {
        var notInListLabel = new UILabel
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            TextAlignment = UITextAlignment.Left,
            Font = UIFont.SystemFontOfSize(13),
            TextColor = UIColor.Red,
            Text = "Picked item not in pick list"
        };
        this.ContentView.AddSubview(notInListLabel);
        this.ContentView.AddConstraints(new[]
        {
            notInListLabel.LeadingAnchor.ConstraintEqualTo(this.ContentView.LeadingAnchor, NotInListLeadingMargin),
            notInListLabel.TopAnchor.ConstraintEqualTo(this.ContentView.TopAnchor, NotInListTopMargin),
            notInListLabel.HeightAnchor.ConstraintEqualTo(NotInListHeight)
        });

        return notInListLabel;
    }
}
