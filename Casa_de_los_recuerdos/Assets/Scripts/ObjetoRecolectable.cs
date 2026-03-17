using UnityEngine;

public class ObjetoRecolectable : MonoBehaviour
{
    public string nombreObjeto = "Objeto";

    [HideInInspector] public bool estaEnInventario = false;
    [HideInInspector] public bool yaFueColocado = false;

    public void GuardarEnInventario()
    {
        estaEnInventario = true;
        yaFueColocado = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        foreach (Collider col in colliders)
            col.enabled = false;

        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        foreach (Renderer rend in renderers)
            rend.enabled = false;

        gameObject.SetActive(false);

        Debug.Log($"[Recolectable] '{nombreObjeto}' guardado en inventario.");
    }

    public void ColocarEnMundo(Transform destino)
    {
        estaEnInventario = false;
        yaFueColocado = true;

        gameObject.SetActive(true);

        transform.SetParent(destino);
        transform.position = destino.position;
        transform.rotation = destino.rotation;

        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        foreach (Renderer rend in renderers)
            rend.enabled = true;

        // Los dejamos apagados para que no se vuelva a recolectar por proximidad
        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        foreach (Collider col in colliders)
            col.enabled = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        Debug.Log($"[Recolectable] '{nombreObjeto}' colocado nuevamente en escena.");
    }
}