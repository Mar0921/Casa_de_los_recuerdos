using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class FinalNivel : MonoBehaviour
{
    [Header("Puerta")]
    public string nombreLlaveRequerida = "Llave";
    public float anguloApertura = 90f;
    public float velocidadApertura = 5f;
    public AudioSource audioSource;
    public AudioClip sonidoPuertaAbrir;
    public AudioClip sonidoSinLlave;

    [Header("Rotación Manual (opcional)")]
    public bool usarRotacionManual = false;
    public Vector3 rotacionCerradaManual = new Vector3(90, 0, 180);
    public Vector3 rotacionAbiertaManual = new Vector3(90, 0, -90);

    [Header("Carga de escena")]
    public bool cargarEscenaAlAbrir = false;

    [Header("Luces")]
    public Light[] luces;
    public float intensidadMax = 3f;
    public float velocidadTransicion = 5f;

    [Header("Imagen en el mundo")]
    public Renderer imagenMundo;
    public float delayImagen = 0.3f;
    public float velocidadFade = 3f;

    [Header("Siguiente escena")]
    public string nombreEscena;
    public float tiempoAntesDeCargar = 1f;
    public float duracionFadeOut = 1f;

    private bool estaAbierta = false;
    private bool estaAbriendose = false;
    private bool yaIntentoAbrir = false;
    private Quaternion rotacionCerrada;
    private Quaternion rotacionAbierta;

    private bool finalActivado = false;
    private bool imagenActivada = false;
    private float timerImagen = 0f;
    private float timerEscena = 0f;
    private Material mat;
    private bool usarTintColor = false;

    private GameObject jugador;
    private Renderer[] renderersJugador;
    private bool jugadorDentroTrigger = false;

    void Start()
    {
        if (usarRotacionManual)
        {
            rotacionCerrada = Quaternion.Euler(rotacionCerradaManual);
            rotacionAbierta = Quaternion.Euler(rotacionAbiertaManual);
        }
        else
        {
            rotacionCerrada = transform.rotation;
            rotacionAbierta = rotacionCerrada * Quaternion.Euler(0, -anguloApertura, 0);
        }

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        foreach (Light luz in luces)
        {
            luz.intensity = 0f;
            luz.enabled = true;
        }

        if (imagenMundo != null)
        {
            mat = imagenMundo.material;
            if (mat.HasProperty("_Color"))
            {
                Color c = mat.GetColor("_Color");
                c.a = 0f;
                mat.SetColor("_Color", c);
                usarTintColor = false;
            }
            else if (mat.HasProperty("_TintColor"))
            {
                Color c = mat.GetColor("_TintColor");
                c.a = 0f;
                mat.SetColor("_TintColor", c);
                usarTintColor = true;
            }
        }

        jugador = GameObject.FindGameObjectWithTag("Player");
        if (jugador != null)
            renderersJugador = jugador.GetComponentsInChildren<Renderer>();
    }

    void Update()
    {
        if (estaAbriendose)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionAbierta, Time.deltaTime * velocidadApertura);
            if (Quaternion.Angle(transform.rotation, rotacionAbierta) < 0.5f)
            {
                transform.rotation = rotacionAbierta;
                estaAbriendose = false;
                estaAbierta = true;
                Debug.Log("Puerta abierta");
            }
        }

        if (estaAbierta && jugadorDentroTrigger && !finalActivado && !cargarEscenaAlAbrir)
        {
            ActivarFinal();
        }

        if (finalActivado)
        {
            foreach (Light luz in luces)
                luz.intensity = Mathf.MoveTowards(luz.intensity, intensidadMax, velocidadTransicion * Time.deltaTime);

            if (renderersJugador != null)
            {
                foreach (Renderer rend in renderersJugador)
                {
                    if (rend.material != null)
                    {
                        Color c = rend.material.color;
                        c.a = Mathf.MoveTowards(c.a, 0f, Time.deltaTime * 1.5f);
                        rend.material.color = c;
                    }
                }
            }

            if (!imagenActivada)
            {
                timerImagen += Time.deltaTime;
                if (timerImagen >= delayImagen)
                    imagenActivada = true;
            }

            if (imagenActivada && mat != null)
            {
                Color col;
                if (usarTintColor)
                    col = mat.GetColor("_TintColor");
                else
                    col = mat.GetColor("_Color");

                col.a = Mathf.MoveTowards(col.a, 1f, velocidadFade * Time.deltaTime);

                if (usarTintColor)
                    mat.SetColor("_TintColor", col);
                else
                    mat.SetColor("_Color", col);

                if (col.a >= 0.99f)
                {
                    timerEscena += Time.deltaTime;
                    if (timerEscena >= tiempoAntesDeCargar)
                    {
                        StartCoroutine(FadeOutYCargarEscena());
                        finalActivado = false;
                    }
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorDentroTrigger = true;

            if (!estaAbierta && !estaAbriendose && !yaIntentoAbrir)
                IntentarAbrirPuerta();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            jugadorDentroTrigger = false;
    }

    void ActivarFinal()
    {
        finalActivado = true;
        Debug.Log("Final activado");

        if (jugador != null)
        {
            MovimientoPJ mov = jugador.GetComponent<MovimientoPJ>();
            if (mov != null) mov.enabled = false;

            VyreController vyre = jugador.GetComponent<VyreController>();
            if (vyre != null) vyre.enabled = false;
        }
    }

    void IntentarAbrirPuerta()
    {
        yaIntentoAbrir = true;

        if (TieneJugadorLlave())
        {
            estaAbriendose = true;
            if (sonidoPuertaAbrir != null)
                audioSource.PlayOneShot(sonidoPuertaAbrir);
            ConsumirLlave();

            if (cargarEscenaAlAbrir)
                StartCoroutine(EsperarYCargarEscena());

            Debug.Log("Puerta abriéndose");
        }
        else
        {
            if (sonidoSinLlave != null)
                audioSource.PlayOneShot(sonidoSinLlave);
            Debug.Log("❌ Necesitas la llave para abrir esta puerta");
            Invoke(nameof(ReintentarAbrir), 2f);
        }
    }

    void ReintentarAbrir()
    {
        yaIntentoAbrir = false;
    }

    bool TieneJugadorLlave()
    {
        if (Inventario.instancia == null) return false;
        foreach (ObjetoRecolectable obj in Inventario.instancia.ObtenerObjetos())
            if (obj.nombreObjeto == nombreLlaveRequerida) return true;
        return false;
    }

    void ConsumirLlave()
    {
        if (Inventario.instancia == null) return;
        foreach (ObjetoRecolectable obj in Inventario.instancia.ObtenerObjetos())
        {
            if (obj.nombreObjeto == nombreLlaveRequerida)
            {
                Inventario.instancia.QuitarObjeto(obj);
                break;
            }
        }
    }

    IEnumerator EsperarYCargarEscena()
    {
        yield return new WaitForSeconds(tiempoAntesDeCargar);
        SceneManager.LoadScene(nombreEscena);
    }

    IEnumerator FadeOutYCargarEscena()
    {
        GameObject fadeCanvas = new GameObject("FadeCanvas");
        Canvas canvas = fadeCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        UnityEngine.UI.Image fadeImage = fadeCanvas.AddComponent<UnityEngine.UI.Image>();
        fadeImage.color = Color.black;
        fadeImage.raycastTarget = false;

        Color color = fadeImage.color;
        color.a = 0f;
        fadeImage.color = color;

        float tiempo = 0f;
        while (tiempo < duracionFadeOut)
        {
            tiempo += Time.deltaTime;
            color.a = Mathf.Clamp01(tiempo / duracionFadeOut);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color;

        yield return new WaitForSeconds(0.1f);
        SceneManager.LoadScene(nombreEscena);
    }

    void OnDrawGizmos()
    {
        BoxCollider triggerCol = GetComponentInChildren<BoxCollider>();
        if (triggerCol != null && triggerCol.isTrigger)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawCube(triggerCol.bounds.center, triggerCol.bounds.size);
        }
    }
}