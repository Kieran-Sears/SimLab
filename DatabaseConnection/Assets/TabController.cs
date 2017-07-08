using System;
using System.Text;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TabController : MonoBehaviour {

    public GameObject tabPrefab;
    public GameObject contentPrefab;
    public GameObject activeTabs;
    public GameObject inactiveTabs;

    public AdminSetup activeAdministrationMethod;

    public Dictionary<Toggle, AdminSetup> tabGraphs = new Dictionary<Toggle, AdminSetup>();

    public void SwitchTab() {
        foreach (AdminSetup item in tabGraphs.Values) {
            if (item != null) {
                item.gameObject.SetActive(false);
            }
        }
        Toggle selected = activeTabs.GetComponent<ToggleGroup>().ActiveToggles().FirstOrDefault();
        if (selected == null) {
            print("no tags selected to switch to! \n\nLine 22, TabManager");
            return;
        }
        activeAdministrationMethod = tabGraphs[selected];
        activeAdministrationMethod.gameObject.SetActive(true);
    }

    public AdminSetup GenerateTab(string name) {
        GameObject tab = Instantiate(tabPrefab, activeTabs.transform);
        tab.transform.localScale = Vector3.one;
        tab.transform.localPosition = Vector3.one;
        tab.gameObject.name = name;
        tab.transform.GetComponentInChildren<Text>().text = name;
        GameObject adminSetup = Instantiate(contentPrefab, transform) as GameObject;
        adminSetup.name = name;
        RectTransform graphTrans = adminSetup.GetComponent<RectTransform>();
        graphTrans.sizeDelta = transform.GetComponent<RectTransform>().sizeDelta - (Vector2.up * activeTabs.GetComponent<RectTransform>().sizeDelta.y) ;
        graphTrans.position = transform.transform.position;
        graphTrans.localScale = Vector3.one;
        Toggle toggle = tab.GetComponent<Toggle>();
        tabGraphs.Add(toggle, adminSetup.GetComponent<AdminSetup>());
        toggle.onValueChanged.AddListener((bool toggled) => { SwitchTab(); });
        toggle.group = activeTabs.GetComponent<ToggleGroup>();
        toggle.isOn = false;
        return tabGraphs[toggle];
    }


}

