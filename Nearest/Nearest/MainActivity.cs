using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.App;

namespace Nearest
{
    [Activity(Label = "@string/app_name", MainLauncher = true, Icon = "@mipmap/ic_launcher")]
    public class MainActivity : AppCompatActivity
    {
        private DrawerLayout drawerLayout;
        private IMenu actionBarMenu;
        private bool doubleBackToExitPressedOnce = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.main);
            drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);

            // init toolbar
            var toolbar = FindViewById<Toolbar>(Resource.Id.app_bar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetTitle(Resource.String.app_name);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);

            // attach item selected handler to navigation view
            var navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.SetCheckedItem(Resource.Id.nav_map);
            navigationView.NavigationItemSelected += ChangeMainFragment;

            // create ActionBarDrawerToggle button and add it to the toolbar
            var drawerToggle = new ActionBarDrawerToggle(this, drawerLayout, toolbar, Resource.String.open_drawer, Resource.String.close_drawer);
            drawerLayout.SetDrawerListener(drawerToggle);
            drawerToggle.SyncState();

            // load default home screen
            if (savedInstanceState == null)
            {
                using (var ft = FragmentManager.BeginTransaction())
                {
                    ft.AddToBackStack(null);
                    ft.Add(Resource.Id.main_fragment, new MapFragment());
                    ft.Commit();
                }
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            actionBarMenu = menu;
            MenuInflater.Inflate(Resource.Menu.action_menu, menu);
            UpdateActionBar();

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
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

                Android.Widget.Toast.MakeText(this, GetString(Resource.String.double_click_to_exit), 
                    Android.Widget.ToastLength.Short).Show();

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

            UpdateActionBar();
            drawerLayout.CloseDrawers();
        }

        private void ShowAlert(object sender, PopupMenu.MenuItemClickEventArgs e)
        {
            switch (e.Item.ItemId)
            {
                case (Resource.Id.action_add_fav_query):
                    ShowAddToFavAlert(() => Android.Widget.Toast.MakeText(this, "save to fav queries", Android.Widget.ToastLength.Short).Show());
                    break;
                case (Resource.Id.action_add_fav_place):
                    ShowAddToFavAlert(() => Android.Widget.Toast.MakeText(this, "save to fav places", Android.Widget.ToastLength.Short).Show());
                    break;
            }
        }

        private void ShowAddToFavAlert(System.Action saveAction)
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

        private void UpdateActionBar()
        {
            FragmentManager.ExecutePendingTransactions();

            int actionBarTitleId = Resource.String.app_name;
            bool actionAddToFavEnabled = true;
             
            if (FragmentManager.BackStackEntryCount == 1)
            {
                var navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
                navigationView.SetCheckedItem(Resource.Id.nav_map);
            }
            else if (FragmentManager.BackStackEntryCount > 1)
            {
                Fragment currentFragment = FragmentManager.FindFragmentByTag("SECOND_FRAGMENT");
                switch (currentFragment.GetType().Name)
                {
                    case nameof(FavoriteQueriesFragment):
                        actionBarTitleId = Resource.String.favorite_fragment_name;
                        actionAddToFavEnabled = false;
                        break;
                    case nameof(FavoritePlacesFragment):
                        actionBarTitleId = Resource.String.favorite_fragment_name;
                        actionAddToFavEnabled = false;
                        break;
                    case nameof(SettingsFragment):
                        actionBarTitleId = Resource.String.settings_fragment_name;
                        actionAddToFavEnabled = false;
                        break;
                }
            }

            SupportActionBar.SetTitle(actionBarTitleId);
            if (actionBarMenu != null)
            {
                OnPrepareOptionsMenu(actionBarMenu);
                actionBarMenu.FindItem(Resource.Id.action_add_to_fav).SetVisible(actionAddToFavEnabled);
            }
        }
    }
}
