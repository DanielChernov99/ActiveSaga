using System;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveSaga.BossFight.Core
{
    public static class EventManager
    {
        private static readonly Dictionary<Type, Delegate> _handlers = new Dictionary<Type, Delegate>();

        public static void Subscribe<T>(Action<T> handler)
        {
            Type type = typeof(T);
            if (!_handlers.ContainsKey(type))
            {
                _handlers[type] = handler;
            }
            else
            {
                _handlers[type] = Delegate.Combine(_handlers[type], handler);
            }
        }

        public static void Unsubscribe<T>(Action<T> handler)
        {
            Type type = typeof(T);
            if (_handlers.ContainsKey(type))
            {
                _handlers[type] = Delegate.Remove(_handlers[type], handler);
                if (_handlers[type] == null)
                {
                    _handlers.Remove(type);
                }
            }
        }

        public static void Trigger<T>(T eventData)
        {
            Type type = typeof(T);
            if (_handlers.TryGetValue(type, out Delegate handler))
            {
                (handler as Action<T>)?.Invoke(eventData);
            }
        }
    }

    // Event Definitions
    public struct HealthChangedEvent { public float current; public float max; public bool isPlayer; }
    public struct WaveStartedEvent { public int waveIndex; public string name; }
    public struct WaveCompletedEvent { public bool success; }
    public struct EnemySpawnedEvent { public GameObject enemy; }
    public struct EnemyDespawnedEvent { public GameObject enemy; public bool wasKilledByPlayer; }
    public struct ProjectileSpawnedEvent { public GameObject projectile; }
    public struct ProjectileDespawnedEvent { public GameObject projectile; public bool wasDodged; public bool wasHitPlayer; }
    public struct FeedbackEvent { public string message; public float duration; }
}

