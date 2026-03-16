using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class MenuGlow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI texto;
    private Material materialTexto;

    void Start()
    {
        materialTexto = texto.fontMaterial;

        materialTexto.SetFloat("_GlowPower", 0f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        materialTexto.SetFloat("_GlowPower", 0.8f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        materialTexto.SetFloat("_GlowPower", 0f);
    }
}