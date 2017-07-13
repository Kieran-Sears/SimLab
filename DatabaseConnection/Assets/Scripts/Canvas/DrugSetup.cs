﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrugSetup : MonoBehaviour {

    public TabManager tabManager;
    public GameObject togglePrefab;
    public InputField drugName;
    public GameObject administrationsChosen;

    public InputField unitOfMeasure;
    public InputField drugDurationMinutes;
    public InputField drugDurationSeconds;
    public GameObject vitalsChosen;
    public Button submitButton;

    private Graph graph;
    private int duration = -1;
    private bool replaceExistingGraphs;

    public void Start() {
        PopulateAdministrations();
        ResetValues();
    }

    public void ResetValues() {
        submitButton.interactable = false;
        unitOfMeasure.interactable = false;
        drugDurationMinutes.interactable = false;
        drugDurationSeconds.interactable = false;
        for (int i = 0; i < vitalsChosen.transform.childCount; i++) {
            Destroy(vitalsChosen.transform.GetChild(i).gameObject);
        }
    }

    public void ToggleElementsActive() {
        submitButton.interactable = !submitButton.interactable;
        unitOfMeasure.interactable = !unitOfMeasure.interactable;
        drugDurationMinutes.interactable = !drugDurationMinutes.interactable;
        drugDurationSeconds.interactable = !drugDurationSeconds.interactable;
    }

    void PopulateAdministrations() {
        Drugs drugs = ExportManager.instance.Load("drugs") as Drugs;
        foreach (Drug drug in drugs.drugs) {
            print("Exploring drug " + drug.name + " for administrations");
            foreach (Administration admin in drug.administrations) {
                print("found " + admin.name);
                if (administrationsChosen.transform.FindChild(admin.name) == null) {
                    print("no duplicate found so adding to administrationsChosen");
                    GameObject toggleObject = Instantiate(togglePrefab);
                    toggleObject.transform.SetParent(administrationsChosen.transform);
                    toggleObject.transform.localScale = Vector3.one;
                    toggleObject.transform.localPosition = Vector3.zero;
                    toggleObject.transform.GetChild(1).GetComponent<Text>().text = admin.name;
                    Toggle toggle = toggleObject.GetComponent<Toggle>();
                    toggle.name = admin.name;
                    toggle.isOn = false;
                    toggle.onValueChanged.AddListener((bool value) => LoadChosenAdministration(value, toggleObject.transform.GetSiblingIndex(), admin.name));
                }
            }
        }
    }

    public void SubmitDuration() {
        int newDuration = 0;

        int minutes = 0;
        int seconds = 0;
        if (int.TryParse(drugDurationMinutes.text, out minutes)) {
            if (minutes > 60) {
                Error.instance.informMessageText.text = "Minutes cannot exceed 60.";
                Error.instance.informPanel.SetActive(true);
                Error.instance.informOkButton.onClick.AddListener(SelectMinutesDuration);
            }
            else if (minutes < 0) {
                Error.instance.informMessageText.text = "Minutes cannot be less than 0.";
                Error.instance.informPanel.SetActive(true);
                Error.instance.informOkButton.onClick.AddListener(SelectMinutesDuration);
            }
            else {
                newDuration += minutes * 60;
            }
        }

        if (int.TryParse(drugDurationSeconds.text, out seconds)) {
            if (seconds > 60) {
                Error.instance.informMessageText.text = "Seconds cannot exceed 60.";
                Error.instance.informPanel.SetActive(true);
                Error.instance.informOkButton.onClick.AddListener(SelectSecondsDuration);
            }
            else if (seconds < 0) {
                Error.instance.informMessageText.GetComponentInChildren<Text>().text = "Seconds cannot be less than 0.";
                Error.instance.informPanel.SetActive(true);
                Error.instance.informOkButton.onClick.AddListener(SelectMinutesDuration);
            }
            else if (minutes == 0 && seconds < 5) {
                Error.instance.informMessageText.text = "Simulation duration should exeed 5 seconds.";
                Error.instance.informPanel.SetActive(true);
                Error.instance.informOkButton.onClick.AddListener(SelectMinutesDuration);
            }
            else {
                newDuration += seconds;
            }
            if (duration != newDuration && duration != -1 && replaceExistingGraphs == false) {
                Error.instance.boolMessageText.text = "This action may overwrite data on already active graphs. Are you sure?";
                Error.instance.boolPanel.SetActive(true);
                Error.instance.boolRightButton.onClick.AddListener(ChangeActiveGraphDurations);
                Error.instance.boolLeftButton.onClick.AddListener(ResetDurationBack);
            }
            else {
                duration = newDuration;
                submitButton.interactable = false;
                replaceExistingGraphs = false;
            }
        }
    }

    private void SelectMinutesDuration() {
        Error.instance.informPanel.SetActive(false);
        drugDurationMinutes.Select();
        drugDurationMinutes.ActivateInputField();
        Error.instance.informOkButton.onClick.RemoveAllListeners();
    }

    private void SelectSecondsDuration() {
        Error.instance.informPanel.SetActive(false);
        drugDurationSeconds.Select();
        drugDurationSeconds.ActivateInputField();
        Error.instance.informOkButton.onClick.RemoveAllListeners();
    }

    void PopulateVitals() {
        foreach (Vital vital in SimulationSetup.instance.vitals.vitalList) {
            GameObject toggleObject = Instantiate(togglePrefab);
            toggleObject.transform.SetParent(vitalsChosen.transform);
            toggleObject.transform.localScale = Vector3.one;
            toggleObject.transform.localPosition = Vector3.zero;
            toggleObject.transform.GetChild(1).GetComponent<Text>().text = vital.name;
            Toggle toggle = toggleObject.GetComponent<Toggle>();
            toggle.name = vital.name;
            toggle.isOn = false;
            toggle.onValueChanged.AddListener((bool value) => loadChosenVital(value, toggleObject.transform.GetSiblingIndex(), vital.name));
        }
    }

    public void ResetDurationBack() {
        Error.instance.boolPanel.SetActive(false);
        Error.instance.boolRightButton.onClick.RemoveAllListeners();
        Error.instance.boolLeftButton.onClick.RemoveAllListeners();
        drugDurationMinutes.text = (duration / 60).ToString();
        drugDurationSeconds.text = (duration % 60).ToString();
    }

    void ChangeActiveGraphDurations() {
        Error.instance.boolPanel.SetActive(false);
        Error.instance.boolDropdown.gameObject.SetActive(false);
        Error.instance.boolLeftButton.onClick.RemoveAllListeners();
        Error.instance.boolRightButton.onClick.RemoveAllListeners();
        Error.instance.boolLeftButton.GetComponentInChildren<Text>().text = "no";

        replaceExistingGraphs = true;

        //if (!someBool) {
        //    SubmitDuration();
        //}

        for (int i = 0; i < vitalsChosen.transform.childCount; i++) {

            Toggle toggle = vitalsChosen.transform.GetChild(i).GetComponent<Toggle>();

            if (toggle.isOn) {

                string vitalName = toggle.gameObject.name;

                tabManager.GetComponent<TabManager>().tabGraphs.Remove(toggle);
                tabManager.GetComponent<ToggleGroup>().UnregisterToggle(toggle);

                Destroy(tabManager.transform.FindChild(vitalName).gameObject);

                GameObject graphObject = tabManager.transform.FindChild(vitalName).gameObject;
                Graph graph = tabManager.transform.FindChild(vitalName).GetComponent<Graph>();

                SortedList<float, Slider> sortedGraphPointsList = graph.sortedGraphPointsList;
                SortedList<float, Slider> pointsUpperThreshold = graph.pointsUpperThreshold;
                SortedList<float, Slider> pointsLowerThreshold = graph.pointsLowerThreshold;

                int vitalMin = graph.yStart;
                int vitalMax = graph.yEnd;

                string vitalUnits = graph.yAxisLabel.GetComponent<Text>().text;

                Destroy(graphObject);

                graph = tabManager.GenerateTab(vitalName).GetComponent<Graph>();
                graph.GenerateGraph(0, duration, vitalMin, vitalMax, vitalUnits);

                toggle.onValueChanged.RemoveAllListeners();
                toggle.isOn = true;
                toggle.onValueChanged.AddListener((bool value) => loadChosenVital(value, i, vitalName));

                if (i != 0) {
                    graph.transform.gameObject.SetActive(false);
                }

                foreach (KeyValuePair<float, Slider> item in sortedGraphPointsList) {
                    if (item.Key < duration) {
                        graph.AddPoint(item.Key, item.Value.value);
                    }
                }
                graph.AddPoint(duration, ((vitalMax - vitalMin) / 2));

                foreach (KeyValuePair<float, Slider> item in pointsUpperThreshold) {
                    if (item.Key < duration) {
                        graph.AddThresholdPointUpper(item.Key, item.Value.value);
                    }
                }

                foreach (KeyValuePair<float, Slider> item in pointsLowerThreshold) {
                    if (item.Key < duration) {
                        graph.AddThresholdPointLower(item.Key, item.Value.value);
                    }
                }

                if (!graph.pointsLowerThreshold.ContainsKey(duration)) {
                    graph.AddThresholdPointLower(duration, graph.pointsLowerThreshold[0].value);
                }

                if (!graph.pointsUpperThreshold.ContainsKey(duration)) {
                    graph.AddThresholdPointUpper(duration, graph.pointsUpperThreshold[0].value);
                }

            }
        }
    }

    public void loadChosenVital(bool chosen, int index, string vitalName) {
        tabManager.activeTabs.transform.GetComponent<ToggleGroup>().SetAllTogglesOff();
        if (chosen) {
            Transform vitalTrans = tabManager.transform.FindChild(vitalName);
            Transform vitalTab = tabManager.inactiveTabs.transform.FindChild(vitalName);
            if (vitalTrans == null || vitalTab == null) {
                if (duration == -1) {
                    Error.instance.informMessageText.text = "Please set the duration of the drugs effects before adding vitals.";
                    Error.instance.informPanel.SetActive(true);
                    Error.instance.informOkButton.onClick.AddListener(SelectMinutesDuration);
                    vitalsChosen.transform.GetChild(index).GetComponent<Toggle>().isOn = false;
                    return;
                }
                graph = tabManager.GenerateTab(SimulationSetup.instance.vitals.vitalList[index].name).GetComponent<Graph>();
                graph.GenerateGraph(0, duration, (int)Math.Ceiling(SimulationSetup.instance.vitals.vitalList[index].min), (int)Math.Ceiling(SimulationSetup.instance.vitals.vitalList[index].max), SimulationSetup.instance.vitals.vitalList[index].units);
                if (graph.sortedGraphPointsList.Count == 0) {
                    int halfValue = (int)Math.Ceiling(((SimulationSetup.instance.vitals.vitalList[index].max - SimulationSetup.instance.vitals.vitalList[index].min) / 2) + SimulationSetup.instance.vitals.vitalList[index].min);
                    graph.AddPoint(0, halfValue);
                    graph.AddPoint(duration, halfValue);
                }
                graph.gameObject.SetActive(false);
                tabManager.SwitchTab();
            }
            else {
                vitalTrans.gameObject.SetActive(true);
                vitalTab.SetParent(tabManager.transform);
                vitalTab.gameObject.SetActive(true);
                tabManager.SwitchTab();
            }
        }
        else {
            Transform tab = tabManager.transform.FindChild(vitalName);
            if (tab != null) {
                tab.gameObject.SetActive(false);
                tab.SetParent(tabManager.inactiveTabs.transform);
                tabManager.transform.FindChild(vitalName).gameObject.SetActive(false);
            }
        }
    }

    public void LoadChosenAdministration(bool chosen, int index, string administrationName) {

       

      //  AdminSetup administrationSetup;
        //tabManager.activeTabs.GetComponent<ToggleGroup>().SetAllTogglesOff();
        //if (chosen) {
        //    Transform vitalTrans = tabManager.transform.FindChild(administrationName);
        //    Transform vitalTab = tabManager.inactiveTabs.transform.FindChild(administrationName);
        //    if (vitalTrans == null || vitalTab == null) {
             //   administrationSetup = tabManager.GenerateTab(administrationName).GetComponent<AdminSetup>();
        //        administrationSetup.gameObject.SetActive(false);
        //        tabManager.SwitchTab();
        //    }
        //    else {
        //        vitalTrans.gameObject.SetActive(true);
        //        vitalTab.SetParent(tabManager.transform);
        //        vitalTab.gameObject.SetActive(true);
        //        tabManager.SwitchTab();
        //    }
        //}
        //else {
        //    Transform tab = tabManager.transform.FindChild(administrationName);
        //    if (tab != null) {
        //        tab.gameObject.SetActive(false);
        //        tab.SetParent(tabManager.inactiveTabs.transform);
        //        tabManager.transform.FindChild(administrationName).gameObject.SetActive(false);
        //    }
        //}
    }

    public void AddNewDrug() {

        if (drugName.text.Length == 0) {
            Error.instance.informMessageText.text = "Enter a name for the drug.";
            Error.instance.informOkButton.onClick.AddListener(Error.instance.DeactivateErrorInformPanel);
            Error.instance.informPanel.SetActive(true);
            return;
        }

        Drug drug = new Drug();
        List<Administration> administrations = new List<Administration>();

        for (int i = 0; i < tabManager.contentArea.transform.childCount; i++) {
            AdminSetup admin = tabManager.contentArea.transform.GetChild(i).GetComponent<AdminSetup>();
            Administration administration = GetAdministration();
            if (administration == null) {
                return;
            }
            else {
                administrations.Add(GetAdministration());
            }
        }
        drug.administrations = administrations;
        drug.name = drugName.text;
        SimulationSetup.instance.drugs.drugs.Add(drug);

        GameObject toggleObject = Instantiate(togglePrefab);
        toggleObject.transform.SetParent(SimulationSetup.instance.drugsChosen.transform);
        toggleObject.transform.localScale = Vector3.one;
        toggleObject.transform.localPosition = Vector3.zero;
        toggleObject.transform.GetChild(1).GetComponent<Text>().text = drug.name;
        Toggle toggle = toggleObject.GetComponent<Toggle>();
        toggle.name = drug.name;
        SimulationSetup.instance.NewDrugPanelToggleActive();
    }

    public Administration GetAdministration() {

        if (unitOfMeasure.text.Length == 0) {
            Error.instance.informMessageText.text = "Enter units for " + gameObject.name + ".";
            Error.instance.informOkButton.onClick.AddListener(Error.instance.DeactivateErrorInformPanel);
            Error.instance.informPanel.SetActive(true);
            return null;
        }

        if (duration == -1) {
            Error.instance.informMessageText.text = "Enter a duration for " + gameObject.name + ".";
            Error.instance.informOkButton.onClick.AddListener(Error.instance.DeactivateErrorInformPanel);
            Error.instance.informPanel.SetActive(true);
            return null;
        }

        Administration administration = new Administration();
        administration.duration = duration;

        VitalData vitalData;
        TimeLine timeLine;
        List<Value> values;
        Value value;

        for (int j = 0; j < vitalsChosen.transform.childCount; j++) {

            Toggle toggle = vitalsChosen.transform.GetChild(j).GetComponent<Toggle>();

            if (toggle.isOn) {

                string vitalName = toggle.gameObject.name;

                GameObject graphObject = tabManager.contentArea.transform.FindChild(vitalName).gameObject;
                Graph graph = tabManager.contentArea.transform.FindChild(vitalName).GetComponent<Graph>();

                timeLine = new TimeLine();

                values = new List<Value>();
                foreach (KeyValuePair<float, Slider> item in graph.sortedGraphPointsList) {
                    value = new Value();
                    value.second = item.Key;
                    value.value = item.Value.value;
                    values.Add(value);
                }
                timeLine.vitalValues = values;

                values = new List<Value>();
                foreach (KeyValuePair<float, Slider> item in graph.pointsUpperThreshold) {
                    value = new Value();
                    value.second = item.Key;
                    value.value = item.Value.value;
                    values.Add(value);
                }
                timeLine.upperThresholdValues = values;

                values = new List<Value>();
                foreach (KeyValuePair<float, Slider> item in graph.pointsLowerThreshold) {
                    value = new Value();
                    value.second = item.Key;
                    value.value = item.Value.value;
                    values.Add(value);
                }
                timeLine.lowerThresholdValues = values;

                vitalData = new VitalData();
                vitalData.vital = SimulationSetup.instance.GetVital(vitalName);
                vitalData.timeline = timeLine;

                administration.vitalsData.Add(vitalData);
            }
        }
        administration.duration = duration;
        // administration.max = float.Parse(drugMax.text);
        // administration.min = float.Parse(drugMin.text);
        administration.units = unitOfMeasure.text;
        administration.name = name;
        return administration;
    }
}