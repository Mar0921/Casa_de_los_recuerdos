using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class LuzPersonaje : MonoBehaviour
{
    [Header("Luz")]
    public Light luz;
    public float intensidadMax = 3f;
    public float duracionLuz = 6f;
    public float cooldown = 8f;
    public float velocidadTransicion = 2f;

    [Header("Animación")]
    public Animator animator;
    public float tiempoAnimacion = 1.5f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip sonidoLuz;

    [Header("Estrellas")]
    public float radioEstrellas = 5f;
    public float tiempoBrilloEstrella = 2f;

    [Header("Objetos")]
    public float tiempoParaRecoger = 1f;  // Tiempo que tarda en recogerse después de revelado

    private float tiempoRestante = 0f;
    private float tiempoCooldown = 0f;
    private float tiempoAnim = 0f;
    private bool luzActiva = false;
    private bool animActiva = false;
    private bool yaActivoEstrellas = false;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        luz.intensity = 0f;
        luz.enabled = false;

        if (animator != null)
            animator.SetBool("ilumine", false);
    }

    void Update()
    {
        // Activar luz
        if (Keyboard.current.lKey.wasPressedThisFrame && tiempoCooldown <= 0f && !luzActiva)
        {
            luzActiva = true;
            animActiva = true;
            tiempoRestante = duracionLuz;
            tiempoAnim = tiempoAnimacion;
            luz.enabled = true;
            yaActivoEstrellas = false;

            if (audioSource != null && sonidoLuz != null)
                audioSource.PlayOneShot(sonidoLuz);

            if (animator != null)
                animator.SetBool("ilumine", true);
        }

        // Activar estrellas y objetos (solo una vez al inicio de la luz)
        if (luzActiva && !yaActivoEstrellas)
        {
            ActivarEstrellas();
            ActivarObjetos();
            yaActivoEstrellas = true;
        }

        // Control animación
        if (animActiva)
        {
            tiempoAnim -= Time.deltaTime;
            if (tiempoAnim <= 0f)
            {
                animActiva = false;
                if (animator != null)
                    animator.SetBool("ilumine", false);
            }
        }

        // Control intensidad de la luz
        if (luzActiva)
        {
            luz.intensity = Mathf.MoveTowards(luz.intensity, intensidadMax, velocidadTransicion * Time.deltaTime);
            tiempoRestante -= Time.deltaTime;
            if (tiempoRestante <= 0f)
            {
                luzActiva = false;
                tiempoCooldown = cooldown;
            }
        }
        else
        {
            luz.intensity = Mathf.MoveTowards(luz.intensity, 0f, velocidadTransicion * Time.deltaTime);
            if (luz.intensity <= 0f)
                luz.enabled = false;
            if (tiempoCooldown > 0f)
                tiempoCooldown -= Time.deltaTime;
        }
    }

    void ActivarEstrellas()
    {
        Collider[] estrellasCercanas = Physics.OverlapSphere(transform.position, radioEstrellas);
        foreach (Collider col in estrellasCercanas)
        {
            if (col.CompareTag("Estrella"))
            {
                col.gameObject.SendMessage("Brillar", tiempoBrilloEstrella, SendMessageOptions.DontRequireReceiver);
            }
        }
        Debug.Log($"[LuzPersonaje] Activadas estrellas cercanas: {estrellasCercanas.Length}");
    }

    void ActivarObjetos()
    {
        // Busca todos los ObjetoOscuro en la escena sin depender del collider
        ObjetoOscuro[] todosLosObjetos = FindObjectsByType<ObjetoOscuro>(FindObjectsSortMode.None);
        int contador = 0;

        foreach (ObjetoOscuro obj in todosLosObjetos)
        {
            float distancia = Vector3.Distance(transform.position, obj.transform.position);
            if (distancia <= radioEstrellas)
            {
                obj.RevelarConAutoRecoger(tiempoParaRecoger);
                contador++;
            }
        }

        Debug.Log($"[LuzPersonaje] {contador} objetos programados para recogerse en {tiempoParaRecoger} segundos");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioEstrellas);
    }
}