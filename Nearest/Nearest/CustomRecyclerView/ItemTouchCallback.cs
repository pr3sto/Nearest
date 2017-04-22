using Android.Content;
using Android.Views;
using Android.Support.V7.Widget;
using Android.Graphics;
using Android.Support.V7.Widget.Helper;
using Android.Graphics.Drawables;
using Android.Support.V4.Content;

namespace Nearest.CustomRecyclerView
{
    class ItemTouchCallback : ItemTouchHelper.SimpleCallback
    {
        private Drawable background;
        private Drawable trashMark;
        private int trashMarkMargin;
        bool initiated;

        private Context context;
        private CustomAdapter adapter;

        public ItemTouchCallback(int dragDirs, int swipeDirs, Context context, CustomAdapter adapter) :
            base(dragDirs, swipeDirs)
        {
            this.context = context;
            this.adapter = adapter;
        }

        private void Init()
        {
            background = new ColorDrawable(new Color(ContextCompat.GetColor(context, Resource.Color.item_view_removed)));
            trashMark = ContextCompat.GetDrawable(context, Resource.Drawable.ic_action_delete);
            trashMarkMargin = (int)context.Resources.GetDimension(Resource.Dimension.item_view_trash_mark_margin);
            initiated = true;
        }

        public override bool OnMove(RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder, RecyclerView.ViewHolder target)
        {
            return false;
        }

        public override int GetSwipeDirs(RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder)
        {
            int position = viewHolder.AdapterPosition;
            if (adapter.IsPendingRemoval(position))
                return 0;
            else
                return base.GetSwipeDirs(recyclerView, viewHolder);
        }

        public override void OnSwiped(RecyclerView.ViewHolder viewHolder, int swipeDir)
        {
            int swipedPosition = viewHolder.AdapterPosition;
            adapter.PendingRemoval(swipedPosition);
        }

        public override void OnChildDraw(Canvas canvas, RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder, float dX, float dY, int actionState, bool isCurrentlyActive)
        {
            View itemView = viewHolder.ItemView;

            // when this method get's called for viewholder that are already swiped away
            if (viewHolder.AdapterPosition == -1)
                return;

            if (!initiated)
                Init();

            // draw background
            background.SetBounds(itemView.Right + (int)dX, itemView.Top, itemView.Right, itemView.Bottom);
            background.Draw(canvas);

            // draw trash mark
            int itemHeight = itemView.Bottom - itemView.Top;
            int intrinsicWidth = trashMark.IntrinsicWidth;
            int intrinsicHeight = trashMark.IntrinsicWidth;

            int markLeft = itemView.Right - trashMarkMargin - intrinsicWidth;
            int markRight = itemView.Right - trashMarkMargin;
            int markTop = itemView.Top + (itemHeight - intrinsicHeight) / 2;
            int markBottom = markTop + intrinsicHeight;
            trashMark.SetBounds(markLeft, markTop, markRight, markBottom);

            trashMark.Draw(canvas);

            base.OnChildDraw(canvas, recyclerView, viewHolder, dX, dY, actionState, isCurrentlyActive);
        }
    }
}
