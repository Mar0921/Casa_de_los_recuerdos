using UnityEngine;
using UnityEngine.InputSystem;

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
    public float radioEstrellas = 5f;           // Distancia para afectar estrellas
    public float tiempoBrilloEstrella = 2f;     // Duración del brillo en las estrellas (opcional)

    private float tiempoRestante = 0f;
    private float tiempoCooldown = 0f;
    private float tiempoAnim = 0f;
    private bool luzActiva = false;
    private bool animActiva = false;
    private bool yaActivoEstrellas = false;      // Para llamar solo una vez por activación

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
            yaActivoEstrellas = false;   // Reset para la próxima activación

            if (audioSource != null && sonidoLuz != null)
                audioSource.PlayOneShot(sonidoLuz);

            if (animator != null)
                animator.SetBool("ilumine", true);
        }

        // Activar estrellas (solo una vez al inicio de la luz)
        if (luzActiva && !yaActivoEstrellas)
        {
            ActivarEstrellas();
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
            luz.intensity = Mathf.MoveTowards(
                luz.intensity,
                intensidadMax,
                velocidadTransicion * Time.deltaTime
            );

            tiempoRestante -= Time.deltaTime;
            if (tiempoRestante <= 0f)
            {
                luzActiva = false;
                tiempoCooldown = cooldown;
            }
        }
        else
        {
            luz.intensity = Mathf.MoveTowards(
                luz.intensity,
                0f,
                velocidadTransicion * Time.deltaTime
            );

            if (luz.intensity <= 0f)
                luz.enabled = false;

            if (tiempoCooldown > 0f)
                tiempoCooldown -= Time.deltaTime;
        }
    }

    void ActivarEstrellas()
    {
        // Buscar objetos con tag "Estrella" dentro del radio
        Collider[] estrellasCercanas = Physics.OverlapSphere(transform.position, radioEstrellas);

        foreach (Collider col in estrellasCercanas)
        {
            if (col.CompareTag("Estrella"))
            {
                // Intentar obtener un script que tenga el método "Brillar"
                // Puede ser cualquier componente: EstrellaBrillo, Star, etc.
                var estrella = col.GetComponent<MonoBehaviour>(); // O una interfaz específica

                // Usamos reflection? Mejor buscar un método conocido por nombre.
                // Opción 1: Enviar mensaje (menos eficiente pero flexible)
                col.gameObject.SendMessage("Brillar", tiempoBrilloEstrella, SendMessageOptions.DontRequireReceiver);

                // Opción 2: Si todas las estrellas tienen un componente específico:
                // EstrellaController esc = col.GetComponent<EstrellaController>();
                // if (esc != null) esc.Brillar(tiempoBrilloEstrella);
            }
        }

        Debug.Log($"[LuzPersonaje] Activadas estrellas cercanas: {estrellasCercanas.Length}");
    }

    // Opcional: dibujar el radio en la escena para debugging
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioEstrellas);
    }
}