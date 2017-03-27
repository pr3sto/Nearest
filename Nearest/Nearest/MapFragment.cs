using System;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Gms.Maps;

namespace Nearest
{
    public class MyOnMapReady : Java.Lang.Object, IOnMapReadyCallback
    {
        public GoogleMap Map { get; private set; }

        public event EventHandler MapReady;

        public void OnMapReady(GoogleMap googleMap)
        {
            Map = googleMap;
            MapReady?.Invoke(this, EventArgs.Empty);
        }
    }

    public class MapFragment : Fragment
    {
        private MapView mapView;
        private GoogleMap map;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (savedInstanceState != null)
            {
                // TODO create state
            }
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            
            // TODO save state
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.map, container, false);

            mapView = (MapView)view.FindViewById(Resource.Id.map_view);
            mapView.OnCreate(savedInstanceState);
            mapView.OnResume(); // needed to get the map to display immediately
            MapsInitializer.Initialize(Activity.ApplicationContext);

            var mapReadyCallback = new MyOnMapReady();
            mapReadyCallback.MapReady += (sender, args) =>
            {
                map = mapReadyCallback.Map;
                map.MapType = GoogleMap.MapTypeNormal;
                map.MyLocationEnabled = true;
            };

            mapView.GetMapAsync(mapReadyCallback);

            return view;
        }

        public override void OnResume()
        {
            base.OnResume();
            mapView.OnResume();
        }

        public override void OnPause()
        {
            base.OnPause();
            mapView.OnPause();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            mapView.OnDestroy();
        }

        public override void OnLowMemory()
        {
            base.OnLowMemory();
            mapView.OnLowMemory();
        }
    }
}