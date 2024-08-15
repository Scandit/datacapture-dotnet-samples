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

public class ResultListSectionHeaderView : UITableViewHeaderFooterView
{
    private const int leadingMargin = 16;
    private UILabel? _headerLabel;

    public const string Identifier = nameof(ResultListSectionHeaderView);

    public ResultListSectionHeaderView(NativeHandle handle) : base(handle)
    {
        // Note: this .ctor should not contain any initialization logic.
    }

    public void Configure(nint section, IList<DisplayProduct> items)
    {
        _headerLabel ??= CreateHeaderLabel();

        switch (section)
        {
            case 0:
                _headerLabel.Text = "Picklist";
                break;
            case 1:
                _headerLabel.Text = $"Inventory ({items.Count})";
                break;
            default:
                break;
        }
    }

    private UILabel CreateHeaderLabel()
    {
        var headerLabel = new UILabel
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            TextAlignment = UITextAlignment.Left,
            Font = UIFont.BoldSystemFontOfSize(18),
            TextColor = UIColor.Black
        };
        this.ContentView.AddSubview(headerLabel);
        this.ContentView.AddConstraints(new []
        {
            headerLabel.TopAnchor.ConstraintEqualTo(this.ContentView.TopAnchor),
            headerLabel.LeadingAnchor.ConstraintEqualTo(this.ContentView.LeadingAnchor, leadingMargin)
        });

        return headerLabel;
    }
}
