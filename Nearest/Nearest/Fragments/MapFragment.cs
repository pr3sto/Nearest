using System;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Support.V4.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Nearest.GoogleApi.Models;
using Nearest.GoogleApi.Helpers;

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

        public Location MyLocation { get; set; } = null;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.map_fragment, container, false);

            mapView = view.FindViewById<MapView>(Resource.Id.map_view);
            mapView.OnCreate(savedInstanceState);
            mapView.OnResume();
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
                map.MyLocationChange += (s1, e1) => MyLocation = new Location()
                { lat = e1.Location.Latitude, lng = e1.Location.Longitude };
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

        public void DrawRouteToPlace(Route route, Place place)
        {
            if (MyLocation == null)
                return;

            map.Clear();

            string snippet = route.legs[0].distance.text +
                ", " + route.legs[0].duration.text;

            var marker = new MarkerOptions();
            marker.SetTitle(place.name);
            marker.SetSnippet(snippet);
            marker.SetPosition(new LatLng(place.geometry.location.lat, place.geometry.location.lng));
            map.AddMarker(marker);

            var polyline = new PolylineOptions();
            polyline.InvokeColor(ContextCompat.GetColor(Activity.ApplicationContext, Resource.Color.route_polyline));
            var points = RouteHelper.GetPointsFromRoute(route);
            foreach (var point in points)
                polyline.Add(new LatLng(point.lat, point.lng));

            map.AddPolyline(polyline);

            UpdateCameraPosition(new LatLng(MyLocation.lat, MyLocation.lng));
        }

        #region Private methods

        private void UpdateCameraPosition(LatLng pos)
        {
            CameraPosition.Builder builder = CameraPosition.InvokeBuilder();
            builder.Target(pos);
            builder.Zoom(14);
            CameraPosition cameraPosition = builder.Build();
            CameraUpdate cameraUpdate = CameraUpdateFactory.NewCameraPosition(cameraPosition);
            map.AnimateCamera(cameraUpdate);
        }

        #endregion
    }
}
