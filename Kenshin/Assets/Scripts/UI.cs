using UnityEngine;
using UnityEngine.EventSystems;

public class UI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private GameObject uiObject;
    [SerializeField] private GameObject HealthBar;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (uiObject == null)
        uiObject = GameObject.Find("UI");
        HealthBar.SetActive(false);
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
            HealthBar.SetActive(true);
        }
    }
}
