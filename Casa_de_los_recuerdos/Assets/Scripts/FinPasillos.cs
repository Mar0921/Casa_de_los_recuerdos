using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class FinPasillos : MonoBehaviour
{
    [Header("Configuración")]
    public string nombreEscena = "SiguienteEscena";
    public string tagJugador = "Player";
    public float duracionFade = 1f;              // Duración del fade a negro
    public float delayAntesDeCargar = 0.5f;      // Espera antes del fade

    [Header("Efectos")]
    public AudioClip sonidoAlTocar;
    public AudioSource audioSource;
    public GameObject efectoParticula;

    private bool yaToco = false;

    void Start()
    {
        if (sonidoAlTocar != null && audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (yaToco) return;

        if (other.CompareTag(tagJugador))
        {
            yaToco = true;
            Debug.Log($"🎯 Cargando escena: {nombreEscena}");

            // Efectos inmediatos
            if (sonidoAlTocar != null && audioSource != null)
                audioSource.PlayOneShot(sonidoAlTocar);

            if (efectoParticula != null)
                Instantiate(efectoParticula, transform.position, Quaternion.identity);

            // Iniciar corrutina de fade
            StartCoroutine(FadeYCargarEscena());
        }
    }

    IEnumerator FadeYCargarEscena()
    {
        // Esperar un poco antes del fade
        yield return new WaitForSeconds(delayAntesDeCargar);

        // Crear imagen de fade si no existe
        GameObject fadeObj = new GameObject("FadeCanvas");
        Canvas canvas = fadeObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        UnityEngine.UI.Image image = fadeObj.AddComponent<UnityEngine.UI.Image>();
        image.color = Color.black;
        image.raycastTarget = false;

        // Hacer fade in (a negro)
        float tiempo = 0f;
        Color color = image.color;
        color.a = 0f;
        image.color = color;

        while (tiempo < duracionFade)
        {
            tiempo += Time.deltaTime;
            color.a = Mathf.Clamp01(tiempo / duracionFade);
            image.color = color;
            yield return null;
        }

        // Cargar escena
        SceneManager.LoadScene(nombreEscena);
    }
}