using System.Linq;
using System.Collections.Generic;
using Android.Content;
using Newtonsoft.Json;

namespace Nearest.Storage
{
    public class FavoritesStorage<T> where T : Storable
    {
        private const string STORAGE_NAME = "NEAREST";

        private Context context;
        private string storageKey;

        public FavoritesStorage(Context context, string storageKey)
        {
            this.context = context;
            this.storageKey = storageKey;
        }

        public int GetUniqueId()
        {
            List<int> ids = GetItems().Select(x => x.Id).ToList();
            ids.Sort();

            for (int i = 0; i < ids.Count; i++)
            {
                // free id
                if (ids[i] != i)
                    return i;
            }

            // new id
            return ids.Count;
        }

        public void AddItem(T item)
        {
            List<T> items = GetItems();
            items.Add(item);
            SaveItems(items);
        }

        public void RemoveItem(T item)
        {
            List<T> items = GetItems();
            if (items.Count > 0)
            {
                var i = items.Single(x => x.Id == item.Id);
                items.Remove(i);
                SaveItems(items);
            }
        }

        public List<T> GetItems()
        {
            var preferences = context.GetSharedPreferences(STORAGE_NAME, FileCreationMode.Private);

            if (preferences.Contains(storageKey))
            {
                string jsonItems = preferences.GetString(storageKey, null);
                return JsonConvert.DeserializeObject<List<T>>(jsonItems);
            }
            else
                return new List<T>();
        }

        public void SaveItems(List<T> items)
        {
            var preferences = context.GetSharedPreferences(STORAGE_NAME, FileCreationMode.Private);
            var editor = preferences.Edit();

            string jsonItems = JsonConvert.SerializeObject(items);

            editor.PutString(storageKey, jsonItems);
            editor.Commit();
        }
    }
}
