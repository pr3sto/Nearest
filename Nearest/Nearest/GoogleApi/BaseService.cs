using System;
using System.Threading.Tasks;
using System.Net;
using Nearest.GoogleApi.Models;

namespace Nearest.GoogleApi
{
    public class BaseService
    {
        public static async Task<string> GetResponse(string request)
        {
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
            return response;
        }
    }
}
