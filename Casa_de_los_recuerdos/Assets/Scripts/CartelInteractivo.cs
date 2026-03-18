using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CartelInteractivo : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject panelImagen;
    public GameObject textoInteractuar;
    public GameObject botonCerrar;

    [Header("Configuración")]
    public float distanciaDeteccion = 3f;

    private Transform jugador;
    private bool imagenVisible = false;

    void Start()
    {
        jugador = GameObject.FindGameObjectWithTag("Player").transform;
        panelImagen.SetActive(false);
        textoInteractuar.SetActive(false);
        botonCerrar.SetActive(false);

        botonCerrar.GetComponent<Button>().onClick.AddListener(CerrarImagen);
    }

    void Update()
    {
        float distancia = Vector3.Distance(transform.position, jugador.position);
        bool cercano = distancia <= distanciaDeteccion;

        textoInteractuar.SetActive(cercano && !imagenVisible);

        if (cercano && Keyboard.current.fKey.wasPressedThisFrame)
        {
            imagenVisible = true;
            panelImagen.SetActive(true);
            botonCerrar.SetActive(true);
        }

        if (!cercano && imagenVisible)
        {
            CerrarImagen();
        }
    }

    public void CerrarImagen()
    {
        imagenVisible = false;
        panelImagen.SetActive(false);
        botonCerrar.SetActive(false);
    }
}