using UnityEngine;
using System.Collections;

public class GestorObjetosYPuertas : MonoBehaviour
{
    [Header("Conteo de Objetos")]
    public int totalObjetos = 3;
    private int objetosRecogidos = 0;

    [Header("Puertas")]
    public GameObject[] puertas;
    public float duracionDesvanecimiento = 1.5f;

    [Header("Animación")]
    public AnimationCurve curvaDesvanecimiento = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    [Header("Partículas")]
    public ParticleSystem[] particulasAlActivar;

    [Header("Audio Global")]
    public AudioClip sonidoPuertas;
    public AudioSource audioSourceGlobal;

    void Start()
    {
        if (audioSourceGlobal == null)
            audioSourceGlobal = GetComponent<AudioSource>();

        // Asegurarse que las partículas estén apagadas al inicio
        foreach (ParticleSystem ps in particulasAlActivar)
            if (ps != null) ps.Stop();

        ObjetoOscuro[] objetos = FindObjectsByType<ObjetoOscuro>(FindObjectsSortMode.None);
        Debug.Log($"📦 Objetos ObjetoOscuro encontrados: {objetos.Length}");

        foreach (ObjetoOscuro obj in objetos)
        {
            obj.RegistrarGestor(this);
            Debug.Log($"   ✅ Registrado: {obj.name}");
        }
    }

    public void RecogerObjeto(string nombreObjeto)
    {
        objetosRecogidos++;
        Debug.Log($"🎉 OBJETO RECOGIDO: {nombreObjeto} ({objetosRecogidos}/{totalObjetos})");

        if (objetosRecogidos >= totalObjetos)
        {
            Debug.Log("🏆 ¡TODOS LOS OBJETOS RECOGIDOS! Desvaneciendo puertas...");
            DesvanecerPuertas();
        }
    }

    void DesvanecerPuertas()
    {
        if (sonidoPuertas != null && audioSourceGlobal != null)
            audioSourceGlobal.PlayOneShot(sonidoPuertas);

        // Activar partículas permanentemente
        foreach (ParticleSystem ps in particulasAlActivar)
        {
            if (ps != null)
            {
                // Configurar para que no se detengan solas
                var main = ps.main;
                main.loop = true;
                main.duration = 999f;
                ps.Play();
                Debug.Log($"✨ Partículas activadas: {ps.name}");
            }
        }

        foreach (GameObject puerta in puertas)
        {
            if (puerta != null)
                StartCoroutine(AnimarDesaparicion(puerta));
        }
    }

    IEnumerator AnimarDesaparicion(GameObject puerta)
    {
        Vector3 escalaInicial = puerta.transform.localScale;
        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < duracionDesvanecimiento)
        {
            tiempoTranscurrido += Time.deltaTime;
            float progreso = tiempoTranscurrido / duracionDesvanecimiento;
            float escala = curvaDesvanecimiento.Evaluate(progreso);
            puerta.transform.localScale = escalaInicial * escala;
            yield return null;
        }

        puerta.transform.localScale = Vector3.zero;
        puerta.SetActive(false);
        Debug.Log($"✅ {puerta.name} desaparecida");
    }
}