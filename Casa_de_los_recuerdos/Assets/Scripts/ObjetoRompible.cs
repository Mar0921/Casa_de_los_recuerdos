using UnityEngine;

public class ObjetoRompible : MonoBehaviour
{
    private bool yaNotificado = false;

    void Start()
    {
        if (GestorRompibles.instancia != null)
            GestorRompibles.instancia.RegistrarRompible(this);
    }

    void OnDestroy()
    {
        if (yaNotificado) return;
        yaNotificado = true;

        if (GestorRompibles.instancia != null)
        {
            GestorRompibles.instancia.NotificarRompibleDestruido(this);
            Debug.Log($" [ObjetoRompible] '{gameObject.name}' destruido y notificado.");
        }
    }
}