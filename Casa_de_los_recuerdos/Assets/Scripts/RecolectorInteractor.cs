using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class RecolectorInteractor : MonoBehaviour
{
    public float radioDeteccion = 2f;
    public LayerMask capasDetectables;

    public GameObject panelInteraccion;
    public TMP_Text textoInteraccion;
    public GameObject panelRecoleccion;
    public TMP_Text textoRecoleccion;

    private ObjetoInteractuable interactuableActual = null;
    private ObjetoRecolectable recolectableActual = null;
    private bool panelAbierto = false;

    void Update()
    {
        BuscarObjetosCercanos();

        // Interacción con E
        if (interactuableActual != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            ObjetoRecolectable objetoInventario = Inventario.instancia.ObtenerUltimoObjeto();

            if (objetoInventario != null)
            {
                bool seColoco = interactuableActual.IntentarColocarObjeto(objetoInventario);

                if (seColoco)
                {
                    Inventario.instancia.QuitarObjeto(objetoInventario);

                    Debug.Log($"[Interactor] Se colocó '{objetoInventario.nombreObjeto}' usando la tecla E.");

                    if (panelRecoleccion != null)
                        panelRecoleccion.SetActive(false);

                    return;
                }
                else
                {
                    Debug.Log("[Interactor] No se pudo colocar el objeto del inventario.");
                }
            }

            // Si no hay objeto en inventario, se comporta como antes
            if (!panelAbierto)
                AbrirPanel(interactuableActual.mensaje);
            else
                CerrarPanel();
        }

        // Recolectar con click derecho
        if (recolectableActual != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            Recolectar(recolectableActual);
        }

        // Cerrar panel si el jugador se aleja
        if (panelAbierto && interactuableActual == null)
        {
            CerrarPanel();
        }
    }

    void BuscarObjetosCercanos()
    {
        interactuableActual = null;
        recolectableActual = null;

        Collider[] collidersEnRango = Physics.OverlapSphere(transform.position, radioDeteccion);

        foreach (Collider col in collidersEnRango)
        {
            if (col.CompareTag("Recolectable"))
            {
                ObjetoRecolectable or = col.GetComponent<ObjetoRecolectable>();

                if (or != null && !or.estaEnInventario && !or.yaFueColocado)
                {
                    recolectableActual = or;
                    MostrarIndicadorRecoleccion("Click derecho para recoger");
                    Debug.Log($"[Interactor] Recolectable detectado: {or.nombreObjeto}");
                }
            }

            if (col.CompareTag("Interactuable"))
            {
                ObjetoInteractuable oi = col.GetComponent<ObjetoInteractuable>();

                if (oi != null)
                {
                    interactuableActual = oi;

                    if (Inventario.instancia != null && Inventario.instancia.TieneObjetos())
                        MostrarIndicadorRecoleccion("Presiona E para colocar objeto");
                    else
                        MostrarIndicadorRecoleccion("Presiona E para interactuar");

                    Debug.Log($"[Interactor] Interactuable detectado: {oi.gameObject.name}");
                }
            }
        }

        if (interactuableActual == null && recolectableActual == null)
        {
            if (panelRecoleccion != null)
                panelRecoleccion.SetActive(false);
        }
    }

    void AbrirPanel(string mensaje)
    {
        panelAbierto = true;

        if (panelInteraccion != null)
        {
            panelInteraccion.SetActive(true);

            if (textoInteraccion != null)
                textoInteraccion.text = mensaje;
        }
    }

    void CerrarPanel()
    {
        panelAbierto = false;

        if (panelInteraccion != null)
            panelInteraccion.SetActive(false);
    }

    void Recolectar(ObjetoRecolectable objeto)
    {
        if (objeto == null)
        {
            Debug.LogWarning("[Interactor] Se intentó recolectar un objeto nulo.");
            return;
        }

        Inventario.instancia.AgregarObjeto(objeto);
        objeto.GuardarEnInventario();

        recolectableActual = null;

        if (panelRecoleccion != null)
            panelRecoleccion.SetActive(false);

        Debug.Log($"[Interactor] '{objeto.nombreObjeto}' fue recolectado correctamente y enviado al inventario.");
    }

    void MostrarIndicadorRecoleccion(string texto)
    {
        if (panelRecoleccion != null)
        {
            panelRecoleccion.SetActive(true);

            if (textoRecoleccion != null)
                textoRecoleccion.text = texto;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);
    }
}