using System;
using System.Linq;
using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Support.V7.Widget.Helper;
using Android.Views;
using Android.Widget;
using Nearest.Models;
using Nearest.GoogleApi.Models;
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
        private List<FavoritePlace> favPlaces;

        public EventHandler<Place> ItemClicked;

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
            View view = inflater.Inflate(Resource.Layout.favorites_fragment, container, false);

            string key = Activity.ApplicationContext.GetString(Resource.String.fav_places_storage_key);
            var storage = new FavoritesStorage<FavoritePlace>(Activity.ApplicationContext, key);

            favPlaces = storage.GetItems();

            adapter = new CustomAdapter(Activity.ApplicationContext);
            adapter.ItemClicked += (s, item) =>
            {
                var place = favPlaces.Single(x => x.Id == item.Id);
                ItemClicked?.Invoke(this, place.Place);
            };
            adapter.ItemRemoved += (s, item) =>
            {
                if (adapter.ItemCount == 0)
                {
                    recyclerView.Visibility = ViewStates.Gone;
                    emptyTextView.Visibility = ViewStates.Visible;
                }
                storage.RemoveItem(favPlaces.Single(x => x.Id == item.Id));
            };

            foreach (var place in favPlaces)
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
