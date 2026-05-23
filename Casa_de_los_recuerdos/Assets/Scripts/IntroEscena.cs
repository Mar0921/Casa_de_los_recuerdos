using UnityEngine;
using System.Collections;

public class IntroEscena : MonoBehaviour
{
    [Header("Partículas")]
    public ParticleSystem[] particulasIniciales;

    [Header("Puerta")]
    public Transform puerta;
    public Vector3 rotacionDestino;
    public float duracionMovimiento = 1.5f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip sonidoPuerta;

    void Start()
    {
        // Activar partículas al inicio
        foreach (ParticleSystem ps in particulasIniciales)
            if (ps != null) ps.Play();

        StartCoroutine(SecuenciaIntro());
    }

    IEnumerator SecuenciaIntro()
    {
        // Esperar 1 segundo
        yield return new WaitForSeconds(1f);

        // Apagar partículas
        foreach (ParticleSystem ps in particulasIniciales)
            if (ps != null) ps.Stop();

        // Mover puerta
        if (puerta != null)
        {
            if (audioSource != null && sonidoPuerta != null)
                audioSource.PlayOneShot(sonidoPuerta);

            StartCoroutine(RotarPuerta());
        }
    }

    IEnumerator RotarPuerta()
    {
        Quaternion rotInicio = puerta.rotation;
        // Rotar relativo a como está la puerta ahora, no al mundo
        Quaternion rotFinal = rotInicio * Quaternion.Euler(0f, -rotacionDestino.y, 0f);
        float tiempo = 0f;

        while (tiempo < duracionMovimiento)
        {
            tiempo += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, tiempo / duracionMovimiento);
            puerta.rotation = Quaternion.Slerp(rotInicio, rotFinal, t);
            yield return null;
        }

        puerta.rotation = rotFinal;
        Debug.Log("✅ Puerta en posición final");
    }
}