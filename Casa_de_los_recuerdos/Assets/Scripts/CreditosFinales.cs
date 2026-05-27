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
    [Tooltip("Píxeles adicionales que sube el contenido después de salir de la pantalla")]
    public float extraScroll = 200f;   // NUEVO: ajustable desde Inspector

    [Header("Referencias UI")]
    public RectTransform contenido;
    public Image[] imagenesDesarrollo;
    public TMP_Text textoCreditos;

    [Header("Logo Final")]
    public Image logoFinal;
    public float tiempoLogo = 5f;

    [Header("Música")]
    public AudioSource musicaCreditos;
    public float volumenMusica = 0.5f;

    [Header("Créditos adicionales")]
    [TextArea(5, 10)]
    public string creditosModelos = " MODELOS 3D DE TERCEROS:\n" +
        "- 'Abandoned House' de 3D_Avenue (CGTrader)\n" +
        "- 'Victorian Furniture Pack' de DarioSanchez (Sketchfab)\n" +
        "- 'Low Poly Characters' de Quaternius (modificados por el equipo)\n" +
        "- Animaciones de Mixamo (Adobe)\n\n" +
        " HERRAMIENTAS UTILIZADAS:\n" +
        "- Unity 2021.3 LTS\n" +
        "- Blender 3.0\n" +
        "- Photoshop / GIMP\n" +
        "- Audacity\n";

    [TextArea(5, 10)]
    public string creditosDesarrollo = " PROCESO DE DESARROLLO (Basado en GDD 'La Casa de los Recuerdos'):\n" +
        "- Diseño narrativo: Documento de 30 páginas con estructura Inicio-Nudo-Desenlace.\n" +
        "- Prototipado: 3 iteraciones de mecánicas de luz/sombra con Lumen y Vyre.\n" +
        "- Niveles: Cocina (tutorial), Sala (puzzle de muebles), Habitación niño (objetos oscuros), Habitación hermana (tentáculos + fusión).\n" +
        "- Implementación de IA de persecución (Sombra) con sistema de ruido.\n" +
        "- Sistema de objetos coleccionables (revelados con luz Lumen).\n" +
        "- Transiciones de escena con fade blanco/negro y animación 'MeLevante'.\n" +
        "- Testing interno: 15+ horas de juego para ajustar dificultad y ritmo.\n";

    void Start()
    {
        if (logoFinal != null)
            logoFinal.gameObject.SetActive(false);

        string textoCredito = GenerarTextoCreditos();
        if (textoCreditos != null)
            textoCreditos.text = textoCredito;

        if (contenido != null)
        {
            contenido.anchoredPosition = new Vector2(contenido.anchoredPosition.x, -Screen.height * 0.5f);
        }

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
        texto += "<size=22><i>Donde tus miedos toman forma</i></size>\n\n";

        // Equipo de desarrollo MAGO
        texto += "<b>Equipo de desarrollo MAGO:</b><br>\n";
        texto += "• Daniel Muñoz Delgado\n";
        texto += "• Estefania del Amor Restrepo\n";
        texto += "• Juan Felipe Fernandez Losada\n";
        texto += "• Mariana Parra Hernández\n\n";

        texto += "<b>Dirección y narrativa:</b> Mariana Parra Hernández\n";
        texto += "<b>Testing y QA:</b> Los cuatro integrantes\n";
        texto += "<b>Agradecimientos especiales:</b> A nuestras familias y a todos los que apoyaron el proyecto.<br>\n\n";
        texto += creditosModelos;
        texto += creditosDesarrollo;
        texto += "\n<size=28>Has acompañado a Lumen y Vyre a unirse de nuevo.</size>\n";
        texto += "<b><size=26>¡Gracias por jugar!</size></b>\n";

        return texto;
    }

    IEnumerator SecuenciaCreditos()
    {
        yield return new WaitForSeconds(tiempoInicialEspera);

        float alturaContenido = contenido.rect.height;
        // Sumamos la pantalla completa y un extra para que suba más
        float distanciaTotal = alturaContenido + Screen.height + extraScroll;
        float tiempoTotal = distanciaTotal / velocidadScroll;
        float tiempoScroll = 0f;

        while (tiempoScroll < tiempoTotal)
        {
            tiempoScroll += Time.deltaTime;
            contenido.anchoredPosition += Vector2.up * velocidadScroll * Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(FadeOutPantalla());

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

        if (logoFinal != null)
        {
            logoFinal.gameObject.SetActive(true);
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
            yield return new WaitForSeconds(2f);
        }

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