using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;


public class Toolbar : MonoBehaviour {

    public Dropdown file;
    public int returnIndex;
    public GameObject savePanel;

    public void Start() {
        file.captionText.text = "File";
     
    }

    public void disableFirstOption() {
        FileDropdownOption[] options = file.GetComponentsInChildren<FileDropdownOption>();

        foreach (FileDropdownOption option in options) {
            print("cycling " + option.GetIndex());
            if (option.GetIndex() == 1) {
                print("disabling " + option.gameObject.name);
                option.gameObject.SetActive(false);
            }
        }
    }



    public void FileDropdownSelection(int selection) {

        print(file.options[file.value].text + " selected from toolbar (" + file.value + ")");
        switch (file.options[file.value].text) {
            case "Save":
                savePanel.SetActive(true);
                break;
            case "Save As...":
                break;
            case "Load...":
                break;
            case "Export to Database...":
                break;
            case "":
                print("Nothing registered in File dropdown");
                break;

        }
        file.value = 0;
        file.captionText.text = "File";
    }



}
