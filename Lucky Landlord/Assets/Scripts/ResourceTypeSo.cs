using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ResourceTypeSo", menuName = "Scriptable Objects/ResourceTypeSo")]
public class ResourceTypeSo : ScriptableObject
{
    public string resourceName;
    public int baseIncomeValue;
    public GameObject resourcePrefab;
    public string specialEffectDescription;
    public bool doubleGridIncome;
    public List<LandscapeTypeSo> bonusFromAdjacentLandscapes;
    public List<ResourceTypeSo> bonusFromAdjacentResources;
    public List<BuildingType> bonusFromAdjacentBuildings;
    public List<LandscapeTypeSo> canDoubleIncomeLandscapes;
    public List<BuildingType> canDoubleIncomeBuildings;
    public int incomePerMatch;

}
