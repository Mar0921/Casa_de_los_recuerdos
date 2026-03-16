using UnityEngine;

public class CamaraSeguir : MonoBehaviour
{
    public Transform objetivo;
    public Vector3 offset = new Vector3(0f, 5f, 7f);
    public float suavidad = 5f;

    void LateUpdate()
    {
        if (!objetivo) return;

        // Sigue la posición del personaje + offset fijo, ignora su rotación
        transform.position = Vector3.Lerp(transform.position, objetivo.position + offset, suavidad * Time.deltaTime);
        transform.LookAt(objetivo);
    }
}