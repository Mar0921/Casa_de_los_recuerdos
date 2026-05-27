using UnityEngine;

public class Sonidos : MonoBehaviour
{
    [Header("Música de Fondo")]
    public AudioSource audioSourceMusica;
    public AudioClip musicaFondo;

    [Range(0f, 1f)]
    public float volumenMusica = 1f;

    void Start()
    {
        if (audioSourceMusica == null)
        {
            audioSourceMusica = gameObject.AddComponent<AudioSource>();
        }

        if (musicaFondo != null)
        {
            audioSourceMusica.clip = musicaFondo;
            audioSourceMusica.loop = true;
            audioSourceMusica.playOnAwake = false;

            // Usa SIEMPRE el valor del Inspector
            float volumenInicial = volumenMusica;

            CambiarVolumenMusica(volumenInicial);

            audioSourceMusica.Play();

            Debug.Log("Música de fondo iniciada con volumen: " + audioSourceMusica.volume);
        }
    }

    public void CambiarVolumenMusica(float nuevoVolumen)
    {
        volumenMusica = Mathf.Clamp01(nuevoVolumen);

        if (audioSourceMusica != null)
        {
            audioSourceMusica.volume = volumenMusica;
        }

        // Guarda el nuevo volumen
        PlayerPrefs.SetFloat("VolumenMusica", volumenMusica);
        PlayerPrefs.Save();
    }

    public void DetenerMusica()
    {
        if (audioSourceMusica != null)
        {
            audioSourceMusica.Stop();
        }
    }

    public void ReanudarMusica()
    {
        if (audioSourceMusica != null && !audioSourceMusica.isPlaying)
        {
            audioSourceMusica.Play();
        }
    }
}