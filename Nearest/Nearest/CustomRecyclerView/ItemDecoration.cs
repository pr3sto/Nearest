using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;

namespace Nearest.CustomRecyclerView
{
    class ItemDecoration : RecyclerView.ItemDecoration
    {
        private Context context;
        private Drawable background;
        private Drawable divider;
        private bool initiated;

        public ItemDecoration(Context context)
        {
            this.context = context;
        }

        private void Init()
        {
            background = new ColorDrawable(new Color(ContextCompat.GetColor(context, Resource.Color.item_view_removed)));
            divider = ContextCompat.GetDrawable(context, Resource.Drawable.line_divider);
            initiated = true;
        }

        public override void OnDraw(Canvas canvas, RecyclerView parent, RecyclerView.State state)
        {
            if (!initiated)
                Init();

            if (parent.GetItemAnimator().IsRunning)
            {
                View lastViewComingDown = null;
                View firstViewComingUp = null;

                // this is fixed
                int left = 0;
                int right = parent.Width;

                // this we need to find out
                int top = 0;
                int bottom = 0;

                // find relevant translating views
                int childCount = parent.GetLayoutManager().ChildCount;
                for (int i = 0; i < childCount; i++)
                {
                    View child = parent.GetLayoutManager().GetChildAt(i);
                    if (child.TranslationY < 0)
                    {
                        // view is coming down
                        lastViewComingDown = child;
                    }
                    else if (child.TranslationY > 0)
                    {
                        // view is coming up
                        if (firstViewComingUp == null)
                        {
                            firstViewComingUp = child;
                        }
                    }
                }

                if (lastViewComingDown != null && firstViewComingUp != null)
                {
                    // views are coming down AND going up to fill the void
                    top = lastViewComingDown.Bottom + (int)lastViewComingDown.TranslationY;
                    bottom = firstViewComingUp.Top + (int)firstViewComingUp.TranslationY;
                }
                else if (lastViewComingDown != null)
                {
                    // views are going down to fill the void
                    top = lastViewComingDown.Bottom + (int)lastViewComingDown.TranslationY;
                    bottom = lastViewComingDown.Bottom;
                }
                else if (firstViewComingUp != null)
                {
                    // views are coming up to fill the void
                    top = firstViewComingUp.Top;
                    bottom = firstViewComingUp.Top + (int)firstViewComingUp.TranslationY;
                }

                background.SetBounds(left, top, right, bottom);
                background.Draw(canvas);
            }

            int dividerLeft = parent.PaddingLeft;
            int dividerRight = parent.Width - parent.PaddingRight;

            int childCount1 = parent.ChildCount;
            for (int i = 0; i < childCount1; i++)
            {
                View child = parent.GetChildAt(i);

                RecyclerView.LayoutParams lParams = (RecyclerView.LayoutParams)child.LayoutParameters;

                int dividerTop = child.Bottom + lParams.BottomMargin;
                int dividerBottom = dividerTop + divider.IntrinsicHeight;

                divider.SetBounds(dividerLeft, dividerTop, dividerRight, dividerBottom);
                divider.Draw(canvas);
            }

            base.OnDraw(canvas, parent, state);
        }

        public override void GetItemOffsets(Rect outRect, View view, RecyclerView parent, RecyclerView.State state)
        {
            outRect.Bottom = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 1, context.Resources.DisplayMetrics);
        }
    }
}
