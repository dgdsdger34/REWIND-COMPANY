//-----------------------------------------------------------------------
// REWIND COMPANY — Rewind UI
// Simple cooldown + rewind availability display in world space.
//-----------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;

namespace RewindCompany.Systems
{
    public class RewindUI : MonoBehaviour
    {
        public static RewindUI Instance { get; private set; }

        [Header("Visuals")]
        public Image cooldownBar;
        public Image readyIcon;
        public Text statusText;
        public Image chargeIndicator;

        [Header("Config")]
        public float barFillRate = 1f;  // bar fills over time during charge
        public float cooldownDuration = 8f;

        public float cooldownTimer;
        public float chargeTime;
        public bool canRewind;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnEnable() { Initialize(); }

        void Initialize()
        {
            cooldownTimer = 0f;
            chargeTime = 0f;
            canRewind = false;
            readyIcon.color = Color.clear;
            chargeIndicator.color = Color.clear;
            if (cooldownBar != null) cooldownBar.color = Color.clear;
        }

        void Update()
        {
            if (RewindCooldownSystem.Instance == null) return;

            if (RewindCooldownSystem.Instance.isOnCooldown)
            {
                cooldownTimer = RewindCooldownSystem.Instance.cooldownProgress;
                chargeTime = RewindCooldownSystem.Instance.windupProgress;

                if (cooldownBar != null)
                {
                    cooldownBar.color = Color.Lerp(Color.clear, Color.green, cooldownTimer / RewindCooldownSystem.Instance.cooldownDuration);
                    cooldownBar.fillAmount = 1f - cooldownTimer / RewindCooldownSystem.Instance.cooldownDuration;
                }
                if (statusText != null)
                    statusText.text = "Rewind on cooldown: " + (RewindCooldownSystem.Instance.cooldownDuration - cooldownTimer).ToString("F1") + "s";

                if (chargeTime > 0f && chargeTime < RewindCooldownSystem.Instance.windUpDuration)
                {
                    chargeIndicator.color = Color.Lerp(Color.clear, Color.yellow, chargeTime / RewindCooldownSystem.Instance.windUpDuration);
                }
                else if (chargeTime >= RewindCooldownSystem.Instance.windUpDuration)
                {
                    chargeIndicator.color = Color.green;
                    statusText.text = "Ready to rewind!";
                }
            }
            else
            {
                cooldownBar.color = Color.clear;
                if (statusText != null) statusText.text = "Press R to rewind";
                canRewind = false;
            }

            // Update rewind availability (availability managed by rewind system).
            if (RewindSystem.Instance != null)
            {
                canRewind = RewindSystem.Instance.hasUndo;
            }
        }

        public void SetRewindAvailable(bool available)
        {
            if (statusText != null)
                statusText.text = available ? "Rewind available (R)" : "No rewind in window";
        }

        public void SetCharged(bool charged)
        {
            readyIcon.color = charged ? Color.green : Color.clear;
            chargeIndicator.color = charged ? Color.green : Color.clear;
        }
    }
}
