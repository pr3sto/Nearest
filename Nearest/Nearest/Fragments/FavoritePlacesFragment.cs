using System.Linq;
using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Support.V7.Widget.Helper;
using Android.Views;
using Android.Widget;
using Nearest.Models;
using Nearest.CustomRecyclerView;
using Nearest.CustomRecyclerView.Models;
using Nearest.Storage;

namespace Nearest.Fragments
{
    public class FavoritePlacesFragment : Fragment
    {
        private CustomAdapter adapter;
        private RecyclerView recyclerView;
        private TextView emptyTextView;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.favorites, container, false);

            string key = Activity.ApplicationContext.GetString(Resource.String.fav_places_storage_key);
            var storage = new SharedPreference<FavoritePlace>(Activity.ApplicationContext, key);

            adapter = new CustomAdapter(Activity.ApplicationContext);
            adapter.ItemRemoved += (s, item) =>
            {
                if (adapter.ItemCount == 0)
                {
                    recyclerView.Visibility = ViewStates.Gone;
                    emptyTextView.Visibility = ViewStates.Visible;
                }

                var items = storage.GetItems();
                storage.RemoveItem(items.Where(x => x.Id == item.Id).FirstOrDefault());
            };

            List<FavoritePlace> places = storage.GetItems();
            foreach (var place in places)
                adapter.Items.Add(new CustomItem() { Id = place.Id, Name = place.Name, Description = place.Description });

            emptyTextView = view.FindViewById<TextView>(Resource.Id.fav_list_empty);
            recyclerView = view.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity.ApplicationContext));
            recyclerView.SetAdapter(adapter);
            recyclerView.AddItemDecoration(new ItemDecoration(Activity.ApplicationContext));
            recyclerView.HasFixedSize = true;

            // swipe to remove
            var mItemTouchHelper = new ItemTouchHelper(
                new ItemTouchCallback(0, ItemTouchHelper.Left, Activity.ApplicationContext, adapter));
            mItemTouchHelper.AttachToRecyclerView(recyclerView);

            if (adapter.ItemCount == 0)
            {
                recyclerView.Visibility = ViewStates.Gone;
                emptyTextView.Visibility = ViewStates.Visible;
            }
            else
            {
                recyclerView.Visibility = ViewStates.Visible;
                emptyTextView.Visibility = ViewStates.Gone;
            }

            return view;
        }
    }
}
