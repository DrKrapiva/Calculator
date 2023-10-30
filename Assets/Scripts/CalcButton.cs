using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalcButton : MonoBehaviour
{
    public void EnterNameButton()
    {
        CalculatorLogic.Instance.Calculation(gameObject.name);
    }
}
