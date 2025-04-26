using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public GridManager gridManager;
    public RandomSelector randomSelector;
    public int currentTurn = 0;
    public int currentLeftTurns;
    public UIManager uiManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentLeftTurns = 5;
        uiManager = GetComponent<UIManager>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTaxPaymentTurns();
    }

    public void EndTurn()
    {
        GridManager.wealth += gridManager.GetAllIncomeValue();
        randomSelector.rollTimes -= 1;
        randomSelector.StartRandomSelection();
        currentTurn += 1;
        currentLeftTurns -= 1;
    }

    public int GetCurrentTaxNeedToPay()
    {
        if (currentTurn <= 5)
        {
            return 200;
        }
        if (currentTurn > 5 && currentTurn <= 11)
        {
            return 1000;
        }
        if (currentTurn > 11 && currentTurn <= 18)
        {
            return 2000;
        }
        if (currentTurn > 18 && currentTurn <= 26)
        {
            return 5000;
        }
        else return 0;
    }

    private void UpdateTaxPaymentTurns()
    {
        if (currentLeftTurns == 0)
        {
            uiManager.PayTax();
        }
        
    }
}
