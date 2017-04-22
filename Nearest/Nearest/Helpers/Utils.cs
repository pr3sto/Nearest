using Android.Content;
using Android.Net;

namespace Nearest.Helpers
{
    class Utils
    {
        public static bool IsNetworkAvailable(Context context)
        {
            ConnectivityManager connectivityManager = (ConnectivityManager)
                context.GetSystemService(Context.ConnectivityService);

            NetworkInfo networkInfo = connectivityManager.ActiveNetworkInfo;
            return networkInfo != null && networkInfo.IsConnected;
        }
    }
}
