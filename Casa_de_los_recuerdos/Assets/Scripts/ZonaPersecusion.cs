using UnityEngine;

public class ZonaPersecucion : MonoBehaviour
{
    [Header("Referencias")]
    public MonstruoPerseguir monstruo;
    public Transform puntoAparicionMonstruo;

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
        {
            monstruo.ActivarPersecucion(jugador, puntoAparicionMonstruo);
        }

        if (panelPersecucion != null)
            panelPersecucion.SetActive(true);

        Debug.Log("[ZonaPersecucion] El jugador entró a la zona de persecución.");
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