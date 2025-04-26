using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class FogTile : MonoBehaviour
{
    public TextMeshPro priceText;


    private void Start()
    {
        priceText.gameObject.SetActive(false);
    }
    public void SetPrice(int price)
    {
        priceText.text = $"${price}";
    }

    public void SetPriceVisible(bool visible)
    {
        priceText.gameObject.SetActive(visible);
    }

    void OnMouseEnter()
    {
        SetPriceVisible(true);
    }

    void OnMouseExit()
    {
        SetPriceVisible(false);
    }
}
