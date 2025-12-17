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
using LabelCaptureSimpleSample.Services.Internals.Listeners;
using Scandit.DataCapture.Barcode.Data;
using Scandit.DataCapture.Core.Capture;
using Scandit.DataCapture.Label.Capture;
using Scandit.DataCapture.Label.Data;
using Scandit.DataCapture.Label.UI.Overlay;
using Scandit.DataCapture.Label.UI.Overlay.Validation;

namespace LabelCaptureSimpleSample.Services.Internals;

internal class LabelCaptureService : ILabelCaptureService
{
    public const string FIELD_BARCODE = "barcode";
    public const string FIELD_UNIT_PRICE = "unit_price";
    public const string FIELD_WEIGHT = "weight";
    public const string FIELD_EXPIRY_DATE = "expiry_date";
    public const string LABEL_WEIGHT_PRICE = "weighted_item";

    private LabelCapture? labelCapture;

    public void Initialize(DataCaptureContext dataCaptureContext)
    {
        var labelCaptureSettings = this.BuildLabelCaptureSettings();

        this.labelCapture = LabelCapture.Create(dataCaptureContext, labelCaptureSettings);
    }

    public LabelCaptureBasicOverlay BuildOverlay(Context context)
    {
        if (this.labelCapture == null)
        {
            throw new InvalidOperationException("LabelCapture must be initialized before building overlay");
        }

        var overlay = LabelCaptureBasicOverlay.Create(this.labelCapture);
        overlay.Listener = new LabelCaptureBasicOverlayListener(context);

        return overlay;
    }

    public LabelCaptureValidationFlowOverlay BuildValidationFlowOverlay(Context context, Action<string> onLabelScanned)
    {
        if (this.labelCapture == null)
        {
            throw new InvalidOperationException("LabelCapture must be initialized before building overlay");
        }

        // Customize the validation flow by changing labels.
        var settings = LabelCaptureValidationFlowSettings.Create();

        var overlay = LabelCaptureValidationFlowOverlay.Create(this.labelCapture, null);
        overlay.Listener = new LabelCaptureValidationFlowListener(onLabelScanned);
        overlay.ApplySettings(settings);

        return overlay;
    }

    public void Disable()
    {
        if (this.labelCapture != null)
        {
            this.labelCapture.Enabled = false;
        }
    }

    public void Enable()
    {
        if (this.labelCapture != null)
        {
            this.labelCapture.Enabled = true;
        }
    }

    private LabelCaptureSettings BuildLabelCaptureSettings()
    {
        // Demonstrates LabelCaptureSettings by configuring a label
        // containing barcode, date, price, and weight fields.
        var fields = new List<LabelFieldDefinition>();

        // Add custom barcode field
        var customBarcode = CustomBarcode.Builder()
            .IsOptional(false)
            .SetSymbologies(new List<Symbology>
            {
                Symbology.Ean13Upca,
                Symbology.Gs1DatabarExpanded,
                Symbology.Code128
            })
            .Build(FIELD_BARCODE);
        fields.Add(customBarcode);

        // Add expiry date field
        var expiryDateText = ExpiryDateText.Builder()
            .IsOptional(true)
            .SetLabelDateFormat(new LabelDateFormat(LabelDateComponentFormat.MDY, acceptPartialDates: false))
            .Build(FIELD_EXPIRY_DATE);
        fields.Add(expiryDateText);

        // Add unit price field
        var unitPriceText = UnitPriceText.Builder()
            .IsOptional(true)
            .Build(FIELD_UNIT_PRICE);
        fields.Add(unitPriceText);

        // Add weight field
        var weightText = WeightText.Builder()
            .IsOptional(true)
            .Build(FIELD_WEIGHT);
        fields.Add(weightText);

        // Create label definition
        var labelDefinition = LabelDefinition.Create(LABEL_WEIGHT_PRICE, fields);

        // Create settings with the label definition
        var settings = LabelCaptureSettings.Create(new List<LabelDefinition> { labelDefinition });
        return settings;
    }
}
