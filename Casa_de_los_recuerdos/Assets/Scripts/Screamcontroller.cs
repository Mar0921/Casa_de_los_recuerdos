using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class ScreamController : MonoBehaviour
{
    [Header("Animación y Audio")]
    public Animator anim;
    public AudioSource audioSource;
    public AudioClip screamClip;
    public float duracionScream = 2f;
    public bool bloquearMovimientoDuranteScream = true;

    [Header("Scripts a bloquear durante el Scream")]
    [Tooltip("Arrastra aquí ManejadorMovimiento, VyreController, etc. desde cualquier GameObject")]
    public MonoBehaviour[] scriptsMovimiento;

    [Tooltip("Arrastra aquí los Animators del jugador a congelar durante el Scream")]
    public Animator[] animatorsJugador;

    [Header("Fade y Reinicio")]
    public Image pantallaFade;
    public float duracionFade = 1f;
    public float delayAntesFade = 2f;
    [Tooltip("Dejar vacío para recargar la escena actual")]
    public string nombreEscenaReinicio = "";

    private bool estaGritando = false;
    private Image fadeImage;

    void Start()
    {
        if (anim == null)
            anim = GetComponent<Animator>();

        InicializarFade();
    }

    private void InicializarFade()
    {
        if (pantallaFade != null)
        {
            fadeImage = pantallaFade;
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(false);
        }
    }

    public void ForzarScream()
    {
        if (anim == null || estaGritando) return;

        estaGritando = true;

        anim.SetBool("isWalking", false);
        anim.SetTrigger("Scream");

        if (audioSource != null && screamClip != null)
            audioSource.PlayOneShot(screamClip);

        if (bloquearMovimientoDuranteScream)
        {
            BloquearMovimiento(true);
            BloquearAnimaciones(true);
        }

        Debug.Log("[ScreamController] Scream forzado. Movimiento y animaciones bloqueados.");

        StartCoroutine(IniciarFadeConDelay());
    }

    private void BloquearMovimiento(bool bloquear)
    {
        if (scriptsMovimiento == null) return;

        foreach (MonoBehaviour script in scriptsMovimiento)
        {
            if (script != null)
            {
                script.enabled = !bloquear;
                Debug.Log("[ScreamController] " + script.GetType().Name + " -> " + (bloquear ? "BLOQUEADO" : "DESBLOQUEADO"));
            }
        }
    }

    private void BloquearAnimaciones(bool bloquear)
    {
        if (animatorsJugador == null) return;

        foreach (Animator a in animatorsJugador)
        {
            if (a != null)
            {
                a.enabled = !bloquear;
                Debug.Log("[ScreamController] Animator " + a.gameObject.name + " -> " + (bloquear ? "CONGELADO" : "ACTIVO"));
            }
        }
    }

    private IEnumerator IniciarFadeConDelay()
    {
        yield return new WaitForSeconds(delayAntesFade);
        yield return StartCoroutine(FadeYReiniciar());
    }

    private IEnumerator FadeYReiniciar()
    {
        if (fadeImage == null)
        {
            GameObject canvasObj = new GameObject("FadeCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            canvasObj.AddComponent<CanvasGroup>().blocksRaycasts = false;

            GameObject imageObj = new GameObject("FadeImage");
            imageObj.transform.SetParent(canvas.transform, false);
            Image img = imageObj.AddComponent<Image>();
            img.color = Color.black;

            RectTransform rect = img.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            img.gameObject.SetActive(false);
            fadeImage = img;
            DontDestroyOnLoad(canvasObj);
        }

        fadeImage.gameObject.SetActive(true);
        Color color = fadeImage.color;
        color.a = 0f;
        fadeImage.color = color;

        Debug.Log("[ScreamController] Iniciando fade a negro.");

        float tiempo = 0f;
        while (tiempo < duracionFade)
        {
            tiempo += Time.deltaTime;
            color.a = Mathf.Clamp01(tiempo / duracionFade);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color;

        // El reinicio resetea todo automáticamente al recargar la escena
        if (string.IsNullOrEmpty(nombreEscenaReinicio))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        else
            SceneManager.LoadScene(nombreEscenaReinicio);
    }

    public bool EstaGritando()
    {
        return estaGritando;
    }
}