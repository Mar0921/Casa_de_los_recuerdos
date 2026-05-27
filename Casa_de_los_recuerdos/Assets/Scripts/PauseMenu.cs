using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("Panel de Pausa")]
    public GameObject panelPausa;

    [Header("Menú Principal")]
    public string nombreEscenaMenu = "MenuPrincipal"; // pon aquí el nombre exacto de tu escena

    private bool enPausa = false;

    void Start()
    {
        if (panelPausa != null)
            panelPausa.SetActive(false);
    }

    void Update()
    {
        // Escape para pausar y reanudar
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (enPausa)
                Reanudar();
            else
                Pausar();
        }

        // Shift para volver al menú principal (solo si está en pausa)
        if (enPausa && (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)))
        {
            VolverAlMenu();
        }
    }

    public void Pausar()
    {
        enPausa = true;
        Time.timeScale = 0f;
        if (panelPausa != null)
            panelPausa.SetActive(true);

        Debug.Log("Juego en pausa");
    }

    public void Reanudar()
    {
        enPausa = false;
        Time.timeScale = 1f;
        if (panelPausa != null)
            panelPausa.SetActive(false);

        Debug.Log("Juego reanudado");
    }

    public void VolverAlMenu()
    {
        Time.timeScale = 1f; // resetea el tiempo antes de cambiar de escena
        Debug.Log("Volviendo al menú principal...");
        SceneManager.LoadScene(nombreEscenaMenu);
    }
}