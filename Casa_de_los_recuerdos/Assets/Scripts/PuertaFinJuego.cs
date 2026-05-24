using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class PuertaFinJuego : MonoBehaviour
{
    [Header("Configuración")]
    public string tagJugador = "Player";
    public float distanciaActivacion = 3f;

    [Header("Puerta Física")]
    public GameObject puerta;
    public bool desactivarPuerta = false; // CAMBIADO a false por defecto
    public Vector3 rotacionApertura = new Vector3(0, -90f, 0);
    public float velocidadApertura = 2f;

    [Header("UI - TextMeshPro")]
    public TMP_Text textoMensaje;
    public string mensaje = "Presiona K para salir";

    [Header("Cámara - Punto final (Opcional)")]
    public Transform puntoCamaraFinal;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip sonidoMusicaFinal;
    public float volumenMusica = 0.7f;
    public float tiempoEsperaMusica = 2f;

    [Header("Fade Out")]
    public float duracionFadeOut = 2f;
    public Color colorFade = Color.black;

    [Header("Escena de Créditos")]
    public string nombreEscenaCreditos = "Creditos";

    private GameObject jugador;
    private bool jugadorCerca = false;
    private bool puertaAbierta = false;
    private Camera camaraPrincipal;
    private Quaternion rotacionInicialPuerta;

    void Start()
    {
        jugador = GameObject.FindGameObjectWithTag(tagJugador);
        camaraPrincipal = Camera.main;

        if (textoMensaje != null)
            textoMensaje.gameObject.SetActive(false);

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (puerta == null)
            puerta = gameObject;

        if (puerta != null)
            rotacionInicialPuerta = puerta.transform.rotation;
    }

    void Update()
    {
        if (puertaAbierta) return;
        if (jugador == null || camaraPrincipal == null) return;

        float distancia = Vector3.Distance(transform.position, jugador.transform.position);
        jugadorCerca = distancia <= distanciaActivacion;

        if (textoMensaje != null)
        {
            textoMensaje.gameObject.SetActive(jugadorCerca && !puertaAbierta);
            if (jugadorCerca && !puertaAbierta)
                textoMensaje.text = mensaje;
        }

        if (jugadorCerca && !puertaAbierta && Keyboard.current.kKey.wasPressedThisFrame)
            AbrirPuerta();
    }

    void AbrirPuerta()
    {
        puertaAbierta = true;

        if (textoMensaje != null)
            textoMensaje.gameObject.SetActive(false);

        DesactivarControlJugador(true);
        StartCoroutine(AbrirPuertaFisica());

        if (puntoCamaraFinal != null && camaraPrincipal != null)
            StartCoroutine(MoverCamaraFinal());
        else
            StartCoroutine(EsperarYContinuar());
    }

    IEnumerator AbrirPuertaFisica()
    {
        if (puerta == null) yield break;

        if (desactivarPuerta)
        {
            puerta.SetActive(false);
        }
        else
        {
            Quaternion rotacionDestino = rotacionInicialPuerta * Quaternion.Euler(rotacionApertura);
            float tiempo = 0f;
            while (tiempo < 1f)
            {
                tiempo += Time.deltaTime * velocidadApertura;
                if (puerta != null)
                    puerta.transform.rotation = Quaternion.Slerp(rotacionInicialPuerta, rotacionDestino, tiempo);
                yield return null;
            }
            if (puerta != null)
                puerta.transform.rotation = rotacionDestino;
        }

        // Desactivar collider para que el jugador pueda pasar
        Collider col = puerta.GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }

    IEnumerator MoverCamaraFinal()
    {
        if (camaraPrincipal == null || puntoCamaraFinal == null) yield break;

        Vector3 posInicial = camaraPrincipal.transform.position;
        Quaternion rotInicial = camaraPrincipal.transform.rotation;
        float duracion = 1f;
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, tiempo / duracion);
            camaraPrincipal.transform.position = Vector3.Lerp(posInicial, puntoCamaraFinal.position, t);
            camaraPrincipal.transform.rotation = Quaternion.Slerp(rotInicial, puntoCamaraFinal.rotation, t);
            yield return null;
        }

        camaraPrincipal.transform.position = puntoCamaraFinal.position;
        camaraPrincipal.transform.rotation = puntoCamaraFinal.rotation;

        yield return new WaitForSeconds(tiempoEsperaMusica);

        if (audioSource != null && sonidoMusicaFinal != null)
        {
            audioSource.clip = sonidoMusicaFinal;
            audioSource.volume = volumenMusica;
            audioSource.loop = true;
            audioSource.Play();
        }

        StartCoroutine(FadeOutYCargarCreditos());
    }

    IEnumerator EsperarYContinuar()
    {
        yield return new WaitForSeconds(tiempoEsperaMusica);

        if (audioSource != null && sonidoMusicaFinal != null)
        {
            audioSource.clip = sonidoMusicaFinal;
            audioSource.volume = volumenMusica;
            audioSource.Play();
        }

        StartCoroutine(FadeOutYCargarCreditos());
    }

    IEnumerator FadeOutYCargarCreditos()
    {
        GameObject canvasObj = new GameObject("CanvasFadeOut");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        GameObject imagenObj = new GameObject("ImagenFade");
        imagenObj.transform.SetParent(canvas.transform, false);
        UnityEngine.UI.Image imagenFade = imagenObj.AddComponent<UnityEngine.UI.Image>();
        imagenFade.color = new Color(colorFade.r, colorFade.g, colorFade.b, 0f);
        imagenFade.raycastTarget = false;

        RectTransform rect = imagenFade.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        float tiempo = 0f;
        while (tiempo < duracionFadeOut)
        {
            tiempo += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, tiempo / duracionFadeOut);
            imagenFade.color = new Color(colorFade.r, colorFade.g, colorFade.b, alpha);
            yield return null;
        }

        imagenFade.color = new Color(colorFade.r, colorFade.g, colorFade.b, 1f);
        SceneManager.LoadScene(nombreEscenaCreditos);
    }

    void DesactivarControlJugador(bool desactivar)
    {
        if (jugador == null) return;

        // Busca el script ProtaMovimiento (el que usas en esta escena)
        ProtaMovimiento movimiento = jugador.GetComponent<ProtaMovimiento>();
        if (movimiento != null)
        {
            movimiento.SetPuedeMover(!desactivar);
            return;
        }

        // Si no, busca otros posibles scripts (por si acaso)
        MovimientoPJ movLumen = jugador.GetComponent<MovimientoPJ>();
        if (movLumen != null)
        {
            movLumen.enabled = !desactivar;
            return;
        }

        VyreController movVyre = jugador.GetComponent<VyreController>();
        if (movVyre != null)
        {
            movVyre.enabled = !desactivar;
            return;
        }

        Debug.LogWarning("No se encontró script de movimiento en el jugador");
    }

    void OnDrawGizmosSelected()
    {
        // Rango de activación
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, distanciaActivacion);

        // Mostrar hacia dónde apuntará la cámara
        if (puntoCamaraFinal != null)
        {
            // Posición del punto
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(puntoCamaraFinal.position, 0.15f);

            // Línea desde el punto hasta donde mira
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(
                puntoCamaraFinal.position,
                puntoCamaraFinal.position + puntoCamaraFinal.forward * 2f
            );

            // Línea desde la puerta hasta el punto de cámara
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, puntoCamaraFinal.position);
        }
    }
}