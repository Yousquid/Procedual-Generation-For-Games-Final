using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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


}
