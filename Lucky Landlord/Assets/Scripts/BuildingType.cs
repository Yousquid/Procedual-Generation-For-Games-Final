using UnityEngine;
using System.Collections.Generic;

public enum BuildingTypes
{
    Empty, LumberCamp, Mine, Farm, Ranch, FishingBoat, Cityhall,
    House, Market, Canal, Dock, Waterwell
}

[CreateAssetMenu(fileName = "BuildingType", menuName = "Scriptable Objects/BuildingType")]
public class BuildingType : ScriptableObject
{
    public string buildingName;
    public int baseIncomeValue;
    public GameObject buildingPrefab;
    public BuildingTypes buildingTypeEnum;
    public string buildingCategory; // Production or Enhancement
    public string specialEffectDescription;
    public SpriteRenderer iconRenderer;          // չʾͼ��
    public List<LandscapeTypeSo> bonusFromAdjacentLandscapes;
    public List<ResourceTypeSo> bonusFromAdjacentResources;
    public List<BuildingType> bonusFromAdjacentBuildings;
    public List<LandscapeTypeSo> allowedLandscapes;
    public List<ResourceTypeSo> allowedResources;
    public List<BuildingType> allowedBuildings;
    public List<BuildingType> mustHaveBuildings;
    public List<LandscapeTypeSo> mustHaveLandscapes;
    public int incomePerMatch;
    public bool doublesAdjacentIncome;
}
