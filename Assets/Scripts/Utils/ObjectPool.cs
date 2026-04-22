using UnityEngine;
using System.Collections.Generic;

namespace GameJam.Utils
{
    public class ObjectPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Queue<T> _pool = new();
        private readonly List<T> _activeObjects = new();
        private readonly int _initialSize;
        private readonly bool _autoExpand;

        public int ActiveCount => _activeObjects.Count;
        public int PoolCount => _pool.Count;

        public ObjectPool(T prefab, int initialSize, Transform parent = null, bool autoExpand = true)
        {
            _prefab = prefab;
            _initialSize = initialSize;
            _parent = parent;
            _autoExpand = autoExpand;

            for (int i = 0; i < initialSize; i++)
            {
                CreateObject();
            }
        }

        private T CreateObject()
        {
            T obj = Object.Instantiate(_prefab, _parent);
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
            return obj;
        }

        public T Get()
        {
            T obj;

            if (_pool.Count > 0)
            {
                obj = _pool.Dequeue();
            }
            else if (_autoExpand)
            {
                obj = CreateObject();
                _pool.Dequeue();
            }
            else
            {
                return null;
            }

            obj.gameObject.SetActive(true);
            _activeObjects.Add(obj);
            return obj;
        }

        public T Get(Vector3 position, Quaternion rotation)
        {
            T obj = Get();
            if (obj != null)
            {
                obj.transform.position = position;
                obj.transform.rotation = rotation;
            }
            return obj;
        }

        public void Return(T obj)
        {
            if (obj == null) return;

            obj.gameObject.SetActive(false);
            _activeObjects.Remove(obj);
            _pool.Enqueue(obj);
        }

        public void ReturnAll()
        {
            for (int i = _activeObjects.Count - 1; i >= 0; i--)
            {
                Return(_activeObjects[i]);
            }
        }

        public void Clear()
        {
            ReturnAll();

            while (_pool.Count > 0)
            {
                T obj = _pool.Dequeue();
                if (obj != null)
                {
                    Object.Destroy(obj.gameObject);
                }
            }

            _activeObjects.Clear();
        }
    }

    public class PooledObject : MonoBehaviour
    {
        public System.Action<PooledObject> OnReturnToPool;

        public void ReturnToPool()
        {
            OnReturnToPool?.Invoke(this);
        }

        public void ReturnAfterDelay(float delay)
        {
            Invoke(nameof(ReturnToPool), delay);
        }

        private void OnDisable()
        {
            CancelInvoke();
        }
    }

    public class SimplePool : MonoBehaviour
    {
        [SerializeField] private PooledObject prefab;
        [SerializeField] private int initialSize = 10;
        [SerializeField] private bool autoExpand = true;

        private ObjectPool<PooledObject> _pool;

        private void Awake()
        {
            _pool = new ObjectPool<PooledObject>(prefab, initialSize, transform, autoExpand);
        }

        public PooledObject Spawn()
        {
            var obj = _pool.Get();
            if (obj != null)
            {
                obj.OnReturnToPool = Return;
            }
            return obj;
        }

        public PooledObject Spawn(Vector3 position, Quaternion rotation)
        {
            var obj = _pool.Get(position, rotation);
            if (obj != null)
            {
                obj.OnReturnToPool = Return;
            }
            return obj;
        }

        public void Return(PooledObject obj)
        {
            _pool.Return(obj);
        }

        public void ReturnAll()
        {
            _pool.ReturnAll();
        }
    }
}
