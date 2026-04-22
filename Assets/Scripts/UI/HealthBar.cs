using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameJam.UI
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image damageImage;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private float damageFadeSpeed = 2f;
        [SerializeField] private Gradient healthGradient;

        private float _currentHealth;
        private float _maxHealth;
        private float _damageAmount;

        public void Initialize(float maxHealth)
        {
            _maxHealth = maxHealth;
            _currentHealth = maxHealth;
            UpdateUI();
        }

        public void SetHealth(float health)
        {
            float previousHealth = _currentHealth;
            _currentHealth = Mathf.Clamp(health, 0, _maxHealth);

            if (health < previousHealth && damageImage != null)
            {
                _damageAmount = previousHealth - health;
            }

            UpdateUI();
        }

        public void SetMaxHealth(float maxHealth)
        {
            _maxHealth = maxHealth;
            _currentHealth = Mathf.Min(_currentHealth, _maxHealth);
            UpdateUI();
        }

        private void Update()
        {
            if (damageImage != null && _damageAmount > 0)
            {
                _damageAmount = Mathf.MoveTowards(_damageAmount, 0, damageFadeSpeed * Time.deltaTime);
                float targetFill = (_currentHealth + _damageAmount) / _maxHealth;
                damageImage.fillAmount = targetFill;
            }
        }

        private void UpdateUI()
        {
            float fillAmount = _maxHealth > 0 ? _currentHealth / _maxHealth : 0;

            if (fillImage != null)
            {
                fillImage.fillAmount = fillAmount;
                if (healthGradient != null)
                {
                    fillImage.color = healthGradient.Evaluate(fillAmount);
                }
            }

            if (healthText != null)
            {
                healthText.text = $"{Mathf.CeilToInt(_currentHealth)} / {Mathf.CeilToInt(_maxHealth)}";
            }
        }
    }
}
