using UnityEngine;

public class Sonidos : MonoBehaviour
{
    [Header("Música de Fondo")]
    public AudioSource audioSourceMusica;
    public AudioClip musicaFondo;
    [Range(0f, 1f)]
    public float volumenMusica = 0.5f;

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
            audioSourceMusica.playOnAwake = true;

            float volumenGuardado = PlayerPrefs.GetFloat("VolumenMusica", 0.5f);
            CambiarVolumenMusica(volumenGuardado);

            audioSourceMusica.Play();
            Debug.Log("Música de fondo iniciada con volumen: " + audioSourceMusica.volume);
        }
    }

    public void CambiarVolumenMusica(float nuevoVolumen)
    {
        volumenMusica = Mathf.Clamp01(nuevoVolumen);
        if (audioSourceMusica != null)
            audioSourceMusica.volume = volumenMusica;
    }

    public void DetenerMusica()
    {
        if (audioSourceMusica != null)
            audioSourceMusica.Stop();
    }

    public void ReanudarMusica()
    {
        if (audioSourceMusica != null && !audioSourceMusica.isPlaying)
            audioSourceMusica.Play();
    }
}