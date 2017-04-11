using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SimulationSetup : MonoBehaviour {

    public TabManager tabs;
    public GameObject tableEntryPrefab;

    public Dropdown presets;
    public InputField simulationDuration;
    public GameObject vitalsChosen;
    public GameObject drugsChosen;
    public GameObject equipmentChosen;

    public GameObject togglePrefab;

    private Drugs drugs;
    private Vitals vitals;
    private Equipment equipment;
    private GameObject tab;
    private Graph graph;

    void Start() {
        PopulatePresets();
        PopulateVitals();
        PopulateDrugs();
        PopulateEquipment();
    }

    void PopulatePresets() {
        UnityEngine.Object[] files = Resources.LoadAll("Conditions");
        presets.options.Add(new Dropdown.OptionData() { text = "None" });
        for (int i = 0; i < files.Length; i++) {
            presets.options.Add(new Dropdown.OptionData() { text = files[i].name });
        }
        presets.captionText.text = "Preset Conditions...";

    }

    void PopulateVitals() {
        vitals = ExportManager.instance.Load("vitals") as Vitals;
        foreach (Vital item in vitals.vitalList) {
            GameObject toggleObject = Instantiate(togglePrefab);
            toggleObject.transform.SetParent(vitalsChosen.transform);
            toggleObject.transform.localScale = Vector3.one;
            toggleObject.transform.localPosition = Vector3.zero;
            toggleObject.transform.GetChild(1).GetComponent<Text>().text = item.name;
            Toggle toggle = toggleObject.GetComponent<Toggle>();
            toggle.name = item.name;
        }
    }

    void PopulateDrugs() {
        drugs = ExportManager.instance.Load("drugs") as Drugs;
        foreach (Drug item in drugs.drugList) {
            GameObject toggleObject = Instantiate(togglePrefab);
            toggleObject.transform.SetParent(drugsChosen.transform);
            toggleObject.transform.localScale = Vector3.one;
            toggleObject.transform.localPosition = Vector3.zero;
            toggleObject.transform.GetChild(1).GetComponent<Text>().text = item.name;
            Toggle toggle = toggleObject.GetComponent<Toggle>();
            toggle.name = item.name;

        }
    }

    void PopulateEquipment() {
        equipment = ExportManager.instance.Load("equipment") as Equipment;
        foreach (Item item in equipment.itemList) {
            GameObject toggleObject = Instantiate(togglePrefab);
            toggleObject.transform.SetParent(equipmentChosen.transform);
            toggleObject.transform.localScale = Vector3.one;
            toggleObject.transform.localPosition = Vector3.zero;
            toggleObject.transform.GetChild(1).GetComponent<Text>().text = item.name;
            Toggle toggle = toggleObject.GetComponent<Toggle>();
            toggle.name = item.name;
        }
    }

    void ClearPreviousTabs() {
        if (tabs.transform.childCount > 0) {
            for (int i = 0; i < tabs.transform.childCount; i++) {
                tabs.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    public void LoadCondition(int index) {
        ClearPreviousTabs();
        Debug.Log("Finding condition: " + presets.options[index].text);
        Condition condition = ExportManager.instance.Load("Conditions/" + presets.options[index].text) as Condition;
        Debug.Log(condition.timeline.Count);
        List<Value> vitalData = condition.timeline[0].vitalValues;
        Debug.Log(vitalData.Count);
        for (int i = 0; i < vitalData.Count; i++) {
            Vital vital = vitals.vitalList[vitalData[i].vitalID];
            Debug.Log(vital.name);
            tab = tabs.GenerateTab(vital.name);
            graph = tab.GetComponent<Graph>();
            graph.container.transform.localPosition += new Vector3(-tab.GetComponent<RectTransform>().rect.width * i, 0, 0);
            graph.GenerateGrid(1, condition.timeline.Count, 1, (int)Math.Ceiling(vital.max - vital.min));
            if (i != 0) {
                graph.container.transform.gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < condition.timeline.Count; i++) {
            Time time = condition.timeline[i];
            foreach (Value data in time.vitalValues) {
            Vital vital = vitals.vitalList[data.vitalID];
            tabs.transform.GetChild(vital.nodeID).GetComponent<Graph>().AddPoint(i, data.value);
            }
        }
    }

    int GetDuration() {
        int duration;
        if (int.TryParse(simulationDuration.text, out duration)) {
            return duration;
        }
        Debug.Log("Not a number exception : TODO Provide warning to user ");
        return -1;
    }


    void LoadVitalsTabs() {
        for (int i = 0; i < vitalsChosen.transform.childCount; i++) {
            if (vitalsChosen.transform.GetChild(i).GetComponent<Toggle>().isOn) {
                Debug.Log("creating tab " + vitals.vitalList[i].name);
                tab = tabs.GenerateTab(vitals.vitalList[i].name);
                graph = tab.GetComponent<Graph>();
                graph.container.transform.localPosition += new Vector3(-tab.GetComponent<RectTransform>().rect.width * i, 0, 0);
                graph.GenerateGrid(1, GetDuration(), 1, (int)Math.Ceiling(vitals.vitalList[i].max - vitals.vitalList[i].min));
                if (i != 0) {
                    graph.container.transform.gameObject.SetActive(false);
                }
            }
        }
    }

    public void GenerateGraph() {
        ClearPreviousTabs();
        LoadVitalsTabs();
    }

}
