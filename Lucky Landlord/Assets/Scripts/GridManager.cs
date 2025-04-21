using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;



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

    [SerializeField] private Transform gridParent; 


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
        GenerateResources();
        VisualizeGrid();
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
            SetSortingOrder(resource, 1); // 资源在顶层

            // 添加资源动画（可选）
            if (resource.TryGetComponent<Animator>(out var anim))
            {
                anim.Play("ResourceIdle");
            }
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
                gridObject.landscape.bonusFromAdjacentLandscapes.Contains(neighbor.landscape))
            {
                income += gridObject.landscape.incomePerMatch;
            }

            // 建筑加成
            foreach (var building in gridObject.buildings)
            {
                if (building.bonusFromAdjacentLandscapes.Contains(neighbor.landscape))
                    income += building.incomePerMatch;

                if (building.bonusFromAdjacentResources.Contains(neighbor.resource))
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
        if (gridObject.resource != null && gridObject.resource.doubleGridIncome)
        {
            income *= 2;
        }

        return income;
    }


}
