using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class CreditosFinales : MonoBehaviour
{
    [Header("Configuración de Scroll")]
    public float velocidadScroll = 30f;
    public float tiempoInicialEspera = 1f;
    public float duracionFadeOut = 2f;
    public float duracionFadeMusica = 1.5f;

    [Header("Referencias UI")]
    public RectTransform contenido;
    public Image[] imagenesDesarrollo;
    public TMP_Text textoCreditos;

    [Header("Logo Final")]
    public Image logoFinal;               // Imagen del logo que aparecerá al final
    public float tiempoLogo = 5f;         // Tiempo que se muestra el logo antes de ir al menú

    [Header("Música")]
    public AudioSource musicaCreditos;
    public float volumenMusica = 0.5f;

    [Header("Asignación Manual")]
    public List<AsignacionRol> asignaciones = new List<AsignacionRol>();

    [System.Serializable]
    public class AsignacionRol
    {
        public string rol;
        public string persona;
    }

    [Header("Créditos adicionales")]
    [TextArea(5, 10)]
    public string creditosModelos = " MODELOS 3D DE TERCEROS:\n" +
        "- 'Abandoned House' de 3D_Avenue (CGTrader)\n" +
        "- 'Victorian Furniture Pack' de DarioSanchez (Sketchfab)\n" +
        "- 'Low Poly Characters' de Quaternius (modificados por el equipo)\n" +
        "- Animaciones de Mixamo (Adobe)\n\n" +
        "🛠 HERRAMIENTAS UTILIZADAS:\n" +
        "- Unity 2021.3 LTS\n" +
        "- Blender 3.0\n" +
        "- Photoshop / GIMP\n" +
        "- Audacity\n";

    [TextArea(5, 10)]
    public string creditosDesarrollo = "📖 PROCESO DE DESARROLLO (Basado en GDD 'La Casa de los Recuerdos'):\n" +
        "- Diseño narrativo: Documento de 30 páginas con estructura Inicio-Nudo-Desenlace.\n" +
        "- Prototipado: 3 iteraciones de mecánicas de luz/sombra con Lumen y Vyre.\n" +
        "- Niveles: Cocina (tutorial), Sala (puzzle de muebles), Habitación niño (objetos oscuros), Habitación hermana (tentáculos + fusión).\n" +
        "- Implementación de IA de persecución (Sombra) con sistema de ruido.\n" +
        "- Sistema de objetos coleccionables (revelados con luz Lumen).\n" +
        "- Transiciones de escena con fade blanco/negro y animación 'MeLevante'.\n" +
        "- Testing interno: 15+ horas de juego para ajustar dificultad y ritmo.\n";

    private bool finalizado = false;

    void Start()
    {
        // Ocultar logo al inicio si existe
        if (logoFinal != null)
            logoFinal.gameObject.SetActive(false);

        // Generar texto
        string textoCredito = GenerarTextoCreditos();
        if (textoCreditos != null)
            textoCreditos.text = textoCredito;

        // Posición inicial del contenido (abajo)
        if (contenido != null)
        {
            contenido.anchoredPosition = new Vector2(contenido.anchoredPosition.x, -Screen.height * 0.5f);
        }

        // Música
        if (musicaCreditos != null)
        {
            musicaCreditos.volume = volumenMusica;
            musicaCreditos.loop = true;
            musicaCreditos.Play();
        }

        StartCoroutine(ImagenesFadeIn());
        StartCoroutine(SecuenciaCreditos());
    }

    string GenerarTextoCreditos()
    {
        string texto = "<b><size=36>LA CASA DE LOS RECUERDOS</size></b>\n\n";
        texto += "<size=28>Créditos finales</size>\n\n";
        texto += "<size=22><i>Un viaje entre recuerdos y superación</i></size>\n\n";
        texto += "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n\n";

        foreach (var asign in asignaciones)
        {
            if (!string.IsNullOrEmpty(asign.rol) && !string.IsNullOrEmpty(asign.persona))
                texto += $"<b>{asign.rol}:</b> {asign.persona}\n";
        }

        texto += "\n<b>Dirección y narrativa:</b> Mariana Parra Hernández\n";
        texto += "<b>Testing y QA:</b> Los cuatro integrantes\n";
        texto += "<b>Agradecimientos especiales:</b> A nuestras familias y a todos los que apoyaron el proyecto.\n\n";
        texto += creditosModelos;
        texto += creditosDesarrollo;
        texto += "\n<size=20>Has acompañado a Lumen y Vyre a unirse de nuevo.</size>\n";
        texto += "<b><size=24>¡Gracias por jugar!</size></b>\n";

        return texto;
    }

    IEnumerator SecuenciaCreditos()
    {
        yield return new WaitForSeconds(tiempoInicialEspera);

        // Scroll basado en tiempo (garantiza que termine)
        float alturaContenido = contenido.rect.height;
        float distanciaTotal = alturaContenido + Screen.height;
        float tiempoTotal = distanciaTotal / velocidadScroll;
        float tiempoScroll = 0f;

        while (tiempoScroll < tiempoTotal)
        {
            tiempoScroll += Time.deltaTime;
            contenido.anchoredPosition += Vector2.up * velocidadScroll * Time.deltaTime;
            yield return null;
        }

        // Pequeña pausa antes del fade
        yield return new WaitForSeconds(0.5f);

        // Fade out de la pantalla
        yield return StartCoroutine(FadeOutPantalla());

        // Desvanecer música
        if (musicaCreditos != null)
        {
            float tiempo = 0;
            float volInicial = musicaCreditos.volume;
            while (tiempo < duracionFadeMusica)
            {
                tiempo += Time.deltaTime;
                musicaCreditos.volume = Mathf.Lerp(volInicial, 0, tiempo / duracionFadeMusica);
                yield return null;
            }
            musicaCreditos.Stop();
        }

        // Mostrar logo y esperar
        if (logoFinal != null)
        {
            logoFinal.gameObject.SetActive(true);
            // Opcional: efecto fade in del logo
            Color c = logoFinal.color;
            c.a = 0;
            logoFinal.color = c;
            float fadeLogo = 0.5f;
            float t = 0;
            while (t < fadeLogo)
            {
                t += Time.deltaTime;
                c.a = Mathf.Lerp(0, 1, t / fadeLogo);
                logoFinal.color = c;
                yield return null;
            }
            logoFinal.color = new Color(c.r, c.g, c.b, 1);

            yield return new WaitForSeconds(tiempoLogo);
        }
        else
        {
            // Si no hay logo, esperar 2 segundos
            yield return new WaitForSeconds(2f);
        }

        // Cargar menú
        SceneManager.LoadScene("menu");
    }

    IEnumerator FadeOutPantalla()
    {
        GameObject fadeObj = new GameObject("FadeOutCreditos");
        fadeObj.transform.SetParent(FindObjectOfType<Canvas>().transform, false);
        Image fadeImage = fadeObj.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0);
        fadeImage.raycastTarget = false;

        RectTransform rect = fadeImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        float tiempo = 0;
        while (tiempo < duracionFadeOut)
        {
            tiempo += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, tiempo / duracionFadeOut);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        fadeImage.color = Color.black;
    }

    IEnumerator ImagenesFadeIn()
    {
        foreach (Image img in imagenesDesarrollo)
        {
            if (img != null)
            {
                img.gameObject.SetActive(true);
                Color c = img.color;
                c.a = 0;
                img.color = c;
                float tiempo = 0;
                while (tiempo < 1f)
                {
                    tiempo += Time.deltaTime;
                    c.a = Mathf.Lerp(0, 1, tiempo);
                    img.color = c;
                    yield return null;
                }
                img.color = new Color(c.r, c.g, c.b, 1);
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}