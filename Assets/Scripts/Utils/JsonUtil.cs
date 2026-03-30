using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Utils
{
    public static class JsonFileName
    {
        public const string PlayerData = "PlayerData";
    }

    public class JsonUtil : SingletonBase<JsonUtil>
    {
        public static async Task<T> LoadDataAsync<T>(string filePath) where T : new()
        {
            if (!File.Exists(filePath))
            {
                return new T();
            }

            try
            {
                var jsonFile = await File.ReadAllTextAsync(filePath);
                if (string.IsNullOrWhiteSpace(jsonFile))
                {
                    return new T();
                }

                var data = JsonConvert.DeserializeObject<T>(jsonFile);
                return data ?? new T();
            }
            catch (Exception ex)
            {
                Debug.LogError($"鍔犺浇鏂囦欢澶辫触: {ex.Message}");
                return default;
            }
        }

        public static async Task SaveDataAsync<T>(T data, string filePath)
        {
            try
            {
                var json = await Task.Run(() => JsonConvert.SerializeObject(data, Formatting.None));
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"淇濆瓨鏁版嵁澶辫触: {ex.Message}");
            }
        }

        public static void DeleteData(string fileName)
        {
            var path = Path.Combine(Application.persistentDataPath, fileName);
            if (!File.Exists(path))
            {
                return;
            }

            try
            {
                File.Delete(path);
            }
            catch (Exception ex)
            {
                Debug.LogError($"鍒犻櫎鏁版嵁澶辫触: {ex.Message}");
            }
        }
    }
}
