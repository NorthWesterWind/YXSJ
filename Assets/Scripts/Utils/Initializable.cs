namespace Utils
{
    public interface Initializable 
    {
        /// <summary>
        ///初始化执行（由管理器调用）
        ///</summary>
        void Initialize();
        /// <summary>
        ///初始化优先级（数字越小越先执行）
        /// </summary>
        int Priority { get;}
    }
}
