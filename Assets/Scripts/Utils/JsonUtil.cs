using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;

namespace Utils
{
    public static class JsonFileName
    {
        public const string PlayerData = "PlayerData";
        public const string PlayerChapterData = "PlayerChapterData";
        public const string PlayerTeamData = "PlayerTeamData";
        public const string DailyRewardClaimData = "DailyRewardClaimData";

    }

    public class JsonUtil : SingletonBase<JsonUtil>
    {
        // 异步加载 JSON
        public static async Task<T> LoadDataAsync<T>(string filePath) where T : new()
        {
           
            if(!File.Exists(filePath))
                return new T();
            try
            {
                Debug.Log($"正在读取文件：{filePath}");
                var jsonFile = await File.ReadAllTextAsync(filePath);
                if (string.IsNullOrWhiteSpace(jsonFile))
                {
                    Debug.LogError($"文件内容为空：{filePath}");
                    return new T();
                }
                Debug.Log("读取到的JSON文本：" + jsonFile);
                var data = JsonConvert.DeserializeObject<T>(jsonFile);
                if (data == null)
                {
                    Debug.LogError($"反序列化返回 null：{filePath}\n内容：{jsonFile}");
                    return new T();
                }

                Debug.Log($"成功反序列化：{typeof(T)}");
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError($"加载文件失败: {ex.Message}");
                return default;
            }
        }
        
        public static void DeleteData(string fileName)
        {
            var path = Path.Combine(Application.persistentDataPath, fileName);
            if (File.Exists(path))
            {
                try
                {
                    File.Delete(path);
                    Debug.Log($"数据文件已删除: {path}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"删除数据失败: {ex.Message}");
                }
            }
            else
            {
                Debug.Log($"没有找到要删除的数据文件: {path}");
            }
        }

    }
}