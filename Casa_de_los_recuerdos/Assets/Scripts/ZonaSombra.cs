using UnityEngine;
using UnityEngine.UI;

public class ZonaSombra : MonoBehaviour
{
    [Header("Overlay oscuro")]
    public Image overlayOscuro;
    public float velocidadTransicion = 2f;
    public float alphaMax = 0.4f;

    [Header("Panel o imagen opcional")]
    public GameObject panelZona;

    private bool enSombra = false;
    private MovimientoPJ pj;

    void Start()
    {
        if (overlayOscuro != null)
        {
            Color color = overlayOscuro.color;
            color.a = 0f;
            overlayOscuro.color = color;
        }

        if (panelZona != null)
            panelZona.SetActive(false);
    }

    void Update()
    {
        if (overlayOscuro != null)
        {
            float targetAlpha = enSombra ? alphaMax : 0f;
            Color color = overlayOscuro.color;
            color.a = Mathf.MoveTowards(color.a, targetAlpha, velocidadTransicion * Time.deltaTime);
            overlayOscuro.color = color;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        MovimientoPJ movimiento = other.GetComponentInParent<MovimientoPJ>();

        if (movimiento != null)
        {
            pj = movimiento;
            pj.EnSombra(true);
            enSombra = true;

            if (panelZona != null)
                panelZona.SetActive(true);

            Debug.Log("[ZonaSombra] Entró a la zona.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        MovimientoPJ movimiento = other.GetComponentInParent<MovimientoPJ>();

        if (movimiento != null && movimiento == pj)
        {
            pj.EnSombra(false);
            enSombra = false;

            if (panelZona != null)
                panelZona.SetActive(false);

            Debug.Log("[ZonaSombra] Salió de la zona.");

            pj = null;
        }
    }
}