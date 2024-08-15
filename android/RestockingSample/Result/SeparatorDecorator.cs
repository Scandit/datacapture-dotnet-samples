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
using Android.Graphics;
using Android.Views;
using AndroidX.RecyclerView.Widget;

namespace RestockingSample.Result;

public class SeparatorDecorator : RecyclerView.ItemDecoration
{
    private readonly int _headerSeparatorHeight;
    private readonly int _itemSeparatorHeight;
    private readonly Color _headerSeparatorColor = Color.LightBlue;
    private readonly Color _itemSeparatorColor = Color.Gray;

    private readonly Rect _bounds = new();
    private readonly Paint _paint;

    public SeparatorDecorator(Context context)
    {
        _headerSeparatorHeight = DpToPx(context, 2);
        _itemSeparatorHeight = DpToPx(context, 4);
        _paint = new Paint();
    }

    public override void GetItemOffsets(Rect outRect, View view, RecyclerView parent, RecyclerView.State state)
    {
        RecyclerView.Adapter? adapter = parent.GetAdapter();

        if (adapter == null)
        {
            return;
        }

        int adapterPosition = parent.GetChildAdapterPosition(view);
        int viewType = adapter.GetItemViewType(adapterPosition);

        if (viewType == (int)ProductListAdapter.DisplayType.Header)
        {
            outRect.Bottom = _headerSeparatorHeight;
        }
        else if (viewType == (int)ProductListAdapter.DisplayType.Item)
        {
            outRect.Bottom = _itemSeparatorHeight;
        }
    }

    public override void OnDraw(Canvas c, RecyclerView parent, RecyclerView.State state)
    {
        RecyclerView.Adapter? adapter = parent.GetAdapter();

        if (adapter == null)
        {
            return;
        }

        for (int index = 0; index < parent.ChildCount; index++)
        {
            View? child = parent.GetChildAt(index);

            if (child != null)
            {
                int adapterPosition = parent.GetChildAdapterPosition(child);
                int viewType = adapter.GetItemViewType(adapterPosition);

                int separatorHeight;
                Color color;

                if (viewType == (int)ProductListAdapter.DisplayType.Header)
                {
                    separatorHeight = _headerSeparatorHeight;
                    color = _headerSeparatorColor;
                }
                else
                {
                    separatorHeight = _itemSeparatorHeight;
                    color = _itemSeparatorColor;
                }

                parent.GetDecoratedBoundsWithMargins(child, _bounds);
                _bounds.Left = 0;
                _bounds.Right = parent.Width;
                _bounds.Bottom += (int)Math.Round(child.TranslationY);
                _bounds.Top = _bounds.Bottom - separatorHeight;
                DrawSeparator(c, _bounds, color);
            }
        }
    }

    private void DrawSeparator(Canvas canvas, Rect bounds, Color color)
    {
        _paint.Color = color;
        canvas.DrawRect(bounds, _paint);
    }

    private static int DpToPx(Context context, int dp)
    {
        float density = context.Resources?.DisplayMetrics?.Density ??
            throw new InvalidOperationException(
                "Failed to retrieve the display density. Ensure 'context.Resources' and 'DisplayMetrics' are not " +
                "null.");

        return (int)(dp * density);
    }
}
