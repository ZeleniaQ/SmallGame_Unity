using UnityEngine;
public class SpinBob : MonoBehaviour
{
    public float flipSpeed = 2f;
    public float bobAmplitude = 0.1f;
    public float bobSpeed = 3f;

    float baseY;
    Vector3 baseScale;

    void Start(){
        baseY = transform.localPosition.y;
        baseScale = transform.localScale;
    }

    void Update(){
        float c = Mathf.Cos(Time.time * Mathf.PI * flipSpeed);
        float minThickness = 0.03f; 
        float sx = Mathf.Sign(c) * Mathf.Max(minThickness, Mathf.Abs(c));
        transform.localScale = new UnityEngine.Vector3(baseScale.x * sx, baseScale.y, baseScale.z);

        var p = transform.localPosition;
        p.y = baseY + Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
        transform.localPosition = p;
    }
}
