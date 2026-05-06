using UnityEngine;

public class ZonaPersecucion : MonoBehaviour
{
    [Header("Referencias")]
    public MonstruoPerseguir monstruo;
    public Transform puntoAparicionMonstruo;

    [Header("UI opcional")]
    public GameObject panelPersecucion;

    [Header("Configuración")]
    public bool desactivarTriggerAlActivar = true; // 

    private bool yaActivado = false;

    private void Start()
    {
        if (panelPersecucion != null)
            panelPersecucion.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (yaActivado) return; // ⬅️ Solo primera vez

        Transform jugador = ObtenerJugador(other);
        if (jugador == null) return;

        // Activar persecución una sola vez
        if (monstruo != null)
        {
            monstruo.ActivarPersecucion(jugador, puntoAparicionMonstruo);
        }

        if (panelPersecucion != null)
            panelPersecucion.SetActive(true);

        yaActivado = true;

        if (desactivarTriggerAlActivar)
        {
            // Desactivar el trigger para que no vuelva a entrar mientras se persigue
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }

        Debug.Log("[ZonaPersecucion] Persecución activada (solo una vez).");
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