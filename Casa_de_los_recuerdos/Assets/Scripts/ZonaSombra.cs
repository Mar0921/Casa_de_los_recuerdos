using UnityEngine;
using UnityEngine.UI;

public class ZonaSombra : MonoBehaviour
{
    [Header("Overlay oscuro")]
    public Image overlayOscuro;
    public float velocidadTransicion = 15f;
    public float alphaMax = 1f;

    [Header("Palpito")]
    public bool activarPalpito = true;
    public float velocidadPalpito = 2f;
    public float intensidadPalpito = 0.2f; 
    public GameObject panelZona;

    private bool enSombra = false;
    private MovimientoPJ pj;
    private float tiempoPalpito = 0f;
    private float alphaBase = 0f;

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
            Color color = overlayOscuro.color;

            if (enSombra)
            {
                alphaBase = alphaMax;

                if (activarPalpito)
                {
                    tiempoPalpito += Time.deltaTime * velocidadPalpito;
                    float palpito = Mathf.Sin(tiempoPalpito) * intensidadPalpito;
                    float alphaConPalpito = Mathf.Clamp(alphaBase + palpito, 0.5f, alphaMax);
                    color.a = alphaConPalpito;
                }
                else
                {
                    color.a = Mathf.MoveTowards(color.a, alphaMax, velocidadTransicion * Time.deltaTime);
                }
            }
            else
            {
                color.a = Mathf.MoveTowards(color.a, 0f, velocidadTransicion * Time.deltaTime);
                tiempoPalpito = 0f;
            }

            overlayOscuro.color = color;

            if (enSombra)
            {
                Debug.Log($"Alpha actual: {color.a}");
            }
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

            Debug.Log("[ZonaSombra] Entró a la zona");
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

            Debug.Log("[ZonaSombra] Salió de la zona");
            pj = null;
        }
    }
}