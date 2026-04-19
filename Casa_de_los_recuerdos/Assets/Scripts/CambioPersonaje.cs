using UnityEngine;
using System.Collections;

public class CambioPersonaje : MonoBehaviour
{
    public GameObject lumen;
    public GameObject vyre;
    public float cooldown = 2f;

    private bool puedeCambiar = true;
    private GameObject personajeActual;

    void Start()
    {
        personajeActual = lumen;
        lumen.SetActive(true);
        vyre.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && puedeCambiar)
        {
            StartCoroutine(CambiarPersonaje());
        }
    }

    IEnumerator CambiarPersonaje()
    {
        puedeCambiar = false;

        // Guardar posición y rotación
        Vector3 posicion = personajeActual.transform.position;
        Quaternion rotacion = personajeActual.transform.rotation;

        // 1️⃣ Efecto de desaparición en personaje ACTUAL (mientras está activo)
        yield return StartCoroutine(EfectoCambio(personajeActual, true)); // true = desaparecer

        // 2️⃣ Determinar nuevo personaje
        GameObject nuevoPersonaje;
        if (personajeActual == lumen)
        {
            nuevoPersonaje = vyre;
        }
        else
        {
            nuevoPersonaje = lumen;
        }

        // 3️⃣ ACTIVAR nuevo personaje ANTES de aplicar transformaciones
        nuevoPersonaje.SetActive(true);

        // 4️⃣ Aplicar posición y rotación
        nuevoPersonaje.transform.position = posicion;
        nuevoPersonaje.transform.rotation = rotacion;

        // 5️⃣ AHORA desactivar el anterior
        personajeActual.SetActive(false);

        // 6️⃣ Actualizar referencia
        personajeActual = nuevoPersonaje;

        // 7️⃣ Efecto de aparición (ahora el nuevo personaje está activo)
        yield return StartCoroutine(EfectoCambio(personajeActual, false)); // false = aparecer

        // Cooldown
        yield return new WaitForSeconds(cooldown);
        puedeCambiar = true;
    }

    IEnumerator EfectoCambio(GameObject personaje, bool desaparecer)
    {
        Renderer rend = personaje.GetComponentInChildren<Renderer>();

        if (rend != null)
        {
            float tiempo = 0.2f;
            float t = 0;
            Color colorOriginal = rend.material.color;

            if (desaparecer)
            {
                // Fade out (1 → 0)
                while (t < tiempo)
                {
                    t += Time.deltaTime;
                    float alpha = Mathf.Lerp(1f, 0f, t / tiempo);
                    rend.material.color = new Color(colorOriginal.r, colorOriginal.g, colorOriginal.b, alpha);
                    yield return null;
                }
            }
            else
            {
                // Fade in (0 → 1)
                rend.material.color = new Color(colorOriginal.r, colorOriginal.g, colorOriginal.b, 0f);
                while (t < tiempo)
                {
                    t += Time.deltaTime;
                    float alpha = Mathf.Lerp(0f, 1f, t / tiempo);
                    rend.material.color = new Color(colorOriginal.r, colorOriginal.g, colorOriginal.b, alpha);
                    yield return null;
                }
            }
        }
        else
        {
            yield return new WaitForSeconds(0.2f);
        }
    }
}