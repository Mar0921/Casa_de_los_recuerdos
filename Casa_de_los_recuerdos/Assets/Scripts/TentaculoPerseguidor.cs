using UnityEngine;
using System.Collections;

public class TentaculoPerseguidor : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject modeloVisual;
    public CampoBurbujaSombra campoBurbuja;

    [Header("Configuración de persecución")]
    public float distanciaDeteccion = 8f;
    public float distanciaAtaque = 2.5f;
    public float velocidadPersecucion = 3.5f;
    public float frecuenciaBusquedaJugador = 0.5f;

    [Header("Movimiento ondulante")]
    public bool movimientoOndulante = true;
    public float amplitudOndulacion = 0.1f;
    public float frecuenciaOndulacion = 5f;

    [Header("Ataque - Latigazo")]
    public float tiempoEsperaAntesAtaque = 2f;
    public float distanciaLatigazo = 3f;
    public float velocidadLatigazo = 25f;
    public float tiempoRetroceso = 0.2f;
    public float cooldownPostAtaque = 2f;
    public float retrocesoAtaque = 1.5f;

    [Header("Efectos")]
    public AudioClip sonidoAtaque;
    public AudioClip sonidoGolpe;
    public AudioClip sonidoMuerte;
    public AudioSource audioSource;
    public GameObject particulaGolpe;
    public ParticleSystem particulasMuerte;

    [Header("Sistema de Golpes")]
    public int golpesNecesarios = 1;
    private int golpesRecibidos = 0;
    private bool estaMuerto = false;

    [Header("Efecto al atacar al jugador")]
    public float duracionEfectoOscuridad = 0.5f;
    public float duracionFadeOut = 1.5f;
    public Color colorOscuridad = Color.black;

    private Transform jugadorActual;
    private bool estaAtacando = false;
    private bool puedeAtacar = true;
    private float temporizadorBusqueda = 0f;
    private Vector3 posicionInicial;
    private Quaternion rotacionInicial;
    private bool jugadorSiendoAtacado = false;

    void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        if (modeloVisual == null)
            modeloVisual = gameObject;

        posicionInicial = transform.position;
        rotacionInicial = transform.rotation;

        ActualizarJugadorActivo();

        if (campoBurbuja == null)
        {
            campoBurbuja = FindFirstObjectByType<CampoBurbujaSombra>();
            if (campoBurbuja != null)
            {
                Debug.Log($"[Tentáculo {name}] Campo burbuja encontrado automáticamente.");
            }
            else
            {
                Debug.LogWarning($"[Tentáculo {name}] ¡No se encontró CampoBurbujaSombra en la escena!");
            }
        }
    }

    void Update()
    {
        if (estaMuerto) return;

        temporizadorBusqueda -= Time.deltaTime;
        if (temporizadorBusqueda <= 0f)
        {
            ActualizarJugadorActivo();
            temporizadorBusqueda = frecuenciaBusquedaJugador;
        }

        if (jugadorActual == null || estaAtacando || jugadorSiendoAtacado) return;

        float distancia = Vector3.Distance(transform.position, jugadorActual.position);

        if (distancia <= distanciaAtaque && puedeAtacar && !estaAtacando)
        {
            StartCoroutine(Atacar());
        }
        else if (distancia <= distanciaDeteccion && !estaAtacando)
        {
            MoverHaciaJugador();
        }
    }

    void ActualizarJugadorActivo()
    {
        GameObject[] jugadores = GameObject.FindGameObjectsWithTag("Player");
        Transform nuevoJugador = null;

        foreach (GameObject jugador in jugadores)
        {
            if (jugador.activeInHierarchy)
            {
                nuevoJugador = jugador.transform;
                break;
            }
        }

        if (nuevoJugador != jugadorActual)
        {
            jugadorActual = nuevoJugador;
        }
    }

    void MoverHaciaJugador()
    {
        if (jugadorActual == null) return;

        Vector3 direccion = jugadorActual.position - transform.position;
        direccion.y = 0f;

        float distancia = direccion.magnitude;
        if (distancia < distanciaAtaque * 0.8f)
        {
            transform.position += direccion.normalized * velocidadPersecucion * 0.5f * Time.deltaTime;
        }
        else
        {
            transform.position += direccion.normalized * velocidadPersecucion * Time.deltaTime;
        }

        Vector3 pos = transform.position;
        pos.y = posicionInicial.y;
        transform.position = pos;

        if (movimientoOndulante && modeloVisual != null && modeloVisual != gameObject)
        {
            float offsetX = Mathf.Sin(Time.time * frecuenciaOndulacion) * amplitudOndulacion;
            float offsetZ = Mathf.Cos(Time.time * frecuenciaOndulacion * 0.7f) * amplitudOndulacion;
            modeloVisual.transform.localPosition = new Vector3(offsetX, 0f, offsetZ);
        }
    }

    IEnumerator Atacar()
    {
        if (!puedeAtacar || estaAtacando || estaMuerto) yield break;

        estaAtacando = true;
        puedeAtacar = false;

        yield return new WaitForSeconds(tiempoEsperaAntesAtaque);

        if (jugadorActual == null || !jugadorActual.gameObject.activeInHierarchy || estaMuerto)
        {
            ResetearEstadoAtaque();
            yield break;
        }

        float distanciaActual = Vector3.Distance(transform.position, jugadorActual.position);
        if (distanciaActual > distanciaAtaque * 1.5f)
        {
            ResetearEstadoAtaque();
            yield break;
        }

        if (sonidoAtaque != null && audioSource != null)
            audioSource.PlayOneShot(sonidoAtaque);

        // Movimiento de ataque hacia adelante
        Vector3 posicionOriginal = transform.position;
        Vector3 direccionAtaque = (jugadorActual.position - transform.position).normalized;
        direccionAtaque.y = 0f;
        Vector3 posicionImpacto = transform.position + direccionAtaque * distanciaLatigazo;

        float tiempo = 0f;
        float duracionLatigazo = 0.15f;

        while (tiempo < duracionLatigazo)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / duracionLatigazo;
            transform.position = Vector3.Lerp(posicionOriginal, posicionImpacto, t);
            yield return null;
        }

        if (particulaGolpe != null && jugadorActual != null)
        {
            Instantiate(particulaGolpe, jugadorActual.position, Quaternion.identity);
        }

        // Retroceso del tentáculo
        Vector3 posicionDespuesLatigazo = transform.position;
        tiempo = 0f;

        while (tiempo < tiempoRetroceso)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / tiempoRetroceso;
            transform.position = Vector3.Lerp(posicionDespuesLatigazo, posicionOriginal, t);
            yield return null;
        }

        transform.position = posicionOriginal;

        // ✅ AHORA SÍ: Después del retroceso, activar el efecto de derrota
        if (jugadorActual != null)
        {
            yield return StartCoroutine(ActivarEfectoDerrota());
        }

        yield return new WaitForSeconds(cooldownPostAtaque);
        ResetearEstadoAtaque();
    }

    IEnumerator ActivarEfectoDerrota()
    {
        jugadorSiendoAtacado = true;

        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("No se encontró la cámara principal");
            ReiniciarEscena();
            yield break;
        }

        // Crear canvas con imagen oscura
        GameObject canvasObj = new GameObject("CanvasOscuridadTemp");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        GameObject imagenObj = new GameObject("ImagenOscura");
        imagenObj.transform.SetParent(canvas.transform, false);
        UnityEngine.UI.Image imagenOscura = imagenObj.AddComponent<UnityEngine.UI.Image>();
        imagenOscura.color = new Color(colorOscuridad.r, colorOscuridad.g, colorOscuridad.b, 0f);

        RectTransform rect = imagenOscura.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        // Efecto de oscurecimiento progresivo
        float tiempo = 0f;
        while (tiempo < duracionEfectoOscuridad)
        {
            tiempo += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, tiempo / duracionEfectoOscuridad);
            imagenOscura.color = new Color(colorOscuridad.r, colorOscuridad.g, colorOscuridad.b, alpha);
            yield return null;
        }

        imagenOscura.color = new Color(colorOscuridad.r, colorOscuridad.g, colorOscuridad.b, 1f);

        // Mantener oscuro un momento
        yield return new WaitForSeconds(0.3f);

        // Reiniciar escena
        ReiniciarEscena();
    }

    void ReiniciarEscena()
    {
        Debug.Log("Reiniciando escena por ataque del tentáculo...");
        string escenaActual = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        UnityEngine.SceneManagement.SceneManager.LoadScene(escenaActual);
    }

    void ResetearEstadoAtaque()
    {
        estaAtacando = false;
        puedeAtacar = true;
    }

    public void RecibirGolpe()
    {
        if (estaMuerto) return;

        golpesRecibidos++;
        Debug.Log($"[Tentáculo {name}] Golpe {golpesRecibidos}/{golpesNecesarios}");

        if (audioSource != null && sonidoGolpe != null)
            audioSource.PlayOneShot(sonidoGolpe);

        if (golpesRecibidos >= golpesNecesarios)
        {
            Morir();
        }
    }

    void Morir()
    {
        if (estaMuerto) return;
        estaMuerto = true;

        Debug.Log($"[Tentáculo {name}] ¡MUERTO! Notificando al campo burbuja...");

        if (audioSource != null && sonidoMuerte != null)
            audioSource.PlayOneShot(sonidoMuerte);

        if (particulasMuerte != null)
        {
            particulasMuerte.transform.SetParent(null);
            particulasMuerte.Play();
            Destroy(particulasMuerte.gameObject, 2f);
        }

        if (campoBurbuja != null)
        {
            campoBurbuja.TentaculoEliminado();
            Debug.Log($"[Tentáculo {name}] Campo burbuja notificado.");
        }
        else
        {
            Debug.LogError($"[Tentáculo {name}] ¡No hay referencia al campo burbuja! No se notificará la muerte.");
        }

        StopAllCoroutines();
        Destroy(gameObject, 0.2f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (estaMuerto) return;

        ColisionPuno puno = other.GetComponent<ColisionPuno>();
        if (puno != null)
        {
            Debug.Log($"[Tentáculo {name}] ¡Golpe detectado por puño!");
            RecibirGolpe();
        }
    }

    public void ResetearTentaculo()
    {
        StopAllCoroutines();
        estaAtacando = false;
        puedeAtacar = true;
        estaMuerto = false;
        jugadorSiendoAtacado = false;
        golpesRecibidos = 0;
        transform.position = posicionInicial;
        transform.rotation = rotacionInicial;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaDeteccion);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaAtaque);
    }
}