using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;

namespace Nearest.CustomRecyclerView
{
    class CustomViewHolder : RecyclerView.ViewHolder
    {
        public TextView NameTextView { get; set; }
        public TextView DescriptionTextView { get; set; }
        public Button UndoButton { get; set; }

        public CustomViewHolder(ViewGroup parent) :
            base(LayoutInflater.From(parent.Context).Inflate(Resource.Layout.row_view, parent, false))
        {
            NameTextView = ItemView.FindViewById<TextView>(Resource.Id.text_view_name);
            DescriptionTextView = ItemView.FindViewById<TextView>(Resource.Id.text_view_description);
            UndoButton = ItemView.FindViewById<Button>(Resource.Id.undo_button);
        }
    }
}
