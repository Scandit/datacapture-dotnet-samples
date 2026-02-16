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

using Microsoft.Maui.Platform;
using Scandit.DataCapture.Label.Data;
using Scandit.DataCapture.Label.UI.Overlay;
using ScanditBrush = Scandit.DataCapture.Core.UI.Style.Brush;

namespace LabelCaptureSimpleSample.Services.Internals.Listeners;

internal class LabelCaptureBasicOverlayListener : ILabelCaptureBasicOverlayListener
{
    private readonly ScanditBrush upcBrush = new(
        fillColor: Color.FromRgb(46, 193, 206).ToPlatform(),    // #2EC1CE
        strokeColor: Color.FromRgb(46, 193, 206).ToPlatform(),
        strokeWidth: 1f);
    private readonly ScanditBrush expiryDateBrush = new(
        fillColor: Color.FromRgb(250, 68, 70).ToPlatform(),     // #FA4446
        strokeColor: Color.FromRgb(250, 68, 70).ToPlatform(),
        strokeWidth: 1f);
    private readonly ScanditBrush totalPriceBrush = new(
        fillColor: Color.FromRgb(10, 51, 144).ToPlatform(),     // #0A3390
        strokeColor: Color.FromRgb(10, 51, 144).ToPlatform(),
        strokeWidth: 1f);
    private readonly ScanditBrush transparentBrush = ScanditBrush.TransparentBrush;

    public ScanditBrush? BrushForField(LabelCaptureBasicOverlay overlay, LabelField field, CapturedLabel label)
    {
        return field.Name switch
        {
            LabelCaptureService.FIELD_BARCODE => this.upcBrush,
            LabelCaptureService.FIELD_EXPIRY_DATE => this.expiryDateBrush,
            LabelCaptureService.FIELD_TOTAL_PRICE => this.totalPriceBrush,
            _ => null,
        };
    }

    public ScanditBrush? BrushForLabel(LabelCaptureBasicOverlay overlay, CapturedLabel label)
    {
        return this.transparentBrush;
    }

    public void OnLabelTapped(LabelCaptureBasicOverlay overlay, CapturedLabel label)
    {
    }
}

