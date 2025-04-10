using UnityEngine;

[CreateAssetMenu(fileName = "ResourceTypeSo", menuName = "Scriptable Objects/ResourceTypeSo")]
public class ResourceTypeSo : ScriptableObject
{
    public string resourceName;
    public int baseIncomeValue;
    public GameObject resourcePrefab;
    public string specialEffectDescription;
}
