using UnityEngine;

public class GlobalLightChange : MonoBehaviour
{
    [Tooltip("The Speed of the rotation of the SUN!")]
    [SerializeField]
    private float rotateDegreePerSecond = 0.75f;

    [Tooltip("Max or normal intensity of the sky light")]
    [SerializeField] private float maxAmbientIntensity = 0.6f;
    
    [Tooltip("Min intensity of the sky light")]
    [SerializeField] private float minAmbientIntensity = 0.0f;

    private void Update()
    {
        transform.Rotate(Vector3.left * rotateDegreePerSecond * Time.deltaTime, Space.Self);
        
        // notice that the value of eulerAngles of each component's range is (0,360) , while the editor shows (-180,180)
        float xRotation = transform.rotation.eulerAngles.x;
        if (xRotation < 315.0f && xRotation > 225.0f)
        {
            float lerpFactor = 1.0f-Mathf.Abs(xRotation - 270.0f)/45.0f;
            float ans = Mathf.Lerp(maxAmbientIntensity, minAmbientIntensity, lerpFactor);
            RenderSettings.ambientIntensity = ans;
        }
    }
}