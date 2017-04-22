namespace Nearest.GoogleApi.Helpers
{
    public static class Utils
    {
        private const string FORMAT = "0.000000";

        public static string GetFormatedCoord(double coord)
        {
            return coord.ToString(FORMAT, System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
