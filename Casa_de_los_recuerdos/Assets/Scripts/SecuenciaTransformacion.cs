using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SecuenciaTransformacion : MonoBehaviour
{
    [Header("Configuración de Zona")]
    public string tagJugador = "Player";

    [Header("Puntos de Cámara (Posición + Rotación)")]
    public Transform puntoCamara1;
    public Transform puntoCamara2;
    public Transform puntoCamara3;
    public Transform puntoCamara4;

    [Header("Tiempos")]
    public float tiempoPorPunto = 2f;
    public float tiempoPuntoOriginal = 1f;
    public float duracionTransicion = 0.5f;

    [Header("Rotación Final Adicional")]
    public Vector3 rotacionAdicionalFinal = new Vector3(3f, 0f, 0f);

    [Header("Animación del Jugador")]
    public string parametroIdle = "VelY";
    public string parametroTransformacion = "MeTransforme";
    public float tiempoAnimacion = 2f;

    [Header("Partículas del Jugador")]
    public ParticleSystem particulasTransformacion;

    [Header("Efecto de Borde/Enfoque")]
    public Material materialDestacado;
    public float duracionDestacado = 0.3f;

    [Header("Sonidos")]
    public AudioSource audioSource;
    public AudioClip sonidoInicioSecuencia;
    public AudioClip sonidoTransformacion;
    public float volumenSonido = 1f;

    [Header("Fade Out")]
    public float duracionFadeOut = 1f;
    public Color colorFade = Color.black;

    [Header("Cambio de Escena")]
    public string nombreEscenaDestino = "NombreDeTuEscena";

    // Variables internas
    private Camera camaraPrincipal;
    private Vector3 posicionOriginalCamara;
    private Quaternion rotacionOriginalCamara;
    private GameObject jugador;
    private Animator animatorJugador;
    private bool secuenciaActiva = false;
    private Canvas canvasFade;
    private UnityEngine.UI.Image imagenFade;
    private Material materialOriginal;
    private Renderer rendererJugador;
    private Vector3 escalaOriginalJugador;
    private Transform parentOriginalCamara;

    // 🔴 NUEVO: Guardar la posición del jugador también
    private Vector3 posicionOriginalJugador;
    private Quaternion rotacionOriginalJugador;

    void Start()
    {
        camaraPrincipal = Camera.main;
        if (camaraPrincipal == null)
        {
            Debug.LogError("No se encontró la cámara principal");
            return;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        audioSource.volume = volumenSonido;

        CrearCanvasFade();
    }

    void CrearCanvasFade()
    {
        GameObject canvasObj = new GameObject("CanvasFade");
        canvasFade = canvasObj.AddComponent<Canvas>();
        canvasFade.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasFade.sortingOrder = 999;

        var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;

        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        GameObject imagenObj = new GameObject("ImagenFade");
        imagenObj.transform.SetParent(canvasObj.transform, false);

        imagenFade = imagenObj.AddComponent<UnityEngine.UI.Image>();
        imagenFade.color = new Color(colorFade.r, colorFade.g, colorFade.b, 0f);
        imagenFade.raycastTarget = false;

        RectTransform rect = imagenFade.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tagJugador) && !secuenciaActiva)
        {
            jugador = other.gameObject;
            animatorJugador = jugador.GetComponent<Animator>();
            rendererJugador = jugador.GetComponentInChildren<Renderer>();

            if (animatorJugador == null)
            {
                Debug.LogError("El jugador no tiene componente Animator");
                return;
            }

            StartCoroutine(EjecutarSecuencia());
        }
    }

    IEnumerator EjecutarSecuencia()
    {
        secuenciaActiva = true;

        // 🔴 1. GUARDAR POSICIONES ORIGINALES ANTES DE MODIFICAR NADA
        posicionOriginalCamara = camaraPrincipal.transform.position;
        rotacionOriginalCamara = camaraPrincipal.transform.rotation;

        posicionOriginalJugador = jugador.transform.position;
        rotacionOriginalJugador = jugador.transform.rotation;

        Debug.Log("Posición original guardada - Cámara: " + posicionOriginalCamara + " Jugador: " + posicionOriginalJugador);

        // 2. Desactivar scripts de cámara
        DesactivarScriptsCamara(true);

        // 3. Desparentar la cámara
        if (camaraPrincipal.transform.parent != null)
        {
            parentOriginalCamara = camaraPrincipal.transform.parent;
            camaraPrincipal.transform.SetParent(null);
        }

        // 4. Desactivar control del jugador y congelar
        DesactivarControlJugador(true);
        CongelarJugador(true);

        // 5. Reproducir sonido
        ReproducirSonido(sonidoInicioSecuencia);

        // 6. Forzar animación IDLE
        if (animatorJugador != null)
        {
            animatorJugador.SetFloat(parametroIdle, 0f);
            animatorJugador.SetBool(parametroTransformacion, false);
        }

        // 7. Guardar material original
        if (rendererJugador != null && materialDestacado != null)
        {
            materialOriginal = rendererJugador.material;
        }

        // 8. Movimiento por los 4 puntos
        yield return StartCoroutine(MoverCamaraDirecto(puntoCamara1, tiempoPorPunto));
        yield return StartCoroutine(MoverCamaraDirecto(puntoCamara2, tiempoPorPunto));
        yield return StartCoroutine(MoverCamaraDirecto(puntoCamara3, tiempoPorPunto));
        yield return StartCoroutine(MoverCamaraDirecto(puntoCamara4, tiempoPorPunto));

        // 🔴 9. VOLVER A POSICIÓN ORIGINAL (usando los valores guardados)
        yield return StartCoroutine(VolverAPosicionOriginal(tiempoPuntoOriginal));

        // 10. Aplicar rotación adicional
        yield return StartCoroutine(AplicarRotacionAdicional(rotacionAdicionalFinal, 0.5f));

        // 11. Reproducir sonido de transformación
        ReproducirSonido(sonidoTransformacion);

        // 12. Activar partículas y transformación
        ActivarParticulas(true);

        if (animatorJugador != null)
        {
            animatorJugador.SetBool(parametroTransformacion, true);
        }

        yield return new WaitForSeconds(tiempoAnimacion);

        ActivarParticulas(false);

        // 13. Fade out y cambio de escena
        yield return StartCoroutine(FadeOut(duracionFadeOut));
        SceneManager.LoadScene(nombreEscenaDestino);
    }

    void DesactivarScriptsCamara(bool desactivar)
    {
        if (camaraPrincipal == null) return;

        var scripts = camaraPrincipal.GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            if (script != this && script != null)
            {
                script.enabled = !desactivar;
            }
        }
    }

    void CongelarJugador(bool congelar)
    {
        if (jugador == null) return;

        var rb = jugador.GetComponent<Rigidbody>();
        if (rb != null)
        {
            if (congelar)
            {
                rb.constraints = RigidbodyConstraints.FreezeAll;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            else
            {
                rb.constraints = RigidbodyConstraints.FreezeRotation;
            }
        }

        // También desactivar CharacterController si existe
        var controller = jugador.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = !congelar;
        }
    }

    IEnumerator MoverCamaraDirecto(Transform puntoDestino, float tiempoEstancia)
    {
        if (puntoDestino == null)
        {
            Debug.LogWarning("Punto de cámara no asignado");
            yield return new WaitForSeconds(tiempoEstancia);
            yield break;
        }

        Vector3 posInicial = camaraPrincipal.transform.position;
        Quaternion rotInicial = camaraPrincipal.transform.rotation;
        Vector3 posDestino = puntoDestino.position;
        Quaternion rotDestino = puntoDestino.rotation;

        // Moverse al punto
        float tiempoMovimiento = 0f;
        while (tiempoMovimiento < duracionTransicion)
        {
            tiempoMovimiento += Time.deltaTime;
            float t = tiempoMovimiento / duracionTransicion;

            camaraPrincipal.transform.position = Vector3.Lerp(posInicial, posDestino, t);
            camaraPrincipal.transform.rotation = Quaternion.Slerp(rotInicial, rotDestino, t);

            yield return null;
        }

        // Asegurar posición exacta
        camaraPrincipal.transform.position = posDestino;
        camaraPrincipal.transform.rotation = rotDestino;

        // Efecto destacado
        yield return StartCoroutine(EfectoDestacado());

        // Esperar (con verificación)
        float tiempoEspera = 0f;
        while (tiempoEspera < tiempoEstancia)
        {
            if (Vector3.Distance(camaraPrincipal.transform.position, posDestino) > 0.01f)
            {
                camaraPrincipal.transform.position = posDestino;
                camaraPrincipal.transform.rotation = rotDestino;
            }
            tiempoEspera += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator VolverAPosicionOriginal(float tiempoEstancia)
    {
        Debug.Log("Volviendo a posición original - " + posicionOriginalCamara);

        Vector3 posInicial = camaraPrincipal.transform.position;
        Quaternion rotInicial = camaraPrincipal.transform.rotation;

        // Usar los valores guardados
        Vector3 posDestino = posicionOriginalCamara;
        Quaternion rotDestino = rotacionOriginalCamara;

        float tiempoMovimiento = 0f;
        while (tiempoMovimiento < duracionTransicion)
        {
            tiempoMovimiento += Time.deltaTime;
            float t = tiempoMovimiento / duracionTransicion;

            camaraPrincipal.transform.position = Vector3.Lerp(posInicial, posDestino, t);
            camaraPrincipal.transform.rotation = Quaternion.Slerp(rotInicial, rotDestino, t);

            yield return null;
        }

        // Asegurar posición original exacta
        camaraPrincipal.transform.position = posDestino;
        camaraPrincipal.transform.rotation = rotDestino;

        // Verificar que llegó bien
        Debug.Log("Cámara regresó a: " + camaraPrincipal.transform.position);
        Debug.Log("Debería estar en: " + posicionOriginalCamara);
        Debug.Log("Diferencia: " + Vector3.Distance(camaraPrincipal.transform.position, posicionOriginalCamara));

        // Esperar el tiempo indicado
        float tiempoEspera = 0f;
        while (tiempoEspera < tiempoEstancia)
        {
            if (Vector3.Distance(camaraPrincipal.transform.position, posDestino) > 0.01f)
            {
                Debug.LogWarning("¡La cámara se movió! Recolocando...");
                camaraPrincipal.transform.position = posDestino;
                camaraPrincipal.transform.rotation = rotDestino;
            }
            tiempoEspera += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator EfectoDestacado()
    {
        if (rendererJugador == null || materialDestacado == null)
        {
            yield break;
        }

        rendererJugador.material = materialDestacado;
        escalaOriginalJugador = jugador.transform.localScale;
        jugador.transform.localScale = escalaOriginalJugador * 1.05f;

        yield return new WaitForSeconds(duracionDestacado);

        rendererJugador.material = materialOriginal;
        jugador.transform.localScale = escalaOriginalJugador;
    }

    IEnumerator AplicarRotacionAdicional(Vector3 rotacionExtra, float duracion)
    {
        Quaternion rotacionInicial = camaraPrincipal.transform.rotation;
        Quaternion rotacionFinal = rotacionInicial * Quaternion.Euler(rotacionExtra);

        float tiempo = 0f;
        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / duracion;
            camaraPrincipal.transform.rotation = Quaternion.Slerp(rotacionInicial, rotacionFinal, t);
            yield return null;
        }

        camaraPrincipal.transform.rotation = rotacionFinal;
    }

    void ReproducirSonido(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, volumenSonido);
        }
    }

    void ActivarParticulas(bool activar)
    {
        if (particulasTransformacion != null)
        {
            if (activar)
            {
                particulasTransformacion.gameObject.SetActive(true);
                particulasTransformacion.Play();
            }
            else
            {
                particulasTransformacion.Stop();
            }
        }
    }

    IEnumerator FadeOut(float duracion)
    {
        if (imagenFade == null) yield break;

        float tiempo = 0f;
        Color color = imagenFade.color;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, tiempo / duracion);
            imagenFade.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        imagenFade.color = new Color(color.r, color.g, color.b, 1f);
    }

    void DesactivarControlJugador(bool desactivar)
    {
        if (jugador != null)
        {
            var movimiento = jugador.GetComponent<ProtaMovimiento>();
            if (movimiento != null)
                movimiento.enabled = !desactivar;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        if (puntoCamara1 != null) DibujarGizmoPunto(puntoCamara1);
        if (puntoCamara2 != null) DibujarGizmoPunto(puntoCamara2);
        if (puntoCamara3 != null) DibujarGizmoPunto(puntoCamara3);
        if (puntoCamara4 != null) DibujarGizmoPunto(puntoCamara4);
    }

    void DibujarGizmoPunto(Transform punto)
    {
        Gizmos.DrawWireSphere(punto.position, 0.5f);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(punto.position, punto.forward * 1f);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(punto.position, punto.up * 0.8f);
    }
}