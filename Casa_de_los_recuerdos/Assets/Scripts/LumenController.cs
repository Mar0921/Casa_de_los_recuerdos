using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class LumenController : MonoBehaviour
{
    private MovimientoPJ movimiento;

    [Header("Pistas")]
    public GameObject[] pistas;
    public float duracionVision = 3f;
    private bool viendoPistas = false;

    void Start()
    {
        movimiento = GetComponent<MovimientoPJ>();

        foreach (GameObject pista in pistas)
        {
            pista.SetActive(false);
        }
    }

    void Update()
    {
        if (Keyboard.current.qKey.wasPressedThisFrame && !viendoPistas)
        {
            StartCoroutine(VisionPistas());
        }

        // 🔒 Lumen NO corre (forzamos velocidad base)
        if (movimiento != null)
        {
            movimiento.velocidadMovimiento = movimiento.velocidadNormal;
        }
    }

    IEnumerator VisionPistas()
    {
        viendoPistas = true;

        foreach (GameObject pista in pistas)
            pista.SetActive(true);

        yield return new WaitForSeconds(duracionVision);

        foreach (GameObject pista in pistas)
            pista.SetActive(false);

        viendoPistas = false;
    }
}