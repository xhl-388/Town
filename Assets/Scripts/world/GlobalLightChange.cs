using UnityEngine;

public class GlobalLightChange : MonoBehaviour
{
    private float _rotateDegreePerSecond = 0.75f;

    private void Update()
    {
        transform.Rotate(Vector3.left * _rotateDegreePerSecond * Time.deltaTime, Space.Self);
        float xRotation = transform.rotation.eulerAngles.x;
        if (xRotation < 315.0f && xRotation > 225.0f)
        {
            float lerpFactor = 1.0f-Mathf.Abs(xRotation - 270.0f)/45.0f;
            float ans = Mathf.Lerp(0.6f, 0.0f, lerpFactor);
            RenderSettings.ambientIntensity = ans;
        }
    }
}