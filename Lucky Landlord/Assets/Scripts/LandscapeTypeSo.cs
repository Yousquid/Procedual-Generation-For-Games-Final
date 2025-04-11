using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LandscapeInfluence
{
    public LandscapeTypeSo target;
    public float weight;
}

[System.Serializable]
public class ResourceProbability
{
    public ResourceTypeSo resource;
    [Range(0f, 1f)]
    public float probability;
}

[CreateAssetMenu(fileName = "LandscapeTypeSo", menuName = "Scriptable Objects/LandscapeTypeSo")]
public class LandscapeTypeSo : ScriptableObject
{
    public List<LandscapeInfluence> influenceModifiers;

    public string landscapeName;
    public GameObject landscapePrefab;
    public int maxBuildingLimit;
    public int baseIncomeValue;
    public string specialEffectDescription;
    public float baseProbability;
}
