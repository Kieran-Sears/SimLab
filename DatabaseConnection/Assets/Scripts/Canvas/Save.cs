using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class Save : MonoBehaviour {

    public Button leftButton;
    public Button rightButton;
    public Text messageText;
    public InputField filepathInputField;

    public void OnEnable() {
        if (SimulationSetup.instance.duration == -1) {
            messageText.text = "Cannot save when no graphs or duration are present.";
            rightButton.interactable = false;
            filepathInputField.interactable = false;
        }
        else {
            rightButton.interactable = true;
            filepathInputField.interactable = true;
            messageText.text = "Enter the filepath and name to save to...";
        }
    }

    public void SaveCondition() {
        print("Attempting to save");

        Condition condition = new Condition();
        condition.vitalsData = new List<VitalData>();
        condition.duration = SimulationSetup.instance.duration;

        VitalData vitalData;
        TimeLine timeline;
        List<Value> values;
        Value value;

        for (int i = 0; i < SimulationSetup.instance.vitalsChosen.transform.childCount; i++) {

            Toggle toggle = SimulationSetup.instance.vitalsChosen.transform.GetChild(i).GetComponent<Toggle>();

            if (toggle.isOn) {

                string vitalName = toggle.gameObject.name;

                GameObject graphObject = SimulationSetup.instance.graphs.transform.FindChild(vitalName).gameObject;
                Graph graph = SimulationSetup.instance.graphs.transform.FindChild(vitalName).GetComponent<Graph>();

                timeline = new TimeLine();

                values = new List<Value>();
                foreach (KeyValuePair<float, Slider> item in graph.sortedGraphPointsList) {
                    value = new Value();
                    value.second = item.Key;
                    value.value = item.Value.value;
                    values.Add(value);
                }
                timeline.vitalValues = values;

                values = new List<Value>();
                foreach (KeyValuePair<float, Slider> item in graph.pointsUpperThreshold) {
                    value = new Value();
                    value.second = item.Key;
                    value.value = item.Value.value;
                    values.Add(value);
                }
                timeline.upperThresholdValues = values;

                values = new List<Value>();
                foreach (KeyValuePair<float, Slider> item in graph.pointsLowerThreshold) {
                    value = new Value();
                    value.second = item.Key;
                    value.value = item.Value.value;
                    values.Add(value);
                }
                timeline.lowerThresholdValues = values;



                vitalData = new VitalData();
                vitalData.vital = SimulationSetup.instance.GetVital(vitalName);
                vitalData.timeline = timeline;

                condition.vitalsData.Add(vitalData);

            }
        }
        ExportManager.instance.SaveCondition(condition, Path.GetFullPath(Application.dataPath + filepathInputField.text));
        gameObject.SetActive(false);
    }
}
