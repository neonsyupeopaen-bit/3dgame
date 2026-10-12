using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    public Camera viewCamera;

    private void Awake()
    {
        if (viewCamera == null)
            viewCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (viewCamera == null)
            return;

        transform.rotation = viewCamera.transform.rotation;
    }
}