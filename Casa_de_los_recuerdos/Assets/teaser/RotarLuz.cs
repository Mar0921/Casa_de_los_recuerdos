using UnityEngine;

public class RotarLuz : MonoBehaviour
{
    public float velocidad = 50f;

    void Update()
    {
        transform.Rotate(0, velocidad * Time.deltaTime, 0);
    }
}