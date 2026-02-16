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

using Scandit.DataCapture.Core.UI.Style;
using Scandit.DataCapture.Label.Data;
using Scandit.DataCapture.Label.UI.Overlay;

namespace LabelCaptureSimpleSample.Services.Internals.Listeners;

internal class LabelCaptureBasicOverlayListener : NSObject, ILabelCaptureBasicOverlayListener
{
    private readonly Brush upcBrush = new(
        fillColor: UIColor.FromRGB(46, 193, 206),    // #2EC1CE
        strokeColor: UIColor.FromRGB(46, 193, 206),
        strokeWidth: 1f);
    private readonly Brush expiryDateBrush = new(
        fillColor: UIColor.FromRGB(250, 68, 70),     // #FA4446
        strokeColor: UIColor.FromRGB(250, 68, 70),
        strokeWidth: 1f);
    private readonly Brush totalPriceBrush = new(
        fillColor: UIColor.FromRGB(10, 51, 144),     // #0A3390
        strokeColor: UIColor.FromRGB(10, 51, 144),
        strokeWidth: 1f);
    private readonly Brush transparentBrush = Brush.TransparentBrush;

    public Brush? BrushForField(LabelCaptureBasicOverlay overlay, LabelField field, CapturedLabel label)
    {
        return field.Name switch
        {
            LabelCaptureService.FIELD_BARCODE => this.upcBrush,
            LabelCaptureService.FIELD_EXPIRY_DATE => this.expiryDateBrush,
            LabelCaptureService.FIELD_TOTAL_PRICE => this.totalPriceBrush,
            _ => null,
        };
    }

    public Brush? BrushForLabel(LabelCaptureBasicOverlay overlay, CapturedLabel label)
    {
        return this.transparentBrush;
    }

    public void OnLabelTapped(LabelCaptureBasicOverlay overlay, CapturedLabel label)
    {
    }
}