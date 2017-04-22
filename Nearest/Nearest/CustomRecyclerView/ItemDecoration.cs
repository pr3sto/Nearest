using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;

namespace Nearest.CustomRecyclerView
{
    class ItemDecoration : RecyclerView.ItemDecoration
    {
        private Context context;
        private Drawable background;
        private bool initiated;

        public ItemDecoration(Context context)
        {
            this.context = context;
        }

        private void Init()
        {
            background = new ColorDrawable(new Color(ContextCompat.GetColor(context, Resource.Color.item_view_removed)));
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

            base.OnDraw(canvas, parent, state);
        }
    }
}
