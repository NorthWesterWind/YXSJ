using System.Collections.Generic;

namespace Utils
{
    [System.Serializable]
    public class SerializableIntDictionary<T>
    {
        public List<IntKeyValue<T>> list = new();

        public T Get(int key)
        {
            foreach (var kv in list)
                if (kv.key == key)
                    return kv.value;
            return default;
        }
       


        public void Set(int key, T value)
        {
            foreach (var kv in list)
            {
                if (kv.key == key)
                {
                    kv.value = value;
                    return;
                }
            }
            list.Add(new IntKeyValue<T>(){ key = key, value = value });
        }

        public bool ContainsKey(int key)
        {
            foreach (var kv in list)
                if (kv.key == key)
                    return true;
            return false;
        }
    }
    [System.Serializable]
    public class IntKeyValue<T>
    {
        public int key;
        public T value;
    }
}