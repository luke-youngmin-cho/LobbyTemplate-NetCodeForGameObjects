﻿using UnityEngine;

namespace T.UnityServices
{
    public class RateLimitCooldown
    {
        public float CooldownTimeLength => _cooldownTimeLength;

        readonly float _cooldownTimeLength;
        private float _cooldownFinishedTime;

        public RateLimitCooldown(float cooldownTimeLength)
        {
            this._cooldownTimeLength = cooldownTimeLength;
            _cooldownFinishedTime = -1f;
        }

        public bool CanCall => Time.unscaledTime > _cooldownFinishedTime;

        public void PutOnCooldown()
        {
            _cooldownFinishedTime = Time.unscaledTime + _cooldownTimeLength;
        }
    }
}