using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TabManager : MonoBehaviour {
    public GameObject tabPrefab;
    public GameObject componentPrefab;
    public GameObject activeTabs;
    public GameObject inactiveTabs;
    public GameObject contentArea;
    public GameObject activeComponent;

    public Dictionary<Toggle, GameObject> tabGraphs = new Dictionary<Toggle, GameObject>();

    // function which displays the graph associated with the tab selected by the user
    public void SwitchTab() {
        foreach (GameObject item in tabGraphs.Values) {
            if (item != null) {
                item.SetActive(false);
            }
        }
        Toggle selected = activeTabs.GetComponent<ToggleGroup>().ActiveToggles().FirstOrDefault();
        if (selected == null) {
            print("no tags selected to switch to! \n\nLine 22, TabManager");
            return;
        } 
        activeComponent = tabGraphs[selected];
        activeComponent.SetActive(true);
    }

    // instantiates and populates the graph prefab tabs area with any selected vitals
    // based on their name and returns the graph that controls them
    public GameObject GenerateTab(string vitalName) {
        GameObject tab = Instantiate(tabPrefab, activeTabs.transform);
        tab.transform.localScale = Vector3.one;
        tab.transform.localPosition = Vector3.one;
        tab.gameObject.name = vitalName;
        tab.transform.GetComponentInChildren<Text>().text = vitalName;
        GameObject component = Instantiate(componentPrefab) as GameObject;
        component.name = vitalName;
        RectTransform componentTrans = component.GetComponent<RectTransform>();
        componentTrans.SetParent(contentArea.transform);
        componentTrans.sizeDelta = transform.GetComponent<RectTransform>().sizeDelta;
        componentTrans.position = contentArea.transform.position;
        componentTrans.localScale = Vector3.one;
        Toggle toggle = tab.GetComponent<Toggle>();
        tabGraphs.Add(toggle, component);
        toggle.onValueChanged.AddListener((bool toggled) => { SwitchTab(); });
        toggle.group = activeTabs.GetComponent<ToggleGroup>();
        toggle.isOn = false;
        return tabGraphs[toggle];
    }


}
