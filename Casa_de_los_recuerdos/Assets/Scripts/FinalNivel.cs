using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalNivel : MonoBehaviour
{
    [Header("Puerta")]
    public string nombreLlaveRequerida = "Llave";
    public float anguloApertura = 90f;
    public float velocidadApertura = 2f;
    public AudioSource audioSource;
    public AudioClip sonidoPuertaAbrir;
    public AudioClip sonidoSinLlave;

    [Header("Luces")]
    public Light[] luces;
    public float intensidadMax = 3f;
    public float velocidadTransicion = 1.5f;

    [Header("Imagen en el mundo")]
    public Renderer imagenMundo;
    public float delayImagen = 2f;
    public float velocidadFade = 1f;

    [Header("Siguiente escena")]
    public string nombreEscena;
    public float tiempoAntesDeCargar = 5f;

    private bool estaAbierta = false;
    private bool estaAbriendose = false;
    private Quaternion rotacionCerrada;
    private Quaternion rotacionAbierta;

    private bool finalActivado = false;
    private bool imagenActivada = false;
    private float timerImagen = 0f;
    private float timerEscena = 0f;
    private Material mat;

    private GameObject jugador;
    private Renderer rendererJugador;
    private bool jugadorDesapareciendo = false;

    void Start()
    {
        rotacionCerrada = transform.rotation;
        rotacionAbierta = rotacionCerrada * Quaternion.Euler(0, -anguloApertura, 0);

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
            Color c = mat.color;
            c.a = 0f;
            mat.color = c;
        }

        jugador = GameObject.FindGameObjectWithTag("Player");
        if (jugador != null)
            rendererJugador = jugador.GetComponent<Renderer>();
    }

    void Update()
    {
        // Abrir puerta
        if (estaAbriendose)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionAbierta, Time.deltaTime * velocidadApertura);
            if (Quaternion.Angle(transform.rotation, rotacionAbierta) < 0.5f)
            {
                transform.rotation = rotacionAbierta;
                estaAbriendose = false;
                estaAbierta = true;
            }
        }

        if (!estaAbierta && Input.GetKeyDown(KeyCode.P))
            IntentarAbrirPuerta();

        // Final activado: luces, imagen, desvanecimiento jugador y cambio de escena
        if (finalActivado)
        {
            // Prender luces suavemente
            foreach (Light luz in luces)
                luz.intensity = Mathf.MoveTowards(luz.intensity, intensidadMax, velocidadTransicion * Time.deltaTime);

            // Desvanecer jugador
            if (jugadorDesapareciendo && rendererJugador != null)
            {
                Color colorJugador = rendererJugador.material.color;
                colorJugador.a = Mathf.MoveTowards(colorJugador.a, 0f, Time.deltaTime * 0.5f);
                rendererJugador.material.color = colorJugador;
            }

            // Delay antes de la imagen
            if (!imagenActivada)
            {
                timerImagen += Time.deltaTime;
                if (timerImagen >= delayImagen)
                    imagenActivada = true;
            }

            // Fade in imagen y cambio de escena
            if (imagenActivada && mat != null)
            {
                Color colorImagen = mat.color;
                colorImagen.a = Mathf.MoveTowards(colorImagen.a, 1f, velocidadFade * Time.deltaTime);
                mat.color = colorImagen;

                Debug.Log("Alpha imagen: " + colorImagen.a);

                if (colorImagen.a >= 1f)
                {
                    timerEscena += Time.deltaTime;
                    Debug.Log("Timer escena: " + timerEscena);
                    if (timerEscena >= tiempoAntesDeCargar)
                        SceneManager.LoadScene(nombreEscena);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger tocado por: " + other.name + " | estaAbierta: " + estaAbierta);
        if (other.CompareTag("Player") && estaAbierta && !finalActivado)
        {
            finalActivado = true;
            jugadorDesapareciendo = true;
            Debug.Log("Final activado");
            MovimientoPJ mov = other.GetComponent<MovimientoPJ>();
            if (mov != null) mov.enabled = false;
        }
    }

    void IntentarAbrirPuerta()
    {
        if (TieneJugadorLlave())
        {
            estaAbriendose = true;
            if (sonidoPuertaAbrir != null) audioSource.PlayOneShot(sonidoPuertaAbrir);
            Debug.Log("Puerta abierta");
        }
        else
        {
            if (sonidoSinLlave != null) audioSource.PlayOneShot(sonidoSinLlave);
            Debug.Log("Necesitas una llave");
        }
    }

    bool TieneJugadorLlave()
    {
        if (Inventario.instancia == null) return false;
        foreach (ObjetoRecolectable obj in Inventario.instancia.ObtenerObjetos())
            if (obj.nombreObjeto == nombreLlaveRequerida) return true;
        return false;
    }
}