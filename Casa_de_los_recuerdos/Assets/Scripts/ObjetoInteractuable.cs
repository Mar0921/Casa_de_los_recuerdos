using UnityEngine;

public class ObjetoInteractuable : MonoBehaviour
{
    public string mensaje = "Este es un mensaje de interacción.";

    [Header("Colocación")]
    public Transform puntoColocacion;
    public bool yaTieneObjeto = false;

    [Tooltip("Déjalo vacío si aceptará cualquier objeto.")]
    public string nombreObjetoRequerido = "";

    public bool IntentarColocarObjeto(ObjetoRecolectable objeto)
    {
        if (yaTieneObjeto)
        {
            Debug.Log($"[Interactuable] '{gameObject.name}' ya tiene un objeto colocado.");
            return false;
        }

        if (objeto == null)
        {
            Debug.LogWarning("[Interactuable] No se recibió ningún objeto.");
            return false;
        }

        if (!string.IsNullOrEmpty(nombreObjetoRequerido) &&
            objeto.nombreObjeto != nombreObjetoRequerido)
        {
            Debug.Log($"[Interactuable] '{objeto.nombreObjeto}' no corresponde. Se esperaba '{nombreObjetoRequerido}'.");
            return false;
        }

        Transform destino = puntoColocacion != null ? puntoColocacion : transform;

        objeto.ColocarEnMundo(destino);
        yaTieneObjeto = true;

        Debug.Log($"[Interactuable] Se colocó '{objeto.nombreObjeto}' sobre '{gameObject.name}'.");
        return true;
    }
}