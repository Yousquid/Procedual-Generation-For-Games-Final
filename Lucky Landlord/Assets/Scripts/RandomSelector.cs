using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class RandomSelector : MonoBehaviour
{
    [Header("����")]
    [SerializeField] private List<BuildingType> allBuildings;
    [SerializeField] private List<ResourceTypeSo> allResources;
    [SerializeField] private Transform selectionPanel;
    [SerializeField] private SelectionItemUI itemPrefab;
    [SerializeField] private Button confirmButton;

    private List<SelectionItem> currentOptions = new List<SelectionItem>();
    private SelectionItem selectedItem;
    public GridManager gridManager;

    void Start()
    {
        selectionPanel.gameObject.SetActive(false);
        confirmButton.onClick.AddListener(OnConfirm);
    }

    public void StartRandomSelection()
    {
        gridManager.currentMode = GridManager.MouseMode.UISelection;
        
        var allItems = new List<SelectionItem>();

        foreach (var building in allBuildings)
        {
            allItems.Add(new SelectionItem
            {
                icon = building.iconRenderer.sprite,
                description = building.specialEffectDescription,
                data = building,
                name = building.buildingName,
                isBuilding = true
            });;
        }

        foreach (var resource in allResources)
        {
            allItems.Add(new SelectionItem
            {
                icon = resource.iconRenderer.sprite,
                description = resource.specialEffectDescription,
                data = resource,
                name = resource.resourceName,
                isBuilding = false
            });
        }

        // ���ѡ���������ظ���ѡ��
        currentOptions = allItems
            .OrderBy(x => Random.value)
            .Take(3)
            .ToList();

        // ��ʾUI
        ShowSelectionUI();
    }

    void ShowSelectionUI()
    {
        selectionPanel.gameObject.SetActive(true);
        confirmButton.gameObject.SetActive(true);
        Time.timeScale = 0; // ��ͣ��Ϸ

        // �������ѡ��
        foreach (Transform child in selectionPanel)
        {
            if (child != confirmButton.transform) Destroy(child.gameObject);
        }

        // ������ѡ��
        float spacing = 640f; // ѡ����
        for (int i = 0; i < currentOptions.Count; i++)
        {
            var option = currentOptions[i];  // ���ᵱǰ��
            SelectionItemUI item = Instantiate(itemPrefab, selectionPanel);
            item.transform.localPosition = new Vector3(
                (i - 1) * spacing,
                0,
                0
            );

            item.Initialize(
                currentOptions[i],
                () => OnSelectItem(option)
            );
        }
    }

    void OnSelectItem(SelectionItem item)
    {
        selectedItem = item;
        confirmButton.interactable = true;

        // ����ѡ�����ҪSelectionItemUIʵ�֣�
        foreach (var ui in selectionPanel.GetComponentsInChildren<SelectionItemUI>())
        {
            ui.SetSelected(ui.Item == item);
        }
    }

    void OnConfirm()
    {
        Time.timeScale = 1;
        selectionPanel.gameObject.SetActive(false);

        if (selectedItem != null)
        {
            if (selectedItem.isBuilding)
            {
                BuildingType building = selectedItem.data as BuildingType;
                gridManager.selectedBuilding = building;
                gridManager.currentMode = GridManager.MouseMode.PlaceBuilding;
                confirmButton.gameObject.SetActive(false);

            }
            else
            {
                ResourceTypeSo resource = selectedItem.data as ResourceTypeSo;
                gridManager.selectedResource = resource;
                gridManager.currentMode = GridManager.MouseMode.PlaceResource;
                confirmButton.gameObject.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        { 
            StartRandomSelection();
        }
    }
}
