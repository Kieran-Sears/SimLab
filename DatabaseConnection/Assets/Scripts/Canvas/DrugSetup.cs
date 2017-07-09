using UnityEngine;
using UnityEngine.UI;

public class DrugSetup : MonoBehaviour {

    public TabManager tabManager;
    public GameObject togglePrefab;
    public GameObject administrationsChosen;

    private AdminSetup administrationSetup;

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
        print(tabManager);
        print(tabManager.activeTabs);
        print(tabManager.activeTabs.GetComponent<ToggleGroup>());
        tabManager.activeTabs.GetComponent<ToggleGroup>().SetAllTogglesOff();
        if (chosen) {
            Transform vitalTrans = tabManager.transform.FindChild(administrationName);
            Transform vitalTab = tabManager.inactiveTabs.transform.FindChild(administrationName);
            if (vitalTrans == null || vitalTab == null) {
                administrationSetup = tabManager.GenerateTab(administrationName).GetComponent<AdminSetup>();
                administrationSetup.gameObject.SetActive(false);
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
            Transform tab = tabManager.transform.FindChild(administrationName);
            if (tab != null) {
                tab.gameObject.SetActive(false);
                tab.SetParent(tabManager.inactiveTabs.transform);
                tabManager.transform.FindChild(administrationName).gameObject.SetActive(false);
            }
        }
    }

    public void AddNewDrug() {
        for (int i = 0; i < tabManager.contentArea.transform.childCount; i++) {
            tabManager.contentArea.transform.GetChild(i);
            Administration administration = new Administration();
            administration.duration = 

        }
        Drug drug = new Drug();
        drug.administrations.Add
    }
}