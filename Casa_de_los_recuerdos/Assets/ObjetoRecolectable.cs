using UnityEngine;

public class ObjetoRecolectable : MonoBehaviour
{
    public string nombreObjeto = "Objeto";
    [HideInInspector] public bool estaEnInventario = false;
    [HideInInspector] public bool yaFueColocado = false;

    [Header("Sonido (solo para objetos con tag 'Llave')")]
    public AudioClip sonidoRecoger;
    [Range(0f, 1f)]
    public float volumenSonido = 1f;

    public void GuardarEnInventario()
    {
        // Si tiene sonido asignado, reproducirlo (sin importar el tag)
        if (sonidoRecoger != null)
        {
            AudioSource.PlayClipAtPoint(sonidoRecoger, Camera.main.transform.position, volumenSonido);
            Debug.Log($"[Recolectable] Reproduciendo sonido de '{nombreObjeto}'");
        }

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