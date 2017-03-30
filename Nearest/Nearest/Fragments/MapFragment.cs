using System;
using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Nearest.PlacesApi.Model;

namespace Nearest.Fragments
{
    public class MapFragment : Fragment
    {
        #region Callbacks

        internal class OnMapReadyCallback : Java.Lang.Object, IOnMapReadyCallback
        {
            public GoogleMap Map { get; private set; }

            public event EventHandler MapReady;

            public void OnMapReady(GoogleMap googleMap)
            {
                Map = googleMap;
                MapReady?.Invoke(this, EventArgs.Empty);
            }
        }

        #endregion

        private MapView mapView;
        private GoogleMap map;

        public Android.Locations.Location MyLocation { get; set; } = null;

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

            mapView = view.FindViewById<MapView>(Resource.Id.map_view);
            mapView.OnCreate(savedInstanceState);
            mapView.OnResume(); // needed to get the map to display immediately
            MapsInitializer.Initialize(Activity.ApplicationContext);

            var mapReadyCallback = new OnMapReadyCallback();
            mapReadyCallback.MapReady += (s, e) =>
            {
                map = mapReadyCallback.Map;
                map.MapType = GoogleMap.MapTypeNormal;
                map.MyLocationEnabled = true;
                map.UiSettings.CompassEnabled = true;
                map.UiSettings.IndoorLevelPickerEnabled = true;
                map.UiSettings.MapToolbarEnabled = true;
                map.UiSettings.MyLocationButtonEnabled = true;
                map.UiSettings.RotateGesturesEnabled = true;
                map.UiSettings.ScrollGesturesEnabled = true;
                map.UiSettings.TiltGesturesEnabled = true;
                map.UiSettings.ZoomControlsEnabled = true;
                map.UiSettings.ZoomGesturesEnabled = true;
                map.MyLocationChange += (s1, e1) => MyLocation = e1.Location;
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

        public void MarkPlaces(List<Place> places)
        {
            foreach (var place in places)
                MarkOnMap(place.name, new LatLng(place.geometry.location.lat, place.geometry.location.lng));

            if (MyLocation != null)
                UpdateCameraPosition(new LatLng(MyLocation.Latitude, MyLocation.Longitude));
        }

        #region Private methods

        private void MarkOnMap(string title, LatLng pos)
        {
            Activity.RunOnUiThread(() =>
            {
                var marker = new MarkerOptions();
                marker.SetTitle(title);
                marker.SetPosition(pos);
                map.AddMarker(marker);
            });
        }

        private void UpdateCameraPosition(LatLng pos)
        {
            CameraPosition.Builder builder = CameraPosition.InvokeBuilder();
            builder.Target(pos);
            builder.Zoom(14);
            builder.Bearing(45);
            builder.Tilt(90);
            CameraPosition cameraPosition = builder.Build();
            CameraUpdate cameraUpdate = CameraUpdateFactory.NewCameraPosition(cameraPosition);
            map.AnimateCamera(cameraUpdate);
        }

        #endregion
    }
}
