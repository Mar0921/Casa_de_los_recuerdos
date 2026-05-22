using UnityEngine;

public class CameraBreathing : MonoBehaviour
{
    [Header("Movimiento de respiración")]
    public float breathingSpeed = 1.2f;   // Velocidad de respiración
    public float breathingAmount = 0.03f; // Intensidad del movimiento

    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.localPosition;
    }

    void Update()
    {
        float offsetY = Mathf.Sin(Time.time * breathingSpeed) * breathingAmount;

        transform.localPosition = initialPosition + new Vector3(0, offsetY, 0);
    }
}