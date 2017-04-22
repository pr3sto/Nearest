using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Preferences;
using Android.Support.V4.Content;
using Android.Views;
using Android.Provider;
using Nearest.GoogleApi;

namespace Nearest.Fragments
{
    public class SettingsFragment : PreferenceFragment, ISharedPreferencesOnSharedPreferenceChangeListener
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            AddPreferencesFromResource(Resource.Layout.settings_fragment);

            PreferenceManager.GetDefaultSharedPreferences(Activity.ApplicationContext).
                RegisterOnSharedPreferenceChangeListener(this);
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = base.OnCreateView(inflater, container, savedInstanceState);
            view.SetBackgroundColor(new Color(ContextCompat.GetColor(Activity.ApplicationContext, Resource.Color.background)));

            Preference button = FindPreference("gps_key");
            button.PreferenceClick += (s, e) => StartActivity(new Intent(Settings.ActionLocationSourceSettings));

            return view;
        }

        public override void OnResume()
        {
            base.OnResume();
            PreferenceManager.GetDefaultSharedPreferences(Activity.ApplicationContext).
                RegisterOnSharedPreferenceChangeListener(this);
        }

        public override void OnPause()
        {
            base.OnPause();
            PreferenceManager.GetDefaultSharedPreferences(Activity.ApplicationContext).
                RegisterOnSharedPreferenceChangeListener(this);
        }

        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            Preference pref = FindPreference(key);

            if (pref is ListPreference)
            {
                ListPreference listPref = (ListPreference)pref;

                if (listPref.Key == "vechicle_mode_key")
                    DirectionsService.Mode = listPref.Value;
                else if (listPref.Key == "unit_system_key")
                    DirectionsService.Unit = listPref.Value;
            }
        }
    }
}
