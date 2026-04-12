using UnityEngine;

public class CameraForwardMovement : MonoBehaviour
{
    public float speed = 4f;
    public float delay = 5f;

    private float timer = 0f;
    private bool canMove = false;

    void Update()
    {
        // Contador de tiempo
        timer += Time.deltaTime;

        // Activar movimiento después del delay
        if (timer >= delay)
        {
            canMove = true;
        }

        // Movimiento hacia adelante
        if (canMove)
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
    }
}