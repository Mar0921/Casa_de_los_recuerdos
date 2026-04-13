using UnityEngine;

public class MoverObjetos : MonoBehaviour
{
    [Header("Ejes activos")]
    public bool moverEnX = true;
    public bool moverEnY = false;
    public bool moverEnZ = false;

    [Header("Configuración")]
    public float velocidad = 3f;
    public float amplitud = 2f;

    private Vector3 posicionInicial;

    void Start()
    {
        posicionInicial = transform.position;
    }

    void Update()
    {
        float desplazamiento = Mathf.Sin(Time.time * velocidad) * amplitud;

        float x = posicionInicial.x + (moverEnX ? desplazamiento : 0f);
        float y = posicionInicial.y + (moverEnY ? desplazamiento : 0f);
        float z = posicionInicial.z + (moverEnZ ? desplazamiento : 0f);

        transform.position = new Vector3(x, y, z);
    }
}