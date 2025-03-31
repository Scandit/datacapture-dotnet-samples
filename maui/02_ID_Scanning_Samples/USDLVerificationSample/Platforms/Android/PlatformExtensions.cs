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

namespace USDLVerificationSample.Platform;

#nullable enable

public static class PlatformExtensions
{
    public static ImageSource? FromPlatform(this Android.Graphics.Bitmap convertible)
    {
        var data = new MemoryStream();
        convertible.Compress(Android.Graphics.Bitmap.CompressFormat.Png!, 100, data);
        data.Seek(0L, SeekOrigin.Begin);

        return ImageSource.FromStream(() => data);
    }
}
