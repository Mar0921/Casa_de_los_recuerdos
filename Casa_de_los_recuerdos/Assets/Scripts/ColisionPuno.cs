using UnityEngine;

public class ColisionPuno : MonoBehaviour
{
    [Header("Configuración")]
    public GameObject particulaRotura;      // Sistema de partículas a instanciar al romper
    public AudioClip sonidoRotura;          // Sonido opcional al romper
    public float tiempoDestruirParticulas = 2f; // Tiempo que duran las partículas antes de autodestruirse

    private VyreController vyreController;
    private Collider coliderPuno;
    private AudioSource audioSource;

    void Start()
    {
        vyreController = GetComponentInParent<VyreController>();
        coliderPuno = GetComponent<Collider>();
        coliderPuno.isTrigger = true;
        coliderPuno.enabled = false; // empieza desactivado

        // Si hay sonido y no hay AudioSource, agregamos uno temporal
        if (sonidoRotura != null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Rompible"))
        {
            // 1. Instanciar partículas en la posición del objeto roto
            if (particulaRotura != null)
            {
                GameObject efecto = Instantiate(particulaRotura, other.transform.position, Quaternion.identity);
                // Destruir el sistema de partículas después de un tiempo para no saturar la escena
                Destroy(efecto, tiempoDestruirParticulas);
            }

            // 2. Reproducir sonido de rotura
            if (sonidoRotura != null && audioSource != null)
            {
                audioSource.PlayOneShot(sonidoRotura);
            }

            // 3. Finalmente destruir el objeto rompible
            Destroy(other.gameObject);
        }
    }

    // Llamado por VyreController al empezar el golpe
    public void ActivarPuno()
    {
        coliderPuno.enabled = true;
    }

    // Llamado por VyreController al terminar el golpe
    public void DesactivarPuno()
    {
        coliderPuno.enabled = false;
    }
}