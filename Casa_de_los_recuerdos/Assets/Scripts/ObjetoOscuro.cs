using UnityEngine;
using System.Collections;

public class ObjetoOscuro : MonoBehaviour
{
    [Header("Configuración")]
    public AudioClip sonidoRevelar;
    public AudioClip sonidoRecoger;

    private Renderer[] rends; // Ahora es un array
    private Collider col;
    private AudioSource audioSource;
    private bool fueRevelado = false;
    private bool fueRecogido = false;
    private GestorObjetosYPuertas gestor;
    private Coroutine autoRecogerCoroutine;

    void Start()
    {
        // Busca todos los Renderers en este objeto Y sus hijos
        rends = GetComponentsInChildren<Renderer>(includeInactive: true);
        col = GetComponent<Collider>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null && (sonidoRevelar != null || sonidoRecoger != null))
            audioSource = gameObject.AddComponent<AudioSource>();

        // Ocultar todos los renderers al inicio
        SetRenders(false);

        if (col != null)
        {
            col.enabled = true;
            col.isTrigger = true;
        }
    }

    // Función helper para activar/desactivar todos los renderers de una
    void SetRenders(bool estado)
    {
        foreach (Renderer r in rends)
            if (r != null) r.enabled = estado;
    }

    public void RegistrarGestor(GestorObjetosYPuertas gestorReferencia)
    {
        gestor = gestorReferencia;
    }

    public void RevelarConAutoRecoger(float tiempoDelay)
    {
        if (fueRecogido || fueRevelado) return;

        fueRevelado = true;
        SetRenders(true); // Muestra todos los renderers hijos también

        if (audioSource != null && sonidoRevelar != null)
            audioSource.PlayOneShot(sonidoRevelar);

        if (autoRecogerCoroutine != null) StopCoroutine(autoRecogerCoroutine);
        autoRecogerCoroutine = StartCoroutine(AutoRecoger(tiempoDelay));
    }

    IEnumerator AutoRecoger(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!fueRecogido && fueRevelado) Recoger();
    }

    void OnTriggerEnter(Collider other)
    {
        if (fueRecogido || !fueRevelado) return;
        if (other.CompareTag("Player"))
        {
            if (autoRecogerCoroutine != null) StopCoroutine(autoRecogerCoroutine);
            Recoger();
        }
    }

    void Recoger()
    {
        fueRecogido = true;

        if (audioSource != null && sonidoRecoger != null)
            audioSource.PlayOneShot(sonidoRecoger);

        SetRenders(false); // Oculta todo al recoger

        if (gestor == null)
            gestor = FindFirstObjectByType<GestorObjetosYPuertas>();

        if (gestor != null)
            gestor.RecogerObjeto(name);
        else
            Debug.LogError($"No hay GestorObjetosYPuertas en la escena!");

        Destroy(gameObject, 0.2f);
    }
}