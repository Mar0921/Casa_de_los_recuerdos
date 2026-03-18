using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    [Header("Audio")]
    public AudioSource musicaFondo;
    public AudioSource efectosSonido;
    public Slider sliderMusica;
    public Slider sliderSonido;
    void Start()
    {
        // Conectar eventos de sliders automáticamente
        if (sliderMusica != null)
            sliderMusica.onValueChanged.AddListener(CambiarVolumenMusica);

        if (sliderSonido != null)
            sliderSonido.onValueChanged.AddListener(CambiarVolumenSonido);

        CargarPreferencias();
        MostrarMenuPrincipal();
    }
    void CargarPreferencias()
    {
        float volumenMusica = PlayerPrefs.GetFloat("VolumenMusica", 0.5f);
        if (musicaFondo != null)
            musicaFondo.volume = volumenMusica;
        if (sliderMusica != null)
            sliderMusica.value = volumenMusica;

        float volumenSonido = PlayerPrefs.GetFloat("VolumenSonido", 0.5f);
        if (efectosSonido != null)
            efectosSonido.volume = volumenSonido;
        if (sliderSonido != null)
            sliderSonido.value = volumenSonido;
    }
    public void CambiarVolumenMusica(float volumen)
    {
        if (musicaFondo != null)
            musicaFondo.volume = volumen;
        PlayerPrefs.SetFloat("VolumenMusica", volumen);
    }

    public void CambiarVolumenSonido(float volumen)
    {
        if (efectosSonido != null)
            efectosSonido.volume = volumen;
        PlayerPrefs.SetFloat("VolumenSonido", volumen);

        if (efectosSonido != null && !efectosSonido.isPlaying)
            efectosSonido.Play();
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