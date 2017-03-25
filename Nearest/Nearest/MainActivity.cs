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
            navigationView.NavigationItemSelected += NavigationView_NavigationItemSelected;

            // create ActionBarDrawerToggle button and add it to the toolbar
            var drawerToggle = new ActionBarDrawerToggle(this, drawerLayout, toolbar, Resource.String.open_drawer, Resource.String.close_drawer);
            drawerLayout.SetDrawerListener(drawerToggle);
            drawerToggle.SyncState();

            // load default home screen
            if (savedInstanceState == null)
            {
                var ft = FragmentManager.BeginTransaction();
                ft.AddToBackStack(null);
                ft.Add(Resource.Id.main_fragment, new HomeFragment(ContentType.Map));
                ft.Commit();
            }
            
            UpdateInterface();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            actionBarMenu = menu;
            MenuInflater.Inflate(Resource.Menu.action_menu, menu);
            UpdateInterface();

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    return true;
                case Resource.Id.action_add_to_fav:
                    // open 'add to fav' popup
                    var popup = new PopupMenu(this, FindViewById(Resource.Id.action_add_to_fav));
                    popup.MenuInflater.Inflate(Resource.Menu.add_to_fav_popup_menu, popup.Menu);
                    popup.MenuItemClick += PopupMenu_MenuItemClick;
                    popup.Show();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }
        
        private void NavigationView_NavigationItemSelected(object sender, NavigationView.NavigationItemSelectedEventArgs e)
        {
            switch (e.MenuItem.ItemId)
            {
                case (Resource.Id.nav_map):
                    ShowMapFragment();
                    break;
                case (Resource.Id.nav_fav_queries):
                    ShowHomeFragment(ContentType.ListOfFavoritQueries);
                    break;
                case (Resource.Id.nav_fav_places):
                    ShowHomeFragment(ContentType.ListOfFavoritPlaces);
                    break;
                case (Resource.Id.nav_settings):
                    ShowHomeFragment(ContentType.Settings);
                    break;
                case (Resource.Id.nav_exit):
                    FinishAffinity();
                    break;
            }
            drawerLayout.CloseDrawers();
        }
        
        private void PopupMenu_MenuItemClick(object sender, PopupMenu.MenuItemClickEventArgs e)
        {
            switch (e.Item.ItemId)
            {
                case (Resource.Id.action_add_fav_query):
                    ShowAddToFavDialog();
                    break;
                case (Resource.Id.action_add_fav_place):
                    ShowAddToFavDialog();
                    break;
            }
        }
        
        public override void OnBackPressed()
        {
            if (drawerLayout.IsDrawerOpen(Android.Support.V4.View.GravityCompat.Start))
            {
                drawerLayout.CloseDrawers();
            }
            else if (FragmentManager.BackStackEntryCount > 1)
            {
                FragmentManager.PopBackStack();
                UpdateInterface();
            }
            else
            {
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

        private void ShowAddToFavDialog()
        {
            LayoutInflater layoutInflater = LayoutInflater.From(this);
            View promptView = layoutInflater.Inflate(Resource.Layout.add_to_fav_alert, null);
            var alertDialogBuilder = new Android.Support.V7.App.AlertDialog.Builder(this);
            alertDialogBuilder.SetView(promptView);

            // setup a dialog window
            alertDialogBuilder.SetCancelable(false);
            alertDialogBuilder.SetPositiveButton(Resource.String.add_to_fav_alert_save, (s, e) => 
            {
                Android.Widget.Toast.MakeText(this, "save", Android.Widget.ToastLength.Short).Show();
            });
            alertDialogBuilder.SetNegativeButton(Resource.String.add_to_fav_alert_dismiss, (s, e) => 
            {
                Android.Widget.Toast.MakeText(this, "dismiss", Android.Widget.ToastLength.Short).Show();
            });
                    
		    // create an alert dialog
		    var alert = alertDialogBuilder.Create();
            alert.Show();
	    }

        private void ShowMapFragment()
        {
            int n = FragmentManager.BackStackEntryCount - 1;
            while (n-- > 0)
                FragmentManager.PopBackStack();

            UpdateInterface();
        }

        private void ShowHomeFragment(ContentType type)
        {
            if (FragmentManager.BackStackEntryCount > 1)
                FragmentManager.PopBackStack();

            var ft1 = FragmentManager.BeginTransaction();
            ft1.AddToBackStack(null);
            ft1.Add(Resource.Id.main_fragment, new HomeFragment(type), "SECOND_FRAGMENT");
            ft1.Commit();

            UpdateInterface();
        }

        private void UpdateInterface()
        {
            FragmentManager.ExecutePendingTransactions();
            if (FragmentManager.BackStackEntryCount == 1)
            {
                SupportActionBar.SetTitle(Resource.String.app_name);
                if (actionBarMenu != null)
                {
                    OnPrepareOptionsMenu(actionBarMenu);
                    actionBarMenu.FindItem(Resource.Id.action_add_to_fav).SetVisible(true);
                }

                var navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
                navigationView.SetCheckedItem(Resource.Id.nav_map);
            }
            else if (FragmentManager.BackStackEntryCount > 1)
            {
                switch (((HomeFragment)FragmentManager.FindFragmentByTag("SECOND_FRAGMENT")).ContentType)
                {
                    case ContentType.ListOfFavoritQueries:
                        SupportActionBar.SetTitle(Resource.String.favorite_fragment_name);
                        if (actionBarMenu != null)
                        {
                            OnPrepareOptionsMenu(actionBarMenu);
                            actionBarMenu.FindItem(Resource.Id.action_add_to_fav).SetVisible(false);
                        }
                        break;
                    case ContentType.ListOfFavoritPlaces:
                        SupportActionBar.SetTitle(Resource.String.favorite_fragment_name);
                        if (actionBarMenu != null)
                        {
                            OnPrepareOptionsMenu(actionBarMenu);
                            actionBarMenu.FindItem(Resource.Id.action_add_to_fav).SetVisible(false);
                        }
                        break;
                    case ContentType.Settings:
                        SupportActionBar.SetTitle(Resource.String.settings_fragment_name);
                        if (actionBarMenu != null)
                        {
                            OnPrepareOptionsMenu(actionBarMenu);
                            actionBarMenu.FindItem(Resource.Id.action_add_to_fav).SetVisible(false);
                        }
                        break;
                    default:
                        SupportActionBar.SetTitle(Resource.String.app_name);
                        if (actionBarMenu != null)
                        {
                            OnPrepareOptionsMenu(actionBarMenu);
                            actionBarMenu.FindItem(Resource.Id.action_add_to_fav).SetVisible(true);
                        }
                        break;
                }
            }
        }
    }
}
