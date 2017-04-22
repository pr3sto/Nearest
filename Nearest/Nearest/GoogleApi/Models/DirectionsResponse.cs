using System.Collections.Generic;

namespace Nearest.GoogleApi.Models
{
    public class DirectionsResponse
    {
        public List<GeocodedWaypoint> geocoded_waypoints { get; set; }
        public List<Route> routes { get; set; }
        public string status { get; set; }
    }

    public class GeocodedWaypoint
    {
        public string geocoder_status { get; set; }
        public string place_id { get; set; }
        public List<string> types { get; set; }
    }

    public class Route
    {
        public Bounds bounds { get; set; }
        public string copyrights { get; set; }
        public List<Leg> legs { get; set; }
        public Polyline overview_polyline { get; set; }
        public string summary { get; set; }
        public List<string> warnings { get; set; }
        public List<object> waypoint_order { get; set; }
    }

    public class Bounds
    {
        public Location northeast { get; set; }
        public Location southwest { get; set; }
    }

    public class Leg
    {
        public TextValue distance { get; set; }
        public TextValue duration { get; set; }
        public string end_address { get; set; }
        public Location end_location { get; set; }
        public string start_address { get; set; }
        public Location start_location { get; set; }
        public List<Step> steps { get; set; }
        public List<object> traffic_speed_entry { get; set; }
        public List<object> via_waypoint { get; set; }
    }

    public class Step
    {
        public TextValue distance { get; set; }
        public TextValue duration { get; set; }
        public Location end_location { get; set; }
        public string html_instructions { get; set; }
        public Polyline polyline { get; set; }
        public Location start_location { get; set; }
        public string travel_mode { get; set; }
        public string maneuver { get; set; }
    }

    public class TextValue
    {
        public string text { get; set; }
        public int value { get; set; }
    }

    public class Polyline
    {
        public string points { get; set; }
    }
}
