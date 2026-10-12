using UnityEngine;

public class SimpleProjectile : MonoBehaviour
{
    public float speed = 12f;
    public float lifetime = 1f;

    public int damage = 30;
    public float hitRadius = 0.15f;
    public LayerMask hitMask;

    private bool hasHit;

    private void Start()
    {
        Destroy(gameObject, lifetime);

        // 생성된 순간 이미 적과 겹쳐 있는 경우를 검사합니다.
        Collider[] overlaps = Physics.OverlapSphere(
            transform.position,
            hitRadius,
            hitMask,
            QueryTriggerInteraction.Ignore
        );

        if (overlaps.Length > 0)
        {
            Hit(overlaps[0]);
        }
    }

    private void Update()
    {
        if (hasHit)
            return;

        float moveDistance = speed * Time.deltaTime;

        if (moveDistance <= 0f)
            return;

        // 이번 프레임에 이동할 구간에서 가장 먼저 맞는 대상을 찾습니다.
        if (Physics.SphereCast(
            transform.position,
            hitRadius,
            transform.forward,
            out RaycastHit hit,
            moveDistance,
            hitMask,
            QueryTriggerInteraction.Ignore))
        {
            transform.position += transform.forward * hit.distance;

            Hit(hit.collider);
            return;
        }

        transform.position += transform.forward * moveDistance;
    }

    private void Hit(Collider target)
    {
        if (hasHit)
            return;

        hasHit = true;

        // Body에 맞아도 부모 Enemy에 있는 Health를 찾습니다.
        Health health = target.GetComponentInParent<Health>();

        if (health != null)
        {
            health.TakeDamage(damage);
        }

        // 이 발사체는 한 번 맞으면 사라집니다.
        Destroy(gameObject);
    }
}