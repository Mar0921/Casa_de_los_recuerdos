using UnityEngine;
using System.Collections;

public class InicioEscena : MonoBehaviour
{
    [Header("Fade In")]
    public float duracionFadeIn = 1.5f;
    public Color colorFade = Color.white;

    [Header("Animación del Personaje")]
    public Animator animatorPersonaje;
    public string parametroAnimacion = "MeLevante";
    public float tiempoAnimacion = 2f;

    [Header("Sonido al final de la animación")]
    public AudioClip sonidoFinal;
    public AudioSource audioSource;
    public float volumenSonido = 1f;

    [Header("Referencias")]
    public GameObject jugador;

    private Camera camaraPrincipal;
    private Canvas canvasFade;
    private UnityEngine.UI.Image imagenFade;

    void Start()
    {
        camaraPrincipal = Camera.main;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && sonidoFinal != null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }
        if (audioSource != null)
            audioSource.volume = volumenSonido;

        DesactivarControlJugador(true);
        CrearCanvasFade();
        StartCoroutine(SecuenciaInicio());
    }

    void CrearCanvasFade()
    {
        GameObject canvasObj = new GameObject("CanvasFadeInicio");
        canvasFade = canvasObj.AddComponent<Canvas>();
        canvasFade.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasFade.sortingOrder = 999;

        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        GameObject imagenObj = new GameObject("ImagenFade");
        imagenObj.transform.SetParent(canvasFade.transform, false);
        imagenFade = imagenObj.AddComponent<UnityEngine.UI.Image>();
        imagenFade.color = new Color(colorFade.r, colorFade.g, colorFade.b, 1f);
        imagenFade.raycastTarget = false;

        RectTransform rect = imagenFade.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        Debug.Log("Canvas de fade creado - Pantalla completamente blanca");
    }

    IEnumerator SecuenciaInicio()
    {
        if (imagenFade == null) yield break;

        yield return new WaitForSeconds(0.05f);

        // 🔴 ANIMACIÓN MUCHO MÁS TEMPRANA (20% del fade)
        float puntoInicioAnimacion = 0.2f; // Antes era 0.5, ahora mucho antes
        bool animacionIniciada = false;

        float tiempo = 0f;
        Color colorInicial = imagenFade.color;

        while (tiempo < duracionFadeIn)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / duracionFadeIn;
            float alpha = Mathf.Lerp(1f, 0f, t);
            imagenFade.color = new Color(colorInicial.r, colorInicial.g, colorInicial.b, alpha);

            if (!animacionIniciada && t >= puntoInicioAnimacion)
            {
                if (animatorPersonaje != null)
                {
                    animatorPersonaje.SetBool(parametroAnimacion, true);
                    Debug.Log($"🎬 Animación {parametroAnimacion} activada (fade en {Mathf.RoundToInt(t * 100)}%)");
                }
                animacionIniciada = true;
            }

            yield return null;
        }

        imagenFade.color = new Color(colorInicial.r, colorInicial.g, colorInicial.b, 0f);
        Destroy(canvasFade.gameObject);

        if (!animacionIniciada && animatorPersonaje != null)
        {
            animatorPersonaje.SetBool(parametroAnimacion, true);
            animacionIniciada = true;
        }

        // Calcular tiempo restante de animación
        float tiempoRestanteAnimacion = tiempoAnimacion;
        if (animacionIniciada)
        {
            float tiempoTranscurridoAnim = duracionFadeIn * puntoInicioAnimacion;
            tiempoRestanteAnimacion = Mathf.Max(0, tiempoAnimacion - tiempoTranscurridoAnim);
        }

        if (tiempoRestanteAnimacion > 0)
        {
            Debug.Log($"⏳ Esperando {tiempoRestanteAnimacion:F2} segundos para que termine la animación");
            yield return new WaitForSeconds(tiempoRestanteAnimacion);
        }

        // 🔴 ESPERAR 1 SEGUNDO ANTES DEL SONIDO (como pediste)
        yield return new WaitForSeconds(1f);

        // Reproducir sonido al final de la animación
        if (audioSource != null && sonidoFinal != null)
        {
            audioSource.PlayOneShot(sonidoFinal, volumenSonido);
            Debug.Log($"🔊 Reproduciendo sonido al final de la animación");
            yield return new WaitForSeconds(0.2f);
        }

        // Desactivar bool para volver al blend tree
        if (animatorPersonaje != null)
        {
            animatorPersonaje.SetBool(parametroAnimacion, false);
            Debug.Log($"✅ Animación {parametroAnimacion} desactivada");
        }

        DesactivarControlJugador(false);
        Debug.Log("🎉 Secuencia de inicio completada - Jugador puede moverse");
    }

    void DesactivarControlJugador(bool desactivar)
    {
        if (jugador != null)
        {
            ProtaMovimiento movimiento = jugador.GetComponent<ProtaMovimiento>();
            if (movimiento != null)
                movimiento.SetPuedeMover(!desactivar);
        }
    }
}