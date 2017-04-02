using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;
using Nearest.GoogleApi.Models;

namespace Nearest.GoogleApi
{
    public static class PlacesService
    {
        private const string AUTOCOMPLETE_API = "https://maps.googleapis.com/maps/api/place/queryautocomplete/json";
        private const string NEARBYSEARCH_API = "https://maps.googleapis.com/maps/api/place/nearbysearch/json";

        public static string ApiKey { get; set; } = "";
        public static string Language { get; set; } = "en";
        public static int Radius { get; set; } = 5000;

        public static async Task<List<Prediction>> GetSearchQueryPredictions(string searchQueryText, Location myLocation)
        {
            string request = AUTOCOMPLETE_API + "?input=" + searchQueryText;

            string latitude = myLocation.lat.ToString("0.000000", System.Globalization.CultureInfo.InvariantCulture);
            string longitude = myLocation.lng.ToString("0.000000", System.Globalization.CultureInfo.InvariantCulture);

            request += "&location=" + latitude + "," + longitude + "&radius=" + Radius;
            request += "&language=" + Language + "&key=" + ApiKey;

            string response = "";

            try
            {
                using (WebClient webclient = new WebClient())
                    response = await webclient.DownloadStringTaskAsync(new Uri(request));
            }
            catch (WebException)
            {
                throw new ApiCallException();
            }

            var responseJson = JsonConvert.DeserializeObject<QueryAutoCompleteResponse>(response);

            if (!new List<string>() { "OK", "ZERO_RESULTS", "OVER_QUERY_LIMIT" }.Contains(responseJson.status))
                throw new QueryAutoCompleteException(responseJson.status);

            return responseJson.predictions;
        }

        public static async Task<List<Place>> GetPlacesByQuery(string searchQueryText, Location myLocation)
        {
            if (myLocation == null)
                throw new NearbyPlacesSearchException("location is null");

            string request = NEARBYSEARCH_API + "?keyword=" + searchQueryText;

            string latitude = myLocation.lat.ToString("0.000000", System.Globalization.CultureInfo.InvariantCulture);
            string longitude = myLocation.lng.ToString("0.000000", System.Globalization.CultureInfo.InvariantCulture);

            request += "&location=" + latitude + "," + longitude + "&radius=" + Radius;
            request += "&language=" + Language + "&key=" + ApiKey;  

            string response = "";

            try
            {
                using (WebClient webclient = new WebClient())
                    response = await webclient.DownloadStringTaskAsync(new Uri(request));
            }
            catch (WebException)
            {
                throw new ApiCallException();
            }

            var responseJson = JsonConvert.DeserializeObject<NearbyPlacesSearchResponse>(response);

            if (!new List<string>() { "OK", "ZERO_RESULTS", "OVER_QUERY_LIMIT" }.Contains(responseJson.status))
                throw new NearbyPlacesSearchException(responseJson.status);

            return responseJson.results;
        }
    }
}
