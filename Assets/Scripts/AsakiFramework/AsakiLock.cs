using System;
using System.Threading;

namespace AsakiFramework
{
    /// <summary>
    /// 轻量级线程锁封装，支持超时、静态锁、对象锁
    /// </summary>
    public static class AsakiLock
    {
        /// <summary>
        /// 静态锁对象，用于全局单例组件保护
        /// </summary>
        public static readonly object StaticLock = new object();

        /// <summary>
        /// 线程安全地执行某个操作（静态锁）
        /// </summary>
        public static void LockStatic(Action action, int timeoutMillis = -1)
        {
            bool lockTaken = false;
            try
            {
                if (timeoutMillis < 0)
                    Monitor.Enter(StaticLock, ref lockTaken);
                else
                    lockTaken = Monitor.TryEnter(StaticLock, timeoutMillis);

                if (!lockTaken)
                    throw new TimeoutException("静态锁等待超时");

                action?.Invoke();
            }
            finally
            {
                if (lockTaken) Monitor.Exit(StaticLock);
            }
        }

        /// <summary>
        /// 线程安全地执行某个操作（对象锁）
        /// </summary>
        public static void Lock(object lockObj, Action action, int timeoutMillis = -1)
        {
            bool lockTaken = false;
            try
            {
                if (timeoutMillis < 0)
                    Monitor.Enter(lockObj, ref lockTaken);
                else
                    lockTaken = Monitor.TryEnter(lockObj, timeoutMillis);

                if (!lockTaken)
                    throw new TimeoutException("对象锁等待超时");

                action?.Invoke();
            }
            finally
            {
                if (lockTaken) Monitor.Exit(lockObj);
            }
        }
    }
}
