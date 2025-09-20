using UnityEngine;

namespace AsakiFramework.ObjectPool
{
    /// <summary>
    /// 对象池使用示例
    /// </summary>
    public class ObjectPoolExample : MonoBehaviour
    {
        [Header("示例配置")]
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float spawnInterval = 1f;

        private float _lastSpawnTime;

        private void Start()
        {
            // 创建子弹对象池
            if (bulletPrefab != null)
            {
                ObjectPool.Create(bulletPrefab, 20, 50, "BulletPool");
                Debug.Log("子弹对象池已创建");
            }

            // 创建敌人对象池
            if (enemyPrefab != null)
            {
                ObjectPool.Create(enemyPrefab, 10, 30, "EnemyPool");
                Debug.Log("敌人对象池已创建");
            }
        }

        private void Update()
        {
            // 示例：定时生成对象
            if (Time.time - _lastSpawnTime >= spawnInterval)
            {
                SpawnBullet();
                SpawnEnemy();
                _lastSpawnTime = Time.time;
            }

            // 示例：按键测试
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SpawnBullet();
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                SpawnEnemy();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                ReturnAllObjects();
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                ClearAllPools();
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                ShowPoolStats();
            }
        }

        /// <summary>
        /// 生成子弹
        /// </summary>
        private void SpawnBullet()
        {
            if (bulletPrefab == null) return;

            // 从池中获取子弹
            var bullet = ObjectPool.Get(bulletPrefab, spawnPoint.position, spawnPoint.rotation);
            if (bullet != null)
            {
                // 设置子弹行为（这里只是示例）
                var bulletScript = bullet.GetComponent<Bullet>();
                if (bulletScript != null)
                {
                    bulletScript.Initialize(Vector3.forward * 10f);
                }
                else
                {
                    // 如果没有脚本，简单移动
                    var rb = bullet.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.linearVelocity = Vector3.forward * 10f;
                    }
                }

                // 2秒后自动归还
                Invoke("ReturnBullet", 2f);
            }
        }

        /// <summary>
        /// 生成敌人
        /// </summary>
        private void SpawnEnemy()
        {
            if (enemyPrefab == null) return;

            // 随机位置
            var randomPosition = spawnPoint.position + new Vector3(
                Random.Range(-5f, 5f),
                0,
                Random.Range(-5f, 5f)
            );

            // 从池中获取敌人
            var enemy = ObjectPool.Get(enemyPrefab, randomPosition, Quaternion.identity);
            if (enemy != null)
            {
                // 设置敌人行为（这里只是示例）
                var enemyScript = enemy.GetComponent<Enemy>();
                if (enemyScript != null)
                {
                    enemyScript.Initialize(Random.Range(50f, 100f));
                }

                Debug.Log($"生成敌人: {enemy.name}");
            }
        }

        /// <summary>
        /// 归还子弹
        /// </summary>
        private void ReturnBullet()
        {
            // 这里应该找到特定的子弹对象并归还
            // 为了示例简单，我们直接显示统计信息
            ShowPoolStats();
        }

        /// <summary>
        /// 归还所有对象
        /// </summary>
        private void ReturnAllObjects()
        {
            ObjectPool.ReturnAll();
            Debug.Log("所有对象已归还到池中");
        }

        /// <summary>
        /// 清空所有池
        /// </summary>
        private void ClearAllPools()
        {
            ObjectPool.ClearAll();
            Debug.Log("所有对象池已清空");
        }

        /// <summary>
        /// 显示池统计信息
        /// </summary>
        private void ShowPoolStats()
        {
            var stats = ObjectPool.GetStats();
            Debug.Log($"对象池统计:\n{stats}");
        }

        private void OnDestroy()
        {
            // 场景销毁时清理
            if (Application.isPlaying)
            {
                ObjectPool.ClearAll();
            }
        }
    }

    /// <summary>
    /// 子弹示例组件
    /// </summary>
    public class Bullet : MonoBehaviour, IPoolable
    {
        private Vector3 _velocity;
        private Rigidbody _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        public void Initialize(Vector3 velocity)
        {
            _velocity = velocity;
            if (_rb != null)
            {
                _rb.linearVelocity = velocity;
            }
        }

        private void Update()
        {
            // 简单的生命周期管理
            if (transform.position.magnitude > 100f)
            {
                // 超出范围，归还到池
                ObjectPool.Return(gameObject);
            }
        }

        // IPoolable接口实现
        public void OnGetFromPool()
        {
            Debug.Log($"子弹 {name} 从池中获取");
        }

        public void OnReturnToPool()
        {
            Debug.Log($"子弹 {name} 归还到池");
            if (_rb != null)
            {
                _rb.linearVelocity = Vector3.zero;
            }
        }

        public void OnDestroyFromPool()
        {
            Debug.Log($"子弹 {name} 从池中销毁");
        }
    }

    /// <summary>
    /// 敌人示例组件
    /// </summary>
    public class Enemy : MonoBehaviour, IPoolable
    {
        private float _health;

        public void Initialize(float health)
        {
            _health = health;
            Debug.Log($"敌人初始化 - 生命值: {health}");
        }

        public void TakeDamage(float damage)
        {
            _health -= damage;
            if (_health <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            Debug.Log($"敌人死亡: {name}");
            // 归还到池
            ObjectPool.Return(gameObject);
        }

        // IPoolable接口实现
        public void OnGetFromPool()
        {
            Debug.Log($"敌人 {name} 从池中获取");
            gameObject.SetActive(true);
        }

        public void OnReturnToPool()
        {
            Debug.Log($"敌人 {name} 归还到池");
            gameObject.SetActive(false);
            _health = 0;
        }

        public void OnDestroyFromPool()
        {
            Debug.Log($"敌人 {name} 从池中销毁");
        }
    }
}