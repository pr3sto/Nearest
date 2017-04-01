using Android.App;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Support.V7.Widget.Helper;
using Android.Views;
using Android.Widget;
using Nearest.CustomRecyclerView;
using Nearest.CustomRecyclerView.Models;

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

            if (savedInstanceState != null)
            {
                // TODO create state
            }
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            // TODO save state
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.favorites, container, false);

            adapter = new CustomAdapter(Activity.ApplicationContext);

            adapter.ItemRemoved += (s, item) =>
            {
                if (adapter.ItemCount == 0)
                {
                    recyclerView.Visibility = ViewStates.Gone;
                    emptyTextView.Visibility = ViewStates.Visible;
                }
            };

            for (int i = 0; i < 20; i++)
                adapter.Items.Add(new CustomItem() { Id = i, Name = "name " + i, Description = "description " + i });


            emptyTextView = view.FindViewById<TextView>(Resource.Id.fav_list_empty);
            recyclerView = view.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity.ApplicationContext));
            recyclerView.SetAdapter(adapter);
            recyclerView.AddItemDecoration(new ItemDecoration(Activity.ApplicationContext));
            recyclerView.HasFixedSize = true;

            // swipe to remove
            ItemTouchHelper mItemTouchHelper = new ItemTouchHelper(
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
