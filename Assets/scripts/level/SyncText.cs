using UnityEngine;
using TMPro;

public class SyncText : MonoBehaviour
{
    public TextMeshProUGUI sourceText; 
    private TextMeshProUGUI myText;

    void Start() => myText = GetComponent<TextMeshProUGUI>();

    void LateUpdate()
    {
        if (myText.text != sourceText.text)
        {
            myText.text = sourceText.text;
        }
    }
}