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
    private float remindMinimalShowingTimeLimit = .5f;
    private float remindTimer = 0;
    private bool isShowingRemindText = false;
    public Image remindImageBackground;

    private void Awake()
    {
        Instance = this;
        remindText.gameObject.SetActive(false);
        cancelPlacementButton.gameObject.SetActive(false);
        continuePlacementButton.gameObject.SetActive(false);
        remindImageBackground.gameObject.SetActive(false);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        wealthText.text = "Wealth:" + GridManager.wealth + "+ " + gridManager.GetAllIncomeValue();

        CancelBuildingorResourcePlacement();

        RemindTextTimer();


    }

    private void LateUpdate()
    {
        //if (!cancelPlacementButton.gameObject.activeSelf)
        //{
        //    if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        //    {
        //        if (!isShowingRemindText)
        //        CloseRemindText(); 
        //    }
        //}
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
