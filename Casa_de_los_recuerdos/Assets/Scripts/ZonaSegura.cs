using UnityEngine;

public class ZonaSegura : MonoBehaviour
{
    [Header("Referencias")]
    public MonstruoController monstruo;
    public ZonaPersecucion zonaPersecucion;

    [Header("UI opcional")]
    public GameObject panelPersecucion;

    private void Start()
    {
        if (panelPersecucion != null)
            panelPersecucion.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        Transform jugador = ObtenerJugador(other);
        if (jugador == null)
            return;

        if (monstruo != null)
            monstruo.DesactivarYDesaparecer();

        if (panelPersecucion != null)
            panelPersecucion.SetActive(false);

        if (zonaPersecucion != null)
            zonaPersecucion.Resetear();

        Debug.Log("[ZonaSegura] El jugador entró a una zona segura.");
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