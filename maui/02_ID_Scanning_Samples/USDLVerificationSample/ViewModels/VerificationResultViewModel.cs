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

using System.Collections.ObjectModel;
using Scandit.DataCapture.ID.Data;
using USDLVerificationSample.Platform;
using USDLVerificationSample.Results;
using USDLVerificationSample.Results.Presenters;

namespace USDLVerificationSample.ViewModels;

public class VerificationResultViewModel(CapturedId? capturedId, RejectionReason rejectionReason) : BaseViewModel
{
    public ObservableCollection<ResultEntry> Items { get; private set; } = capturedId != null
        ? new ObservableCollection<ResultEntry>(new CommonResultPresenter(capturedId).Rows.ToList())
        : [];

    public string VerificationText { get; } = rejectionReason switch
    {
        RejectionReason.DocumentExpired => "The document has expired.",
        RejectionReason.Timeout => "Document capture failed. Make sure the document is well lit and free of glare. "
                                   + "Alternatively, try scanning another document",
        RejectionReason.InconsistentData => "Information on front and back does not match.",
        RejectionReason.ForgedAamvaBarcode => "Document barcode is forged.",
        RejectionReason.NotAcceptedDocumentType =>
            capturedId?.IssuingCountry == IdCaptureRegion.Us
                ? "Document not supported. Try scanning another document"
                : "Document is not a US driver’s license",
        _ => "Document not supported. Try scanning another document"
    };

    public bool VerificationVisible
    {
        get { return !string.IsNullOrEmpty(VerificationText); }
    }
    
    public ImageSource? FaceImage { get; } = capturedId?.Images.Face?.FromPlatform();
    public ImageSource? IdFrontImage { get; } = capturedId?.Images.GetCroppedDocument(IdSide.Front)?.FromPlatform();
    public ImageSource? IdBackImage { get; } = capturedId?.Images.GetCroppedDocument(IdSide.Back)?.FromPlatform();
}