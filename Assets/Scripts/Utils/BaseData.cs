using System;

namespace Utils
{
    [Serializable]
    public abstract class BaseData
    {
        public string id;
        public string name;
        

        public override string ToString()
        {
            return $"{GetType().Name}: id={id}, name={name}";
        }
    }
}