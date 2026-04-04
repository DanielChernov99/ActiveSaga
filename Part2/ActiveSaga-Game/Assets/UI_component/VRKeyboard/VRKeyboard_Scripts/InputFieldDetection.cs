using UnityEngine;
using UnityEngine.EventSystems;
using TMPro; // Added TextMeshPro namespace

[RequireComponent(typeof(TMP_InputField))] // Changed from InputField to TMP_InputField
public class InputFieldDetection : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    private TMP_InputField myselfInputField; 
    private TMP_Text inputFieldText; 
    private TMP_InputField.LineType inputFieldLineType; 
    private OpenVirtualKeyboard keyboardController; 

    private void Awake()
    {
        if (myselfInputField == null)
            myselfInputField = GetComponent<TMP_InputField>();
        
        if (inputFieldText == null)
            inputFieldText = myselfInputField.textComponent;
        
        inputFieldLineType = myselfInputField.lineType;
        
        if (keyboardController == null)
            keyboardController = GameObject.Find("Virtual Keyboard Controller").GetComponent<OpenVirtualKeyboard>();
    }

    private void OnEnable()
    {
        if (myselfInputField == null)
            myselfInputField = GetComponent<TMP_InputField>();
        
        if (inputFieldText == null)
            inputFieldText = myselfInputField.textComponent;
        
        inputFieldLineType = myselfInputField.lineType;
        
        if (keyboardController == null)
            keyboardController = GameObject.Find("Virtual Keyboard Controller").GetComponent<OpenVirtualKeyboard>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
#if(UNITY_EDITOR)
        // print("InputField OnPointerEnter");
#endif
    }

    public void OnPointerExit(PointerEventData eventData)
    {
#if(UNITY_EDITOR)
        // print("InputField OnPointerExit");
#endif
    }

    public void OnPointerDown(PointerEventData eventData)
    {
#if(UNITY_EDITOR)
        // print("InputField OnPointerDown");
#endif
    }

    public void OnPointerUp(PointerEventData eventData)
    {
#if(UNITY_EDITOR)
        // print("InputField OnPointerUp");
#endif
    }

    /// <summary>
    /// Run the function after the pointer clicks the input field
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        keyboardController.onExitKeyboardArea = false;
        GetInputFieldTarget.SelectInputFieldName = transform.name;
        
#if(UNITY_EDITOR)
         // print("SelectInputFieldName = " + transform.name);
#endif

        // Using TextMeshPro's built-in utility to find the exact character index based on click position!
        // This replaces 70 lines of legacy math code.
        int cursorIndex = TMP_TextUtilities.GetCursorIndexFromPosition(inputFieldText, eventData.position, eventData.pressEventCamera);
        
        if (cursorIndex < 0) cursorIndex = 0;
        
        GetInputFieldTarget.Index = cursorIndex;
        
#if(UNITY_EDITOR)
         // print("index = " + GetInputFieldTarget.Index);
#endif

        keyboardController.OnOpenVirtualKeyboard();
    }
}