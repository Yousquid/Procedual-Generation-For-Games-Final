using UnityEngine;

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
    public BuildingType buildingType;
    public string buildingCategory; // Production or Enhancement
    public string specialEffectDescription;
}
