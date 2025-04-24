using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public GridManager gridManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void EndTurn()
    {
        GridManager.wealth += gridManager.GetAllIncomeValue();
    }
}
