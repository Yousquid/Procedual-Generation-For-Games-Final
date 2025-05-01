using UnityEngine;
using TMPro;
using System.Collections.Generic;
using static GridManager;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GridManager gridManager;
    public TextMeshProUGUI wealthText;
    public static UIManager Instance;
    public TextMeshProUGUI remindText;
    public Button cancelPlacementButton;
    public Button continuePlacementButton;
    private float remindMinimalShowingTimeLimit = 1f;
    private float remindTimer = 0;
    private bool isShowingRemindText = false;
    public Image remindImageBackground;
    public TextMeshProUGUI shopText;
    public RandomSelector randomSelector;
    public Button shopButton;
    public Button turnButton;
    public TextMeshProUGUI taxText;
    public TurnManager turnManager;
    public Button payTaxButton;
    public Image payTaxBackground;
    public TextMeshProUGUI taxBackgroundText;
    public Button rulebookButton;
    public TextMeshProUGUI rulebookText;
    public Image rublebookImage;
    


    private void Awake()
    {
        Instance = this;
        remindText.gameObject.SetActive(false);
        cancelPlacementButton.gameObject.SetActive(false);
        continuePlacementButton.gameObject.SetActive(false);
        remindImageBackground.gameObject.SetActive(false);
        payTaxButton.gameObject.SetActive(false);
        payTaxBackground.gameObject.SetActive(false);
        rublebookImage.gameObject.SetActive(false);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        turnManager = GetComponent<TurnManager>();
    }

    public void OnClickCheckRuleBook()
    {
        if (rublebookImage.gameObject.activeSelf)
        {
            rublebookImage.gameObject.SetActive(false);
            gridManager.currentMode = MouseMode.ExplorationMap;
            rulebookText.text = "?";
        }
        else if (!rublebookImage.gameObject.activeSelf && gridManager.currentMode == MouseMode.ExplorationMap)
        {
            rublebookImage.gameObject.SetActive(true);
            gridManager.currentMode = MouseMode.UISelection;
            rulebookText.text = "X";
        }
    }

    private void UpdateTaxText()
    {
        taxText.text = $"Pay ${turnManager.GetCurrentTaxNeedToPay()} after {turnManager.currentLeftTurns} Turns.";
    }
    // Update is called once per frame
    void Update()
    {
        wealthText.text = "Wealth:" + GridManager.wealth + "+ " + gridManager.GetAllIncomeValue();

        CancelBuildingorResourcePlacement();

        RemindTextTimer();

        UpdateTaxText();

        shopText.text = $"${randomSelector.rollPrice}: Get a Roll";

    }

    public void PayTax(int amount)
    {
        payTaxButton.gameObject.SetActive(true);
        payTaxBackground.gameObject.SetActive(true);
        taxBackgroundText.text = $"You need to pay ${amount} as rent!";
    }

    private void ClosePayTaxUIs()
    {
        payTaxButton.gameObject.SetActive(false);
        payTaxBackground.gameObject.SetActive(false);
    }

    public void OnClickPayTax()
    {
        int amount = turnManager.GetCurrentTaxNeedToPay();
        if (GridManager.wealth < amount)
        {
            SetRemindText("You Lose!!!! Press R to restart.");
            return;
        }

        GridManager.wealth -= amount;
        turnManager.OnTaxPaid();
        ClosePayTaxUIs();
    }
    public void OnShopButtonPressed()
    {
        if (GridManager.wealth >= randomSelector.rollPrice)
        {
            randomSelector.StartRandomSelection();
            GridManager.wealth -= randomSelector.rollPrice;
        }
        else
        {
            SetRemindText("No enough money to get a roll");
        }
    }
    public void SetRemindText(string remindTextToUse)
    { 
        isShowingRemindText = true;
        remindText.gameObject.SetActive (true);
        remindImageBackground.gameObject.SetActive(true);
        remindText.text = remindTextToUse;
    }

    private void RemindTextTimer()
    {
        if (isShowingRemindText)
        {
            remindTimer += Time.deltaTime;
        }
        if (remindTimer > remindMinimalShowingTimeLimit)
        { 
            isShowingRemindText = false;
            remindTimer = 0;
            CloseRemindText();
        }
    }
    private void CloseRemindText()
    {
        remindText.gameObject.SetActive(false);
        remindImageBackground.gameObject.SetActive(false);
    }

    private void CancelBuildingorResourcePlacement()
    {
        if (gridManager.currentMode == MouseMode.PlaceResource || gridManager.currentMode == MouseMode.PlaceBuilding)
        {
            if (Input.GetMouseButtonDown(1))
            {
                SetRemindText("Are you sure you want to cancel the placement of this Resource/Building? No money will pay back if you canel the placement");
                cancelPlacementButton.gameObject.SetActive(true);
                continuePlacementButton.gameObject.SetActive(true);
            }
        }
    }

    public void CloseCancelInquiry()
    {
        CloseRemindText ();
        cancelPlacementButton.gameObject.SetActive(false);
        continuePlacementButton.gameObject.SetActive(false);
    }
}
