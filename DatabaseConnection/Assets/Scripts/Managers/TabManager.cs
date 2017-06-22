using System;
using System.Text;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TabManager : MonoBehaviour {
    public GameObject tabPrefab;
    public GameObject containerPrefab;
    public Graph activeGraph;

    public Dictionary<Toggle, Graph> tabGraphs = new Dictionary<Toggle, Graph>();

    public void SwitchTab() {
        foreach (Graph item in tabGraphs.Values) {
            item.gameObject.SetActive(false);
        }
        Toggle selected = GetComponent<ToggleGroup>().ActiveToggles().FirstOrDefault();
        if (selected == null) {
            print("no tags selected to switch to! \n\nLine 22, TabManager");
            return;
        } 
        activeGraph = tabGraphs[selected];
        activeGraph.gameObject.SetActive(true);
    }

    public Graph GenerateTab(string vitalName) {
        GameObject tab = Instantiate(tabPrefab, transform);
        tab.transform.localScale = Vector3.one;
        tab.transform.localPosition = Vector3.one;
        tab.gameObject.name = vitalName;
        tab.transform.GetComponentInChildren<Text>().text = vitalName;
        GameObject graph = Instantiate(containerPrefab) as GameObject;
        graph.name = vitalName;
        RectTransform graphTrans = graph.GetComponent<RectTransform>();
        graphTrans.SetParent(transform.parent);
        graphTrans.sizeDelta = transform.parent.GetComponent<RectTransform>().sizeDelta;
        graphTrans.position = transform.parent.transform.position;
        graphTrans.localScale = Vector3.one;
        Toggle toggle = tab.GetComponent<Toggle>();
        tabGraphs.Add(toggle, graph.GetComponent<Graph>());
        toggle.onValueChanged.AddListener((bool toggled) => { SwitchTab(); });
        toggle.group = GetComponent<ToggleGroup>();
        toggle.isOn = false;
        return tabGraphs[toggle];
    }


}

// code for changing camelCase formatting to regular with capitol first letters
//StringBuilder builder = new StringBuilder();
//char[] letters = vitalName.ToCharArray();
//for (int i = 0; i < letters.Length; i++) {
//    if (i == 0) {
//        letters[i] = Char.ToUpper(letters[i]);
//    }
//    if (Char.IsUpper(letters[i]) && builder.Length > 0) {
//        builder.Append(' ');
//    }
//    builder.Append(letters[i]);
//}
//tab.transform.GetComponentInChildren<Text>().text = builder.ToString();