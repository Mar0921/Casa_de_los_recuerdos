using UnityEngine;

public class PuertaRotacionEspecial : MonoBehaviour
{
    private Quaternion rotacionCerrada = Quaternion.Euler(90, 0, 180);
    private Quaternion rotacionAbierta = Quaternion.Euler(90, 0, -90);
    private float velocidad = 2f;
    private bool abierta = false;

    public void Abrir()
    {
        abierta = true;
    }

    void Update()
    {
        if (abierta)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                rotacionAbierta,
                Time.deltaTime * velocidad
            );
        }
    }
}