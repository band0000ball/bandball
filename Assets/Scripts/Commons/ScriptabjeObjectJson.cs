using System;
using UnityEngine;

namespace Commons
{
    public class ScriptableObjectJson
    {
        public static string GetFullPath(string _dataPath)
        {
#if !UNITY_EDITOR
            string appPath = Application.persistentDataPath;
#else
            string appPath = Application.dataPath;
#endif
            return $"{appPath}/{_dataPath}";
        }

        public static bool SaveToJSON(string _dataPath, ScriptableObject _dataSO)
        {
            bool result = false;
            string fullPath = GetFullPath(_dataPath);
            // ファイルにデータを書き込む
            string jsonStr = JsonUtility.ToJson(_dataSO, true);
            Debug.Log("SaveSettings: " + fullPath);
            try
            {
                System.IO.File.WriteAllText(fullPath, jsonStr);
                result = true;
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to save settings data: " + e.Message);
            }
            return result;
        }
    }


}