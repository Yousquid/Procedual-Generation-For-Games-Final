using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SelectionItemUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button selectButton;
    //[SerializeField] private GameObject highlight;

    public SelectionItem Item { get; private set; }

    public void Initialize(SelectionItem item, Action onClick)
    {
        Item = item;
        iconImage.sprite = item.icon;
        descriptionText.text = item.description;
        selectButton.onClick.AddListener(() => onClick());
        //highlight.SetActive(false);
    }

    public void SetSelected(bool isSelected)
    {
        GetComponent<Image>().color = isSelected ? Color.yellow : Color.white;
    }
}
