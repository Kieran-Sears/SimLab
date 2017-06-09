using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;


public class Toolbar : MonoBehaviour {

    public Dropdown file;
    public GameObject graphPanel;

    public void OnEnable() {
        file.captionText.text = "File";
    }

    public void FileDropdownSelection() {
        switch (file.options[file.value].text) {
            case "Save":
                Save();
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


    private void Save() {
        print(Directory.GetDirectories("Resouces/Conditions/")[0]);
     //   string[] files = Directory.GetFiles(path);
    }
}
