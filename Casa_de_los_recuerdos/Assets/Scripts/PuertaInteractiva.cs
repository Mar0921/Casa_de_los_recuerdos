using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;

public class PuertaInteractiva : MonoBehaviour
{
    [Header("Configuración General")]
    public string tagJugador = "Player";
    public float distanciaActivacion = 3f;

    [Header("Tipo de Puerta")]
    public bool puertaBloqueada = false;  // true = bloqueada, false = normal

    [Header("Mensajes")]
    [TextArea(2, 4)]
    public string mensajeNormal = "Presiona K para abrir";
    [TextArea(2, 4)]
    public string mensajeBloqueada = "Puerta bloqueada. No se puede abrir";

    [Header("UI - TextMeshPro (Asignar desde Inspector)")]
    public TMP_Text textoMensaje;

    [Header("Rotación de Puerta")]
    public Vector3 rotacionApertura = new Vector3(0, 90f, 0);
    public float velocidadRotacion = 2f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip sonidoAbrir;
    public AudioClip sonidoBloqueada;
    public float volumenSonido = 1f;

    // Variables internas
    private GameObject jugador;
    private bool jugadorCerca = false;
    private bool puertaAbierta = false;
    private Quaternion rotacionCerrada;
    private Quaternion rotacionAbierta;

    void Start()
    {
        // Buscar al jugador
        jugador = GameObject.FindGameObjectWithTag(tagJugador);

        // Guardar rotaciones
        rotacionCerrada = transform.rotation;
        rotacionAbierta = rotacionCerrada * Quaternion.Euler(rotacionApertura);

        // Configurar audio
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && (sonidoAbrir != null || sonidoBloqueada != null))
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        if (audioSource != null)
        {
            audioSource.volume = volumenSonido;
            audioSource.playOnAwake = false;
        }

        // Verificar TextMeshPro
        if (textoMensaje == null)
        {
            Debug.LogWarning("PuertaInteractiva: No se asignó el TextMeshPro en " + gameObject.name);
        }
        else
        {
            textoMensaje.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (puertaAbierta) return;

        if (jugador != null)
        {
            float distancia = Vector3.Distance(transform.position, jugador.transform.position);
            jugadorCerca = distancia <= distanciaActivacion;

            if (jugadorCerca && !puertaAbierta)
            {
                ActualizarMensaje();
                MostrarMensaje(true);
            }
            else if (!jugadorCerca && !puertaAbierta)
            {
                MostrarMensaje(false);
            }

            if (jugadorCerca && !puertaAbierta && Keyboard.current.kKey.wasPressedThisFrame)
            {
                Interactuar();
            }
        }
    }

    void ActualizarMensaje()
    {
        if (textoMensaje == null) return;

        if (puertaBloqueada)
        {
            textoMensaje.text = mensajeBloqueada;
        }
        else
        {
            textoMensaje.text = mensajeNormal;
        }
    }

    void Interactuar()
    {
        // Si la puerta está bloqueada, solo muestra mensaje y suena, NO se abre
        if (puertaBloqueada)
        {
            if (textoMensaje != null)
            {
                textoMensaje.text = mensajeBloqueada;
                Invoke(nameof(OcultarMensaje), 2f);
            }

            if (audioSource != null && sonidoBloqueada != null)
            {
                audioSource.PlayOneShot(sonidoBloqueada, volumenSonido);
            }

            Debug.Log("Puerta bloqueada - No se puede abrir");
            return;
        }

        // 🔴 Puerta normal - ocultar mensaje IMMEDIATAMENTE y abrir
        MostrarMensaje(false);  // Esto oculta el mensaje al instante
        AbrirPuerta();
    }

    void OcultarMensaje()
    {
        if (textoMensaje != null && !puertaAbierta)
        {
            textoMensaje.gameObject.SetActive(false);
        }
    }

    void MostrarMensaje(bool mostrar)
    {
        if (textoMensaje != null && !puertaAbierta)
        {
            textoMensaje.gameObject.SetActive(mostrar);
        }
    }

    void AbrirPuerta()
    {
        puertaAbierta = true;

        if (audioSource != null && sonidoAbrir != null)
        {
            audioSource.PlayOneShot(sonidoAbrir, volumenSonido);
        }

        StartCoroutine(AbrirPuertaRotacion());
    }

    IEnumerator AbrirPuertaRotacion()
    {
        float tiempo = 0f;

        while (tiempo < 1f)
        {
            tiempo += Time.deltaTime * velocidadRotacion;
            transform.rotation = Quaternion.Slerp(rotacionCerrada, rotacionAbierta, tiempo);
            yield return null;
        }

        transform.rotation = rotacionAbierta;

        Collider puertaCollider = GetComponent<Collider>();
        if (puertaCollider != null)
        {
            puertaCollider.enabled = false;
        }
    }

    // Método para cambiar el estado de la puerta desde otro script
    public void SetPuertaBloqueada(bool bloqueada)
    {
        puertaBloqueada = bloqueada;

        if (jugadorCerca)
        {
            ActualizarMensaje();
        }
    }

    // Resetear puerta
    public void ResetearPuerta()
    {
        puertaAbierta = false;
        jugadorCerca = false;
        transform.rotation = rotacionCerrada;

        Collider puertaCollider = GetComponent<Collider>();
        if (puertaCollider != null && !puertaCollider.enabled)
        {
            puertaCollider.enabled = true;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, distanciaActivacion);
    }
}