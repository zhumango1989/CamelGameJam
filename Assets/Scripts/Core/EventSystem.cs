using System;
using System.Collections.Generic;

namespace GameJam.Core
{
    public static class EventSystem
    {
        private static readonly Dictionary<string, Action<object>> _events = new();

        public static void Subscribe(string eventName, Action<object> callback)
        {
            if (!_events.ContainsKey(eventName))
            {
                _events[eventName] = null;
            }
            _events[eventName] += callback;
        }

        public static void Unsubscribe(string eventName, Action<object> callback)
        {
            if (_events.ContainsKey(eventName))
            {
                _events[eventName] -= callback;
            }
        }

        public static void Emit(string eventName, object data = null)
        {
            if (_events.TryGetValue(eventName, out var callback))
            {
                callback?.Invoke(data);
            }
        }

        public static void Clear()
        {
            _events.Clear();
        }

        public static void Clear(string eventName)
        {
            if (_events.ContainsKey(eventName))
            {
                _events[eventName] = null;
            }
        }
    }

    public static class GameEvents
    {
        public const string GameStart = "GameStart";
        public const string GamePause = "GamePause";
        public const string GameResume = "GameResume";
        public const string GameOver = "GameOver";
        public const string LevelComplete = "LevelComplete";
        public const string PlayerDeath = "PlayerDeath";
        public const string PlayerRespawn = "PlayerRespawn";
        public const string ScoreChanged = "ScoreChanged";
        public const string HealthChanged = "HealthChanged";
        public const string ItemCollected = "ItemCollected";
        public const string EnemyKilled = "EnemyKilled";
    }
}
