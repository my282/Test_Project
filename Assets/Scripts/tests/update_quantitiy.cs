using UnityEngine;
using TMPro;

public class TextController : MonoBehaviour
{
    public TextMeshProUGUI myText;

    void Start(){
        myText.text = GameDatabase.Instance.GetItemQuantity("itemA").ToString();
    }

    public void ChangeText(){
        myText.text = GameDatabase.Instance.GetItemQuantity("itemA").ToString();
    }
}