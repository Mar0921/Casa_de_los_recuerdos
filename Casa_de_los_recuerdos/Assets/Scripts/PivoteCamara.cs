using UnityEngine;

public class PivoteCamara : MonoBehaviour
{
    public Transform objetivo; // El personaje

    void LateUpdate()
    {
        if (!objetivo) return;

        // Solo copia la posición, ignora la rotación
        transform.position = objetivo.position;
    }
}