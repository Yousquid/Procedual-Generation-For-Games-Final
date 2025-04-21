using UnityEngine;
using System.Collections.Generic;

public enum BuildingTypes
{
    Empty, LumberCamp, Mine, Farm, Ranch, FishingBoat, Mill,
    House, Market, Canal, Workshop, Dock, Waterwell
}

[CreateAssetMenu(fileName = "BuildingType", menuName = "Scriptable Objects/BuildingType")]
public class BuildingType : ScriptableObject
{
    public string buildingName;
    public int baseIncomeValue;
    public BuildingTypes buildingTypeEnum;
    public string buildingCategory; // Production or Enhancement
    public string specialEffectDescription;
    public List<LandscapeTypeSo> bonusFromAdjacentLandscapes;
    public List<ResourceTypeSo> bonusFromAdjacentResources;
    public List<BuildingType> bonusFromAdjacentBuildings;
    public int incomePerMatch;
    public bool doublesAdjacentIncome;
}
