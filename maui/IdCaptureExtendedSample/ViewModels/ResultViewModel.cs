﻿/*
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

#nullable enable

using IdCaptureExtendedSample.Platform;
using IdCaptureExtendedSample.Results;
using IdCaptureExtendedSample.Results.Presenters;
using Scandit.DataCapture.ID.Data;

namespace IdCaptureExtendedSample.ViewModels
{
    public class ResultViewModel
    {
        public List<ResultEntry> Items { get; private set; }

        public ImageSource? FaceImage { get; private set; }
        public ImageSource? IdFrontImage { get; private set; }
        public ImageSource? IdBackImage { get; private set; }

        public bool ImagesVisible { get; private set; }

        public ResultViewModel(CapturedId capturedId)
        {
            IResultPresenter resultPresenter = new CommonResultPresenter(capturedId);

            this.Items = resultPresenter.Rows.ToList();
            this.FaceImage = capturedId.Images?.Face?.FromPlatform();
            this.IdFrontImage = capturedId.Images?.GetCroppedDocument(IdSide.Front)?.FromPlatform();
            this.IdBackImage = capturedId.Images?.GetCroppedDocument(IdSide.Back)?.FromPlatform();

            this.ImagesVisible = this.FaceImage != null || this.IdFrontImage != null || this.IdBackImage != null;
        }
    }
}
