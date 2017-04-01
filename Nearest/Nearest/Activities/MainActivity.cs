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
using Nearest.Models;
using Nearest.Storage;
using Nearest.Fragments;
using Nearest.GoogleApi;
using Nearest.GoogleApi.Model;

namespace Nearest.Activities
{
    [Activity(Label = "@string/app_name", MainLauncher = true, Icon = "@mipmap/ic_launcher")]
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

        private DrawerLayout drawerLayout;
        private NavigationView navigationView;
        private IMenu actionBarMenu;
        private AWidget.AutoCompleteTextView searchTextView;

        private bool IsInSearchMode = false;
        private bool IsActionAddToFavEnabled = false;

        private bool doubleBackToExitPressedOnce = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.main);

            // setup google api
            PlacesService.ApiKey = GetString(Resource.String.google_maps_api_key);
            PlacesService.Language = GetString(Resource.String.language);

            // init toolbar
            var toolbar = FindViewById<Toolbar>(Resource.Id.app_bar);
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
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            actionBarMenu = menu;
            MenuInflater.Inflate(Resource.Menu.action_menu, menu);

            // init search text view
            searchTextView = (AWidget.AutoCompleteTextView)menu.FindItem(Resource.Id.action_search).ActionView;
            searchTextView.TextChanged += (s1, e1) => AutoComplete();
            searchTextView.ItemClick += (s1, e1) => { FinishSearch(); GetNearestAddress(); };

            var searchListener = new SearchActionListener();
            searchListener.Search += (s1, e1) => { FinishSearch(); GetNearestAddress(); };
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
                    expandListener.MenuItemActionExpand += (s, e) => { StartSearch(); UpdateActionBar(); };
                    expandListener.MenuItemActionCollapse += (s, e) => { FinishSearch(); UpdateActionBar(); };
                    Android.Support.V4.View.MenuItemCompat.SetOnActionExpandListener(item, expandListener);
                    return true;
                case Resource.Id.action_clear:
                    searchTextView.Text = string.Empty;
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

                new Handler().PostDelayed(() => doubleBackToExitPressedOnce = false, 2000);
            }
        }

        private void ChangeMainFragment(object sender, NavigationView.NavigationItemSelectedEventArgs e)
        {
            // pop all fragments except first
            int num_of_fragments = FragmentManager.BackStackEntryCount;
            while (num_of_fragments > 1)
            {
                FragmentManager.PopBackStack();
                num_of_fragments--;
            }

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
                        ft.Add(Resource.Id.main_fragment, new FavoriteQueriesFragment(), "SECOND_FRAGMENT");
                        ft.Commit();
                        break;
                    case (Resource.Id.nav_fav_places):
                        ft.Add(Resource.Id.main_fragment, new FavoritePlacesFragment(), "SECOND_FRAGMENT");
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

        #region Private methods

        private void ShowAlert(object sender, PopupMenu.MenuItemClickEventArgs e)
        {
            switch (e.Item.ItemId)
            {
                case (Resource.Id.action_add_fav_query):
                    ShowAddToFavAlert(() => AWidget.Toast.MakeText(this, "save to fav queries", AWidget.ToastLength.Short).Show());
                    break;
                case (Resource.Id.action_add_fav_place):
                    ShowAddToFavAlert(() => AWidget.Toast.MakeText(this, "save to fav places", AWidget.ToastLength.Short).Show());
                    break;
            }
        }

        private void ShowAddToFavAlert(Action saveAction)
        {
            LayoutInflater layoutInflater = LayoutInflater.From(this);
            View view = layoutInflater.Inflate(Resource.Layout.add_to_fav_alert, null);

            var alertDialogBuilder = new Android.Support.V7.App.AlertDialog.Builder(this);
            alertDialogBuilder.SetView(view);

            // setup a alert window
            alertDialogBuilder.SetCancelable(false);
            alertDialogBuilder.SetPositiveButton(Resource.String.add_to_fav_alert_ok, (s, e) => 
            {
                saveAction.Invoke();
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
            try
            {
                MapFragment mapFragment = FragmentManager.FindFragmentByTag<MapFragment>("MAP_FRAGMENT");

                List<Prediction> predictions = await PlacesService.GetSearchQueryPredictions(searchTextView.Text,
                        new Location() { lat = mapFragment.MyLocation.Latitude, lng = mapFragment.MyLocation.Longitude });

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

        private async void GetNearestAddress()
        {
            if (searchTextView.Text != string.Empty)
            {
                try
                {
                    MapFragment mapFragment = FragmentManager.FindFragmentByTag<MapFragment>("MAP_FRAGMENT");

                    List<Place> places = await PlacesService.GetPlacesByQuery(searchTextView.Text, 
                        new Location() { lat = mapFragment.MyLocation.Latitude, lng = mapFragment.MyLocation.Longitude });

                    mapFragment.MarkPlaces(places);

                    string key1 = ApplicationContext.GetString(Resource.String.fav_places_storage_key);
                    var storage1 = new SharedPreference<FavoritePlace>(ApplicationContext, key1);
                    storage1.AddItem(new FavoritePlace() { Id = storage1.GetItems().Count, Name = searchTextView.Text, Place = places[0] });

                    string key2 = ApplicationContext.GetString(Resource.String.fav_query_storage_key);
                    var storage2 = new SharedPreference<FavoriteQuery>(ApplicationContext, key2);
                    storage2.AddItem(new FavoriteQuery() { Id = storage2.GetItems().Count, Name = searchTextView.Text, Query = searchTextView.Text });
                }
                catch (ApiCallException)
                {
                }
                catch (NearbyPlacesSearchException)
                {
                }
            }
        }

        private void StartSearch()
        {
            IsInSearchMode = true;
            searchTextView.RequestFocus();
            InputMethodManager inputManager = (InputMethodManager)GetSystemService(InputMethodService);
            inputManager.ToggleSoftInput(0, HideSoftInputFlags.NotAlways);
        }

        private void FinishSearch()
        {
            IsInSearchMode = false;
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
                        IsActionAddToFavEnabled = false;
                        break;
                    case nameof(FavoritePlacesFragment):
                        actionBarTitleId = Resource.String.favorite_fragment_name;
                        searchEnabled = false;
                        IsActionAddToFavEnabled = false;
                        break;
                    case nameof(SettingsFragment):
                        actionBarTitleId = Resource.String.settings_fragment_name;
                        searchEnabled = false;
                        IsActionAddToFavEnabled = false;
                        break;
                }
            }

            SupportActionBar.SetTitle(actionBarTitleId);
            
            OnPrepareOptionsMenu(actionBarMenu);
            actionBarMenu.FindItem(Resource.Id.action_add_to_fav).SetVisible(IsActionAddToFavEnabled);
            actionBarMenu.FindItem(Resource.Id.action_search).SetVisible(searchEnabled);

            if (searchEnabled)
            {
                actionBarMenu.FindItem(Resource.Id.action_search).SetVisible(!IsInSearchMode);
                actionBarMenu.FindItem(Resource.Id.action_clear).SetVisible(IsInSearchMode);
                actionBarMenu.FindItem(Resource.Id.action_voice).SetVisible(IsInSearchMode);
            }
        }

        #endregion
    }
}
