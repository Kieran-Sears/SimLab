using System;
using UnityEngine.Events;
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
    public GameObject vitalNameDuplicateWarning;
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
   // private GameObject tab;
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
        simulationDuration.onValidateInput += delegate (string input, int charIndex, char addedChar) { return SimulationDurationChangeValue(input, charIndex, addedChar); };
        vitalName.onValidateInput += delegate (string input, int charIndex, char addedChar) { return VitalNameChangeValue(input, charIndex, addedChar); };
    }

    public char SimulationDurationChangeValue(string input, int charIndex, char character) {
        if (charIndex == 2) {
            return ':';
        }
        return character;
    }

    public char VitalNameChangeValue(string input, int charIndex, char character) {
        if (vitalsChosen.transform.FindChild(input + character) != null) {
            vitalNameDuplicateWarning.SetActive(true);
        } else {
            vitalNameDuplicateWarning.SetActive(false);
        }
        return character;
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
        foreach (Vital vital in vitals.vitalList) {
            GameObject toggleObject = Instantiate(togglePrefab);
            toggleObject.transform.SetParent(vitalsChosen.transform);
            toggleObject.transform.localScale = Vector3.one;
            toggleObject.transform.localPosition = Vector3.zero;
            toggleObject.transform.GetChild(1).GetComponent<Text>().text = vital.name;
            Toggle toggle = toggleObject.GetComponent<Toggle>();
            toggle.name = vital.name;
            toggle.isOn = false;
            //  toggle.group = vitalsToggleGroup;
            toggle.onValueChanged.AddListener((bool value) => loadChosenVital(value, toggleObject.transform.GetSiblingIndex(), vital.name));
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
            //  toggle.onValueChanged.AddListener((bool value) => loadChosenVital(value, toggleObject.transform.GetSiblingIndex()));
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
                Error.instance.PrintError("Condition could not be found.");
            } else {
                List<Value> vitalData = condition.timeline[0].vitalValues;
                for (int i = 0; i < vitalData.Count; i++) {
                    Vital vital = vitals.vitalList[vitalData[i].vitalID];
                    graph = tabs.GenerateTab(vital.name);
                    graph.GenerateGraph(0, condition.timeline.Count - 1, "Duration", (int)Math.Ceiling(vital.min), (int)Math.Ceiling(vital.max), vital.units);
                    if (i != 0) {
                        graph.transform.gameObject.SetActive(false);
                    }
                }

                for (int i = 0; i < condition.timeline.Count; i++) {
                    Time time = condition.timeline[i];
                    foreach (Value data in time.vitalValues) {
                        Vital vital = vitals.vitalList[data.vitalID];
                        Graph graph = graphs.transform.FindChild(vital.name).GetComponent<Graph>();
                        graph.AddPoint(i, data.value);

                        if (data.upperThreshold != -1) {
                            graph.AddThresholdPointUpper(i, data.upperThreshold);
                        }

                        if (data.lowerThreshold != -1) {
                            graph.AddThresholdPointLower(i, data.lowerThreshold);
                        }
                    }

                    // if contains threshold values (add here)
                }
            }
        }
    }

    int GetDuration() {
        int duration = 0;
        string[] times = simulationDuration.text.Split(':');
        int store;
        if (int.TryParse(times[0], out store)) {
            duration = store * 60;
        } else {
            return -1;
        }
        if (times.Length != 1 && int.TryParse(times[1], out store)) {
            duration += store;
        } else {
            return duration;
        }
        return duration;
    }

    public void AddVital() {
        if (float.Parse(vitalMax.text) <= float.Parse(vitalMin.text)) {
            Error.instance.PrintError("Max value is less or equal to min value for vital");
            return;
        }
        Transform vitalTrans = graphs.transform.FindChild(vitalName.text);
        if (vitalTrans != null) {
            Destroy(vitalTrans);
        }
        Vital vital = new Vital();
        vital.nodeID = vitals.vitalList.Count;
        vital.name = vitalName.text;
        vital.units = vitalUnit.text;
        vital.max = float.Parse(vitalMax.text);
        vital.min = float.Parse(vitalMin.text);
        newVitalPanel.SetActive(false);
        GameObject toggleObject = Instantiate(togglePrefab);
        toggleObject.transform.SetParent(vitalsChosen.transform);
        toggleObject.transform.SetAsFirstSibling();
        toggleObject.transform.localScale = Vector3.one;
        toggleObject.transform.localPosition = Vector3.zero;
        toggleObject.transform.GetChild(1).GetComponent<Text>().text = vital.name;
        Toggle toggle = toggleObject.GetComponent<Toggle>();
        toggle.name = vital.name;
        toggle.onValueChanged.AddListener((bool value) => loadChosenVital(value, toggleObject.transform.GetSiblingIndex(), vital.name));
        vitals.vitalList.Insert(0, vital);
        loadChosenVital(true, 0, vital.name);
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



    public void loadChosenVital(bool chosen, int index, string vitalName) {
        tabs.transform.GetComponent<ToggleGroup>().SetAllTogglesOff();
        if (chosen) {
            Transform vitalTrans = graphs.transform.FindChild(vitalName);
            if (vitalTrans == null || vitalName == "") {
                int duration = GetDuration();
                if (duration == -1) {
                    Error.instance.PrintDurationError("What is the duration of the simulation?");
                    return;
                }
                graph = tabs.GenerateTab(vitals.vitalList[index].name);
                graph.GenerateGraph(1, duration, "Duration (Seconds)", (int)Math.Ceiling(vitals.vitalList[index].min), (int)Math.Ceiling(vitals.vitalList[index].max), vitals.vitalList[index].units);
                graph.gameObject.SetActive(false);
                tabs.SwitchTab();
            } else {
                // Overwrite existing vital
                vitalTrans.gameObject.SetActive(true);
                Transform vitalTab = inactiveTabs.transform.FindChild(vitalName);
                if (vitalTab == null) {
                    Error.instance.PrintError("Could not find inactiveTabs child : " + vitalName);
                    Debug.Break();
                }
                vitalTab.SetParent(tabs.transform);
                vitalTab.gameObject.SetActive(true);
                tabs.SwitchTab();
            }
        } else {
            Debug.Break();         
            Transform tab = tabs.transform.FindChild(vitalName);
            if (tab != null) {
                tab.gameObject.SetActive(false);
                tab.SetParent(inactiveTabs.transform);
                graphs.transform.FindChild(vitalName).gameObject.SetActive(false);
            }
        }
    }

    //void LoadVitalsTabs() {
    //    for (int i = 0; i < vitalsChosen.transform.childCount; i++) {
    //        if (vitalsChosen.transform.GetChild(i).GetComponent<Toggle>().isOn) {
    //            print(vitals.vitalList[i]);
    //            if (vitals.vitalList[i] == null) {
    //                print("add clause for new vital here");
    //            } else {
    //                graph = tabs.GenerateTab(vitals.vitalList[i].name);
    //                graph.GenerateGrid(1, GetDuration(), 1, (int)Math.Ceiling(vitals.vitalList[i].max - vitals.vitalList[i].min));
    //                if (i != 0) {
    //                    graph.gameObject.SetActive(false);
    //                }
    //            }
    //        }
    //    }
    //}

    //public void GenerateGraph() {
    //    ClearPreviousTabs();
    //    LoadVitalsTabs();
    //}

    public void NewVitalPanelToggleActive() {
        newVitalPanel.SetActive(!newVitalPanel.activeInHierarchy);
    }

    public void NewDrugPanelToggleActive() {
        newDrugPanel.SetActive(!newDrugPanel.activeInHierarchy);
    }
}
