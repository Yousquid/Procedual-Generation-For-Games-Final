using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public GridManager gridManager;
    public RandomSelector randomSelector;
    public int currentTurn = 0;
    public int currentLeftTurns;
    public UIManager uiManager;

    public List<TaxMilestone> taxMilestones;

    private int currentMilestoneIndex = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        uiManager = GetComponent<UIManager>();
        gridManager = FindObjectOfType<GridManager>();

        if (taxMilestones.Count > 0)
        {
            currentLeftTurns = taxMilestones[0].turnNumber;
        }
    }

    private void Update()
    {
        if (currentLeftTurns == 0)
        {
            uiManager.PayTax(GetCurrentTaxNeedToPay());
        }
    }

    public void EndTurn()
    {
        GridManager.wealth += gridManager.GetAllIncomeValue();
        randomSelector.rollTimes -= 1;
        randomSelector.StartRandomSelection();

        currentTurn++;
        currentLeftTurns--;

        if (currentLeftTurns < 0) currentLeftTurns = 0; // 防止负数
    }

    public int GetCurrentTaxNeedToPay()
    {
        if (currentMilestoneIndex < taxMilestones.Count)
        {
            return taxMilestones[currentMilestoneIndex].taxAmount;
        }
        return 0;
    }

    public void OnTaxPaid()
    {
        // 玩家点击交税并成功支付后调用
        if (currentMilestoneIndex < taxMilestones.Count)
        {
            currentLeftTurns = taxMilestones[currentMilestoneIndex].nextCycleTurns;
            currentMilestoneIndex++;
        }
    }

    [System.Serializable]
    public class TaxMilestone
    {
        public int turnNumber;      // 到这个回合时触发
        public int taxAmount;       // 需要交的税
        public int nextCycleTurns;  // 成功交税后新的一轮周期有几回合
    }
}
