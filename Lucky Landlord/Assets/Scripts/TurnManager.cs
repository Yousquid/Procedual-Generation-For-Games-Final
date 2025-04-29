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

        if (currentLeftTurns < 0) currentLeftTurns = 0; // ��ֹ����
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
        // ��ҵ����˰���ɹ�֧�������
        if (currentMilestoneIndex < taxMilestones.Count)
        {
            currentLeftTurns = taxMilestones[currentMilestoneIndex].nextCycleTurns;
            currentMilestoneIndex++;
        }
    }

    [System.Serializable]
    public class TaxMilestone
    {
        public int turnNumber;      // ������غ�ʱ����
        public int taxAmount;       // ��Ҫ����˰
        public int nextCycleTurns;  // �ɹ���˰���µ�һ�������м��غ�
    }
}
