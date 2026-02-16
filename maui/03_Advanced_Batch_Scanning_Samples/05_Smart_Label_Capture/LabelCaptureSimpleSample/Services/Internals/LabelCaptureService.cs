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

using LabelCaptureSimpleSample.Services.Internals.Listeners;
using Scandit.DataCapture.Barcode.Data;
using Scandit.DataCapture.Core.Capture;
using Scandit.DataCapture.Label.Capture;
using Scandit.DataCapture.Label.Data;
using Scandit.DataCapture.Label.UI.Overlay;
using Scandit.DataCapture.Label.UI.Overlay.Validation;

namespace LabelCaptureSimpleSample.Services.Internals;

internal class LabelCaptureService(DataCaptureContext dataCaptureContext) : ILabelCaptureService
{
    public const string FIELD_BARCODE = "Barcode";
    public const string FIELD_EXPIRY_DATE = "Expiry Date";
    public const string FIELD_TOTAL_PRICE = "Total Price";
    public const string LABEL_RETAIL_ITEM = "Retail Item";

    private readonly LabelCapture labelCapture = LabelCapture.Create(dataCaptureContext, BuildLabelCaptureSettings());

    public LabelCaptureBasicOverlay BuildOverlay()
    {
        if (this.labelCapture == null)
        {
            throw new InvalidOperationException("LabelCapture must be initialized before building overlay");
        }

        var overlay = LabelCaptureBasicOverlay.Create(this.labelCapture);
        overlay.Listener = new LabelCaptureBasicOverlayListener();

        return overlay;
    }

    public LabelCaptureValidationFlowOverlay BuildValidationFlowOverlay(Action<string> onLabelScanned)
    {
        if (this.labelCapture == null)
        {
            throw new InvalidOperationException("LabelCapture must be initialized before building overlay");
        }

        // Customize the validation flow by changing labels.
        var settings = LabelCaptureValidationFlowSettings.Create();

        var overlay = LabelCaptureValidationFlowOverlay.Create(this.labelCapture, view: null);
        overlay.Listener = new LabelCaptureValidationFlowListener(onLabelScanned);
        overlay.ApplySettings(settings);

        return overlay;
    }

    public void Disable()
    {
        this.labelCapture.Enabled = false;
    }

    public void Enable()
    {
        this.labelCapture.Enabled = true;
    }

    private static LabelCaptureSettings BuildLabelCaptureSettings()
    {
        // Demonstrates LabelCaptureSettings by configuring a label
        // containing barcode, expiry date, and total price fields.
        var fields = new List<LabelFieldDefinition>();

        // Add a custom barcode field
        var customBarcode = CustomBarcode.Builder()
            .SetSymbologies(new List<Symbology>
            {
                Symbology.Ean13Upca,
                Symbology.Gs1DatabarExpanded,
                Symbology.Code128
            })
            .Build(FIELD_BARCODE);
        fields.Add(customBarcode);

        // Add an expiry date field
        var expiryDateText = ExpiryDateText.Builder()
            .SetLabelDateFormat(new LabelDateFormat(LabelDateComponentFormat.MDY, acceptPartialDates: false))
            .Build(FIELD_EXPIRY_DATE);
        fields.Add(expiryDateText);

        // Add total price field
        var totalPriceText = TotalPriceText.Builder()
            .IsOptional(true)
            .Build(FIELD_TOTAL_PRICE);
        fields.Add(totalPriceText);

        // Create label definition
        var labelDefinition = LabelDefinition.Create(LABEL_RETAIL_ITEM, fields);

        // Create settings with the label definition
        var settings = LabelCaptureSettings.Create(new List<LabelDefinition> { labelDefinition });
        return settings;
    }
}
