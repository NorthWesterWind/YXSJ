namespace Utils
{
    public abstract class SingletonBase<T> where T : new()
    {
        private static readonly object _lock = new object();
        private static T _instance;
        
        // 私有构造函数（防止外部实例化）
        protected SingletonBase() { }
        
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new T();
                        }
                    }
                }
                return _instance;
            }
        }
    }
}