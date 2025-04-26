using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using TMPro;

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

    public UIManager uIManager;

    public Button checkMapButton;
    public TextMeshProUGUI checkButtonText;
    private bool isCheckingMap = false;

    public int rollPrice = 15;
    public int rollTimes = 0;

    void Start()
    {
        selectionPanel.gameObject.SetActive(false);
        confirmButton.onClick.AddListener(OnConfirm);
        confirmButton.gameObject.SetActive(false);
        checkMapButton.gameObject.SetActive(false);
    }

    public void OnClickCheckMap()
    {
        if (!isCheckingMap)
        {
            checkButtonText.text = "Return Selection";
            selectionPanel.gameObject.SetActive(false);
            confirmButton.gameObject.SetActive(false);
            isCheckingMap = true;
        }
        else if (isCheckingMap)
        {
            checkButtonText.text = "Check Map";
            selectionPanel.gameObject.SetActive(true);
            confirmButton.gameObject.SetActive(true);
            isCheckingMap = false;
        }


    }
    public void StartRandomSelection()
    {
        rollTimes += 1;

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
        if (!isCheckingMap)
        {
            ShowSelectionUI();

        }
    }

    void ShowSelectionUI()
    {
        selectionPanel.gameObject.SetActive(true);
        confirmButton.gameObject.SetActive(true);
        uIManager.turnButton.gameObject.SetActive(false);
        uIManager.shopButton.gameObject.SetActive(false);
        checkMapButton.gameObject.SetActive(true);
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
                uIManager.turnButton.gameObject.SetActive(true);
                uIManager.shopButton.gameObject.SetActive(true);
            }
            else
            {
                ResourceTypeSo resource = selectedItem.data as ResourceTypeSo;
                gridManager.selectedResource = resource;
                gridManager.currentMode = GridManager.MouseMode.PlaceResource;
                confirmButton.gameObject.SetActive(false);
                uIManager.turnButton.gameObject.SetActive(true);
                uIManager.shopButton.gameObject.SetActive(true);
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        { 
            StartRandomSelection();
        }

        rollPrice = 10 + rollTimes * 10;
    }
}
