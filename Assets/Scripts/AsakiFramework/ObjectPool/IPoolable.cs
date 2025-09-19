namespace AsakiFramework.ObjectPool
{
    /// <summary>
    /// 可池化对象接口
    /// 实现此接口的对象可以在对象池中进行生命周期管理
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// 当对象从池中获取时调用
        /// </summary>
        void OnGetFromPool();

        /// <summary>
        /// 当对象归还到池中时调用
        /// </summary>
        void OnReturnToPool();

        /// <summary>
        /// 当对象从池中销毁时调用
        /// </summary>
        void OnDestroyFromPool();
    }
}
