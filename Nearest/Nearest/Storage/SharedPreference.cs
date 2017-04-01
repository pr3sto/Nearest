using System.Linq;
using System.Collections.Generic;
using Android.Content;
using Newtonsoft.Json;

namespace Nearest.Storage
{ 
    public interface Storable
    {
        int Id { get; set; }
    }

    public class SharedPreference<T> where T : Storable
    {
        private const string PREFS_NAME = "Nearest";

        private Context context;
        private string preferencesKey;

        public SharedPreference(Context context, string preferencesKey)
        {
            this.context = context;
            this.preferencesKey = preferencesKey;
        }
        
        public void SaveItems(List<T> items)
        {
            var preferences = context.GetSharedPreferences(PREFS_NAME, FileCreationMode.Private);
            var editor = preferences.Edit();

            string jsonItems = JsonConvert.SerializeObject(items);

            editor.PutString(preferencesKey, jsonItems);
            editor.Commit();
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
                var r = items.Single(x => x.Id == item.Id);
                items.Remove(r);
                SaveItems(items);
            }
        }

        public List<T> GetItems()
        {
            var preferences = context.GetSharedPreferences(PREFS_NAME, FileCreationMode.Private);

            if (preferences.Contains(preferencesKey))
            {
                string jsonItems = preferences.GetString(preferencesKey, null);
                return JsonConvert.DeserializeObject<List<T>>(jsonItems);
            }
            else
                return new List<T>();
        }
    }
}
