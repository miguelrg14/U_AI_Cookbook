// Miguel Rodríguez Gallego
using UnityEngine;
using UnityEngine.UI;

namespace AICookbook.Shared
{
    /// <summary>
    ///     Stamina of an agent. Actions spend it while moving and recover it while resting;
    ///     once it reaches zero the agent must rest until it is full again.
    /// </summary>
    public class EntityStats : MonoBehaviour
    {
        [Header("Stamina")]
        public float stamina;
        public float maxStamina;
        [SerializeField] float staminaWaste = 15f;
        [SerializeField] float staminaRegen = 10f;
        public bool canRegenStamina;

        [SerializeField] Slider staminaProgressBar;

        protected virtual void Start()
        {
            UpdateStaminaSlider();
            CheckStaminaState();
        }

        protected virtual void Update() => CheckStaminaState();

        /// <summary>
        ///     Empty stamina forces a full regeneration before the agent can act again
        /// </summary>
        void CheckStaminaState()
        {
            if (stamina <= 0)
                canRegenStamina = true;

            if (stamina >= maxStamina)
                canRegenStamina = false;
        }

        /// <summary>
        ///     Lowers stamina with time
        /// </summary>
        public void UseStamina()
        {
            stamina -= staminaWaste * Time.deltaTime;
            UpdateStaminaSlider();
        }

        /// <summary>
        ///     Regens stamina with time
        /// </summary>
        public void RegenStamina()
        {
            stamina += staminaRegen * Time.deltaTime;
            UpdateStaminaSlider();
        }

        void UpdateStaminaSlider()
        {
            if (staminaProgressBar != null && maxStamina > 0)
                staminaProgressBar.value = stamina / maxStamina;
        }

        public float GetStamina() => stamina;
        public bool GetCanRegenStamina() => canRegenStamina;
    }
}
