using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class FinPasillos : MonoBehaviour
{
    [Header("Configuración")]
    public string nombreEscena = "SiguienteEscena";
    public string tagJugador = "Player";
    public float duracionFade = 1f;
    public float delayAntesDeCargar = 0.5f;

    [Header("Efectos")]
    public AudioClip sonidoAlTocar;
    public AudioSource audioSource;
    public GameObject efectoParticula;

    [Header("Fade")]
    public Color colorFade = Color.black; // Cámbialo a blanco desde el Inspector

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

            if (sonidoAlTocar != null && audioSource != null)
                audioSource.PlayOneShot(sonidoAlTocar);

            if (efectoParticula != null)
                Instantiate(efectoParticula, transform.position, Quaternion.identity);

            StartCoroutine(FadeYCargarEscena());
        }
    }

    IEnumerator FadeYCargarEscena()
    {
        yield return new WaitForSeconds(delayAntesDeCargar);

        GameObject fadeObj = new GameObject("FadeCanvas");
        Canvas canvas = fadeObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        UnityEngine.UI.Image image = fadeObj.AddComponent<UnityEngine.UI.Image>();
        image.raycastTarget = false;

        // Usar el color elegido pero con alpha 0 al inicio
        Color color = new Color(colorFade.r, colorFade.g, colorFade.b, 0f);
        image.color = color;

        float tiempo = 0f;
        while (tiempo < duracionFade)
        {
            tiempo += Time.deltaTime;
            color.a = Mathf.Clamp01(tiempo / duracionFade);
            image.color = color;
            yield return null;
        }

        SceneManager.LoadScene(nombreEscena);
    }
}
