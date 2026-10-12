using UnityEngine;

public class HitFlash : MonoBehaviour
{
    public Renderer targetRenderer;
    public Color flashColor = new Color(1f, 0.2f, 0.2f, 1f);
    public float flashDuration = 0.15f;

    private MaterialPropertyBlock originalBlock;
    private MaterialPropertyBlock flashBlock;

    private int colorId;
    private bool flashing;
    private float restoreTime;

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<Renderer>();

        if (targetRenderer == null ||
            targetRenderer.sharedMaterial == null)
        {
            enabled = false;
            return;
        }

        // 프로젝트의 머티리얼에 맞는 색상 항목을 찾습니다.
        string colorName =
            targetRenderer.sharedMaterial.HasProperty("_BaseColor")
            ? "_BaseColor"
            : "_Color";

        if (!targetRenderer.sharedMaterial.HasProperty(colorName))
        {
            Debug.LogWarning(
                "Body에 색상을 변경할 수 있는 머티리얼이 필요합니다.",
                this
            );

            enabled = false;
            return;
        }

        colorId = Shader.PropertyToID(colorName);

        originalBlock = new MaterialPropertyBlock();
        flashBlock = new MaterialPropertyBlock();
    }

    public void PlayFlash()
    {
        if (!isActiveAndEnabled || targetRenderer == null)
            return;

        // 원래 모습을 보관합니다.
        if (!flashing)
            targetRenderer.GetPropertyBlock(originalBlock);

        // 맞은 몸의 색상을 바꿉니다.
        targetRenderer.GetPropertyBlock(flashBlock);
        flashBlock.SetColor(colorId, flashColor);
        targetRenderer.SetPropertyBlock(flashBlock);

        flashing = true;

        restoreTime =
            Time.time + Mathf.Max(0.01f, flashDuration);
    }

    private void Update()
    {
        if (flashing && Time.time >= restoreTime)
            RestoreColor();
    }

    private void RestoreColor()
    {
        if (!flashing)
            return;

        if (targetRenderer != null)
        {
            targetRenderer.SetPropertyBlock(
                originalBlock.isEmpty ? null : originalBlock
            );
        }

        flashing = false;
    }

    private void OnDisable()
    {
        RestoreColor();
    }
}