using UnityEngine;

public class BottleFallAfterTime : MonoBehaviour
{
    [Header("Tiempo antes de caer")]
    public float delay = 7f;

    [Header("Rotación de caída")]
    public Vector3 fallRotation = new Vector3(0f, 0f, 90f);

    [Header("Velocidad de animación")]
    public float fallSpeed = 2f;

    private bool startFalling = false;
    private Quaternion targetRotation;

    void Start()
    {
        // Calcula la rotación final
        targetRotation = Quaternion.Euler(transform.eulerAngles + fallRotation);

        // Ejecuta la caída después de 7 segundos
        Invoke(nameof(StartFall), delay);
    }

    void Update()
    {
        if (startFalling)
        {
            // Interpolación suave de la rotación
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                fallSpeed * Time.deltaTime
            );
        }
    }

    void StartFall()
    {
        startFalling = true;
    }
}