using System;
using System.Linq;
using System.Collections.Generic;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Views.InputMethods;
using Android.App;
using Android.Runtime;
using AWidget = Android.Widget;
using Nearest.Fragments;
using Nearest.Models;
using Nearest.GoogleApi;
using Nearest.GoogleApi.Models;
using Nearest.Storage;
using Nearest.Helpers;
using Android.Speech;
using Android.Content;
using Android.Preferences;
using Android.Content.Res;
using Android.Content.PM;

namespace Nearest.Activities
{
    [Activity(Label = "@string/app_name", MainLauncher = true, Icon = "@mipmap/ic_launcher",
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public class MainActivity : AppCompatActivity
    {
        #region Listeners

        internal class ExpandListener : Java.Lang.Object, Android.Support.V4.View.MenuItemCompat.IOnActionExpandListener
        {
            public event EventHandler MenuItemActionCollapse;
            public event EventHandler MenuItemActionExpand;

            public bool OnMenuItemActionCollapse(IMenuItem item)
            {
                MenuItemActionCollapse?.Invoke(this, EventArgs.Empty);
                return true;
            }

            public bool OnMenuItemActionExpand(IMenuItem item)
            {
                MenuItemActionExpand?.Invoke(this, EventArgs.Empty);
                return true;
            }
        }

        internal class SearchActionListener : Java.Lang.Object, AWidget.TextView.IOnEditorActionListener
        {
            public event EventHandler Search;

            public bool OnEditorAction(AWidget.TextView v, [GeneratedEnum] ImeAction actionId, KeyEvent e)
            {
                if (actionId == ImeAction.Search)
                {
                    Search?.Invoke(this, EventArgs.Empty);
                    return true;
                }
                return false;
            }
        }

        #endregion

        private Place nearestPlace;

        private DrawerLayout drawerLayout;
        private NavigationView navigationView;
        private IMenu actionBarMenu;
        private AWidget.AutoCompleteTextView searchTextView;

        private bool isInSearchMode = false;
        private bool doubleBackToExitPressedOnce = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.main_activity);

            // setup google api
            PlacesService.ApiKey = GetString(Resource.String.google_maps_api_key);
            PlacesService.Language = GetString(Resource.String.language);
            DirectionsService.ApiKey = GetString(Resource.String.google_maps_api_key);
            DirectionsService.Language = GetString(Resource.String.language);

            var sharedPrefference = PreferenceManager.GetDefaultSharedPreferences(this);

            string defMode = Resources.GetStringArray(Resource.Array.vechicle_mode_values)[0];
            string defUnit = Resources.GetStringArray(Resource.Array.unit_system_values)[0];

            DirectionsService.Mode = sharedPrefference.GetString("vechicle_mode_key", defMode);
            DirectionsService.Unit = sharedPrefference.GetString("unit_system_key", defUnit);


            // init toolbar
            var toolbar = FindViewById<Toolbar>(Resource.Id.action_bar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);

            // create ActionBarDrawerToggle button and add it to the toolbar
            drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            var drawerToggle = new ActionBarDrawerToggle(this, drawerLayout, toolbar, Resource.String.open_drawer, Resource.String.close_drawer);
            drawerLayout.SetDrawerListener(drawerToggle);
            drawerToggle.SyncState();

            drawerLayout.DrawerSlide += (s, e) =>
            {
                drawerToggle.OnDrawerSlide(e.DrawerView, e.SlideOffset);
                if (actionBarMenu != null)
                {
                    OnPrepareOptionsMenu(actionBarMenu);
                    actionBarMenu.FindItem(Resource.Id.action_search).CollapseActionView();
                }
            };

            // attach item selected handler to navigation view
            navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.NavigationItemSelected += ChangeMainFragment;

            // load default home screen
            if (savedInstanceState == null)
            {
                using (var ft = FragmentManager.BeginTransaction())
                {
                    ft.AddToBackStack(null);
                    ft.Add(Resource.Id.main_fragment, new MapFragment(), "MAP_FRAGMENT");
                    ft.Commit();
                }
            }

            CheckInternetConnection();
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            UpdateActionBar();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            actionBarMenu = menu;
            MenuInflater.Inflate(Resource.Menu.action_menu, menu);

            // init search text view
            searchTextView = (AWidget.AutoCompleteTextView)menu.FindItem(Resource.Id.action_search).ActionView;
            searchTextView.TextChanged += (s1, e1) => AutoComplete();
            searchTextView.ItemClick += (s1, e1) => { FinishSearch(); ShowRouteToNearestAddress(searchTextView.Text); };

            var searchListener = new SearchActionListener();
            searchListener.Search += (s1, e1) => { FinishSearch(); ShowRouteToNearestAddress(searchTextView.Text); };
            searchTextView.SetOnEditorActionListener(searchListener);

            UpdateActionBar();

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.action_search:
                    var expandListener = new ExpandListener();
                    expandListener.MenuItemActionExpand += (s, e) =>
                    {
                        StartSearch();
                        UpdateActionBar();
                    };
                    expandListener.MenuItemActionCollapse += (s, e) =>
                    {
                        FinishSearch();
                        UpdateActionBar();
                    };
                    Android.Support.V4.View.MenuItemCompat.SetOnActionExpandListener(item, expandListener);
                    CheckInternetConnection();
                    return true;
                case Resource.Id.action_clear:
                    searchTextView.Text = string.Empty;
                    return true;
                case Resource.Id.action_voice:
                    var voiceIntent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
                    voiceIntent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);

                    // message and modal dialog
                    voiceIntent.PutExtra(RecognizerIntent.ExtraPrompt, GetString(Resource.String.speak_now));

                    // end capturing speech if there is 3 seconds of silence
                    voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 3000);
                    voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 3000);
                    voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 30000);
                    voiceIntent.PutExtra(RecognizerIntent.ExtraMaxResults, 1);

                    // method to specify other languages to be recognised here if desired
                    voiceIntent.PutExtra(RecognizerIntent.ExtraLanguage, Java.Util.Locale.Default);
                    StartActivityForResult(voiceIntent, 10);
                    return true;
                case Resource.Id.action_add_to_fav:
                    var popup = new PopupMenu(this, FindViewById(Resource.Id.action_add_to_fav));
                    popup.MenuInflater.Inflate(Resource.Menu.add_to_fav_popup_menu, popup.Menu);
                    popup.MenuItemClick += ShowAlert;
                    popup.Show();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        protected override void OnActivityResult(int requestCode, Result result, Intent data)
        {
            if (requestCode == 10)
            {
                if (result == Result.Ok)
                {
                    var matches = data.GetStringArrayListExtra(RecognizerIntent.ExtraResults);
                    if (matches.Count != 0)
                    {
                        string textInput = matches[0];
                        searchTextView.Text = textInput;
                        ShowRouteToNearestAddress(textInput);
                    }
                    else
                        AWidget.Toast.MakeText(this, GetString(Resource.String.not_recognized),
                            AWidget.ToastLength.Short).Show();
                }
            }

            base.OnActivityResult(requestCode, result, data);
        }

        public override void OnBackPressed()
        {
            if (drawerLayout.IsDrawerOpen(Android.Support.V4.View.GravityCompat.Start))
            {
                // hide nav menu
                drawerLayout.CloseDrawers();
            }
            else if (FragmentManager.BackStackEntryCount > 1)
            {
                // close current fragment
                FragmentManager.PopBackStack();
                UpdateActionBar();
            }
            else
            {
                // close app on double click
                if (doubleBackToExitPressedOnce)
                {
                    base.OnBackPressed();
                    return;
                }
                doubleBackToExitPressedOnce = true;

                AWidget.Toast.MakeText(this, GetString(Resource.String.double_click_to_exit),
                    AWidget.ToastLength.Short).Show();

                int timeout = int.Parse(ApplicationContext.GetString(Resource.String.doubleclick_to_exit_reset_timeout));
                new Handler().PostDelayed(() => doubleBackToExitPressedOnce = false, timeout);
            }
        }

        #region Private methods

        private void CheckInternetConnection()
        {
            if (!Utils.IsNetworkAvailable(ApplicationContext))
                AWidget.Toast.MakeText(this, GetString(Resource.String.no_internet),
                   AWidget.ToastLength.Short).Show();
        }

        private void ClearFragmentsBackStack()
        {
            // pop all fragments except first
            int num_of_fragments = FragmentManager.BackStackEntryCount;
            while (num_of_fragments > 1)
            {
                FragmentManager.PopBackStack();
                num_of_fragments--;
            }
        }

        private void ChangeMainFragment(object sender, NavigationView.NavigationItemSelectedEventArgs e)
        {
            ClearFragmentsBackStack();

            // start transaction
            using (var ft = FragmentManager.BeginTransaction())
            {
                ft.AddToBackStack(null);

                // add needed fragment
                switch (e.MenuItem.ItemId)
                {
                    case (Resource.Id.nav_map):
                        break;
                    case (Resource.Id.nav_fav_queries):
                        var fqf = new FavoriteQueriesFragment();
                        fqf.ItemClicked += (s, query) =>
                        {
                            ClearFragmentsBackStack();
                            OnPrepareOptionsMenu(actionBarMenu);
                            actionBarMenu.FindItem(Resource.Id.action_search).ExpandActionView();
                            searchTextView.Text = query;
                            searchTextView.DismissDropDown();
                            InputMethodManager inputManager = (InputMethodManager)GetSystemService(InputMethodService);
                            inputManager.HideSoftInputFromWindow(searchTextView.WindowToken, HideSoftInputFlags.NotAlways);
                            UpdateActionBar();
                            ShowRouteToNearestAddress(query);
                        };
                        ft.Add(Resource.Id.main_fragment, fqf, "SECOND_FRAGMENT");
                        ft.Commit();
                        break;
                    case (Resource.Id.nav_fav_places):
                        var fpf = new FavoritePlacesFragment();
                        fpf.ItemClicked += (s, place) =>
                        {
                            ClearFragmentsBackStack();
                            UpdateActionBar();
                            ShowRouteToPlace(place);
                        };
                        ft.Add(Resource.Id.main_fragment, fpf, "SECOND_FRAGMENT");
                        ft.Commit();
                        break;
                    case (Resource.Id.nav_settings):
                        ft.Add(Resource.Id.main_fragment, new SettingsFragment(), "SECOND_FRAGMENT");
                        ft.Commit();
                        break;
                    case (Resource.Id.nav_exit):
                        FinishAffinity();
                        break;
                }
            }

            drawerLayout.CloseDrawers();
            UpdateActionBar();
        }

        private void ShowAlert(object sender, PopupMenu.MenuItemClickEventArgs e)
        {
            switch (e.Item.ItemId)
            {
                case (Resource.Id.action_add_fav_query):
                    ShowAddToFavAlert((name) =>
                    {
                        string key = ApplicationContext.GetString(Resource.String.fav_query_storage_key);
                        var storage = new FavoritesStorage<FavoriteQuery>(ApplicationContext, key);
                        storage.AddItem(new FavoriteQuery()
                        { Id = storage.GetUniqueId(), Name = name, Query = searchTextView.Text });

                        AWidget.Toast.MakeText(this, GetString(Resource.String.saved),
                            AWidget.ToastLength.Short).Show();
                    });
                    break;
                case (Resource.Id.action_add_fav_place):
                    ShowAddToFavAlert((name) =>
                    {
                        string key = ApplicationContext.GetString(Resource.String.fav_places_storage_key);
                        var storage = new FavoritesStorage<FavoritePlace>(ApplicationContext, key);
                        storage.AddItem(new FavoritePlace()
                        { Id = storage.GetUniqueId(), Name = name, Place = nearestPlace });

                        AWidget.Toast.MakeText(this, GetString(Resource.String.saved),
                            AWidget.ToastLength.Short).Show();
                    });
                    break;
            }
        }

        private void ShowAddToFavAlert(Action<string> saveAction)
        {
            LayoutInflater layoutInflater = LayoutInflater.From(this);
            View view = layoutInflater.Inflate(Resource.Layout.add_to_fav_alert, null);

            var alertDialogBuilder = new Android.Support.V7.App.AlertDialog.Builder(this);
            alertDialogBuilder.SetView(view);

            var edit = view.FindViewById<AWidget.EditText>(Resource.Id.add_to_fav_alert_edit_text);

            // setup a alert window
            alertDialogBuilder.SetCancelable(false);
            alertDialogBuilder.SetPositiveButton(Resource.String.add_to_fav_alert_ok, (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(edit.Text))
                    saveAction.Invoke(edit.Text);
            });
            alertDialogBuilder.SetNegativeButton(Resource.String.add_to_fav_alert_dismiss, (s, e) =>
            {
                ((Android.Support.V7.App.AlertDialog)s).Dismiss();
            });

            // create an alert dialog
            var alert = alertDialogBuilder.Create();
            alert.Show();
        }

        private async void AutoComplete()
        {
            OnPrepareOptionsMenu(actionBarMenu);
            actionBarMenu.FindItem(Resource.Id.action_add_to_fav).SetVisible(false);

            try
            {
                MapFragment mapFragment = FragmentManager.FindFragmentByTag<MapFragment>("MAP_FRAGMENT");

                // just dont autocomplete
                if (mapFragment.MyLocation == null)
                    return;

                List<Prediction> predictions = await PlacesService.GetSearchQueryPredictions(
                    searchTextView.Text, mapFragment.MyLocation);

                AWidget.ArrayAdapter adapter = new AWidget.ArrayAdapter<string>(
                    this, Android.Resource.Layout.SimpleDropDownItem1Line, predictions.Select(x => x.description).ToArray());

                searchTextView.Adapter = adapter;
                adapter.NotifyDataSetChanged();
            }
            catch (ApiCallException)
            {
            }
            catch (QueryAutoCompleteException)
            {
            }
        }

        private async void ShowRouteToNearestAddress(string query)
        {
            CheckInternetConnection();

            if (!string.IsNullOrWhiteSpace(query))
            {
                ProgressDialog pleaseWaitDialog = new ProgressDialog(this);
                pleaseWaitDialog.SetMessage(GetString(Resource.String.please_wait));
                pleaseWaitDialog.SetCancelable(false);
                pleaseWaitDialog.Show();

                try
                {
                    MapFragment mapFragment = FragmentManager.FindFragmentByTag<MapFragment>("MAP_FRAGMENT");

                    if (mapFragment.MyLocation == null)
                    {
                        AWidget.Toast.MakeText(this, GetString(Resource.String.my_location_unavaliable),
                            AWidget.ToastLength.Short).Show();
                        return;
                    }

                    List<Place> places = await PlacesService.GetPlacesByQuery(query, mapFragment.MyLocation);

                    if (places.Count == 0)
                    {
                        AWidget.Toast.MakeText(this, GetString(Resource.String.no_places_found),
                            AWidget.ToastLength.Short).Show();
                        return;
                    }

                    nearestPlace = null;
                    Route shortestRoute = null;
                    foreach (var place in places)
                    {
                        var route = await DirectionsService.GetShortestRoute(
                            mapFragment.MyLocation, place.geometry.location);

                        if (shortestRoute == null)
                        {
                            nearestPlace = place;
                            shortestRoute = route;
                        }
                        else if (route.legs[0].distance.value < shortestRoute.legs[0].distance.value)
                        {
                            nearestPlace = place;
                            shortestRoute = route;
                        }
                    }

                    mapFragment.DrawRouteToPlace(shortestRoute, nearestPlace);
                    OnPrepareOptionsMenu(actionBarMenu);
                    actionBarMenu.FindItem(Resource.Id.action_add_to_fav).SetVisible(true);
                }
                catch (ApiCallException)
                {
                }
                catch (NearbyPlacesSearchException)
                {
                }
                catch (DirectionsException)
                {
                }
                finally
                {
                    pleaseWaitDialog.Cancel();
                }
            }
        }

        private async void ShowRouteToPlace(Place place)
        {
            ProgressDialog pleaseWaitDialog = new ProgressDialog(this);
            pleaseWaitDialog.SetMessage(GetString(Resource.String.please_wait));
            pleaseWaitDialog.SetCancelable(false);
            pleaseWaitDialog.Show();

            try
            {
                MapFragment mapFragment = FragmentManager.FindFragmentByTag<MapFragment>("MAP_FRAGMENT");

                nearestPlace = place;
                Route route = await DirectionsService.GetShortestRoute(
                    mapFragment.MyLocation, place.geometry.location);

                mapFragment.DrawRouteToPlace(route, nearestPlace);
            }
            catch (ApiCallException)
            {
            }
            catch (DirectionsException)
            {
            }
            finally
            {
                pleaseWaitDialog.Cancel();
            }
        }

        private void StartSearch()
        {
            isInSearchMode = true;
            searchTextView.RequestFocus();
            InputMethodManager inputManager = (InputMethodManager)GetSystemService(InputMethodService);
            inputManager.ToggleSoftInput(0, HideSoftInputFlags.NotAlways);
        }

        private void FinishSearch()
        {
            isInSearchMode = false;
            searchTextView.DismissDropDown();
            InputMethodManager inputManager = (InputMethodManager)GetSystemService(InputMethodService);
            inputManager.HideSoftInputFromWindow(searchTextView.WindowToken, HideSoftInputFlags.NotAlways);
        }

        private void UpdateActionBar()
        {
            FragmentManager.ExecutePendingTransactions();

            int actionBarTitleId = Resource.String.app_name;
            bool searchEnabled = true;

            if (FragmentManager.BackStackEntryCount == 1)
            {
                navigationView.SetCheckedItem(Resource.Id.nav_map);
            }
            else if (FragmentManager.BackStackEntryCount > 1)
            {
                Fragment currentFragment = FragmentManager.FindFragmentByTag("SECOND_FRAGMENT");
                switch (currentFragment.GetType().Name)
                {
                    case nameof(FavoriteQueriesFragment):
                        actionBarTitleId = Resource.String.favorite_fragment_name;
                        searchEnabled = false;
                        break;
                    case nameof(FavoritePlacesFragment):
                        actionBarTitleId = Resource.String.favorite_fragment_name;
                        searchEnabled = false;
                        break;
                    case nameof(SettingsFragment):
                        actionBarTitleId = Resource.String.settings_fragment_name;
                        searchEnabled = false;
                        break;
                }
            }

            SupportActionBar.SetTitle(actionBarTitleId);

            OnPrepareOptionsMenu(actionBarMenu);
            actionBarMenu.FindItem(Resource.Id.action_add_to_fav).SetVisible(false);
            actionBarMenu.FindItem(Resource.Id.action_search).SetVisible(searchEnabled);

            if (searchEnabled)
            {
                actionBarMenu.FindItem(Resource.Id.action_search).SetVisible(!isInSearchMode);
                actionBarMenu.FindItem(Resource.Id.action_clear).SetVisible(isInSearchMode);
                actionBarMenu.FindItem(Resource.Id.action_voice).SetVisible(isInSearchMode);
            }
        }

        #endregion
    }
}
