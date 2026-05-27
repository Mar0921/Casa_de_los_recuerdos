using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MostrarControles : MonoBehaviour
{
    [Header("Panel de Controles")]
    public GameObject panelControles;           // El panel que contiene toda la info
    public Button botonCerrar;                  // Botón con la "X" para cerrar
    public Canvas canvasControles;              // Canvas del panel (opcional)

    [Header("Contenido del Panel (Opcional)")]
    public Image[] imagenesControles;           // Imágenes de cada personaje (Lumen/Vyre)
    public TMP_Text[] textosDescripcion;        // Textos describiendo los controles

    [Header("Configuración")]
    public bool mostrarSoloPrimeraVez = true;   // Solo aparece la primera vez que juegas
    public bool pausarJuego = true;             // Pausar el juego mientras se ve el panel
    public float tiempoAnimacion = 0.3f;        // Duración de animación de aparición/desaparición
    public KeyCode teclaCerrar = KeyCode.Escape; // Tecla para cerrar (opcional)

    [Header("PlayerPrefs")]
    public string keyControlesVistos = "ControlesVistos"; // Nombre para guardar en memoria

    private bool controlesVisible = false;

    void Start()
    {
        // Verificar si debe mostrar el panel
        bool debeMostrar = true;

        if (mostrarSoloPrimeraVez)
        {
            debeMostrar = !PlayerPrefs.HasKey(keyControlesVistos);
            if (!debeMostrar)
            {
                Debug.Log("Controles ya vistos anteriormente. No se muestra el panel.");
                Destroy(gameObject, 0.1f);
                return;
            }
        }

        // Configurar panel
        if (panelControles != null)
        {
            panelControles.SetActive(false);
        }

        // Configurar botón de cerrar
        if (botonCerrar != null)
        {
            botonCerrar.onClick.AddListener(CerrarPanel);
        }

        // Mostrar panel después de un pequeño delay (opcional)
        StartCoroutine(MostrarPanelConDelay(0.2f));
    }

    IEnumerator MostrarPanelConDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        MostrarPanel();
    }

    void Update()
    {
        // Cerrar con tecla Escape
        if (controlesVisible && Input.GetKeyDown(teclaCerrar))
        {
            CerrarPanel();
        }
    }

    public void MostrarPanel()
    {
        if (panelControles == null)
        {
            Debug.LogWarning("No se asignó el panel de controles");
            return;
        }

        panelControles.SetActive(true);
        controlesVisible = true;

        // Pausar juego si está activado
        if (pausarJuego)
        {
            Time.timeScale = 0f;
            Debug.Log("Juego pausado - Mostrando controles");
        }

        // Animar apertura (opcional)
        StartCoroutine(AnimarApertura());
    }

    public void CerrarPanel()
    {
        if (panelControles == null) return;

        StartCoroutine(AnimarCierre());
    }

    IEnumerator AnimarApertura()
    {
        if (panelControles != null)
        {
            CanvasGroup canvasGroup = panelControles.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = panelControles.AddComponent<CanvasGroup>();

            canvasGroup.alpha = 0f;
            float tiempo = 0f;
            while (tiempo < tiempoAnimacion)
            {
                tiempo += Time.unscaledDeltaTime; // Usar unscaled porque el juego está pausado
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, tiempo / tiempoAnimacion);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }
        yield return null;
    }

    IEnumerator AnimarCierre()
    {
        CanvasGroup canvasGroup = panelControles.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = panelControles.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 1f;
        float tiempo = 0f;
        while (tiempo < tiempoAnimacion)
        {
            tiempo += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, tiempo / tiempoAnimacion);
            yield return null;
        }
        canvasGroup.alpha = 0f;

        panelControles.SetActive(false);
        controlesVisible = false;

        // Reanudar juego
        if (pausarJuego)
        {
            Time.timeScale = 1f;
            Debug.Log("Juego reanudado");
        }

        // Guardar que ya se vieron los controles
        if (mostrarSoloPrimeraVez)
        {
            PlayerPrefs.SetInt(keyControlesVistos, 1);
            PlayerPrefs.Save();
            Debug.Log("Controles marcados como vistos");
        }

        Destroy(gameObject, 0.1f);
    }

    void OnDestroy()
    {
        // Asegurar que el tiempo se reanude si el objeto se destruye inesperadamente
        if (pausarJuego && Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
        }
    }
}