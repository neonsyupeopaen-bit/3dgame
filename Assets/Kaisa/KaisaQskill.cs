using UnityEngine;
using UnityEngine.InputSystem;

public class KaisaQskill : MonoBehaviour
{
    public KaisaProjectile projectilePrefab;

    public LayerMask enemyLayer;

    public int projectileAmount = 15;
    public float range = 5;

    // Update is called once per frame
    void Update()
    {
        if(Keyboard.current.qKey.wasPressedThisFrame)
        {
            Debug.Log("스킬 사용");
            UseSkill();
        }
    }

    void UseSkill()
    {
        if (projectilePrefab == null) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, range, enemyLayer);

        Debug.Log($"탐색된 적{hits.Length}");

        foreach (Collider hit in hits)
        {
            Debug.Log($"{hit.gameObject.name}");
            Health enemy = hit.gameObject.GetComponent<Health>();
            if (enemy != null)
            {
                Debug.Log($"적 발견");

                for (int i = 0; i < projectileAmount; i++ )
                {
                    KaisaProjectile p = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                    p.UseSkill(enemy);
                }
            }
        }
    }
}
