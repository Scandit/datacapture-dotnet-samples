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

using LabelCaptureSimpleSample.Services.Internals;

namespace LabelCaptureSimpleSample.Data;

public class ScannedResult(Dictionary<string, string> fields)
{
    public Dictionary<string, string> Fields { get; } = fields ?? [];

    public string Barcode => Fields.GetValueOrDefault(LabelCaptureService.FIELD_BARCODE, string.Empty);

    public string UnitPrice => Fields.GetValueOrDefault(LabelCaptureService.FIELD_TOTAL_PRICE, string.Empty);

    public string ExpiryDate => Fields.GetValueOrDefault(LabelCaptureService.FIELD_EXPIRY_DATE, string.Empty);

    public override string ToString()
    {
        return string.Join(", ", Fields.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
    }
}