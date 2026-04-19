using UnityEngine;

public class LogicaPies : MonoBehaviour
{
    public MovimientoPJ movimientoPJ;
    public VyreController vyreController;

    private void Start()
    {
        if (movimientoPJ == null && vyreController == null)
        {
            movimientoPJ = GetComponentInParent<MovimientoPJ>();
            vyreController = GetComponentInParent<VyreController>();
        }

        if (movimientoPJ == null && vyreController == null)
        {
            Debug.LogError("LogicaPies: No se encontró MovimientoPJ ni VyreController en el padre!");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (movimientoPJ != null)
            movimientoPJ.puedoSaltar = true;

        if (vyreController != null)
            vyreController.puedoSaltar = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (movimientoPJ != null)
            movimientoPJ.puedoSaltar = false;

        if (vyreController != null)
            vyreController.puedoSaltar = false;
    }
}