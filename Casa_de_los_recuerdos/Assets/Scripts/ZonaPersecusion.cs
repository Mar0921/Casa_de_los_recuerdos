using UnityEngine;

public class ZonaPersecucion : MonoBehaviour
{
    public MonstruoController monstruo;
    public Transform puntoAparicionMonstruo;

    [Header("Panel que se muestra cuando inicia la persecución")]
    public GameObject panelPersecucion;
    public float duracionPanelPersecucion = 3f;
    public float delayActivacion = 0.5f;
    public bool desactivarTriggerAlActivar = true;

    private bool yaActivado = false;
    private bool triggerHabilitado = false;

    private void Start()
    {
        if (panelPersecucion != null)
            panelPersecucion.SetActive(false);

        Invoke(nameof(HabilitarTrigger), delayActivacion);
    }

    private void HabilitarTrigger()
    {
        triggerHabilitado = true;
        Debug.Log("[ZonaPersecucion] Trigger habilitado.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!triggerHabilitado || yaActivado) return;

        Transform jugador = ObtenerJugador(other);
        if (jugador == null) return;

        ActivarPersecucion(jugador);
    }

    private void ActivarPersecucion(Transform jugador)
    {
        yaActivado = true;

        if (monstruo != null)
            monstruo.ActivarPersecucion(jugador, puntoAparicionMonstruo);
        else
            Debug.LogWarning("[ZonaPersecucion] ¡Monstruo no asignado!");

        if (panelPersecucion != null)
        {
            panelPersecucion.SetActive(true);
            if (duracionPanelPersecucion > 0)
                Invoke(nameof(OcultarPanel), duracionPanelPersecucion);
        }

        if (desactivarTriggerAlActivar)
        {
            Collider col = GetComponent<Collider>();
            if (col != null && col.isTrigger)
            {
                col.enabled = false;
                Debug.Log("[ZonaPersecucion] Trigger desactivado.");
            }
        }

        Debug.Log("[ZonaPersecucion] Persecución activada por: " + jugador.name);
    }

    // Llamado por ZonaSegura cuando el jugador se pone a salvo
    public void Resetear()
    {
        yaActivado = false;

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = true;

        Debug.Log("[ZonaPersecucion] Reseteada, lista para activarse de nuevo.");
    }

    private void OcultarPanel()
    {
        if (panelPersecucion != null)
            panelPersecucion.SetActive(false);
    }

    private Transform ObtenerJugador(Collider other)
    {
        if (other.CompareTag("Player"))
            return other.transform;
        Transform raiz = other.transform.root;
        if (raiz.CompareTag("Player"))
            return raiz;
        MovimientoPJ movimiento = other.GetComponentInParent<MovimientoPJ>();
        if (movimiento != null && movimiento.CompareTag("Player"))
            return movimiento.transform;
        return null;
    }
}