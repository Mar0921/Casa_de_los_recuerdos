using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class FadeManager : MonoBehaviour
{
    [Header("Configuración General")]
    public float duracionFadeIn = 1f;      // Duración del fade in normal
    public float duracionFadeOut = 1f;     // Duración del fade out
    public Color colorFade = Color.black;

    [Header("Configuración para Logo (Solo primera escena)")]
    public bool esEscenaConLogo = true;    // Marcar true solo en la escena del logo
    public float tiempoLogoVisible = 3f;   // Tiempo que se ve el logo antes de desvanecer
    public Sprite logoImagen;              // Imagen del logo

    [Header("Ajustes del Logo")]
    public Vector2 tamañoLogo = new Vector2(400, 200);  // Ancho y alto del logo
    public Vector2 posicionLogo = Vector2.zero;         // Posición en pantalla (0,0 es centro)
    public bool mantenerAspecto = true;                 // Mantener proporción original de la imagen

    // Variables internas
    private static FadeManager instancia;
    private Canvas canvasFade;
    private UnityEngine.UI.Image imagenFade;
    private UnityEngine.UI.Image imagenLogo;
    private bool fadeEnProgreso = false;

    void Awake()
    {
        // Singleton: asegura que solo haya un FadeManager en toda la aplicación
        if (instancia == null)
        {
            instancia = this;
            DontDestroyOnLoad(gameObject);  // Persiste entre escenas
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        CrearCanvasFade();

        // Suscribirse al evento de carga de escenas
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        // Limpiar suscripción
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void CrearCanvasFade()
    {
        // Crear Canvas si no existe
        canvasFade = GetComponentInChildren<Canvas>();
        if (canvasFade == null)
        {
            GameObject canvasObj = new GameObject("CanvasFade");
            canvasObj.transform.SetParent(transform);
            canvasFade = canvasObj.AddComponent<Canvas>();
            canvasFade.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasFade.sortingOrder = 999;

            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        // Crear imagen de fade
        GameObject imagenObj = new GameObject("ImagenFade");
        imagenObj.transform.SetParent(canvasFade.transform, false);
        imagenFade = imagenObj.AddComponent<UnityEngine.UI.Image>();
        imagenFade.color = new Color(colorFade.r, colorFade.g, colorFade.b, 0f);
        imagenFade.raycastTarget = false;

        RectTransform rect = imagenFade.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!fadeEnProgreso)
        {
            // Verificar si es la escena del logo (primera escena)
            if (esEscenaConLogo && scene.buildIndex == 0)
            {
                StartCoroutine(LogoYFadeIn());
            }
            else
            {
                StartCoroutine(FadeIn());
            }
        }
    }

    // Corutina para mostrar logo y luego fade in
    IEnumerator LogoYFadeIn()
    {
        fadeEnProgreso = true;

        // Asegurar pantalla completamente negra
        imagenFade.color = new Color(colorFade.r, colorFade.g, colorFade.b, 1f);

        // Mostrar logo si existe
        if (logoImagen != null && imagenLogo == null)
        {
            GameObject logoObj = new GameObject("Logo");
            logoObj.transform.SetParent(canvasFade.transform, false);
            imagenLogo = logoObj.AddComponent<UnityEngine.UI.Image>();
            imagenLogo.sprite = logoImagen;

            // Configurar la imagen para mantener el aspecto si se desea
            if (mantenerAspecto)
            {
                imagenLogo.preserveAspect = true;
            }

            // Configurar posición y tamaño del logo
            RectTransform rectLogo = imagenLogo.rectTransform;

            // Anclas al centro
            rectLogo.anchorMin = new Vector2(0.5f, 0.5f);
            rectLogo.anchorMax = new Vector2(0.5f, 0.5f);

            // Tamaño
            rectLogo.sizeDelta = tamañoLogo;

            // Posición (offset desde el centro)
            rectLogo.anchoredPosition = posicionLogo;

            // Asegurar que el logo esté visible
            imagenLogo.color = Color.white;
        }

        // Esperar tiempo del logo
        yield return new WaitForSeconds(tiempoLogoVisible);

        // Desvanecer el logo (efecto opcional)
        if (imagenLogo != null)
        {
            float tiempoDesvanecer = 0.5f;
            float tiempo = 0f;
            Color colorLogo = imagenLogo.color;

            while (tiempo < tiempoDesvanecer)
            {
                tiempo += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, tiempo / tiempoDesvanecer);
                imagenLogo.color = new Color(colorLogo.r, colorLogo.g, colorLogo.b, alpha);
                yield return null;
            }

            Destroy(imagenLogo.gameObject);
            imagenLogo = null;
        }

        // Fade in (de negro a transparente)
        float tiempoFade = 0f;
        Color colorFondo = imagenFade.color;

        while (tiempoFade < duracionFadeIn)
        {
            tiempoFade += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, tiempoFade / duracionFadeIn);
            imagenFade.color = new Color(colorFondo.r, colorFondo.g, colorFondo.b, alpha);
            yield return null;
        }

        imagenFade.color = new Color(colorFondo.r, colorFondo.g, colorFondo.b, 0f);
        fadeEnProgreso = false;
    }

    // Corutina para fade in normal (sin logo)
    IEnumerator FadeIn()
    {
        fadeEnProgreso = true;

        // Empezar completamente negro
        imagenFade.color = new Color(colorFade.r, colorFade.g, colorFade.b, 1f);

        // Transición a transparente
        float tiempo = 0f;
        Color color = imagenFade.color;

        while (tiempo < duracionFadeIn)
        {
            tiempo += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, tiempo / duracionFadeIn);
            imagenFade.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        imagenFade.color = new Color(color.r, color.g, color.b, 0f);
        fadeEnProgreso = false;
    }

    // Método público para hacer fade out antes de cambiar de escena
    public void CambiarEscena(string nombreEscena)
    {
        StartCoroutine(FadeOutYCargar(nombreEscena));
    }

    public void CambiarEscena(int indiceEscena)
    {
        StartCoroutine(FadeOutYCargar(indiceEscena));
    }

    IEnumerator FadeOutYCargar(string nombreEscena)
    {
        fadeEnProgreso = true;

        // Hacer fade out a negro
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

        // Cargar la nueva escena
        SceneManager.LoadScene(nombreEscena);
    }

    IEnumerator FadeOutYCargar(int indiceEscena)
    {
        fadeEnProgreso = true;

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
        SceneManager.LoadScene(indiceEscena);
    }

    // Método para obtener la instancia desde otros scripts
    public static FadeManager Instance
    {
        get { return instancia; }
    }
}