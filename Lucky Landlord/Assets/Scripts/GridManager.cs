using System;
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

    private Grid<GameGridObject> grid;

    public class GameGridObject
    {
        public enum TerrainType
        {
            None,
            Plains,
            Forest,
            Mountain,
            Water
        }

        public enum ResourceType
        {
            None,
            Wood,
            Stone,
            IronOre,
            Fish
        }

        public enum BuildingType
        {
            None,
            House,
            Factory,
            Farm
        }

        // 格子属性
        public TerrainType terrain;
        public ResourceType resource;
        public BuildingType building;
        public int productionOutput;
        public bool hasFogOfWar;

        public GameGridObject(TerrainType terrainType)
        {
            terrain = terrainType;
            resource = ResourceType.None;
            building = BuildingType.None;
            productionOutput = 0;
            hasFogOfWar = true; // 初始默认有战争迷雾
        }

        public override string ToString()
        {
            return $"{terrain}\n{resource}\n{building}";
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
            (g, x, y) => CreateEmptyGridObject()
        );

        // 初始化战争迷雾（假设中心区域可见）
        //InitializeFogOfWar(width / 2, height / 2, 3);
    }

    private GameGridObject CreateEmptyGridObject()
    {
        GameGridObject.TerrainType terrain = GameGridObject.TerrainType.None;

        GameGridObject gridObject = new GameGridObject(terrain);

        gridObject.resource = GameGridObject.ResourceType.None;

        gridObject.building = GameGridObject.BuildingType.None;

        gridObject.hasFogOfWar = true;

        return gridObject;
    }



}
