using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Toolbar : MonoBehaviour {

    public Dropdown file;
    public GameObject graphPanel;

    public void FileDropdownSelection() {
        switch (file.options[file.value].text) {
            case "Save":
                break;
            case "Save As...":
                break;
            case "Load...":
                break;
            case "Export to Database...":
                break;
            case "":
                break;

        }
        file.captionText.text = "File";
    }


    //private void Save() {
    //    foreach (Graph graph in graphPanel.GetComponentsInChildren<Graph>()) {
    //        Condition.Add(graph.gameObject.name, graph.points);
    //    }
    //    ExportManager.instance.SaveCondition(Condition);
    //}
}
