using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nearest.GoogleApi.Models;
using Nearest.GoogleApi.Helpers;

namespace Nearest.GoogleApi
{
    public class PlacesService : BaseService
    {
        private const string AUTOCOMPLETE_API = "https://maps.googleapis.com/maps/api/place/queryautocomplete/json";
        private const string NEARBYSEARCH_API = "https://maps.googleapis.com/maps/api/place/nearbysearch/json";

        public static string ApiKey { get; set; } = "";
        public static string Language { get; set; } = "en";
        public static int Radius { get; set; } = 5000;

        public static async Task<List<Prediction>> GetSearchQueryPredictions(string searchQueryText, Location myLocation)
        {
            if (myLocation == null)
                throw new NearbyPlacesSearchException("my location is null");

            string request = AUTOCOMPLETE_API + "?input=" + searchQueryText;

            // my location
            string latitude = Utils.GetFormatedCoord(myLocation.lat);
            string longitude = Utils.GetFormatedCoord(myLocation.lng);
            request += "&location=" + latitude + "," + longitude;

            // options
            request += "&radius=" + Radius;
            request += "&language=" + Language + "&key=" + ApiKey;

            string responseJson = await GetResponse(request);
            var response = JsonConvert.DeserializeObject<QueryAutoCompleteResponse>(responseJson);

            if (!new List<string>() { "OK", "ZERO_RESULTS" }.Contains(response.status))
                throw new QueryAutoCompleteException(response.status);

            if (response.status == "OVER_QUERY_LIMIT")
                throw new OverQueryLimitException();

            return response.predictions;
        }

        public static async Task<List<Place>> GetPlacesByQuery(string searchQueryText, Location myLocation)
        {
            if (myLocation == null)
                throw new NearbyPlacesSearchException("my location is null");

            string request = NEARBYSEARCH_API + "?keyword=" + searchQueryText;

            // my location
            string latitude = Utils.GetFormatedCoord(myLocation.lat);
            string longitude = Utils.GetFormatedCoord(myLocation.lng);
            request += "&location=" + latitude + "," + longitude;

            // options
            request += "&radius=" + Radius;
            request += "&language=" + Language + "&key=" + ApiKey;

            string responseJson = await GetResponse(request);
            var response = JsonConvert.DeserializeObject<NearbyPlacesSearchResponse>(responseJson);

            if (!new List<string>() { "OK", "ZERO_RESULTS" }.Contains(response.status))
                throw new NearbyPlacesSearchException(response.status);

            if (response.status == "OVER_QUERY_LIMIT")
                throw new OverQueryLimitException();

            return response.results;
        }
    }
}
