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
using AndroidX.RecyclerView.Widget;

namespace RestockingSample.Result;

public class ProductListAdapter : RecyclerView.Adapter
{
    public enum DisplayType
    {
        Header = 0,
        Item = 1
    }

    private readonly List<DisplayItem> _content;

    public ProductListAdapter(IList<DisplayItem> content)
    {
        ArgumentNullException.ThrowIfNull(content, nameof(content));

        _content = content.ToList();
    }

    public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
    {
        DisplayItem item = _content[position];

        if (item is DisplayHeader displayHeader && holder is HeaderHolder headerHolder)
        {
            headerHolder.Bind(displayHeader);
        }
        else if (item is DisplayProduct displayProduct && holder is ProductHolder productHolder)
        {
            productHolder.Bind(displayProduct);
        }
    }

    public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
    {
        if (viewType == (int)DisplayType.Header)
        {
            return HeaderHolder.Create(parent);
        }
        else
        {
            return ProductHolder.Create(parent);
        }
    }

    public override int ItemCount => _content.Count;

    public override int GetItemViewType(int position)
    {
        DisplayItem item = _content[position];

        if (item is DisplayHeader)
        {
            return (int)DisplayType.Header;
        }

        return (int)DisplayType.Item;
    }

    private class HeaderHolder : RecyclerView.ViewHolder
    {
        private readonly TextView _textView;

        public static HeaderHolder Create(ViewGroup parent)
        {
            View itemView = LayoutInflater
                .From(parent.Context)?
                .Inflate(Resource.Layout.item_header, parent, false) ??
                    throw new InvalidOperationException(
                        "LayoutInflater failed to inflate the view. 'item_header' layout could not be found or " +
                        "'parent.Context' is null.");

            return new HeaderHolder(itemView);
        }

        private HeaderHolder(View itemView) : base(itemView)
        {
            _textView = itemView.FindViewById<TextView>(Resource.Id.textView) ??
                throw new InvalidOperationException(
                    "Unable to find TextView with the specified Id. Ensure the `Resource.Id.textView` is correct and " +
                    "the itemView is properly initialized.");
        }

        public void Bind(DisplayHeader header)
        {
            string text;

            if (header.PickList)
            {
                text = _textView.Context?.GetString(Resource.String.items_picked) ??
                    throw new InvalidOperationException(
                        "Unable to find string with the specified Id. Ensure the `Resource.String.items_picked` " +
                        "is correct and the textView is properly initialized.");
            }
            else
            {
                text = _textView.Context?.GetString(Resource.String.items_scanned, header.Total) ??
                    throw new InvalidOperationException(
                        "Unable to find string with the specified Id. Ensure the `Resource.String.items_scanned` " +
                        "is correct and the textView is properly initialized.");
            }

            _textView.Text = text;
        }
    }

    private class ProductHolder : RecyclerView.ViewHolder
    {
        private readonly TextView _itemIdentifier;
        private readonly TextView _itemGtin;
        private readonly TextView _itemWarning;
        private readonly ImageView _checkmark;

        public static ProductHolder Create(ViewGroup parent)
        {
            View itemView = LayoutInflater
                .From(parent.Context)?
                .Inflate(Resource.Layout.item_result, parent, false) ??
                    throw new InvalidOperationException(
                        "LayoutInflater failed to inflate the view. 'item_result' layout could not be found or " +
                        "'parent.Context' is null.");

            return new ProductHolder(itemView);
        }

        private ProductHolder(View itemView) : base(itemView)
        {
            _itemIdentifier = itemView.FindViewById<TextView>(Resource.Id.itemIdentifier) ??
                throw new InvalidOperationException(
                    "Unable to find TextView with the specified Id. Ensure the `Resource.Id.itemIdentifier` is " +
                    "correct and the itemView is properly initialized.");

            _itemGtin = itemView.FindViewById<TextView>(Resource.Id.itemGtin) ??
                throw new InvalidOperationException(
                    "Unable to find TextView with the specified Id. Ensure the `Resource.Id.itemGtin` is " +
                    "correct and the itemView is properly initialized.");

            _itemWarning = itemView.FindViewById<TextView>(Resource.Id.itemWarning) ??
                throw new InvalidOperationException(
                    "Unable to find TextView with the specified Id. Ensure the `Resource.Id.itemWarning` is " +
                    "correct and the itemView is properly initialized.");

            _checkmark = itemView.FindViewById<ImageView>(Resource.Id.checkmark) ??
                throw new InvalidOperationException(
                    "Unable to find ImageView with the specified Id. Ensure the `Resource.Id.checkmark` is " +
                    "correct and the itemView is properly initialized.");
        }

        public void Bind(DisplayProduct product)
        {
            _itemIdentifier.Text = product.Identifier;
            _itemGtin.Text = ItemView.Context?.GetString(Resource.String.item_gtin, product.BarcodeData!);

            if (product.QuantityToPick == 0 && product.Picked)
            {
                _checkmark.Visibility = ViewStates.Visible;
                _checkmark.SetImageResource(Resource.Drawable.ic_warning);
                _itemWarning.Visibility = ViewStates.Visible;
            }
            else if (product.Picked)
            {
                _checkmark.Visibility = ViewStates.Visible;
                _checkmark.SetImageResource(Resource.Drawable.ic_green_check);
                _itemWarning.Visibility = ViewStates.Gone;
            }
            else
            {
                _checkmark.Visibility = ViewStates.Invisible;
                _itemWarning.Visibility = ViewStates.Gone;
            }
        }
    }
}
