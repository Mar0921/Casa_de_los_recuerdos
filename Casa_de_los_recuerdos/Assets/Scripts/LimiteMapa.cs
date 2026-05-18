using UnityEngine;
using System.Collections;
using TMPro;

public class LimiteMapa : MonoBehaviour
{
    [Header("Configuración de Oscurecimiento")]
    public float tiempoParaActivacion = 1f;        // Tiempo que debe estar en el límite para activarse
    public float duracionTransicion = 0.5f;        // Suavizado del oscurecimiento
    public float intensidadMaximaOscuridad = 0.6f; // 0 = nada, 1 = completamente negro

    [Header("UI - Asignar en Inspector")]
    public Canvas canvasUI;                        // Asigna tu Canvas aquí
    public TMP_Text textoMensaje;                  // Asigna tu TextMeshPro aquí (él ya tiene su texto)
    public UnityEngine.UI.Image panelOscurecimiento; // Asigna la imagen del panel oscuro aquí

    // Variables internas
    private bool estaEnLimite = false;
    private float tiempoEnLimite = 0f;
    private float intensidadActual = 0f;
    private bool efectoActivo = false;
    private Color colorOriginalPanel;

    void Start()
    {
        // Verificar referencias
        if (canvasUI == null)
        {
            Debug.LogError("LimiteMapa: No se asignó el Canvas. Por favor, asígnalo en el Inspector.");
        }

        if (textoMensaje == null)
        {
            Debug.LogError("LimiteMapa: No se asignó el TextMeshPro. Por favor, asígnalo en el Inspector.");
        }
        else
        {
            // Solo ocultar el mensaje al inicio, NO modificar su texto
            textoMensaje.gameObject.SetActive(false);
        }

        if (panelOscurecimiento == null)
        {
            Debug.LogError("LimiteMapa: No se asignó el panel de oscurecimiento. Por favor, asígnalo en el Inspector.");
        }
        else
        {
            colorOriginalPanel = panelOscurecimiento.color;
            panelOscurecimiento.color = new Color(colorOriginalPanel.r, colorOriginalPanel.g, colorOriginalPanel.b, 0f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            estaEnLimite = true;
            tiempoEnLimite = 0f;

            // Iniciar rutina de conteo
            if (rutinaOscurecimiento != null)
                StopCoroutine(rutinaOscurecimiento);
            rutinaOscurecimiento = StartCoroutine(ContarTiempoEnLimite());
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            estaEnLimite = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            estaEnLimite = false;
            tiempoEnLimite = 0f;

            if (rutinaOscurecimiento != null)
                StopCoroutine(rutinaOscurecimiento);

            StartCoroutine(DesvanecerOscuridad());
        }
    }

    private Coroutine rutinaOscurecimiento;

    IEnumerator ContarTiempoEnLimite()
    {
        float tiempoTranscurrido = 0f;

        while (estaEnLimite && tiempoTranscurrido < tiempoParaActivacion)
        {
            tiempoTranscurrido += Time.deltaTime;
            tiempoEnLimite = tiempoTranscurrido;

            // Pre-oscurecimiento muy leve (opcional)
            if (panelOscurecimiento != null && !efectoActivo)
            {
                float preIntensidad = Mathf.Lerp(0f, intensidadMaximaOscuridad * 0.3f, tiempoTranscurrido / tiempoParaActivacion);
                panelOscurecimiento.color = new Color(colorOriginalPanel.r, colorOriginalPanel.g, colorOriginalPanel.b, preIntensidad);
            }

            yield return null;
        }

        if (estaEnLimite && !efectoActivo)
        {
            ActivarEfectoCompleto();
        }
    }

    void ActivarEfectoCompleto()
    {
        efectoActivo = true;
        StartCoroutine(TransicionOscuridad(0f, intensidadMaximaOscuridad, duracionTransicion));
        MostrarMensaje();

        Debug.Log("Jugador alcanzó el límite del mapa");
    }

    IEnumerator TransicionOscuridad(float desde, float hasta, float duracion)
    {
        if (panelOscurecimiento == null) yield break;

        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float alpha = Mathf.Lerp(desde, hasta, tiempo / duracion);
            panelOscurecimiento.color = new Color(colorOriginalPanel.r, colorOriginalPanel.g, colorOriginalPanel.b, alpha);
            intensidadActual = alpha;
            yield return null;
        }

        panelOscurecimiento.color = new Color(colorOriginalPanel.r, colorOriginalPanel.g, colorOriginalPanel.b, hasta);
        intensidadActual = hasta;
    }

    IEnumerator DesvanecerOscuridad()
    {
        efectoActivo = false;

        if (panelOscurecimiento != null)
        {
            yield return StartCoroutine(TransicionOscuridad(intensidadActual, 0f, duracionTransicion));
        }

        // Ocultar mensaje
        if (textoMensaje != null && textoMensaje.gameObject.activeSelf)
        {
            textoMensaje.gameObject.SetActive(false);
        }
    }

    void MostrarMensaje()
    {
        if (textoMensaje != null)
        {
            // NO modificamos el texto, solo lo mostramos
            textoMensaje.gameObject.SetActive(true);
            Debug.Log("Mensaje mostrado");
        }
    }

    // Método público para resetear manualmente
    public void ResetearLimite()
    {
        estaEnLimite = false;
        tiempoEnLimite = 0f;
        efectoActivo = false;

        if (rutinaOscurecimiento != null)
            StopCoroutine(rutinaOscurecimiento);

        if (panelOscurecimiento != null)
        {
            panelOscurecimiento.color = new Color(colorOriginalPanel.r, colorOriginalPanel.g, colorOriginalPanel.b, 0f);
            intensidadActual = 0f;
        }

        if (textoMensaje != null)
            textoMensaje.gameObject.SetActive(false);
    }
}