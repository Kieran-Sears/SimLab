using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrugSetup : MonoBehaviour {

    public TabManager tabManager;
    public GameObject togglePrefab;
    public InputField drugName;
    public GameObject administrationsChosen;
    public AdminSetup adminSetup;

    public void Start() {
        PopulateAdministrations();
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
            Administration administration = admin.GetAdministration();
            if (administration == null) {
                return;
            }
            else {
                administrations.Add(admin.GetAdministration());
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
}