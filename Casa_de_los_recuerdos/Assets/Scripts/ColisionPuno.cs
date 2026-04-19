using UnityEngine;

public class ColisionPuno : MonoBehaviour
{
    private VyreController vyreController;
    private Collider coliderPuno;

    void Start()
    {
        vyreController = GetComponentInParent<VyreController>();
        coliderPuno = GetComponent<Collider>();
        coliderPuno.isTrigger = true;
        coliderPuno.enabled = false; // ✅ empieza desactivado
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Rompible"))
        {
            Destroy(other.gameObject);
        }
    }

    // ✅ llamado por VyreController al empezar el golpe
    public void ActivarPuno()
    {
        coliderPuno.enabled = true;
    }

    // ✅ llamado por VyreController al terminar el golpe
    public void DesactivarPuno()
    {
        coliderPuno.enabled = false;
    }
}