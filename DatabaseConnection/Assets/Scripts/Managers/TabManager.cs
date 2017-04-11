using System;
using System.Text;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TabManager : MonoBehaviour {
    public GameObject tabPrefab;
    public Graph activeGraph;


    public void SwitchTab() {
       Graph[] graphs = GetComponentsInChildren<Graph>();
        foreach (Graph item in graphs) {
            item.container.SetActive(false);
        }

        Toggle selected = GetComponent<ToggleGroup>().ActiveToggles().FirstOrDefault();
        activeGraph = selected.GetComponent<Graph>();
        activeGraph.container.SetActive(true); 
    }

    public GameObject GenerateTab(string vitalName) {
        GameObject tab = Instantiate(tabPrefab, transform);
        tab.transform.localScale = Vector3.one;
        tab.transform.localPosition = Vector3.one;
        tab.gameObject.name = vitalName;
        StringBuilder builder = new StringBuilder();
        char[] letters = vitalName.ToCharArray();
        for (int i = 0; i < letters.Length; i++) {
            if (i == 0) {
                letters[i] = Char.ToUpper(letters[i]);
            }
            if (Char.IsUpper(letters[i]) && builder.Length > 0) {
                builder.Append(' ');
            }
            builder.Append(letters[i]);
        }
        tab.transform.GetComponentInChildren<Text>().text = builder.ToString();
        Toggle toggle = tab.GetComponent<Toggle>();
        toggle.onValueChanged.AddListener((bool toggled) => { SwitchTab(); });
        toggle.group = GetComponent<ToggleGroup>();
        toggle.isOn = false;
        return tab;
    }


}
