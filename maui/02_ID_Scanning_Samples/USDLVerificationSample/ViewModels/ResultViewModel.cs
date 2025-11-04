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

public class ResultViewModel : BaseViewModel
{
    public ObservableCollection<ResultEntry> Items { get; private set; }

    public ImageSource? FaceImage { get; }
    public ImageSource? IdFrontImage { get; }
    public ImageSource? IdBackImage { get; }

    public ResultViewModel(CapturedId capturedId)
    {
        var resultPresenter = new CommonResultPresenter(capturedId);
        this.Items = new ObservableCollection<ResultEntry>(resultPresenter.Rows);
        this.FaceImage = capturedId.Images.Face?.FromPlatform();
        this.IdFrontImage = capturedId.Images.GetCroppedDocument(IdSide.Front)?.FromPlatform();
        this.IdBackImage = capturedId.Images.GetCroppedDocument(IdSide.Back)?.FromPlatform();
    }
}