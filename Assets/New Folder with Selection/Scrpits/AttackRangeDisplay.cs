using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class AttackRangeDisplay : MonoBehaviour
{
    public MobaPlayer player;
    public Material rangeMaterial;
    public float lineWidth = 0.04f;

    private const int Segments = 96;
    private LineRenderer line;

    private void Awake()
    {
        if (player == null)
            player = GetComponentInParent<MobaPlayer>();

        line = GetComponent<LineRenderer>();

        line.useWorldSpace = true;
        line.loop = true;
        line.positionCount = Segments;
        line.alignment = LineAlignment.View;

        line.startColor = Color.white;
        line.endColor = Color.white;

        line.shadowCastingMode =
            UnityEngine.Rendering.ShadowCastingMode.Off;

        line.receiveShadows = false;
        line.enabled = false;

        if (rangeMaterial != null)
            line.sharedMaterial = rangeMaterial;
    }

    private void LateUpdate()
    {
        line.enabled =
            player != null &&
            rangeMaterial != null &&
            player.ShowAttackRange;

        if (!line.enabled)
            return;

        line.startWidth = lineWidth;
        line.endWidth = lineWidth;

        // 바닥과 겹치지 않도록 조금 위에 그립니다.
        Vector3 center =
            player.transform.position + Vector3.up * 0.05f;

        for (int i = 0; i < Segments; i++)
        {
            float angle = i * Mathf.PI * 2f / Segments;

            Vector3 offset = new Vector3(
                Mathf.Cos(angle),
                0f,
                Mathf.Sin(angle)
            );

            line.SetPosition(
                i,
                center + offset * player.AttackRange
            );
        }
    }

    private void OnDisable()
    {
        if (line != null)
            line.enabled = false;
    }
}