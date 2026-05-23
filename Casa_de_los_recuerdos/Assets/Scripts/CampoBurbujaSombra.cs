using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class CampoBurbujaSombra : MonoBehaviour
{
    [Header("Configuración de Tentáculos")]
    public int tentaculosNecesarios = 2;
    private int tentaculosEliminados = 0;

    [Header("Configuración de la Burbuja")]
    public Material materialBurbuja;
    public float escalaDestruccion = 0.5f;
    public ParticleSystem particulasRuptura;

    [Header("Sonidos")]
    public AudioSource audioSource;
    public AudioClip sonidoGrito;
    public float volumenGrito = 1f;

    [Header("Fade Out (Transición)")]
    public float duracionFadeOut = 1.5f;
    public Color colorFade = Color.black;

    [Header("Cámara - Punto de enfoque")]
    public Transform puntoCamara;
    public float tiempoMovimientoCamara = 1f;
    public float tiempoEsperaCamara = 1f;

    [Header("Siguiente Escena")]
    public string nombreEscenaDestino = "NombreDeTuEscena";

    private bool burbujaRota = false;
    private Camera camaraPrincipal;
    private Canvas canvasFade;
    private UnityEngine.UI.Image imagenFade;
    private bool transicionIniciada = false;

    void Start()
    {
        camaraPrincipal = Camera.main;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        audioSource.volume = volumenGrito;

        CrearCanvasFade();

        Debug.Log($"Campo burbuja iniciado. Tentáculos necesarios: {tentaculosNecesarios}");
    }

    void CrearCanvasFade()
    {
        GameObject canvasObj = new GameObject("CanvasFadeBurbuja");
        canvasFade = canvasObj.AddComponent<Canvas>();
        canvasFade.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasFade.sortingOrder = 999;

        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        GameObject imagenObj = new GameObject("ImagenFade");
        imagenObj.transform.SetParent(canvasFade.transform, false);
        imagenFade = imagenObj.AddComponent<UnityEngine.UI.Image>();
        imagenFade.color = new Color(colorFade.r, colorFade.g, colorFade.b, 0f);
        imagenFade.raycastTarget = false;

        RectTransform rect = imagenFade.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        // Ocultar el canvas al inicio
        canvasFade.gameObject.SetActive(false);
    }

    public void TentaculoEliminado()
    {
        if (burbujaRota) return;

        tentaculosEliminados++;
        Debug.Log($"Tentáculo eliminado: {tentaculosEliminados}/{tentaculosNecesarios}");

        if (materialBurbuja != null)
        {
            float progreso = (float)tentaculosEliminados / tentaculosNecesarios;
            materialBurbuja.SetFloat("_Progress", progreso);
        }

        if (tentaculosEliminados >= tentaculosNecesarios && !transicionIniciada)
        {
            Debug.Log("🎯 ¡TODOS LOS TENTÁCULOS ELIMINADOS!");
            transicionIniciada = true;
            StartCoroutine(PrepararRomperBurbuja());
        }
    }

    IEnumerator PrepararRomperBurbuja()
    {
        if (puntoCamara != null && camaraPrincipal != null)
        {
            Debug.Log($"Moviendo cámara al punto: {puntoCamara.name}");

            Vector3 posInicio = camaraPrincipal.transform.position;
            Quaternion rotInicio = camaraPrincipal.transform.rotation;
            float tiempo = 0f;

            while (tiempo < tiempoMovimientoCamara)
            {
                tiempo += Time.deltaTime;
                // SmoothStep hace que arranque lento, acelere y frene suavemente
                float t = Mathf.SmoothStep(0f, 1f, tiempo / tiempoMovimientoCamara);
                camaraPrincipal.transform.position = Vector3.Lerp(posInicio, puntoCamara.position, t);
                camaraPrincipal.transform.rotation = Quaternion.Slerp(rotInicio, puntoCamara.rotation, t);
                yield return null;
            }

            // Fijar exactamente al destino
            camaraPrincipal.transform.position = puntoCamara.position;
            camaraPrincipal.transform.rotation = puntoCamara.rotation;

            // Mínimo 2 segundos forzados, o lo que tengas en el Inspector si es mayor
            float espera = Mathf.Max(2f, tiempoEsperaCamara);
            Debug.Log($"Cámara en posición. Esperando {espera} segundo(s)...");
            yield return new WaitForSeconds(espera);
        }
        else
        {
            Debug.LogWarning("No hay punto de cámara asignado o no hay cámara principal");
            yield return new WaitForSeconds(2f);
        }

        RomperBurbuja();
    }

    void RomperBurbuja()
    {
        if (burbujaRota) return;
        burbujaRota = true;

        Debug.Log("💥 ¡La burbuja del campo se ha roto! 💥");

        StartCoroutine(EfectoExplosionBurbuja());

        if (audioSource != null && sonidoGrito != null)
        {
            audioSource.PlayOneShot(sonidoGrito, volumenGrito);
        }

        if (particulasRuptura != null)
        {
            particulasRuptura.transform.SetParent(null);
            particulasRuptura.Play();
            Destroy(particulasRuptura.gameObject, 2f);
        }

        // Iniciar fade out y cambio de escena
        StartCoroutine(FadeOutYCargarEscena());
    }

    IEnumerator EfectoExplosionBurbuja()
    {
        float tiempo = 0f;
        Vector3 escalaOriginal = transform.localScale;
        Vector3 escalaFinal = escalaOriginal * escalaDestruccion;

        while (tiempo < 0.5f)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / 0.5f;
            transform.localScale = Vector3.Lerp(escalaOriginal, escalaFinal, t);

            if (materialBurbuja != null)
            {
                Color color = materialBurbuja.color;
                color.a = Mathf.Lerp(1f, 0f, t);
                materialBurbuja.color = color;
            }

            yield return null;
        }

        // Solo apagar el renderer, NO el GameObject completo
        Renderer rend = GetComponent<Renderer>();
        if (rend != null) rend.enabled = false;

        // Desactivar el collider también
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }

    IEnumerator FadeOutYCargarEscena()
    {
        // Activar el canvas de fade
        if (canvasFade != null)
        {
            canvasFade.gameObject.SetActive(true);
        }

        if (imagenFade == null) yield break;

        Debug.Log($"Iniciando fade out - Cargando escena: {nombreEscenaDestino}");

        float tiempo = 0f;
        Color color = imagenFade.color;

        while (tiempo < duracionFadeOut)
        {
            tiempo += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, tiempo / duracionFadeOut);
            imagenFade.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        imagenFade.color = new Color(color.r, color.g, color.b, 1f);

        // Pequeña pausa para asegurar que el fade esté completo
        yield return new WaitForSeconds(0.2f);

        Debug.Log($"Cargando escena: {nombreEscenaDestino}");

        try
        {
            SceneManager.LoadScene(nombreEscenaDestino);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al cargar la escena '{nombreEscenaDestino}': {e.Message}");
            Debug.LogError("Asegúrate de que:");
            Debug.LogError("1. La escena está en Build Settings");
            Debug.LogError("2. El nombre escrito es EXACTAMENTE igual (mayúsculas/minúsculas)");
        }
    }

    public bool EstaRota()
    {
        return burbujaRota;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 0f, 0.5f, 0.5f);
        SphereCollider col = GetComponent<SphereCollider>();
        if (col != null)
        {
            Gizmos.DrawWireSphere(transform.position, col.radius);
        }
    }
}