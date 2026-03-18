using UnityEngine;

public class SonidoPasos : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip sonidoPaso;
    public float intervaloEntrepasos = 0.5f;
    public float umbralVelocidad = 0.1f;

    private Rigidbody rb;
    private float tiempoUltimoPaso;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.clip = sonidoPaso;
        audioSource.loop = false;
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        Vector3 velocidadHorizontal = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        if (velocidadHorizontal.magnitude > umbralVelocidad)
        {
            if (Time.time - tiempoUltimoPaso >= intervaloEntrepasos)
            {
                audioSource.PlayOneShot(sonidoPaso);
                tiempoUltimoPaso = Time.time;
            }
        }
    }
}