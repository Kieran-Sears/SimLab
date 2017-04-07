using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabManager : MonoBehaviour {
    public GameObject tabPrefab;
    public Graph activeGraph;
    public List<Toggle> toggles;

    void Start() {
        SwitchTab();
    }

    public void SwitchTab() {
        for (int i = 0; i < toggles.Count; i++) {
            Toggle toggle = toggles[i];
            if (toggle.isOn) {
                GameObject graph = transform.GetChild(i).gameObject;
                activeGraph = graph.GetComponent<Graph>();
                graph.SetActive(true);
            } else {
                transform.GetChild(i).GetChild(2).gameObject.SetActive(false);
            }
        }
       
    }

    public void AddTab(string vitalName, string unitsOfMeasure, List<VitalsDatum> data) {
       GameObject tab = Instantiate(tabPrefab, transform);
        tab.transform.localScale = Vector3.one;
        tab.transform.localPosition = Vector3.one;
        Toggle toggle = tab.GetComponent<Toggle>();
        toggle.isOn = false;
        toggle.onValueChanged.AddListener((value) => SwitchTab());
        toggles.Add(tab.GetComponent<Toggle>());
        SwitchTab();
}

}
