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

        //Interacción con E
        if (interactuableActual != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (!panelAbierto)
                AbrirPanel(interactuableActual.mensaje);
            else
                CerrarPanel();
        }

        //Recolectar con click derecho
        if (recolectableActual != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            Recolectar(recolectableActual);
        }

        //Cerrar panel si el jugador se aleja
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
                Debug.Log($"Tag Recolectable encontrado en: {col.gameObject.name} | Componente: {or}");
                if (or != null)
                {
                    recolectableActual = or;
                    Debug.Log("Panel recoleccion debería activarse");
                    MostrarIndicadorRecoleccion("Click derecho para recoger");
                }
            }

            if (col.CompareTag("Interactuable"))
            {
                ObjetoInteractuable oi = col.GetComponent<ObjetoInteractuable>();
                Debug.Log($"Tag Interactuable encontrado en: {col.gameObject.name} | Componente: {oi}");
                if (oi != null)
                {
                    interactuableActual = oi;
                    Debug.Log("Panel interaccion debería activarse");
                    MostrarIndicadorRecoleccion("Presiona E para interactuar");
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
        Inventario.instancia.AgregarObjeto(objeto.nombreObjeto);
        recolectableActual = null;

        if (panelRecoleccion != null)
            panelRecoleccion.SetActive(false);

        Destroy(objeto.gameObject);
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

    // Dibuja el radio de detección en el editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);
    }
}
