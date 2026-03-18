using UnityEngine;

public class PuertaConLlave : MonoBehaviour
{
    [Header("Configuración Llave")]
    public string nombreLlaveRequerida = "Llave";
    public bool consumirLlave = true;

    [Header("Animación Puerta")]
    public float anguloApertura = 90f;
    public float velocidadApertura = 2f;

    [Header("Mensajes")]
    public string mensajeConLlave = "Presiona P para abrir la puerta";
    public string mensajeSinLlave = "Necesitas una llave para abrir esta puerta";

    [Header("Sonidos")]
    public AudioSource audioSource;
    public AudioClip sonidoPuertaAbrir;
    public AudioClip sonidoSinLlave; 

    private bool estaAbierta = false;
    private bool estaAbriendose = false;
    private Quaternion rotacionCerrada;
    private Quaternion rotacionAbierta;

    void Start()
    {
        rotacionCerrada = transform.rotation;
        rotacionAbierta = rotacionCerrada * Quaternion.Euler(0, -anguloApertura, 0);

        // Configurar AudioSource si no existe
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;

        ActualizarMensaje();
    }

    void Update()
    {
        if (estaAbriendose)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                rotacionAbierta,
                Time.deltaTime * velocidadApertura
            );

            if (Quaternion.Angle(transform.rotation, rotacionAbierta) < 0.5f)
            {
                transform.rotation = rotacionAbierta;
                estaAbriendose = false;
                estaAbierta = true;
            }
        }

        if (!estaAbierta && Input.GetKeyDown(KeyCode.P))
        {
            IntentarAbrirPuerta();
        }

        ActualizarMensaje();
    }

    void ActualizarMensaje()
    {
        ObjetoInteractuable interactuable = GetComponent<ObjetoInteractuable>();
        if (interactuable != null)
        {
            interactuable.mensaje = TieneJugadorLlave() ? mensajeConLlave : mensajeSinLlave;
        }
    }

    bool TieneJugadorLlave()
    {
        if (Inventario.instancia == null)
            return false;

        foreach (ObjetoRecolectable obj in Inventario.instancia.ObtenerObjetos())
        {
            if (obj.nombreObjeto == nombreLlaveRequerida)
                return true;
        }
        return false;
    }

    void IntentarAbrirPuerta()
    {
        ObjetoRecolectable llave = BuscarLlaveEnInventario();

        if (llave != null)
        {
            AbrirPuerta();

            if (consumirLlave)
            {
                Inventario.instancia.QuitarObjeto(llave);
                Destroy(llave.gameObject);
                Debug.Log($"Llave '{nombreLlaveRequerida}' consumida.");
            }
        }
        else
        {
            Debug.Log(mensajeSinLlave);

            // Reproducir sonido cuando no tiene llave (opcional)
            if (sonidoSinLlave != null && audioSource != null)
            {
                audioSource.PlayOneShot(sonidoSinLlave);
            }
        }
    }

    ObjetoRecolectable BuscarLlaveEnInventario()
    {
        if (Inventario.instancia == null)
            return null;

        foreach (ObjetoRecolectable obj in Inventario.instancia.ObtenerObjetos())
        {
            if (obj.nombreObjeto == nombreLlaveRequerida)
                return obj;
        }
        return null;
    }

    void AbrirPuerta()
    {
        Debug.Log("¡Puerta abierta!");
        estaAbriendose = true;

        if (sonidoPuertaAbrir != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoPuertaAbrir);
        }
    }
}