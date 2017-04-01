using System;
using System.Collections.Generic;
using Android.OS;
using Android.Views;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Graphics;
using Android.Support.V4.Content;
using Nearest.CustomRecyclerView.Models;

namespace Nearest.CustomRecyclerView
{
    class CustomAdapter : RecyclerView.Adapter
    {
        #region Listeners

        internal class ClickListener : Java.Lang.Object, View.IOnClickListener
        {
            public EventHandler Click;

            public void OnClick(View v)
            {
                Click?.Invoke(this, EventArgs.Empty);
            }
        }

        #endregion

        private const int PENDING_REMOVAL_TIMEOUT = 2000;

        public EventHandler<CustomItem> ItemRemoved;

        public List<CustomItem> Items { get; set; } = new List<CustomItem>();
        public override int ItemCount
        {
            get
            {
                return Items.Count;
            }
        }

        private Context context;
        private List<CustomItem> itemsPendingRemoval = new List<CustomItem>();
        private Handler removeHandler = new Handler();
        private Dictionary<int, Action> pendingRunnables = new Dictionary<int, Action>();

        public CustomAdapter(Context context)
        {
            this.context = context;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            return new CustomViewHolder(parent);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            CustomViewHolder viewHolder = (CustomViewHolder)holder;
            CustomItem item = Items[position];

            if (itemsPendingRemoval.Contains(item))
            {
                // "undo" state
                viewHolder.ItemView.SetBackgroundColor(new Color(ContextCompat.GetColor(context, Resource.Color.item_view_removed)));
                viewHolder.NameTextView.Visibility = ViewStates.Gone;
                viewHolder.DescriptionTextView.Visibility = ViewStates.Gone;
                viewHolder.UndoButton.Visibility = ViewStates.Visible;
                var undoListener = new ClickListener();
                undoListener.Click += (s, e) =>
                {
                    Action pendingRemovalAction = pendingRunnables[item.Id];
                    pendingRunnables.Remove(item.Id);

                    if (pendingRemovalAction != null)
                        removeHandler.RemoveCallbacks(pendingRemovalAction);

                    itemsPendingRemoval.Remove(item);
                    NotifyItemChanged(Items.IndexOf(item));
                };
                viewHolder.UndoButton.SetOnClickListener(undoListener);
            }
            else
            {
                // "normal" state
                viewHolder.ItemView.SetBackgroundColor(new Color(ContextCompat.GetColor(context, Resource.Color.item_view_normal)));
                viewHolder.NameTextView.Visibility = ViewStates.Visible;
                viewHolder.NameTextView.Text = item.Name;
                viewHolder.DescriptionTextView.Visibility = ViewStates.Visible;
                viewHolder.DescriptionTextView.Text = item.Description;
                viewHolder.UndoButton.Visibility = ViewStates.Gone;
                viewHolder.UndoButton.SetOnClickListener(null);
            }

        }

        public void PendingRemoval(int position)
        {
            CustomItem item = Items[position];
            if (!itemsPendingRemoval.Contains(item))
            {
                itemsPendingRemoval.Add(item);
                
                // redraw row in "undo" state
                NotifyItemChanged(position);

                Action pendingRemovalRunnable = new Action(() =>
                {
                    Remove(Items.IndexOf(item));
                    ItemRemoved?.Invoke(this, item);
                });
                removeHandler.PostDelayed(pendingRemovalRunnable, PENDING_REMOVAL_TIMEOUT);
                pendingRunnables.Add(item.Id, pendingRemovalRunnable);
            }
        }

        public bool IsPendingRemoval(int position)
        {
            CustomItem item = Items[position];
            return itemsPendingRemoval.Contains(item);
        }

        public void Remove(int position)
        {
            CustomItem item = Items[position];
            if (itemsPendingRemoval.Contains(item))
            {
                itemsPendingRemoval.Remove(item);
            }
            if (Items.Contains(item))
            {
                Items.RemoveAt(position);
                NotifyItemRemoved(position);
            }
        }
    }
}
