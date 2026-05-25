using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class LumenController : MonoBehaviour
{
    private MovimientoPJ movimiento;

    [Header("Pistas")]
    public GameObject[] pistas;
    public float duracionVision = 3f;
    private bool viendoPistas = false;

    [Header("Animación")]
    public Animator animatorLumen;
    public string parametroRevelar = "Revelar";
    public float duracionAnimacion = 1.5f;

    [Header("Partículas (desde la mano)")]
    public Transform puntoMano;
    public ParticleSystem particulasRevelar;
    public bool detenerParticulasAlFinal = true;

    [Header("Sonido")]
    public AudioSource audioSource;
    public AudioClip sonidoRevelar;
    public float volumenSonido = 1f;

    private Coroutine corrutinaActual;
    private Vector3 posicionOriginalParticulas;
    private Quaternion rotacionOriginalParticulas;

    void Start()
    {
        movimiento = GetComponent<MovimientoPJ>();

        if (animatorLumen == null)
            animatorLumen = GetComponent<Animator>();

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && sonidoRevelar != null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }
        if (audioSource != null)
            audioSource.volume = volumenSonido;

        if (particulasRevelar != null)
        {
            posicionOriginalParticulas = particulasRevelar.transform.position;
            rotacionOriginalParticulas = particulasRevelar.transform.rotation;
            particulasRevelar.Stop();
            // Asegurar que no esté activo al inicio
            particulasRevelar.Clear();
        }

        foreach (GameObject pista in pistas)
            if (pista != null) pista.SetActive(false);
    }

    void OnEnable()
    {
        // Al reactivar el personaje, asegurarse de que todo esté apagado
        if (particulasRevelar != null)
        {
            particulasRevelar.Stop();
            particulasRevelar.Clear();
        }
        if (animatorLumen != null)
            animatorLumen.SetBool(parametroRevelar, false);
        viendoPistas = false;
        if (corrutinaActual != null)
            StopCoroutine(corrutinaActual);
    }

    void OnDisable()
    {
        // Al desactivar el personaje, limpiar todo
        if (particulasRevelar != null)
        {
            particulasRevelar.Stop();
            particulasRevelar.Clear();
        }
        if (animatorLumen != null)
            animatorLumen.SetBool(parametroRevelar, false);
        viendoPistas = false;
        if (corrutinaActual != null)
            StopCoroutine(corrutinaActual);
    }

    void Update()
    {
        if (Keyboard.current.qKey.wasPressedThisFrame && !viendoPistas && gameObject.activeInHierarchy)
        {
            if (corrutinaActual != null)
                StopCoroutine(corrutinaActual);
            corrutinaActual = StartCoroutine(RevelarYMostrarPistas());
        }

        if (movimiento != null)
            movimiento.velocidadMovimiento = movimiento.velocidadNormal;
    }

    IEnumerator RevelarYMostrarPistas()
    {
        viendoPistas = true;

        // Mover partículas a la mano
        if (puntoMano != null && particulasRevelar != null)
        {
            particulasRevelar.transform.position = puntoMano.position;
            particulasRevelar.transform.rotation = puntoMano.rotation;
        }

        // Sonido
        if (audioSource != null && sonidoRevelar != null)
            audioSource.PlayOneShot(sonidoRevelar, volumenSonido);

        // Partículas
        if (particulasRevelar != null)
        {
            particulasRevelar.Stop();
            particulasRevelar.Clear();
            particulasRevelar.Play();
        }

        // Animación
        if (animatorLumen != null)
            animatorLumen.SetBool(parametroRevelar, true);

        // Pistas
        foreach (GameObject pista in pistas)
            if (pista != null) pista.SetActive(true);

        yield return new WaitForSeconds(duracionAnimacion);

        // Desactivar animación
        if (animatorLumen != null)
            animatorLumen.SetBool(parametroRevelar, false);

        // Detener partículas si se desea
        if (particulasRevelar != null && detenerParticulasAlFinal)
            particulasRevelar.Stop();

        float tiempoRestante = duracionVision - duracionAnimacion;
        if (tiempoRestante > 0)
            yield return new WaitForSeconds(tiempoRestante);

        // Ocultar pistas
        foreach (GameObject pista in pistas)
            if (pista != null) pista.SetActive(false);

        viendoPistas = false;
        corrutinaActual = null;
    }
}