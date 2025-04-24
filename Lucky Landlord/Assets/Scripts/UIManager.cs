using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public GridManager gridManager;
    public TextMeshProUGUI wealthText;
    public static UIManager Instance;

    private void Awake()
    {
        Instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        wealthText.text = "Wealth:" + GridManager.wealth + "+ " + gridManager.GetAllIncomeValue();
    }

    

}
