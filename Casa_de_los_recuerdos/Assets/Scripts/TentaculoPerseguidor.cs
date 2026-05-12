using UnityEngine;
using System.Collections;

public class TentaculoPerseguidor : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject modeloVisual;

    [Header("Configuración de persecución")]
    public float distanciaDeteccion = 8f;
    public float distanciaAtaque = 2.5f;        // AUMENTÉ de 1.5 a 2.5
    public float velocidadPersecucion = 3.5f;   // AUMENTÉ ligeramente
    public float frecuenciaBusquedaJugador = 0.5f;

    [Header("Movimiento ondulante")]
    public bool movimientoOndulante = true;
    public float amplitudOndulacion = 0.1f;
    public float frecuenciaOndulacion = 5f;

    [Header("Ataque - Latigazo")]
    public float tiempoEsperaAntesAtaque = 2f;
    public float distanciaLatigazo = 3f;        // AUMENTÉ para que llegue
    public float velocidadLatigazo = 25f;       // AUMENTÉ
    public float tiempoRetroceso = 0.2f;
    public float cooldownPostAtaque = 2f;
    public float retrocesoAtaque = 1.5f;

    [Header("Efectos")]
    public AudioClip sonidoAtaque;
    public AudioSource audioSource;
    public GameObject particulaGolpe;

    private Transform jugadorActual;
    private bool estaAtacando = false;
    private bool puedeAtacar = true;
    private float temporizadorBusqueda = 0f;
    private Vector3 posicionInicial;
    private Quaternion rotacionInicial;

    // NUEVO: Para depuración
    private float ultimaDistanciaRegistrada = 999f;

    void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        if (modeloVisual == null)
            modeloVisual = gameObject;

        posicionInicial = transform.position;
        rotacionInicial = transform.rotation;

        ActualizarJugadorActivo();
    }

    void Update()
    {
        // Buscar jugador activo periódicamente
        temporizadorBusqueda -= Time.deltaTime;
        if (temporizadorBusqueda <= 0f)
        {
            ActualizarJugadorActivo();
            temporizadorBusqueda = frecuenciaBusquedaJugador;
        }

        if (jugadorActual == null || estaAtacando) return;

        float distancia = Vector3.Distance(transform.position, jugadorActual.position);
        ultimaDistanciaRegistrada = distancia;

        // DEBUG: Mostrar distancia cada cierto tiempo
        if (Time.frameCount % 60 == 0)
            Debug.Log($"[Tentáculo] Distancia al jugador: {distancia:F2} | Ataque necesario: {distanciaAtaque}");

        // 🔥 CRÍTICO: Si está en rango de ataque Y puede atacar
        if (distancia <= distanciaAtaque && puedeAtacar && !estaAtacando)
        {
            Debug.Log($"[Tentáculo] ¡DISTANCIA DE ATAQUE ALCANZADA! ({distancia:F2} <= {distanciaAtaque})");
            StartCoroutine(Atacar());
        }
        // Si está en rango de detección, perseguir
        else if (distancia <= distanciaDeteccion && !estaAtacando)
        {
            MoverHaciaJugador();
        }
        else if (distancia > distanciaDeteccion)
        {
            // Opcional: volver a posición inicial si está muy lejos
            // VolverPosicionInicial();
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
            if (jugadorActual != null)
                Debug.Log($"[Tentáculo] Persiguiendo a: {jugadorActual.name}");
        }
    }

    void MoverHaciaJugador()
    {
        if (jugadorActual == null) return;

        // Calcular dirección hacia el jugador
        Vector3 direccion = jugadorActual.position - transform.position;
        direccion.y = 0f;

        // Si está muy cerca, reducir velocidad para no pasarse
        float distancia = direccion.magnitude;
        if (distancia < distanciaAtaque * 0.8f)
        {
            // Reducir velocidad cuando está cerca del rango de ataque
            transform.position += direccion.normalized * velocidadPersecucion * 0.5f * Time.deltaTime;
        }
        else
        {
            transform.position += direccion.normalized * velocidadPersecucion * Time.deltaTime;
        }

        // Mantener altura original
        Vector3 pos = transform.position;
        pos.y = posicionInicial.y;
        transform.position = pos;

        // Efecto ondulante
        if (movimientoOndulante && modeloVisual != null && modeloVisual != gameObject)
        {
            float offsetX = Mathf.Sin(Time.time * frecuenciaOndulacion) * amplitudOndulacion;
            float offsetZ = Mathf.Cos(Time.time * frecuenciaOndulacion * 0.7f) * amplitudOndulacion;
            modeloVisual.transform.localPosition = new Vector3(offsetX, 0f, offsetZ);
        }
    }

    IEnumerator Atacar()
    {
        if (!puedeAtacar || estaAtacando) yield break;

        estaAtacando = true;
        puedeAtacar = false;

        Debug.Log($"[Tentáculo] Preparando ataque... Esperando {tiempoEsperaAntesAtaque}s");

        // 1. Esperar antes del latigazo
        yield return new WaitForSeconds(tiempoEsperaAntesAtaque);

        // Verificar que el jugador siga existiendo Y siga cerca
        if (jugadorActual == null || !jugadorActual.gameObject.activeInHierarchy)
        {
            Debug.Log("[Tentáculo] Jugador desapareció, cancelando ataque.");
            ResetearEstadoAtaque();
            yield break;
        }

        // Verificar distancia nuevamente (no atacar si se alejó)
        float distanciaActual = Vector3.Distance(transform.position, jugadorActual.position);
        if (distanciaActual > distanciaAtaque * 1.5f)
        {
            Debug.Log($"[Tentáculo] Jugador se alejó (distancia {distanciaActual:F2}), cancelando ataque.");
            ResetearEstadoAtaque();
            yield break;
        }

        Debug.Log("[Tentáculo] ¡LATIGAZO AHORA!");

        // 2. Sonido
        if (sonidoAtaque != null && audioSource != null)
            audioSource.PlayOneShot(sonidoAtaque);

        // 3. Guardar posición original y calcular punto de impacto
        Vector3 posicionOriginal = transform.position;
        Vector3 direccionAtaque = (jugadorActual.position - transform.position).normalized;
        direccionAtaque.y = 0f;
        Vector3 posicionImpacto = transform.position + direccionAtaque * distanciaLatigazo;

        // 4. Latigazo rápido hacia adelante
        float tiempo = 0f;
        float duracionLatigazo = 0.15f; // Fijo, más rápido y contundente

        while (tiempo < duracionLatigazo)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / duracionLatigazo;
            transform.position = Vector3.Lerp(posicionOriginal, posicionImpacto, t);
            yield return null;
        }

        // 5. Efecto de impacto (en la posición del jugador)
        if (particulaGolpe != null && jugadorActual != null)
        {
            Instantiate(particulaGolpe, jugadorActual.position, Quaternion.identity);
        }

        Debug.Log("[Tentáculo] IMPACTO!");

        // 6. Pequeña pausa en el impacto
        yield return new WaitForSeconds(0.1f);

        // 7. Retroceso rápido a posición original
        Vector3 posicionDespuesLatigazo = transform.position;
        tiempo = 0f;

        while (tiempo < tiempoRetroceso)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / tiempoRetroceso;
            transform.position = Vector3.Lerp(posicionDespuesLatigazo, posicionOriginal, t);
            yield return null;
        }

        // 8. Cooldown
        Debug.Log($"[Tentáculo] Ataque completado. Cooldown de {cooldownPostAtaque}s");
        yield return new WaitForSeconds(cooldownPostAtaque);

        ResetearEstadoAtaque();
    }

    void ResetearEstadoAtaque()
    {
        estaAtacando = false;
        puedeAtacar = true;
        Debug.Log("[Tentáculo] Listo para nuevo ataque.");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaDeteccion);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaAtaque);
    }

    public void ResetearTentaculo()
    {
        StopAllCoroutines();
        estaAtacando = false;
        puedeAtacar = true;
        transform.position = posicionInicial;
        transform.rotation = rotacionInicial;
    }
}