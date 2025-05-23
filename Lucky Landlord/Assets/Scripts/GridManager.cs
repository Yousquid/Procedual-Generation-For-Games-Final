﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using TMPro;
using static GridBuildingSystem2D;
using Unity.VisualScripting;



public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int width = 10;
    [SerializeField] private int height = 10;
    [SerializeField] private float cellSize = 10f;
    [SerializeField] private Vector3 originPosition = Vector3.zero;

    public List<LandscapeTypeSo> landscapeTypes;

    public List<ResourceTypeSo> resourceTypes;

    public List<BuildingType> buildingTypes;

    private Grid<GameGridObject> grid;

    private Dictionary<Vector2Int, Transform> cellParents = new Dictionary<Vector2Int, Transform>();

    public GameObject fogPrefab;

    public BuildingType selectedBuilding;
    public ResourceTypeSo selectedResource;

    public UIManager uiManager;

    private TurnManager turnManager;

    public enum MouseMode { PlaceBuilding, PlaceResource, ExplorationMap, UISelection }
    public MouseMode currentMode = MouseMode.PlaceBuilding;
    public GameObject currentGhostObject;
    private Vector2Int? currentGhostGridPos = null;

    public static int wealth = 0;

    [SerializeField] private Camera mainCamera;

    [SerializeField] private Transform gridParent;

    [SerializeField] private GameObject incomeTextPrefab; // 拖入TextMeshPro预制体
    private Dictionary<Vector2Int, TextMeshPro> incomeTexts = new Dictionary<Vector2Int, TextMeshPro>();

    public int fogRevealingPrice = 15;

    public int revealedFogNumber = 0;

    public class GameGridObject
    {
        public LandscapeTypeSo landscape;
        public ResourceTypeSo resource;
        public List<BuildingType> buildings;

        public int productionOutput;
        public bool hasFogOfWar;

        public GameGridObject(LandscapeTypeSo landscape, ResourceTypeSo resource)
        {
            this.landscape = landscape;
            this.resource = resource;
            this.buildings = new List<BuildingType>();
            productionOutput = 0;
            hasFogOfWar = true; // 初始默认有战争迷雾
        }

        public override string ToString()
        {
            return $"{landscape}\n{resource}\n{buildings}";
        }
    }

    private void Start()
    {
        InitializeGrid();
        InitializeFogOfWar();
        GenerateResources();
        VisualizeGrid();
        wealth = 50;
        turnManager = GetComponent<TurnManager>();
    }

    private void InitializeGrid()
    {
        grid = new Grid<GameGridObject>(
            width,
            height,
            cellSize,
            originPosition,
            (g, x, y) => new GameGridObject(null, null)
        );

        float[,] noiseMap = GeneratePerlinNoiseMap();
        LandscapeTypeSo[,] tempLandscapeMap = new LandscapeTypeSo[width, height];

        // Step 1: Use Perlin noise to bias terrain regions
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                tempLandscapeMap[x, y] = GetBaseLandscapeType(noiseMap[x, y]);
            }
        }

        // Step 2: Apply influence propagation for smoother blending
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Dictionary<LandscapeTypeSo, float> probabilities = new Dictionary<LandscapeTypeSo, float>();
                foreach (var landscape in landscapeTypes)
                {
                    probabilities[landscape] = landscape.baseProbability;
                }

                // Adjust based on neighbor influence matrix
                foreach (var dir in GetDirections())
                {
                    int nx = x + dir.x;
                    int ny = y + dir.y;

                    if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                    {
                        var neighbor = tempLandscapeMap[nx, ny];
                        if (neighbor != null)
                        {
                            foreach (var influence in neighbor.influenceModifiers)
                            {
                                if (probabilities.ContainsKey(influence.target))
                                {
                                    probabilities[influence.target] += influence.weight;
                                }
                            }
                        }
                    }
                }

                // Pick the most likely terrain
                LandscapeTypeSo chosen = WeightedRandomPick(probabilities);
                grid.SetGridObject(x, y, new GameGridObject(chosen, null));
            }
        }
    }
    public void RevealFogOfWar(Vector2Int center, int radius)
    {
        for (int x = center.x - radius; x <= center.x + radius; x++)
        {
            for (int y = center.y - radius; y <= center.y + radius; y++)
            {
                
                    GameGridObject cell = grid.GetGridObject(x, y);
                    if (cell.hasFogOfWar)
                    {
                        cell.hasFogOfWar = false;
                        grid.TriggerGridObjectChanged(x, y);
                    }
                
            }
        }
    }

    
    private void VisualizeGrid()
    {
        // 使用缓存系统避免重复生成
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int gridPos = new Vector2Int(x, y);
                Vector3 worldPos = grid.GetWorldPosition(x, y) + new Vector3(cellSize / 2, cellSize / 2);

                // 获取或创建父物体
                if (!cellParents.TryGetValue(gridPos, out Transform cellParent))
                {
                    cellParent = CreateCellParent(x, y, worldPos);
                    cellParents.Add(gridPos, cellParent);
                }

                // 清除旧的可视化内容
                ClearCellVisualization(cellParent);

                // 生成新内容
                GameGridObject gridObj = grid.GetGridObject(x, y);
                VisualizeTerrain(gridObj, cellParent);
                VisualizeResource(gridObj, cellParent);
                VisualizeBuilding(gridObj, cellParent);
                VisualizeFogOfWar(gridObj, cellParent); // 新增迷雾可视化
                UpdateIncomeVisuals();

            }
        }
    }

    private void ClearCellVisualization(Transform parent)
    {
        // 使用倒序遍历避免修改集合问题
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(parent.GetChild(i).gameObject);
        }
    }

    private void InitializeFogOfWar()
    {
        int centerX = width / 2;
        int centerY = height / 2;

        // 设置中心4x4区域（适配不同尺寸）
        int revealSize = 3;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                bool isCenterArea =
                    x >= centerX - revealSize / 2 &&
                    x <= centerX + revealSize / 2 &&
                    y >= centerY - revealSize / 2 &&
                    y <= centerY + revealSize / 2;

                grid.GetGridObject(x, y).hasFogOfWar = !isCenterArea;
            }
        }
    }

    private void VisualizeFogOfWar(GameGridObject gridObj, Transform parent)
    {
        if (gridObj.hasFogOfWar && fogPrefab != null)
        {
            GameObject fog = Instantiate(
                fogPrefab,
                parent.position,
                Quaternion.identity,
                parent
            );

            FogTile fogScript = fog.GetComponent<FogTile>();

            fogScript.SetPrice(fogRevealingPrice);

        }
    }


    private void OnGridChanged(int x, int y)
    {
        if (cellParents.TryGetValue(new Vector2Int(x, y), out Transform parent))
        {
            ClearCellVisualization(parent);
            GameGridObject gridObj = grid.GetGridObject(x, y);
            VisualizeTerrain(gridObj, parent);
            VisualizeResource(gridObj, parent);
        }
    }

    private Transform CreateCellParent(int x, int y, Vector3 position)
    {
        GameObject parentObj = new GameObject($"Cell_{x}_{y}");
        parentObj.transform.position = position;
        parentObj.transform.SetParent(gridParent);
        return parentObj.transform;
    }

    private void VisualizeTerrain(GameGridObject gridObj, Transform parent)
    {
        if (gridObj.landscape?.landscapePrefab != null)
        {
            GameObject terrain = Instantiate(
                gridObj.landscape.landscapePrefab,
                parent.position,
                Quaternion.identity,
                parent
            );
            terrain.name = "Terrain";
            SetSortingOrder(terrain, 0); // 地形在最底层
        }
    }

    private void VisualizeResource(GameGridObject gridObj, Transform parent)
    {
        if (gridObj.resource?.resourcePrefab != null)
        {
         
            GameObject resource = Instantiate(
                gridObj.resource.resourcePrefab,
                parent.position,
                Quaternion.identity,
                parent
            );
            resource.name = "Resource";
            SetSortingOrder(resource, 3); // 资源在顶层

            // 添加资源动画（可选）
            if (resource.TryGetComponent<Animator>(out var anim))
            {
                anim.Play("ResourceIdle");
            }
        }
    }

    private void VisualizeBuilding(GameGridObject gridObj, Transform parent)
    {
        if (gridObj.buildings == null || gridObj.buildings.Count == 0)
            return;

        // 创建建筑容器
        GameObject buildingContainer = new GameObject("Buildings");
        buildingContainer.transform.SetParent(parent);
        buildingContainer.transform.localPosition = Vector3.zero;

        // 遍历所有建筑
        for (int i = 0; i < gridObj.buildings.Count; i++)
        {
            BuildingType buildingType = gridObj.buildings[i];
            if (buildingType?.buildingPrefab == null) continue;

       
            GameObject building = Instantiate(
                buildingType.buildingPrefab,
                parent.position + new Vector3(0, -0.17f, 0),
                Quaternion.identity,
                buildingContainer.transform 
            );
            building.name = $"Building_{buildingType.name}_{i}";

            // 设置层级（建筑在资源之上，迷雾之下）
            SetSortingOrder(building, 2);
        }
    }

    private void SetSortingOrder(GameObject obj, int order)
    {
        if (obj.TryGetComponent<SpriteRenderer>(out var renderer))
        {
            renderer.sortingOrder = order;
        }
    }

    // 修改后的资源生成方法（添加可视化标记）
    private void GenerateResources()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameGridObject cell = grid.GetGridObject(x, y);
                // 清除旧资源（如果需要重新生成）
                cell.resource = null;

                LandscapeTypeSo landscape = cell.landscape;
                if (landscape?.resourceProbabilities == null ||
                    landscape.resourceProbabilities.Count == 0)
                    continue;

                // 使用更精确的概率算法
                float totalWeight = 1 + landscape.resourceProbabilities
                    .Sum(r => r.probability);

                if (totalWeight <= 0) continue;

                float rnd = Random.Range(0f, totalWeight);
                float cumulative = 0f;

                foreach (var rp in landscape.resourceProbabilities)
                {
                    cumulative += rp.probability;
                    if (rnd <= cumulative && rp.resource != null)
                    {
                        cell.resource = rp.resource;
                        grid.TriggerGridObjectChanged(x, y); // 通知刷新显示
                        break;
                    }
                }
            }
        }
    }
    private float[,] GeneratePerlinNoiseMap()
    {
        float[,] noise = new float[width, height];
        float scale = 0.1f;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float xCoord = x * scale;
                float yCoord = y * scale;
                noise[x, y] = Mathf.PerlinNoise(xCoord, yCoord);
            }
        }
        return noise;
    }

    private LandscapeTypeSo GetBaseLandscapeType(float noiseValue)
    {
        // Divide noise range into segments
        float step = 1f / landscapeTypes.Count;
        for (int i = 0; i < landscapeTypes.Count; i++)
        {
            if (noiseValue <= step * (i + 1))
                return landscapeTypes[i];
        }
        return landscapeTypes[landscapeTypes.Count - 1];
    }

    private LandscapeTypeSo WeightedRandomPick(Dictionary<LandscapeTypeSo, float> weights)
    {
        float total = 0f;
        foreach (var w in weights.Values) total += Mathf.Max(w, 0);
        float random = Random.Range(0, total);
        float sum = 0;
        foreach (var pair in weights)
        {
            sum += Mathf.Max(pair.Value, 0);
            if (random <= sum) return pair.Key;
        }
        return null;
    }

    private List<Vector2Int> GetDirections()
    {
        return new List<Vector2Int> {
            new Vector2Int(-1, 0), new Vector2Int(1, 0),
            new Vector2Int(0, -1), new Vector2Int(0, 1)
        };
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(0);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            VisualizeGrid();
        }

        ClickAndRevealFogCheck();
        BuildingModeGhostObjectPreview();
        OnClikEvent();
        if (wealth < 0)
        {
            wealth = 0;
        }
    }

    private List<GameGridObject> GetNeighbors(int x, int y)
    {
        List<GameGridObject> neighbors = new List<GameGridObject>();
        int[,] offsets = new int[,] { 
        { -1,  0 }, // 左
        {  1,  0 }, // 右
        {  0, -1 }, // 下
        {  0,  1 }, // 上
        { -1, -1 }, // 左下
        { -1,  1 }, // 左上
        {  1, -1 }, // 右下
        {  1,  1 }  };// 右上

        for (int i = 0; i < offsets.GetLength(0); i++)
        {
            int nx = x + offsets[i, 0];
            int ny = y + offsets[i, 1];

            if (nx >= 0 && ny >= 0 && nx < grid.GetWidth() && ny < grid.GetHeight())
            {
                neighbors.Add(grid.GetGridObject(nx, ny));
            }
        }

        return neighbors;
    }

    private List<GameGridObject> GetNeighborsIncludingSelf(int x, int y)
    {
        List<GameGridObject> neighbors = new List<GameGridObject>();
        int[,] offsets = new int[,] {
        { -1,  0 }, // 左
        {  1,  0 }, // 右
        {  0, -1 }, // 下
        {  0,  1 }, // 上
        { -1, -1 }, // 左下
        { -1,  1 }, // 左上
        {  1, -1 }, // 右下
        {  1,  1 },
        { 0,0} };// 右上

        for (int i = 0; i < offsets.GetLength(0); i++)
        {
            int nx = x + offsets[i, 0];
            int ny = y + offsets[i, 1];

            if (nx >= 0 && ny >= 0 && nx < grid.GetWidth() && ny < grid.GetHeight())
            {
                neighbors.Add(grid.GetGridObject(nx, ny));
            }
        }

        return neighbors;
    }

    public int CalculateIncome(int x, int y)
    {
        GameGridObject gridObject = grid.GetGridObject(x, y);
        int income = 0;

        // 1. 基础收益
        if (gridObject.landscape != null) income += gridObject.landscape.baseIncomeValue;
        if (gridObject.resource != null) income += gridObject.resource.baseIncomeValue;
        foreach (var building in gridObject.buildings)
        {
            income += building.baseIncomeValue;
        }

        // 2. 相邻格子获取
        List<GameGridObject> neighbors = GetNeighbors(x, y);

        foreach (GameGridObject neighbor in neighbors)
        {
            // 地形加成
            if (gridObject.landscape != null &&
                gridObject.landscape.bonusFromAdjacentLandscapes.Contains(neighbor.landscape)
                && neighbor.hasFogOfWar == false)
            {
                income += gridObject.landscape.incomePerMatch;
            }

            
        }

        List<GameGridObject> neighborsIncludingSelf = GetNeighborsIncludingSelf(x, y);

        // 建筑加成
        foreach (GameGridObject neighbor in neighborsIncludingSelf)
        {
            foreach (var building in gridObject.buildings)
            {
                if (building.bonusFromAdjacentLandscapes.Contains(neighbor.landscape) && neighbor.hasFogOfWar == false)
                    income += building.incomePerMatch;

                if (building.bonusFromAdjacentResources.Contains(neighbor.resource) && neighbor.hasFogOfWar == false)
                    income += building.incomePerMatch;

                foreach (var bonusBuilding in building.bonusFromAdjacentBuildings)
                {
                    if (neighbor.buildings.Contains(bonusBuilding))
                    {
                        income += building.incomePerMatch;
                    }
                }
            }
        }
        

        // 3. 邻居中具有翻倍效果的建筑（每种类型只生效一次）
        HashSet<BuildingType> appliedDoubleTypes = new HashSet<BuildingType>();
        foreach (var neighbor in neighbors)
        {
            foreach (var neighborBuilding in neighbor.buildings)
            {
                if (neighborBuilding.doublesAdjacentIncome &&
                    !appliedDoubleTypes.Contains(neighborBuilding))
                {
                    income *= 2;
                    appliedDoubleTypes.Add(neighborBuilding);
                }
            }
        }

        // 4. 自身资源翻倍效果
        if (gridObject.resource != null && gridObject.resource.doubleGridIncome && gridObject.resource.canDoubleIncomeLandscapes.Contains(gridObject.landscape)
            )
        {
            income *= 2;
        }
        else if (gridObject.resource != null && gridObject.resource.doubleGridIncome && gridObject.resource.canDoubleIncomeBuildings.Any(b => gridObject.buildings.Contains(b)))
        {
            income *= 2;
        }

            return income;
    }

    public Dictionary<Vector2Int, int> GetCalculateAllIncomeWithoutFogMap()
    {
        Dictionary<Vector2Int, int> incomeMap = new Dictionary<Vector2Int, int>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameGridObject gridObject = grid.GetGridObject(x, y);
                if (gridObject.hasFogOfWar) continue;

                int income = CalculateIncome(x, y);
                incomeMap[new Vector2Int(x, y)] = income;
            }
        }

        return incomeMap;
    }
    public bool IsValidGridPosition(int x, int y)
    {
        return x >= 0 && y >= 0 && x < width && y < height;
    }
    public int GetAllIncomeValue()
    {
        int totalIncome = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameGridObject gridObject = grid.GetGridObject(x, y);
                if (gridObject.hasFogOfWar) continue;

                totalIncome += CalculateIncome(x, y);
            }
        }

        return totalIncome;
    }

    private void UpdateIncomeVisuals()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                GameGridObject gridObj = grid.GetGridObject(x, y);

                if (gridObj.hasFogOfWar) continue;

                int income = CalculateIncome(x, y);

                if (!incomeTexts.ContainsKey(pos))
                {
                    GameObject textObj = Instantiate(
                        incomeTextPrefab,
                        grid.GetWorldPosition(x, y) + new Vector3(cellSize / 2f, cellSize / 2f + 0.2f),
                        Quaternion.identity,
                        gridParent // 挂在格子父节点下，便于管理
                    );

                    TextMeshPro text = textObj.GetComponent<TextMeshPro>();
                    incomeTexts[pos] = text;
                }

                incomeTexts[pos].text = $"+{income}";
            }
        }
    }


    private void ClickAndRevealFogCheck()
    {
        if (Input.GetMouseButtonDown(0) &&  currentMode == MouseMode.ExplorationMap)
        {
            int x = 0;
            int y = 0;
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mouseWorldPos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos2D, Vector2.zero);
            if (hit.collider != null)
            {
                Vector3 worldPos = hit.point;
                print(worldPos);
                grid.GetXY(worldPos,out x, out y);

                if (IsValidGridPosition(x, y))
                {
                    GameGridObject gridObject = grid.GetGridObject(x, y);

                    if (gridObject.hasFogOfWar && wealth >= fogRevealingPrice)
                    {
                        gridObject.hasFogOfWar = false;
                        grid.TriggerGridObjectChanged(x, y);
                        revealedFogNumber++;
                        fogRevealingPrice = 10 + (revealedFogNumber * 4) ;
                        wealth -= fogRevealingPrice;
                        VisualizeGrid();
                    }
                    else if (wealth < fogRevealingPrice)
                    {
                        uiManager.SetRemindText("No enough money to buy this land!");
                    }
                }

            }
            
        }
    }

    private void PlaceBuilding(int x, int y)
    {
        var gridObject = grid.GetGridObject(x, y);

        if (gridObject.hasFogOfWar ) return;

        if (selectedBuilding.allowedLandscapes.Count > 0 && !selectedBuilding.allowedLandscapes.Contains(gridObject.landscape))
        {
            uiManager.SetRemindText($"Cannot place {selectedBuilding.buildingName} on {gridObject.landscape.name}.");
            return;
        }

        if (gridObject.buildings != null && gridObject.buildings.Count >0)
        {
            return;
        }

        List<GameGridObject> neighbors = GetNeighbors(x, y);

        List<LandscapeTypeSo> neightborsLandscapes = new List<LandscapeTypeSo>();
        
        List<BuildingType> neighborBuildings = new List<BuildingType>();

        if (selectedBuilding.mustHaveLandscapes.Count > 0)
        {
            foreach (var neighbor in neighbors)
            { 
                neightborsLandscapes.Add(neighbor.landscape);
            }

            if (!selectedBuilding.mustHaveLandscapes.Any(b => neightborsLandscapes.Contains(b)))
            {
                return;
            }
        }

        if (selectedBuilding.mustHaveBuildings.Count > 0)
        {
            foreach (var neighbor in neighbors)
            {
                if (neighbor.buildings.Count > 0) neighborBuildings.Add(neighbor.buildings[0]);
            }

            if (neighborBuildings != null && !selectedBuilding.mustHaveBuildings.Any(b => neighborBuildings.Contains(b)))
            {
                return;
            }
            else if (neighborBuildings == null)
                return;
        }

        gridObject.buildings.Add(selectedBuilding);
        grid.TriggerGridObjectChanged(x, y);
        currentMode = MouseMode.ExplorationMap;
        selectedBuilding = null;
        VisualizeGrid();

    }

    private void PlaceResource(int x, int y)
    {
        var gridObject = grid.GetGridObject(x, y);

        if (gridObject.hasFogOfWar) return;

        if (selectedResource.allowedLandscapes != null && !selectedResource.allowedLandscapes.Contains(gridObject.landscape))
        {
            uiManager.SetRemindText($"Cannot place {selectedResource.resourceName} on {gridObject.landscape.name}.");
            return;
        }

        if (gridObject.resource != null)
        {
            return;
        }

        gridObject.resource = selectedResource;
        grid.TriggerGridObjectChanged(x, y);
        selectedResource = null;
        currentMode = MouseMode.ExplorationMap;
        VisualizeGrid();
    }

    private void BuildingModeGhostObjectPreview()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        grid.GetXY(mouseWorldPos, out int x, out int y);
        GameGridObject gridObject = grid.GetGridObject(x, y);

        // 只在显示时添加偏移！
        Vector3 visualOffset = Vector3.zero;
        if (currentMode == MouseMode.PlaceBuilding)
        {
            visualOffset = new Vector3(0, -0.17f, 0); // 更建议用 Y 轴偏移
        }

        if (IsValidGridPosition(x, y) && !gridObject.hasFogOfWar)
        {
            Vector2Int gridPos = new Vector2Int(x, y);

            // 🔸逻辑坐标比较不加偏移
            if (currentGhostGridPos != null && currentGhostGridPos.Value == gridPos) return;

            currentGhostGridPos = gridPos;

            // 真实位置 + 显示偏移
            Vector3 cellCenter = grid.GetWorldPosition(x, y) + new Vector3(cellSize / 2f, cellSize / 2f, 0f);
            Vector3 displayPosition = cellCenter + visualOffset;

            if (currentGhostObject != null)
            {
                currentGhostObject.transform.position = displayPosition;
            }
            else
            {
                GameObject prefabToUse = null;

                if (selectedBuilding != null && currentMode == MouseMode.PlaceBuilding)
                    prefabToUse = selectedBuilding.buildingPrefab;
                else if (selectedResource != null && currentMode == MouseMode.PlaceResource)
                    prefabToUse = selectedResource.resourcePrefab;

                if (prefabToUse != null && currentMode != MouseMode.ExplorationMap && currentMode != MouseMode.UISelection)
                {
                    currentGhostObject = Instantiate(prefabToUse, displayPosition, Quaternion.identity);
                    SetGhostStyle(currentGhostObject);
                }
            }
        }
        else
        {
            if (currentGhostObject != null)
            {
                Destroy(currentGhostObject);
                currentGhostObject = null;
                currentGhostGridPos = null;
            }
        }
    }



    private void ClearGhostObject()
    {
        if (currentGhostObject != null)
        {
            Destroy(currentGhostObject);
            currentGhostObject = null;
            currentGhostGridPos = null;
        }
    }

    private void SetGhostStyle(GameObject ghost)
    {
        foreach (var renderer in ghost.GetComponentsInChildren<SpriteRenderer>())
        {
            Color color = renderer.color;
            color.a = 0.5f; // 半透明
            renderer.color = color;
        }

        foreach (var collider in ghost.GetComponentsInChildren<Collider2D>())
        {
            collider.enabled = false;
        }
    }

    private void OnClikEvent()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        grid.GetXY(mouseWorldPos, out int x, out int y);

        if (Input.GetMouseButtonDown(0))
        {
            if (currentMode == MouseMode.PlaceBuilding)
            {
                PlaceBuilding(x,y);
            }
            else if (currentMode == MouseMode.PlaceResource)
            {
                PlaceResource(x, y);
            }

        }
    }

    public void StopPlacement()
    { 
        selectedBuilding = null;
        selectedResource = null;
        uiManager.CloseCancelInquiry();
        currentMode = MouseMode.ExplorationMap;
    }
}
