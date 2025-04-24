using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class RandomSelector : MonoBehaviour
{
    [Header("引用")]
    [SerializeField] private List<BuildingType> allBuildings;
    [SerializeField] private List<ResourceTypeSo> allResources;
    [SerializeField] private Transform selectionPanel;
    [SerializeField] private SelectionItemUI itemPrefab;
    [SerializeField] private Button confirmButton;

    private List<SelectionItem> currentOptions = new List<SelectionItem>();
    private SelectionItem selectedItem;

    void Start()
    {
        selectionPanel.gameObject.SetActive(false);
        confirmButton.onClick.AddListener(OnConfirm);
    }

    public void StartRandomSelection()
    {
        // 合并所有可选项
        var allItems = new List<SelectionItem>();

        foreach (var building in allBuildings)
        {
            allItems.Add(new SelectionItem
            {
                icon = building.iconRenderer.sprite,
                description = building.specialEffectDescription,
                data = building,
                isBuilding = true
            });
        }

        foreach (var resource in allResources)
        {
            allItems.Add(new SelectionItem
            {
                icon = resource.iconRenderer.sprite,
                description = resource.specialEffectDescription,
                data = resource,
                isBuilding = false
            });
        }

        // 随机选择三个不重复的选项
        currentOptions = allItems
            .OrderBy(x => Random.value)
            .Take(3)
            .ToList();

        // 显示UI
        ShowSelectionUI();
    }

    void ShowSelectionUI()
    {
        selectionPanel.gameObject.SetActive(true);
        Time.timeScale = 0; // 暂停游戏

        // 清空现有选项
        foreach (Transform child in selectionPanel)
        {
            if (child != confirmButton.transform) Destroy(child.gameObject);
        }

        // 生成新选项
        float spacing = 500f; // 选项间距
        for (int i = 0; i < currentOptions.Count; i++)
        {
            SelectionItemUI item = Instantiate(itemPrefab, selectionPanel);
            item.transform.localPosition = new Vector3(
                (i - 1) * spacing,
                0,
                0
            );

            item.Initialize(
                currentOptions[i],
                () => OnSelectItem(currentOptions[i])
            );
        }
    }

    void OnSelectItem(SelectionItem item)
    {
        selectedItem = item;
        confirmButton.interactable = true;

        // 高亮选中项（需要SelectionItemUI实现）
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
                //FindObjectOfType<BuildingPlacer>().StartPlacing(building);
            }
            else
            {
                ResourceTypeSo resource = selectedItem.data as ResourceTypeSo;
                //FindObjectOfType<ResourcePlacer>().StartPlacing(resource);
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
