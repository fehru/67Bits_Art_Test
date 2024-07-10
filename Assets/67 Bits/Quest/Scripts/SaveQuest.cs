using Newtonsoft.Json;
using System;
using UnityEngine;

namespace SSBQuests
{
    public static class SaveQuest
    {
        #region Save and Load Custom JSON
        public static void SaveGameCustomJson(object saveData)
        {
            var className = saveData.GetType().Name;
            CustomSaveDataFile(saveData);
            Debug.Log($"{className} Data Saved");
        }
        public static T LoadCustomJson<T>(T loadData)
        {
            var className = loadData.GetType().Name;
            if (!PlayerPrefs.HasKey(className)) return default;

            var save = CustomLoadDataFile(loadData);
            if (save == null) return default;

            loadData = (T)save;
            return loadData;
        }
        #endregion
        public static bool CustomSaveDataFile<T>(T saveData)
        {
            try
            {
                var className = saveData.GetType().Name;
                string data = JsonConvert.SerializeObject(saveData);
                PlayerPrefs.SetString(className, data);
                return true;
            }
            catch (Exception ex) { Debug.LogException(ex); }
            return false;
        }
        public static object CustomLoadDataFile<T>(T loadData)
        {
            try
            {
                var className = loadData.GetType().Name;
                string dataJson = PlayerPrefs.GetString(className, "");
                object data = JsonConvert.DeserializeObject<T>(dataJson);
                Debug.Log($"{className} Data Loaded");
                return data;
            }
            catch (Exception ex) { Debug.LogException(ex); return false; }
        }
    }
}