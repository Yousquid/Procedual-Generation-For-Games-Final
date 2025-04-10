using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LandscapeInfluence
{
    public LandscapeTypeSo target;
    public float weight;
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
