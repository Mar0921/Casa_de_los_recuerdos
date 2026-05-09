using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class MonstruoPerseguir : MonoBehaviour
{
    [Header("Referencias")]
    public NavMeshAgent agent;
    public Animator animator;

    [Header("Jugador")]
    public ScreamController screamControllerJugador;

    [Header("Estado visual")]
    public GameObject modeloVisual;

    [Header("Configuracion")]
    public bool ocultoAlIniciar = true;
    public float distanciaMinimaActualizacion = 0.2f;
    [Tooltip("Ajusta si el monstruo flota. Prueba -0.5, -0.9, etc.")]
    public float baseOffset = 0f;

    [Header("Ataque y reinicio")]
    public LayerMask playerLayer;
    public string playerTag = "Player";
    public float duracionFade = 1f;
    public float delayAntesFade = 2f;
    public Image pantallaFade;
    public string nombreEscenaReinicio = "";

    private Transform objetivo;
    private bool persiguiendo = false;
    private bool haAtacado = false;
    private Renderer[] renderersMonstruo;
    private Collider[] collidersMonstruo;
    private Vector3 ultimoDestino;
    private Image fadeImage;

    void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        GameObject raizVisual = modeloVisual != null ? modeloVisual : gameObject;
        renderersMonstruo = raizVisual.GetComponentsInChildren<Renderer>(true);
        collidersMonstruo = GetComponentsInChildren<Collider>(true);
    }

    void Start()
    {
        if (agent != null)
            agent.baseOffset = baseOffset;

        if (ocultoAlIniciar)
            DesactivarYDesaparecer();

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

    void Update()
    {
        if (!persiguiendo || objetivo == null || agent == null || !agent.enabled || haAtacado)
            return;

        float distancia = Vector3.Distance(ultimoDestino, objetivo.position);
        if (distancia >= distanciaMinimaActualizacion)
        {
            agent.SetDestination(objetivo.position);
            ultimoDestino = objetivo.position;
        }

        // Caminar si se esta moviendo, idle si esta quieto
        bool estaCaminando = agent.velocity.magnitude > 0.1f;
        animator.SetBool("isWalking", estaCaminando);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (haAtacado) return;
        if (EsJugador(collision.gameObject)) AtacarJugador();
    }

    void OnTriggerEnter(Collider other)
    {
        if (haAtacado) return;
        if (EsJugador(other.gameObject)) AtacarJugador();
    }

    private bool EsJugador(GameObject obj)
    {
        return obj.CompareTag(playerTag) || ((playerLayer.value & (1 << obj.layer)) != 0);
    }

    private void AtacarJugador()
    {
        haAtacado = true;
        persiguiendo = false;

        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        if (animator != null)
            animator.SetBool("isWalking", false);

        // Scream en el JUGADOR
        if (screamControllerJugador != null)
        {
            screamControllerJugador.ForzarScream();
        }
        else
        {
            GameObject jugador = GameObject.FindGameObjectWithTag(playerTag);
            if (jugador != null)
            {
                ScreamController sc = jugador.GetComponent<ScreamController>();
                if (sc != null) sc.ForzarScream();
            }
        }

        Debug.Log("[MonstruoPerseguidor] Ataque al jugador. Fade en " + delayAntesFade + "s.");
        StartCoroutine(IniciarFadeConDelay());
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

        if (string.IsNullOrEmpty(nombreEscenaReinicio))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        else
            SceneManager.LoadScene(nombreEscenaReinicio);
    }

    public void ActivarPersecucion(Transform objetivoNuevo, Transform puntoAparicion)
    {
        if (objetivoNuevo == null)
        {
            Debug.LogWarning("[MonstruoPerseguidor] No se recibio objetivo.");
            return;
        }

        haAtacado = false;
        objetivo = objetivoNuevo;
        persiguiendo = true;

        MostrarMonstruo(true);

        if (agent != null && !agent.enabled)
            agent.enabled = true;

        if (agent != null)
            agent.baseOffset = baseOffset;

        Vector3 posicionSpawn = puntoAparicion != null ? puntoAparicion.position : transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(posicionSpawn, out hit, 2f, NavMesh.AllAreas))
            agent.Warp(hit.position);
        else
        {
            transform.position = posicionSpawn;
            Debug.LogWarning("[MonstruoPerseguidor] Punto de aparicion fuera del NavMesh.");
        }

        agent.isStopped = false;
        agent.ResetPath();
        agent.SetDestination(objetivo.position);
        ultimoDestino = objetivo.position;

        // Forzar caminata inmediatamente al aparecer
        if (animator != null)
            animator.SetBool("isWalking", true);

        Debug.Log("[MonstruoPerseguidor] Monstruo activado, caminando.");
    }

    public void DesactivarYDesaparecer()
    {
        persiguiendo = false;
        objetivo = null;

        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.enabled = false;
        }

        if (animator != null)
            animator.SetBool("isWalking", false);

        MostrarMonstruo(false);
        Debug.Log("[MonstruoPerseguidor] Monstruo desactivado.");
    }

    void MostrarMonstruo(bool mostrar)
    {
        foreach (Renderer rend in renderersMonstruo)
            rend.enabled = mostrar;
        foreach (Collider col in collidersMonstruo)
            col.enabled = mostrar;
    }
}