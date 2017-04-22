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
using Nearest.CustomRecyclerView;
using Nearest.CustomRecyclerView.Models;
using Nearest.Storage;

namespace Nearest.Fragments
{
    public class FavoriteQueriesFragment : Fragment
    {
        private CustomAdapter adapter;
        private RecyclerView recyclerView;
        private TextView emptyTextView;
        private List<FavoriteQuery> favQueries;

        public EventHandler<string> ItemClicked;

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

            string key = Activity.ApplicationContext.GetString(Resource.String.fav_query_storage_key);
            var storage = new FavoritesStorage<FavoriteQuery>(Activity.ApplicationContext, key);

            favQueries = storage.GetItems();

            adapter = new CustomAdapter(Activity.ApplicationContext);
            adapter.ItemClicked += (s, item) =>
            {
                var query = favQueries.Single(x => x.Id == item.Id);
                ItemClicked?.Invoke(this, query.Query);
            };
            adapter.ItemRemoved += (s, item) =>
            {
                if (adapter.ItemCount == 0)
                {
                    recyclerView.Visibility = ViewStates.Gone;
                    emptyTextView.Visibility = ViewStates.Visible;
                }
                storage.RemoveItem(favQueries.Single(x => x.Id == item.Id));
            };

            foreach (var query in favQueries)
                adapter.Items.Add(new CustomItem() { Id = query.Id, Name = query.Name, Description = query.Query });

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
