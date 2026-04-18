using UnityEngine;
using System.Collections;

public class CambioPersonaje : MonoBehaviour
{
    public GameObject lumen;
    public GameObject vyre;

    public float cooldown = 2f; // tiempo entre cambios
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

        // 🔥 Efecto simple (puedes reemplazarlo por partículas)
        yield return StartCoroutine(EfectoCambio(personajeActual));

        // Cambiar personaje
        if (personajeActual == lumen)
        {
            lumen.SetActive(false);
            vyre.SetActive(true);
            personajeActual = vyre;
        }
        else
        {
            vyre.SetActive(false);
            lumen.SetActive(true);
            personajeActual = lumen;
        }

        // Aplicar misma posición
        personajeActual.transform.position = posicion;
        personajeActual.transform.rotation = rotacion;

        // Efecto al aparecer
        yield return StartCoroutine(EfectoCambio(personajeActual));

        // Cooldown
        yield return new WaitForSeconds(cooldown);
        puedeCambiar = true;
    }

    IEnumerator EfectoCambio(GameObject personaje)
    {
        Renderer rend = personaje.GetComponentInChildren<Renderer>();

        if (rend != null)
        {
            float tiempo = 0.2f;
            float t = 0;

            Color colorOriginal = rend.material.color;

            // Fade a negro (tipo sombra)
            while (t < tiempo)
            {
                t += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, t / tiempo);
                rend.material.color = new Color(colorOriginal.r, colorOriginal.g, colorOriginal.b, alpha);
                yield return null;
            }

            // Restaurar
            t = 0;
            while (t < tiempo)
            {
                t += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, t / tiempo);
                rend.material.color = new Color(colorOriginal.r, colorOriginal.g, colorOriginal.b, alpha);
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(0.2f);
        }
    }
}