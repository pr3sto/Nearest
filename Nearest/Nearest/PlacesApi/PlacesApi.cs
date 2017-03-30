using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using Nearest.PlacesApi.Model;
using Newtonsoft.Json;

namespace Nearest.PlacesApi
{
    public static class PlacesApi
    {
        private const string AUTOCOMPLETE_API = "https://maps.googleapis.com/maps/api/place/queryautocomplete/json";
        private const string NEARBYSEARCH_API = "https://maps.googleapis.com/maps/api/place/nearbysearch/json";

        public static async Task<List<Prediction>> GetSearchQueryPredictions(
            string searchQueryText, string apiKey, Android.Locations.Location location, int radius)
        {
            string request = AUTOCOMPLETE_API + "?input=" + searchQueryText;
            if (location != null)
            {
                string latitude = location.Latitude.ToString("0.00000", System.Globalization.CultureInfo.InvariantCulture);
                string longitude = location.Longitude.ToString("0.00000", System.Globalization.CultureInfo.InvariantCulture);
                request += "&location=" + latitude + "," + longitude + "&radius=" + radius;
            }
            request += "&key=" + apiKey;

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

            QueryAutoCompleteResponse responseJson = JsonConvert.DeserializeObject<QueryAutoCompleteResponse>(response);

            if (!new List<string>() { "OK", "ZERO_RESULTS", "OVER_QUERY_LIMIT" }.Contains(responseJson.status))
                throw new QueryAutoCompleteException(responseJson.status);

            return responseJson.predictions;
        }

        public static async Task<List<Place>> GetPlacesByQuery(
            string searchQueryText, string apiKey, Android.Locations.Location location, int radius)
        {
            if (location == null)
                throw new NearbyPlacesSearchException("location is null");

            string request = NEARBYSEARCH_API + "?keyword=" + searchQueryText;

            string latitude = location.Latitude.ToString("0.00000", System.Globalization.CultureInfo.InvariantCulture);
            string longitude = location.Longitude.ToString("0.00000", System.Globalization.CultureInfo.InvariantCulture);
            request += "&location=" + latitude + "," + longitude + "&radius=" + radius;

            request += "&key=" + apiKey;

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

            NearbyPlacesSearchResponse responseJson = JsonConvert.DeserializeObject<NearbyPlacesSearchResponse>(response);

            if (!new List<string>() { "OK", "ZERO_RESULTS", "OVER_QUERY_LIMIT" }.Contains(responseJson.status))
                throw new NearbyPlacesSearchException(responseJson.status);

            return responseJson.results;
        }
    }
}
