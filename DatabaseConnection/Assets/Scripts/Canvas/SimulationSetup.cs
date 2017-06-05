using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimulationSetup : MonoBehaviour {

    public static SimulationSetup instance { get; private set; }

    public TabManager tabs;
    public GameObject tableEntryPrefab;
    public GameObject togglePrefab;
    public GameObject graphs;

    #region Condition
    public Dropdown presets;
    public InputField simulationDuration;
    public GameObject vitalsChosen;
    public GameObject drugsChosen;
    public GameObject equipmentChosen;
    #endregion

    #region Vital
    public GameObject newVitalPanel;
    public InputField vitalName;
    public InputField vitalUnit;
    public InputField vitalMax;
    public InputField vitalMin;
    #endregion

    #region Drug
    public GameObject newDrugPanel;
    public InputField drugName;
    public InputField drugUnit;
    public InputField drugMax;
    public InputField drugMin;
    public InputField drugDuration;
    public GameObject drugVitalsAffected;
    public GameObject drugAdministrations;
    #endregion

    #region Bins
    public GameObject inactiveTabs;
    #endregion

    private Drugs drugs;
    private Vitals vitals;
    private Equipment equipment;
    private GameObject tab;
    private Graph graph;

    private void Awake() {
        if (instance) {
            DestroyImmediate(this);
        } else {
            instance = this;
        }
    }

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

    void PopulateAdministrations() { }

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
            Debug.Log(tabs.transform.childCount);
            for (int i = tabs.transform.childCount - 1; i >= 0; i--) {
                Transform tab = tabs.transform.GetChild(i);
                Debug.Log("Getting rid of tab " + tab.name);
                tab.gameObject.SetActive(false);
                tab.SetParent(inactiveTabs.transform);
            }
            for (int i = graphs.transform.childCount - 1; i > 1; i--) {
                print(graphs.transform.GetChild(i).gameObject.name);
                graphs.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    public void LoadCondition(int index) {
        ClearPreviousTabs();
        if (presets.options[index].text != "None") {
            Debug.Log("Finding condition: " + presets.options[index].text);
            Condition condition = ExportManager.instance.Load("Conditions/" + presets.options[index].text) as Condition;
            if (condition == null) {
                Error.instance.DisplayMessage("Condition could not be found.");
            } else {
                List<Value> vitalData = condition.timeline[0].vitalValues;
                for (int i = 0; i < vitalData.Count; i++) {
                    Vital vital = vitals.vitalList[vitalData[i].vitalID];
                    graph = tabs.GenerateTab(vital.name);
                    graph.GenerateGrid(1, condition.timeline.Count, 1, (int)Math.Ceiling(vital.max - vital.min));
                    if (i != 0) {
                        graph.transform.gameObject.SetActive(false);
                    }
                }

                for (int i = 0; i < condition.timeline.Count; i++) {
                    Time time = condition.timeline[i];
                    foreach (Value data in time.vitalValues) {
                        Vital vital = vitals.vitalList[data.vitalID];
                        graphs.transform.FindChild(vital.name).GetComponent<Graph>().AddPoint(i, data.value);
                    }
                }
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

    public void AddVital() {
        Vital vital = new Vital();
        vital.nodeID = vitals.vitalList.Count;
        vital.name = vitalName.text;
        vital.units = vitalUnit.text;
        vital.max = float.Parse(vitalMax.text);
        vital.min = float.Parse(vitalMin.text);
        newVitalPanel.SetActive(false);
        GameObject toggleObject = Instantiate(togglePrefab);
        toggleObject.transform.SetParent(vitalsChosen.transform);
        toggleObject.transform.localScale = Vector3.one;
        toggleObject.transform.localPosition = Vector3.zero;
        toggleObject.transform.GetChild(1).GetComponent<Text>().text = vital.name;
        Toggle toggle = toggleObject.GetComponent<Toggle>();
        toggle.name = vital.name;
        vitals.vitalList.Add(vital);
    }

    public void AddDrug() {
        Drug drug = new Drug();
        //drug.nodeID = ; //TODO assign ID based on Id's already present
        drug.name = drugName.text;
        drug.units = drugUnit.text;
        drug.max = float.Parse(drugMax.text);
        drug.min = float.Parse(drugMin.text);
        drug.duration = float.Parse(drugDuration.text);

        for (int i = 0; i < drugVitalsAffected.transform.childCount; i++) {
            drug.vitals.Add(drugVitalsAffected.transform.GetChild(i).name);
        }

        for (int i = 0; i < drugAdministrations.transform.childCount; i++) {
            drug.administrations.Add(drugAdministrations.transform.GetChild(i).name);
        }

        newDrugPanel.SetActive(false);

        GameObject toggleObject = Instantiate(togglePrefab);
        toggleObject.transform.SetParent(drugsChosen.transform);
        toggleObject.transform.localScale = Vector3.one;
        toggleObject.transform.localPosition = Vector3.zero;
        toggleObject.transform.GetChild(1).GetComponent<Text>().text = drug.name;
        Toggle toggle = toggleObject.GetComponent<Toggle>();
        toggle.name = drug.name;
    }

    void LoadVitalsTabs() {
        for (int i = 0; i < vitalsChosen.transform.childCount; i++) {
            if (vitalsChosen.transform.GetChild(i).GetComponent<Toggle>().isOn) {
                print(vitals.vitalList[i]);
                if (vitals.vitalList[i] == null) {
                    print("add clause for new vital here");
                } else {
                    graph = tabs.GenerateTab(vitals.vitalList[i].name);
                    graph.GenerateGrid(1, GetDuration(), 1, (int)Math.Ceiling(vitals.vitalList[i].max - vitals.vitalList[i].min));
                    if (i != 0) {
                        graph.gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    public void GenerateGraph() {
        ClearPreviousTabs();
        LoadVitalsTabs();
    }

}
