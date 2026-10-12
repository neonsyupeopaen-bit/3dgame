using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NavMeshAgent))]
public class MobaPlayer : MonoBehaviour
{
    [Header("기본 설정")]
    public Camera viewCamera;
    public LayerMask groundMask;
    public LayerMask enemyMask;

    [Header("평타 설정")]
    [Min(0.1f)] public float attackRange = 3f;
    [Min(0)] public int attackDamage = 15;
    [Min(0.1f)] public float attacksPerSecond = 1f;

    [Header("Q 스킬")]
    public GameObject qProjectilePrefab;
    public float qCooldown = 1f;

    public float AttackRange => Mathf.Max(0.1f, attackRange);
    public bool ShowAttackRange { get; private set; }

    private NavMeshAgent agent;
    private Health attackTarget;

    private bool hasAttackOrder;
    private float moveStoppingDistance;
    private float nextAttackTime;
    private float nextQTime;
    private float nextPathUpdateTime;

    private bool CanNavigate =>
        agent != null &&
        agent.isActiveAndEnabled &&
        agent.isOnNavMesh;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        moveStoppingDistance = agent.stoppingDistance;

        if (viewCamera == null)
            viewCamera = Camera.main;
    }

    private void Update()
    {
        ReadInput();

        if (hasAttackOrder)
            UpdateBasicAttack();
    }

    private void ReadInput()
    {
        Keyboard keyboard = Keyboard.current;

        // A를 한 번 누르면 사거리 표시를 켭니다.
        if (keyboard != null && keyboard.aKey.wasPressedThisFrame)
            ShowAttackRange = true;

        // Esc는 사거리 표시만 닫습니다.
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            ShowAttackRange = false;

        if (Mouse.current == null || viewCamera == null)
            return;

        Ray ray = viewCamera.ScreenPointToRay(
            Mouse.current.position.ReadValue()
        );

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            ShowAttackRange = false;
            HandleRightClick(ray);
        }

        if (keyboard != null &&
            keyboard.qKey.wasPressedThisFrame &&
            Physics.Raycast(
                ray, out RaycastHit groundHit, 500f,
                groundMask, QueryTriggerInteraction.Ignore))
        {
            CastQ(groundHit.point);
        }
    }

    private void HandleRightClick(Ray ray)
    {
        // 바닥과 적을 모두 검사합니다.
        int clickMask = groundMask.value | enemyMask.value;

        if (!Physics.Raycast(
            ray, out RaycastHit hit, 500f,
            clickMask, QueryTriggerInteraction.Ignore))
        {
            return;
        }

        int hitLayer = 1 << hit.collider.gameObject.layer;

        // 적을 클릭했다면 공격 대상으로 저장합니다.
        if ((enemyMask.value & hitLayer) != 0)
        {
            Health health =
                hit.collider.GetComponentInParent<Health>();

            if (health != null)
            {
                attackTarget = health;
                hasAttackOrder = true;
                nextPathUpdateTime = 0f;
            }

            return;
        }

        // 바닥을 클릭했다면 이동합니다.
        if ((groundMask.value & hitLayer) != 0)
            MoveTo(hit.point);
    }

    private void MoveTo(Vector3 point)
    {
        // 바닥 이동 명령을 내리면 공격을 취소합니다.
        attackTarget = null;
        hasAttackOrder = false;
        StopMoving();

        if (!CanNavigate)
            return;

        agent.stoppingDistance = moveStoppingDistance;

        if (NavMesh.SamplePosition(
            point, out NavMeshHit navHit, 1f, agent.areaMask))
        {
            agent.isStopped = false;
            agent.SetDestination(navHit.position);
        }
    }

    private void UpdateBasicAttack()
    {
        // 적이 죽어 없어지면 공격을 끝냅니다.
        if (attackTarget == null)
        {
            hasAttackOrder = false;
            StopMoving();
            return;
        }

        if (!CanNavigate)
            return;

        Vector3 direction =
            attackTarget.transform.position - transform.position;

        direction.y = 0f;
        float distance = direction.magnitude;

        // 사거리 밖: 적에게 이동합니다.
        if (distance > AttackRange)
        {
            agent.isStopped = false;

            // 사거리 안쪽에 들어올 수 있도록 조금 작게 설정합니다.
            agent.stoppingDistance =
                Mathf.Max(0f, AttackRange - 0.15f);

            if (Time.time >= nextPathUpdateTime)
            {
                nextPathUpdateTime = Time.time + 0.15f;

                if (NavMesh.SamplePosition(
                    attackTarget.transform.position,
                    out NavMeshHit navHit, 1f, agent.areaMask))
                {
                    agent.SetDestination(navHit.position);
                }
                else
                {
                    StopMoving();
                }
            }

            return;
        }

        // 사거리 안: 멈추고 적을 바라봅니다.
        StopMoving();

        if (direction.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(direction);

        // 다음 공격 시간이 아직 안 됐다면 기다립니다.
        if (Time.time < nextAttackTime)
            return;

        nextAttackTime =
            Time.time + 1f / Mathf.Max(0.1f, attacksPerSecond);

        attackTarget.TakeDamage(attackDamage);
    }

    private void StopMoving()
    {
        if (!CanNavigate)
            return;

        agent.isStopped = true;

        if (agent.hasPath || agent.pathPending)
            agent.ResetPath();
    }

    private void CastQ(Vector3 targetPoint)
    {
        if (qProjectilePrefab == null || Time.time < nextQTime)
            return;

        Vector3 direction = targetPoint - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
            return;

        direction.Normalize();

        Instantiate(
            qProjectilePrefab,
            transform.position + Vector3.up,
            Quaternion.LookRotation(direction)
        );

        nextQTime = Time.time + qCooldown;
    }
}