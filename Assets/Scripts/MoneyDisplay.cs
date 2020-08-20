using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoneyDisplay : MonoBehaviour
{
    public Text targetTextbox;
    public string prefix = "";
    int currentlyDisplayedAmount = 0;
    int targetAmount = 0;

    public void SetMoneyTarget(int newAmount)
    {
        targetAmount = newAmount;
    }

    // Update is called once per frame
    void Update()
    {
        // Adjust the speed based on the amount things need to change
        var delta = Mathf.Abs(targetAmount - currentlyDisplayedAmount);

        var changeSpeed = 1;
        if (delta > 10) changeSpeed = 10;
        if (delta > 100) changeSpeed = 100;

        if (currentlyDisplayedAmount < targetAmount)
        {
            currentlyDisplayedAmount += changeSpeed;
        }

        if (currentlyDisplayedAmount > targetAmount)
        {
            currentlyDisplayedAmount -= changeSpeed;
        }

        targetTextbox.text = $"{prefix}${currentlyDisplayedAmount}";
    }
}
