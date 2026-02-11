using UnityEngine;
using UnityEngine.UI;

public class ImageControllerA : MonoBehaviour{
    public Image targetImage;
    public Sprite newSprite;

    void Start(){
        if(GameDatabase.Instance.HasFacility("FacilityA") == true)
        {
            targetImage.sprite = newSprite;
        }
        
    }
}