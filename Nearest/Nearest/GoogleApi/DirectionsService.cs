using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nearest.GoogleApi.Models;
using Nearest.GoogleApi.Helpers;

namespace Nearest.GoogleApi
{
    public class DirectionsService : BaseService
    {
        private const string DIRECTIONS_API = "https://maps.googleapis.com/maps/api/directions/json";

        public static string ApiKey { get; set; } = "";
        public static string Language { get; set; } = "en";
        public static string Mode { get; set; } = "walking";
        public static string Unit { get; set; } = "metric";

        public static async Task<Route> GetShortestRoute(Location myLocation, Location placeLocation)
        {
            if (myLocation == null)
                throw new DirectionsException("my location is null");
            if (placeLocation == null)
                throw new DirectionsException("place location is null");

            string request = DIRECTIONS_API;

            // my location
            string latitude = Utils.GetFormatedCoord(myLocation.lat);
            string longitude = Utils.GetFormatedCoord(myLocation.lng);
            request += "?origin=" + latitude + "," + longitude;

            // destination location
            latitude = Utils.GetFormatedCoord(placeLocation.lat);
            longitude = Utils.GetFormatedCoord(placeLocation.lng);
            request += "&destination=" + latitude + "," + longitude;

            // options
            request += "&mode=" + Mode + "&unit=" + Unit;
            request += "&language=" + Language + "&key=" + ApiKey;

            string responseJson = await GetResponse(request);
            var response = JsonConvert.DeserializeObject<DirectionsResponse>(responseJson);

            if (!new List<string>() { "OK", "ZERO_RESULTS" }.Contains(response.status))
                throw new DirectionsException(response.status);

            if (response.status == "OVER_QUERY_LIMIT")
                throw new OverQueryLimitException();

            // process response
            if (response.routes.Count == 0)
                throw new DirectionsException("no routes");

            Route shortestRoute = response.routes[0];
            if (shortestRoute.legs.Count == 0)
                throw new DirectionsException("no legs in route");

            foreach (var route in response.routes)
            {
                if (route.legs.Count == 0)
                    throw new DirectionsException("no legs in route");

                if (route.legs[0].distance.value < shortestRoute.legs[0].distance.value)
                    shortestRoute = route;
            }

            return shortestRoute;
        }
    }
}
