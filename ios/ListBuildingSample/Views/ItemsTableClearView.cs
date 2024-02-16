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

using ListBuildingSample.Models;

namespace ListBuildingSample.Views
{
    public class ItemsTableClearView : UIView
    {
        public const int ClearButtonWidth = 250;
        public const int ClearButtonHeight = 50;
        public const int TableClearViewHeight = 100;

        public UIButton ClearButton { get; private set; } = null!;

        public ItemsTableClearView()
        {
            this.Initialize();
        }

        private void Initialize()
        {
            this.BackgroundColor = UIColor.FromRGB(242, 242, 242);
            this.ClearButton = CreateButton("CLEAR LIST", () =>
            {
                ListItemManager.Instance.Clear();
            });

            this.AddSubview(this.ClearButton);
            this.AddConstraints(new[]
            {
                this.ClearButton.WidthAnchor.ConstraintEqualTo(ClearButtonWidth),
                this.ClearButton.HeightAnchor.ConstraintEqualTo(ClearButtonHeight),
                this.ClearButton.CenterYAnchor.ConstraintEqualTo(this.CenterYAnchor),
                this.ClearButton.CenterXAnchor.ConstraintEqualTo(this.CenterXAnchor)
            });
        }

        private static UIButton CreateButton(string title, Action action)
        {
            UIButton button = new(UIButtonType.RoundedRect)
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            button.SetTitle(title, UIControlState.Normal);
            button.BackgroundColor = UIColor.White;
            button.Layer.BorderColor = UIColor.Black.CGColor;
            button.Layer.CornerRadius = 3;
            button.Layer.BorderWidth = 2;
            button.TitleLabel.TextColor = UIColor.Black;
            button.TitleLabel.Font = UIFont.BoldSystemFontOfSize(18);

            button.TouchUpInside += (object? sender, EventArgs e) =>
            {
                action?.Invoke();
            };

            return button;
        }
    }
}
