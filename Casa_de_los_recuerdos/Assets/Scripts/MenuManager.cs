using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Paneles principales")]
    public GameObject menuPrincipal;
    public GameObject panelOpciones;

    [Header("Paneles de opciones")]
    public GameObject panelSonido;
    public GameObject panelBrillo;
    public GameObject panelControles;
    public GameObject panelCreditos;

    void Start()
    {
        MostrarMenuPrincipal();
    }

    void DesactivarTodo()
    {
        menuPrincipal.SetActive(false);
        panelOpciones.SetActive(false);

        panelSonido.SetActive(false);
        panelBrillo.SetActive(false);
        panelControles.SetActive(false);
        panelCreditos.SetActive(false);
    }


    public void NuevoJuego()
    {
        SceneManager.LoadScene(1);
    }

    public void CargarJuego()
    {
        Debug.Log("Sistema de carga no definido");
    }

    public void AbrirOpciones()
    {
        DesactivarTodo();
        panelOpciones.SetActive(true);
    }

    public void MostrarMenuPrincipal()
    {
        DesactivarTodo();
        menuPrincipal.SetActive(true);
    }


    public void AbrirSonido()
    {
        DesactivarTodo();
        panelSonido.SetActive(true);
    }

    public void AbrirBrillo()
    {
        DesactivarTodo();
        panelBrillo.SetActive(true);
    }

    public void AbrirControles()
    {
        DesactivarTodo();
        panelControles.SetActive(true);
    }

    public void AbrirCreditos()
    {
        DesactivarTodo();
        panelCreditos.SetActive(true);
    }


    public void CerrarOpciones()
    {
        DesactivarTodo();
        panelOpciones.SetActive(true);
    }

    public void VolverMenuPrincipal()
    {
        MostrarMenuPrincipal();
    }
}