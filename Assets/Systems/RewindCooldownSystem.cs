//-----------------------------------------------------------------------
// REWIND COMPANY — Rewind Cooldown System
// Prevents infinite-rewind abuse by enforcing a hard cooldown.
// After a rewind, the player must wait before rewinding again.
// Design: a simple grace window so struggling players can rewind once more.
//-----------------------------------------------------------------------

using UnityEngine;

namespace RewindCompany.Systems
{
    public class RewindCooldownSystem : MonoBehaviour
    {
        public static RewindCooldownSystem Instance { get; private set; }

        [Header("Config")]
        public float cooldownDuration = 8f;   // seconds before next rewind
        public float windUpDuration = 0.25f;  // visual charge-up in seconds
        public float minimumRewind = 2f;      // minimum time between rewinds

        public float currentCooldown;
        public bool isOnCooldown;
        public bool isCharging;

        [Header("References")]
        public RewindSystem rewind;
        public RewindUI ui;

        public float cooldownProgress;
        public float windupProgress;
        public bool readyToRewind;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnEnable() { Initialize(); }

        void Initialize()
        {
            currentCooldown = 0f;
            cooldownProgress = 0f;
            windupProgress = 0f;
            isOnCooldown = false;
            isCharging = false;
            readyToRewind = false;
        }

        void Update()
        {
            if (rewind == null || rewind.isRewinding) return;

            if (isOnCooldown)
            {
                cooldownProgress += Time.deltaTime;
                if (!isCharging && cooldownProgress > 0f)
                {
                    // After cooldown ends, begin charging up for the next rewind.
                    isCharging = true;
                }
                if (isCharging)
                {
                    windupProgress += Time.deltaTime;
                    if (windupProgress >= windUpDuration)
                    {
                        isOnCooldown = false;
                        isCharging = false;
                        windupProgress = 0f;
                        ui.SetCharged(true);
                    }
                }
            }
            else
            {
                windupProgress += Time.deltaTime;
                if (windupProgress >= windUpDuration)
                {
                    isCharging = true;
                    ui.SetCharged(true);
                }
            }
        }

        // Called by rewind UI / input when the player tries to rewind.
        public bool TryStartRewind()
        {
            if (rewind == null) return false;
            if (rewind.isRewinding) return false;

            if (isOnCooldown)
            {
                // After cooldown ends, allow rewind immediately (no windup) to keep it fair.
                isOnCooldown = false;
                windupProgress = 0f;
                isCharging = false;
                cooldownProgress = 0f;
                ui.SetCharged(true);
                return true;
            }

            // Not on cooldown: allow, then start cooldown.
            rewind.StartRewind();
            isOnCooldown = true;
            cooldownProgress = 0f;
            windupProgress = 0f;
            isCharging = false;
            ui.SetCharged(false);
            return true;
        }

        public void RevokeCharge() { isCharging = false; }
        public bool OnCooldown() => isOnCooldown;
        public bool Ready() => !isOnCooldown && !isCharging;
    }
}
