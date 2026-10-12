using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public int maxHealth = 90;
    public Slider healthBar;

    [SerializeField]
    private int currentHealth;

    private HitFlash hitFlash;

    private void Awake()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = maxHealth;

        // 같은 오브젝트에 붙은 피격 효과를 찾습니다.
        hitFlash = GetComponent<HitFlash>();
    }

    private void Start()
    {
        UpdateHealthBar();
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0 || damage <= 0)
            return;

        currentHealth = Mathf.Max(0, currentHealth - damage);

        UpdateHealthBar();

        Debug.Log(
            $"{name} 체력: {currentHealth} / {maxHealth}",
            this
        );

        if (currentHealth == 0)
        {
            Die();
            return;
        }

        // 피해를 받고 살아 있다면 피격 효과를 실행합니다.
        if (hitFlash != null)
            hitFlash.PlayFlash();
    }

    private void UpdateHealthBar()
    {
        if (healthBar == null)
            return;

        healthBar.minValue = 0;
        healthBar.maxValue = maxHealth;
        healthBar.wholeNumbers = true;
        healthBar.value = currentHealth;
    }

    private void Die()
    {
        Debug.Log($"{name} 사망", this);
        Destroy(gameObject);
    }
}