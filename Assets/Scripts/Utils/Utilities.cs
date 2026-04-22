using UnityEngine;

namespace GameJam.Utils
{
    public static class Extensions
    {
        public static Vector3 WithX(this Vector3 v, float x) => new Vector3(x, v.y, v.z);
        public static Vector3 WithY(this Vector3 v, float y) => new Vector3(v.x, y, v.z);
        public static Vector3 WithZ(this Vector3 v, float z) => new Vector3(v.x, v.y, z);
        public static Vector3 Flat(this Vector3 v) => new Vector3(v.x, 0, v.z);
        public static Vector2 ToXZ(this Vector3 v) => new Vector2(v.x, v.z);
        public static Vector3 FromXZ(this Vector2 v, float y = 0) => new Vector3(v.x, y, v.y);

        public static float Remap(this float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            return toMin + (value - fromMin) * (toMax - toMin) / (fromMax - fromMin);
        }

        public static bool IsInLayerMask(this GameObject obj, LayerMask mask)
        {
            return ((1 << obj.layer) & mask) != 0;
        }

        public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
        {
            T component = obj.GetComponent<T>();
            if (component == null)
            {
                component = obj.AddComponent<T>();
            }
            return component;
        }

        public static void SetLayerRecursively(this GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform child in obj.transform)
            {
                child.gameObject.SetLayerRecursively(layer);
            }
        }
    }

    public static class MathUtils
    {
        public static float SmoothStep(float from, float to, float t)
        {
            t = Mathf.Clamp01(t);
            t = t * t * (3f - 2f * t);
            return from + (to - from) * t;
        }

        public static float EaseInOut(float t)
        {
            return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
        }

        public static float EaseOutBounce(float t)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;

            if (t < 1f / d1) return n1 * t * t;
            if (t < 2f / d1) return n1 * (t -= 1.5f / d1) * t + 0.75f;
            if (t < 2.5f / d1) return n1 * (t -= 2.25f / d1) * t + 0.9375f;
            return n1 * (t -= 2.625f / d1) * t + 0.984375f;
        }

        public static Vector3 RandomPointInSphere(Vector3 center, float radius)
        {
            return center + Random.insideUnitSphere * radius;
        }

        public static Vector3 RandomPointInCircle(Vector3 center, float radius)
        {
            Vector2 point = Random.insideUnitCircle * radius;
            return new Vector3(center.x + point.x, center.y, center.z + point.y);
        }

        public static bool RandomChance(float probability)
        {
            return Random.value <= probability;
        }
    }

    public class Timer
    {
        private float _duration;
        private float _elapsed;
        private bool _isRunning;
        private bool _isLooping;
        private System.Action _onComplete;

        public float Duration => _duration;
        public float Elapsed => _elapsed;
        public float Remaining => Mathf.Max(0, _duration - _elapsed);
        public float Progress => _duration > 0 ? _elapsed / _duration : 0;
        public bool IsRunning => _isRunning;
        public bool IsComplete => _elapsed >= _duration;

        public Timer(float duration, System.Action onComplete = null, bool autoStart = false, bool loop = false)
        {
            _duration = duration;
            _onComplete = onComplete;
            _isLooping = loop;
            if (autoStart) Start();
        }

        public void Start()
        {
            _isRunning = true;
            _elapsed = 0f;
        }

        public void Stop()
        {
            _isRunning = false;
        }

        public void Reset()
        {
            _elapsed = 0f;
        }

        public void Restart()
        {
            Reset();
            Start();
        }

        public bool Update(float deltaTime)
        {
            if (!_isRunning) return false;

            _elapsed += deltaTime;
            if (_elapsed >= _duration)
            {
                _onComplete?.Invoke();

                if (_isLooping)
                {
                    _elapsed -= _duration;
                }
                else
                {
                    _isRunning = false;
                }
                return true;
            }
            return false;
        }

        public void SetDuration(float duration)
        {
            _duration = duration;
        }
    }
}
