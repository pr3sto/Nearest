using System.Collections.Generic;
using Nearest.GoogleApi.Models;

namespace Nearest.GoogleApi.Helpers
{
    public static class RouteHelper
    {
        public static List<Location> GetPointsFromRoute(Route route)
        {
            List<Location> points = new List<Location>();

            foreach (var step in route.legs[0].steps)
            {
                points.Add(new Location() { lat = step.start_location.lat, lng = step.start_location.lng });
                points.AddRange(DecodePolyline(step.polyline.points));
                points.Add(new Location() { lat = step.end_location.lat, lng = step.end_location.lng });
            }

            return points;
        }

        private static List<Location> DecodePolyline(string polyline)
        {
            List<Location> points = new List<Location>();

            int index = 0;
            int lat = 0, lng = 0;

            while (index < polyline.Length)
            {
                int b, shift = 0, result = 0;
                do
                {
                    b = polyline[index++] - 63;
                    result |= (b & 0x1f) << shift;
                    shift += 5;
                } while (b >= 0x20);

                int dlat = ((result & 1) != 0 ? ~(result >> 1) : (result >> 1));

                lat += dlat;
                shift = 0;
                result = 0;

                do
                {
                    b = polyline[index++] - 63;
                    result |= (b & 0x1f) << shift;
                    shift += 5;
                } while (b >= 0x20);

                int dlng = ((result & 1) != 0 ? ~(result >> 1) : (result >> 1));
                lng += dlng;

                Location point = new Location() { lat = (double)lat / 1E5, lng = (double)lng / 1E5 };
                points.Add(point);
            }
            return points;
        }
    }
}