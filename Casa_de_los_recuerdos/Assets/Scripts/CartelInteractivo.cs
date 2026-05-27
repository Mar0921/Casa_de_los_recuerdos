using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CartelInteractivo : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject panelImagen;
    public GameObject textoInteractuar;
    public GameObject botonCerrar;

    private bool jugadorCerca = false;
    private bool imagenVisible = false;

    void Start()
    {
        panelImagen.SetActive(false);
        textoInteractuar.SetActive(false);
        botonCerrar.SetActive(false);
        botonCerrar.GetComponent<Button>().onClick.AddListener(CerrarImagen);
    }

    void Update()
    {
        if (jugadorCerca && !imagenVisible && Keyboard.current.fKey.wasPressedThisFrame)
            AbrirImagen();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = true;
            textoInteractuar.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = false;
            textoInteractuar.SetActive(false);
            if (imagenVisible) CerrarImagen();
        }
    }

    private void AbrirImagen()
    {
        imagenVisible = true;
        panelImagen.SetActive(true);
        botonCerrar.SetActive(true);
        textoInteractuar.SetActive(false);
    }

    public void CerrarImagen()
    {
        imagenVisible = false;
        panelImagen.SetActive(false);
        botonCerrar.SetActive(false);
    }
}