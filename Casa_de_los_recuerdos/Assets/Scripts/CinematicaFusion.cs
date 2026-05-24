using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class CinematicaFusion : MonoBehaviour
{
    [Header("Personajes")]
    public Transform lumen;
    public Animator animLumen;
    public Transform vyre;
    public Animator animVyre;

    [Header("Sombra")]
    public Animator animSombra;
    public float tiempoParaGrito = 1f;

    [Header("Parámetros Animator (Blend Tree)")]
    public string paramVelX = "VelX";
    public string paramVelY = "VelY";
    public string paramBoolCaminar = "";
    public float valorVelYCaminar = 1f;

    [Header("Movimiento")]
    public float velocidadCaminar = 2f;
    public float velocidadConverger = 1f;
    public float distanciaParaFusionar = 0.8f;

    [Header("Partículas de Fusión")]
    public ParticleSystem particulasFusion;

    [Header("Flash Final")]
    public float duracionFlash = 1.5f;
    public float velocidadFlash = 2f;

    [Header("Fade Out Blanco")]
    public float duracionFadeBlanco = 1.5f;

    [Header("Transición")]
    public string nombreEscenaSiguiente;
    public float tiempoFijoParaCambiarEscena = 5f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip musicaCinematica;
    public AudioClip sonidoFusion;

    private bool fusionIniciada = false;
    private bool escenaCargada = false;
    private Camera cam;
    private Color colorFondoOriginal;
    private CameraClearFlags flagsOriginales;

    // Canvas del fade blanco
    private UnityEngine.UI.Image imagenFadeBlanco;

    void Start()
    {
        cam = Camera.main;
        colorFondoOriginal = cam.backgroundColor;
        flagsOriginales = cam.clearFlags;

        CrearFadeBlanco();

        if (particulasFusion != null)
        {
            particulasFusion.gameObject.SetActive(true);
            var main = particulasFusion.main;
            main.loop = true;
            main.stopAction = ParticleSystemStopAction.None;
            particulasFusion.Play();
        }

        if (audioSource != null && musicaCinematica != null)
        {
            audioSource.clip = musicaCinematica;
            audioSource.loop = true;
            audioSource.Play();
        }

        if (animSombra != null)
            StartCoroutine(GritarSombra());

        StartCoroutine(TemporizadorEscena());
        StartCoroutine(SecuenciaCinematica());
    }

    void CrearFadeBlanco()
    {
        GameObject canvasObj = new GameObject("CanvasFadeBlanco");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        GameObject imgObj = new GameObject("FadeBlanco");
        imgObj.transform.SetParent(canvas.transform, false);
        imagenFadeBlanco = imgObj.AddComponent<UnityEngine.UI.Image>();
        imagenFadeBlanco.color = new Color(1f, 1f, 1f, 0f);
        imagenFadeBlanco.raycastTarget = false;

        RectTransform rect = imagenFadeBlanco.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
    }

    IEnumerator GritarSombra()
    {
        yield return new WaitForSeconds(tiempoParaGrito);
        animSombra.SetTrigger("doScream");
        Debug.Log("👹 Sombra gritando");
    }

    IEnumerator TemporizadorEscena()
    {
        yield return new WaitForSeconds(tiempoFijoParaCambiarEscena);

        if (!escenaCargada)
        {
            Debug.Log("⏱️ Tiempo cumplido, iniciando fade...");
            yield return StartCoroutine(FadeBlanco());
            CambiarEscena();
        }
    }

    IEnumerator SecuenciaCinematica()
    {
        while (!fusionIniciada)
        {
            MantenerCaminando(animLumen);
            MantenerCaminando(animVyre);

            lumen.position += lumen.forward * velocidadCaminar * Time.deltaTime;
            vyre.position += vyre.forward * velocidadCaminar * Time.deltaTime;

            lumen.position += lumen.right * velocidadConverger * Time.deltaTime;
            vyre.position -= vyre.right * velocidadConverger * Time.deltaTime;

            if (particulasFusion != null)
            {
                Vector3 puntoMedio = (lumen.position + vyre.position) / 2f;
                puntoMedio.y += 1.5f;
                particulasFusion.transform.position = puntoMedio;

                float distancia = Vector3.Distance(lumen.position, vyre.position);
                float proximidad = Mathf.InverseLerp(5f, distanciaParaFusionar, distancia);
                float escala = Mathf.Lerp(0.1f, 4f, proximidad);
                particulasFusion.transform.localScale = Vector3.one * escala;
            }

            float dist = Vector3.Distance(lumen.position, vyre.position);
            if (dist <= distanciaParaFusionar)
                fusionIniciada = true;

            yield return null;
        }

        SetCaminar(animLumen, false);
        SetCaminar(animVyre, false);

        if (particulasFusion != null)
        {
            var main = particulasFusion.main;
            main.loop = true;
            main.stopAction = ParticleSystemStopAction.None;
            particulasFusion.Play();
        }

        if (audioSource != null && sonidoFusion != null)
            audioSource.PlayOneShot(sonidoFusion);

        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(FlashFusion());

        // Fade blanco antes de cambiar escena
        yield return StartCoroutine(FadeBlanco());

        CambiarEscena();
    }

    IEnumerator FlashFusion()
    {
        cam.clearFlags = CameraClearFlags.SolidColor;
        float tiempo = 0f;

        while (tiempo < duracionFlash)
        {
            tiempo += Time.deltaTime * velocidadFlash;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(tiempo / duracionFlash));
            cam.backgroundColor = Color.Lerp(colorFondoOriginal, Color.white, t);
            yield return null;
        }

        cam.backgroundColor = Color.white;
    }

    IEnumerator FadeBlanco()
    {
        if (imagenFadeBlanco == null) yield break;

        float tiempo = 0f;
        while (tiempo < duracionFadeBlanco)
        {
            tiempo += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, tiempo / duracionFadeBlanco);
            imagenFadeBlanco.color = new Color(1f, 1f, 1f, t);
            yield return null;
        }

        imagenFadeBlanco.color = Color.white;
    }

    void CambiarEscena()
    {
        if (escenaCargada) return;
        escenaCargada = true;

        cam.clearFlags = flagsOriginales;
        cam.backgroundColor = colorFondoOriginal;

        if (!string.IsNullOrEmpty(nombreEscenaSiguiente))
        {
            Debug.Log($"🎬 Cargando: {nombreEscenaSiguiente}");
            SceneManager.LoadScene(nombreEscenaSiguiente);
        }
        else
            Debug.LogError("❌ 'Nombre Escena Siguiente' está vacío en el Inspector");
    }

    void MantenerCaminando(Animator anim)
    {
        if (anim == null) return;
        anim.SetFloat(paramVelX, 0f);
        anim.SetFloat(paramVelY, valorVelYCaminar);
        if (!string.IsNullOrEmpty(paramBoolCaminar))
            anim.SetBool(paramBoolCaminar, true);
    }

    void SetCaminar(Animator anim, bool caminar)
    {
        if (anim == null) return;
        anim.SetFloat(paramVelX, 0f);
        anim.SetFloat(paramVelY, caminar ? valorVelYCaminar : 0f);
        if (!string.IsNullOrEmpty(paramBoolCaminar))
            anim.SetBool(paramBoolCaminar, caminar);
    }
}