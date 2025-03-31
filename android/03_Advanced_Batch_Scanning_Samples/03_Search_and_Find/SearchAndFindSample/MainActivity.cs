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

using Android.Content.PM;
using AndroidX.AppCompat.App;
using SearchAndFindSample.Search;
using static AndroidX.Fragment.App.FragmentManager;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace SearchAndFindSample;

[Activity(Label = "@string/app_name", MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait)]
public class MainActivity : AppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        this.SetContentView(Resource.Layout.activity_main);
        this.SetSupportActionBar(this.FindViewById<Toolbar>(Resource.Id.toolbar));
        this.SupportFragmentManager.BackStackChanged += BackStackChanged;

        if (savedInstanceState == null)
        {
            this.SupportFragmentManager
                .BeginTransaction()
                .Replace(Resource.Id.fragment_container, new SearchScanFragment())
                .Commit();
        }
    }

    private void BackStackChanged(object? sender, EventArgs e)
    {
        bool showHome = this.SupportFragmentManager.BackStackEntryCount >= 1;
        this.SupportActionBar?.SetDisplayHomeAsUpEnabled(showHome);
    }
}
