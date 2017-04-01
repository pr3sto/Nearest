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
        private Drawable xMark;
        private int xMarkMargin;
        bool initiated;

        private Context context;
        private CustomAdapter adapter;

        public ItemTouchCallback(int dragDirs, int swipeDirs, Context context, CustomAdapter adapter) :
            base(dragDirs, swipeDirs)
        {
            this.context = context;
            this.adapter = adapter;
        }

        private void init()
        {
            background = new ColorDrawable(new Color(ContextCompat.GetColor(context, Resource.Color.item_view_removed)));
            xMark = ContextCompat.GetDrawable(context, Resource.Drawable.ic_action_delete);
            xMarkMargin = 16;
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
                init();

            // draw background
            background.SetBounds(itemView.Right + (int)dX, itemView.Top, itemView.Right, itemView.Bottom);
            background.Draw(canvas);

            // draw x mark
            int itemHeight = itemView.Bottom - itemView.Top;
            int intrinsicWidth = xMark.IntrinsicWidth;
            int intrinsicHeight = xMark.IntrinsicWidth;

            int xMarkLeft = itemView.Right - xMarkMargin - intrinsicWidth;
            int xMarkRight = itemView.Right - xMarkMargin;
            int xMarkTop = itemView.Top + (itemHeight - intrinsicHeight) / 2;
            int xMarkBottom = xMarkTop + intrinsicHeight;
            xMark.SetBounds(xMarkLeft, xMarkTop, xMarkRight, xMarkBottom);

            xMark.Draw(canvas);

            base.OnChildDraw(canvas, recyclerView, viewHolder, dX, dY, actionState, isCurrentlyActive);
        }
    }
}
