using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SelectionItemUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button selectButton;
    [SerializeField] private GameObject highlight;
    [SerializeField] private TextMeshProUGUI nameText;


    public SelectionItem Item { get; private set; }

    public void Initialize(SelectionItem item, Action onClick)
    {
        Item = item;
        iconImage.sprite = item.icon;
        descriptionText.text = item.description;
        nameText.text = item.name;
        selectButton.onClick.AddListener(() => onClick());
        highlight.SetActive(false);
    }

    public void SetSelected(bool isSelected)
    {
        highlight.SetActive(isSelected);
    }
}
