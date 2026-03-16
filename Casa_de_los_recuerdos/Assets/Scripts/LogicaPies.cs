using UnityEngine;

public class LogicaPies : MonoBehaviour
{
    public MovimientoPJ movimientoPJ;

    private void OnTriggerStay(Collider other)
    {
        movimientoPJ.puedoSaltar = true;
    }

    private void OnTriggerExit(Collider other)
    {
        movimientoPJ.puedoSaltar = false;
    }
}