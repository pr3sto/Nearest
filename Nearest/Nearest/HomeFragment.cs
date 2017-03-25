using Android.App;
using Android.OS;
using Android.Views;

namespace Nearest
{
    public enum ContentType
    {
        Map,
        ListOfFavoritQueries,
        ListOfFavoritPlaces,
        Settings
    }

    public class HomeFragment : Fragment
    {
        public ContentType ContentType { get; set; }

        public HomeFragment()
        { }

        public HomeFragment(ContentType type)
        {
            ContentType = type;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (savedInstanceState != null)
            {
                ContentType = (ContentType)System.Enum.Parse(typeof(ContentType), savedInstanceState.GetString("TYPE"));
            }
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            outState.PutString("TYPE", ContentType.ToString());
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view;
            switch (ContentType)
            {
                case ContentType.Map:
                    view = inflater.Inflate(Resource.Layout.map, container, false);
                    return view;
                case ContentType.ListOfFavoritQueries:
                    view = inflater.Inflate(Resource.Layout.favorites, container, false);
                    return view;
                case ContentType.ListOfFavoritPlaces:
                    view = inflater.Inflate(Resource.Layout.favorites, container, false);
                    return view;
                case ContentType.Settings:
                    view = inflater.Inflate(Resource.Layout.settings, container, false);
                    return view;
                default:
                    view = inflater.Inflate(Resource.Layout.map, container, false);
                    return view;
            }
        }
    }
}