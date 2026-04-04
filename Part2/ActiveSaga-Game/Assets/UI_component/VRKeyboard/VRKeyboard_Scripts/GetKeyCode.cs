using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class GetKeyCode : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    public Color32 mNormalColor = Color.white;
    public Color32 mHoverColor = Color.gray;
    public Color32 mDownColor = Color.red;
    
    private OpenVirtualKeyboard keyboardController; 
    private Image buttonImage;                      
    private string buttonString;                    
    private Text showString;                        

    private TMP_InputField inputTarget; 

    private bool toLowLetterCase;                   
    private readonly CultureInfo cult = new CultureInfo("en-US", false);
    
    private void OnEnable()
    {
        if (keyboardController == null)
            keyboardController = GameObject.Find("Virtual Keyboard Controller").GetComponent<OpenVirtualKeyboard>();
        
        if(buttonImage == null)
            buttonImage = GetComponent<Image>();
        
        if(buttonString == null)
            buttonString = transform.name;

        if (showString == null)
            showString = transform.Find("Text").GetComponent<Text>();
    }

    private void Update()
    {
        if (toLowLetterCase == LetterCaseDetection.Lowercase)
            return;
        
        toLowLetterCase = LetterCaseDetection.Lowercase;
        
        if (Regex.IsMatch(buttonString, "^[a-zA-Z0-9]*$") && 
            !(string.Equals(buttonString, "delete") || string.Equals(buttonString, "clear") || 
              string.Equals(buttonString, "backward") || string.Equals(buttonString, "forward") ||
              string.Equals(buttonString, "Letter case") || string.Equals(buttonString, "To0") ||
              string.Equals(buttonString, "ToLast")))
        {
            buttonString = toLowLetterCase ? buttonString.ToLower(cult) : buttonString.ToUpper(cult);
            showString.text = buttonString;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        keyboardController.onExitKeyboardArea = false;
        buttonImage.color = mHoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        keyboardController.onExitKeyboardArea = false;
        buttonImage.color = mNormalColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        buttonImage.color = mDownColor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        keyboardController.onExitKeyboardArea = false;
        buttonImage.color = mHoverColor;
        
        string target = GetInputFieldTarget.SelectInputFieldName;
        
        if(inputTarget == null)
            inputTarget = GameObject.Find(GetInputFieldTarget.SelectInputFieldName).GetComponent<TMP_InputField>();
        
        if (inputTarget.gameObject.name != target)
        {
#if(UNITY_EDITOR)
            // print("change target");
#endif
            inputTarget = GameObject.Find(target).GetComponent<TMP_InputField>();
        }
        
#if(UNITY_EDITOR)
        // print("You now click = " + buttonString);
        // print("Your input target = " + inputTarget.gameObject.name);
        // print("Your input index in target = " + index);
#endif
        
        string targetText = inputTarget.text;
        int index = GetInputFieldTarget.Index;

        // --- SAFETY CLAMP: Prevent ArgumentOutOfRangeException ---
        if (index < 0) 
            index = 0;
        if (index > targetText.Length) 
            index = targetText.Length;
            
        GetInputFieldTarget.Index = index; // Sync the safe index back
        // ---------------------------------------------------------
        
        if (!(string.Equals(buttonString, "delete") || string.Equals(buttonString, "clear") || 
            string.Equals(buttonString, "backward") || string.Equals(buttonString, "forward") ||
            string.Equals(buttonString, "Letter case") || string.Equals(buttonString, "To0") ||
            string.Equals(buttonString, "ToLast")))
        {
            inputTarget.text = targetText.Insert(index, buttonString);
#if(UNITY_EDITOR)
            // print("inputTarget.text = " + inputTarget.text);
#endif
            GetInputFieldTarget.Index++;
        }
        else
        {
            switch (buttonString)
            {
                case "delete":
                    if (GetInputFieldTarget.Index > 0)
                    {
                        GetInputFieldTarget.Index--;
                        inputTarget.text = targetText.Remove(GetInputFieldTarget.Index, 1);
                    }
                    break;
                case "clear":
                    if (inputTarget.text.Length > 0)
                    {
                        inputTarget.text = string.Empty;
                        GetInputFieldTarget.Index = 0;
                    }
                    break;
                case "backward":
                    if(GetInputFieldTarget.Index > 0)
                        GetInputFieldTarget.Index--;
                    break;
                case "forward":
                    if (GetInputFieldTarget.Index < inputTarget.text.Length)
                        GetInputFieldTarget.Index++;
                    break;
                case "Letter case":
                    LetterCaseDetection.Lowercase = !LetterCaseDetection.Lowercase;
                    break;
                case "ToLast":
                    GetInputFieldTarget.Index = inputTarget.text.Length;
                    break;
                case "To0":
                    GetInputFieldTarget.Index = 0;
                    break;
            }
        }
    }
}