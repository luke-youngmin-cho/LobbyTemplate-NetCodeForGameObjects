using System;
using System.Collections.Generic;
using UnityEngine;

namespace T.Infrastructure
{
    public class UpdateRunner : MonoBehaviour
    {
        class SubscriberData
        {
            public float period;
            public float nextCallTime;
            public float lastCallTime;
        }

        readonly Queue<Action> _pendingHandlers = new Queue<Action>();
        readonly HashSet<Action<float>> _subscribers = new HashSet<Action<float>>();
        readonly Dictionary<Action<float>, SubscriberData> _subscriberData = new Dictionary<Action<float>, SubscriberData>();

        public void OnDestroy()
        {
            _pendingHandlers.Clear();
            _subscribers.Clear();
            _subscriberData.Clear();
        }

        public void Subscribe(Action<float> onUpdate, float updatePeriod)
        {
            if (onUpdate == null)
            {
                return;
            }

            if (onUpdate.Target == null) // Detect a local function that cannot be Unsubscribed since it could go out of scope.
            {
                Debug.LogError("Can't subscribe to a local function that can go out of scope and can't be unsubscribed from");
                return;
            }

            if (onUpdate.Method.ToString().Contains("<")) // Detect
            {
                Debug.LogError("Can't subscribe with an anonymous function that cannot be Unsubscribed, by checking for a character that can't exist in a declared method name.");
                return;
            }

            if (!_subscribers.Contains(onUpdate))
            {
                _pendingHandlers.Enqueue(() =>
                {
                    if (_subscribers.Add(onUpdate))
                    {
                        _subscriberData.Add(onUpdate, new SubscriberData() { period = updatePeriod, nextCallTime = 0, lastCallTime = Time.time });
                    }
                });
            }
        }

        public void Unsubscribe(Action<float> onUpdate)
        {
            _pendingHandlers.Enqueue(() =>
            {
                _subscribers.Remove(onUpdate);
                _subscriberData.Remove(onUpdate);
            });
        }

        void Update()
        {
            while (_pendingHandlers.Count > 0)
            {
                _pendingHandlers.Dequeue()?.Invoke();
            }

            foreach (var subscriber in _subscribers)
            {
                var subscriberData = _subscriberData[subscriber];

                if (Time.time >= subscriberData.nextCallTime)
                {
                    subscriber.Invoke(Time.time - subscriberData.lastCallTime);
                    subscriberData.lastCallTime = Time.time;
                    subscriberData.nextCallTime = Time.time + subscriberData.period;
                }
            }
        }
    }
}