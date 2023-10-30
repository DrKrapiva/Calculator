using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CalculatorLogic : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI displayText;
    [SerializeField] private Button memoryClearButton;
    [SerializeField] private Button memoryRecallButton;
    private string currentNumber = "";
    private string firstNumber = "";
    private string history = "";
    private double? memoryValue = null;
    private bool shouldClearInput = false;
    private bool justRecalledFromMemory = false;
    private bool isOperating = false;
    
    public static CalculatorLogic Instance
    {
        get
        {
            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;

    }
    static private CalculatorLogic _instance;
    private void Start()
    {
        UpdateMemoryButtons();
    }

    private void UpdateMemoryButtons()
    {
        bool isMemoryEmpty = memoryValue.HasValue;
        memoryClearButton.interactable = isMemoryEmpty;
        memoryRecallButton.interactable = isMemoryEmpty;
    }
    public void Calculation(string symbol)
    {
        if (shouldClearInput)
        {
            currentNumber = "";
            shouldClearInput = false;
        }
        if (justRecalledFromMemory && double.TryParse(symbol, out _))// если использована кнопка MR и пользователь решил ввести другое число
        {
            if (isOperating)
            {
                string[] parts = currentNumber.Split(' ');
                currentNumber = parts[0] + " " + parts[1] + " ";
            }
            else
            {
                currentNumber = "";
            }
            justRecalledFromMemory = false;
        }

        if (symbol == "+" || symbol == "-" || symbol == "*" || symbol == "/")
        {
            
            if (symbol == "+" || symbol == "-" || symbol == "*" || symbol == "/")
            {
                // Проверяем, был ли уже введен оператор
                if (isOperating)
                {
                    string[] parts = currentNumber.Split(' ');
                    if (parts.Length >= 2) // Если есть оператор
                    {
                        // Заменяем текущий оператор на новый
                        currentNumber = parts[0] + " " + symbol + (parts.Length == 3 ? (" " + parts[2]) : "");
                    }
                    else
                    {
                        firstNumber = currentNumber;
                        currentNumber += " " + symbol + " ";
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(currentNumber) && !string.IsNullOrEmpty(history))
                    {
                        string[] lines = history.Trim().Split('\n');
                        if (lines.Length > 0)
                        {
                            string lastLine = lines[lines.Length - 1];
                            if (lastLine.StartsWith("= "))
                            {
                                currentNumber = lastLine.Substring(2).Trim();
                            }
                        }
                    }

                    isOperating = true;
                    firstNumber = currentNumber;
                    currentNumber += " " + symbol + " ";
                }
            }
        }
        else if (symbol == "," && !currentNumber.Contains(","))
        {
            currentNumber += symbol;
        }
        else if (symbol == "%")
        {
            /*if (!isOperating) // Если нет текущей операции
            {
                double num = double.Parse(currentNumber);
                currentNumber = (num / 100).ToString();
                currentNumber += " \n= " + currentNumber;
                history += currentNumber + "\n";
                currentNumber = "";
            }*/
            // Если текущая операция уже начата, "%" будет игнорироваться.
            if (isOperating)
            {
                string[] parts = currentNumber.Split(' ');
                if (parts.Length >= 2)
                {
                    double num1 = double.Parse(parts[0]);
                    double num2 = double.Parse(parts[2]);

                    switch (parts[1])
                    {
                        case "+":
                        case "-":
                            num2 = num1 * (num2 / 100); // процент от первого числа
                            break;
                        case "*":
                        case "/":
                            num2 = num2 / 100; // просто переводим число в доли процента
                            break;
                    }

                    currentNumber = parts[0] + " " + parts[1] + " " + num2.ToString();
                }
            }
            else
            {
                double num = double.Parse(currentNumber);
                currentNumber = (num / 100).ToString();
            }
        }
        else if (symbol == "=")
        {
            string[] parts = currentNumber.Split(' ');

            if (parts.Length >= 3)
            {
                firstNumber = parts[0];
                char op = parts[1][0];
                string secondNumber = parts[2];

                double num1 = double.Parse(firstNumber);
                double num2 = double.Parse(secondNumber);

                switch (op)
                {
                    case '+': currentNumber += " \n=  " + (num1 + num2).ToString("F2"); break;
                    case '-': currentNumber += " \n= " + (num1 - num2).ToString(); break;
                    case '*': currentNumber += " \n= " + (num1 * num2).ToString(); break;
                    case '/':
                        if (num2 != 0)// на ноль делить нельзя
                            currentNumber += " \n= " + (num1 / num2).ToString();
                        else
                            currentNumber += " \n= деление на ноль невозможно";  
                        break;
                }
                history += currentNumber + "\n";
                currentNumber = "";
            }

            isOperating = false;
        }
        else
        {
            currentNumber += symbol;
        }

        displayText.text = history + currentNumber;
    }
    public void DeleteLastCharacter()
    {
        if (!string.IsNullOrEmpty(currentNumber))
        {
            // Удаляем последний символ из текущего числа
            currentNumber = currentNumber.Substring(0, currentNumber.Length - 1);
        }
        else if (!string.IsNullOrEmpty(history))
        {
            // Если currentNumber пуст, и вы хотите удалять из истории, удаляем символы до тех пор, пока не встретим "\n"
            if (history.EndsWith("\n"))
            {
                // Если история оканчивается на "\n", не удаляем этот символ
                return;
            }
            history = history.Substring(0, history.Length - 1);
        }
        isOperating = false;
        displayText.text = history + currentNumber;
    }
    public void ClearInput()
    {
        // Очищаем текущий ввод
        currentNumber = "";
        history = "";
        isOperating = false;
        displayText.text = history;
    }
    public void MemorySave()
    {
        if (!string.IsNullOrEmpty(currentNumber))
        {
            memoryValue = double.Parse(currentNumber);
        }
        else
        {
            // Если у нас нет текущего числа, но есть история с последним результатом, используем этот результат
            string[] lines = history.Trim().Split('\n');
            if (lines.Length > 0)
            {
                string lastLine = lines[lines.Length - 1];
                if (lastLine.StartsWith("= "))
                {
                    memoryValue = double.Parse(lastLine.Substring(2).Trim());
                }
            }
        }
        shouldClearInput = true;
        UpdateMemoryButtons();
    }
    public void MemoryRecall()
    {
        //выводим на экран число из памяти
        if (memoryValue.HasValue)
        {
            if (isOperating && !string.IsNullOrEmpty(firstNumber))
            {
                currentNumber = firstNumber + " " + currentNumber.Split(' ')[1] + " " + memoryValue.Value.ToString();
                shouldClearInput = false;
            }
            else
            {
                currentNumber = memoryValue.Value.ToString();
                shouldClearInput = true;
            }
            displayText.text = history + currentNumber;
            justRecalledFromMemory = true;  
        }
        UpdateMemoryButtons();
    }
    public void MemoryAdd()
    {
        
        double? addToMemory = null;

        // Если на экране запись вида (например 2 + 2)
        if (!string.IsNullOrEmpty(currentNumber) && currentNumber.Contains(" "))
        {
            string[] parts = currentNumber.Split(' ');

            if (parts.Length >= 3)
            {
                double num1;
                double num2;
                char op;

                if (double.TryParse(parts[0], out num1) && double.TryParse(parts[2], out num2))
                {
                    op = parts[1][0];

                    switch (op)
                    {
                        case '+': addToMemory = num1 + num2; break;
                        case '-': addToMemory = num1 - num2; break;
                        case '*': addToMemory = num1 * num2; break;
                        case '/':
                            if (num2 != 0) // на ноль делить нельзя
                                addToMemory = num1 / num2;
                            break;
                    }
                }
            }
        }
        // Если на экране запись вида (например 2 + 2 = 4)
        else if (!string.IsNullOrEmpty(history))
        {
            string[] lines = history.Trim().Split('\n');
            if (lines.Length > 0)
            {
                string lastLine = lines[lines.Length - 1];
                if (lastLine.StartsWith("= "))
                {
                    double.TryParse(lastLine.Substring(2).Trim(), out double resultNum);
                    addToMemory = resultNum;
                }
            }
        }
        // Если на экране введено число
        else if (!string.IsNullOrEmpty(currentNumber) && double.TryParse(currentNumber, out double currentNum))
        {
            addToMemory = currentNum;
        }

        if (addToMemory.HasValue)
        {
            if (memoryValue.HasValue)
            {
                memoryValue += addToMemory.Value;
            }
            else
            {
                memoryValue = addToMemory.Value;
            }
        }

        shouldClearInput = true;
        UpdateMemoryButtons();
    }
    public void MemorySubtract()
    {
        
        double? subtractFromMemory = null;

        // Если на экране запись вида (например 2 + 2)
        if (!string.IsNullOrEmpty(currentNumber) && currentNumber.Contains(" "))
        {
            string[] parts = currentNumber.Split(' ');

            if (parts.Length >= 3)
            {
                double num1;
                double num2;
                char op;

                if (double.TryParse(parts[0], out num1) && double.TryParse(parts[2], out num2))
                {
                    op = parts[1][0];

                    switch (op)
                    {
                        case '+': subtractFromMemory = num1 + num2; break;
                        case '-': subtractFromMemory = num1 - num2; break;
                        case '*': subtractFromMemory = num1 * num2; break;
                        case '/':
                            if (num2 != 0) // на ноль делить нельзя
                                subtractFromMemory = num1 / num2;
                            break;
                    }
                }
            }
        }
        // Если на экране запись вида (например 2 + 2 = 4)
        else if (!string.IsNullOrEmpty(history))
        {
            string[] lines = history.Trim().Split('\n');
            if (lines.Length > 0)
            {
                string lastLine = lines[lines.Length - 1];
                if (lastLine.StartsWith("= "))
                {
                    double.TryParse(lastLine.Substring(2).Trim(), out double resultNum);
                    subtractFromMemory = resultNum;
                }
            }
        }
        // Если на экране введено число
        else if (!string.IsNullOrEmpty(currentNumber) && double.TryParse(currentNumber, out double currentNum))
        {
            subtractFromMemory = currentNum;
        }

        if (subtractFromMemory.HasValue)
        {
            if (memoryValue.HasValue)
            {
                memoryValue -= subtractFromMemory.Value;
            }
            else
            {
                memoryValue = -subtractFromMemory.Value;
            }
        }

        shouldClearInput = true;
        UpdateMemoryButtons();
    }
    public void MemoryClear()
    {
        memoryValue = null;
        shouldClearInput = true;
        UpdateMemoryButtons();
    }

}
