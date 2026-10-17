using UnityEngine;
using UnityEngine.EventSystems;

public class UI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private GameObject uiObject;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (uiObject == null)
        uiObject = GameObject.Find("UI");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
   public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("UI Clicked");
        if (uiObject != null)
        {
            uiObject.SetActive(false);
        }
    }
}
