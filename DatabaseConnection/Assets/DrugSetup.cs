using UnityEngine;
using UnityEngine.UI;

public class DrugSetup : MonoBehaviour {

    public TabController tabContainer;
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
                    toggle.onValueChanged.AddListener((bool value) => loadChosenAdministration(value, toggleObject.transform.GetSiblingIndex(), admin.name));
                }
            }
        }
    }

    public void loadChosenAdministration(bool chosen, int index, string administrationName) {
        tabContainer.activeTabs.transform.GetComponent<ToggleGroup>().SetAllTogglesOff();
        if (chosen) {

            Transform vitalTrans = tabContainer.transform.FindChild(administrationName);
            Transform vitalTab = tabContainer.inactiveTabs.transform.FindChild(administrationName);

            if (vitalTrans == null || vitalTab == null) {
                administrationSetup = tabContainer.GenerateTab(administrationName);
                administrationSetup.gameObject.SetActive(false);
                tabContainer.SwitchTab();
            }
            else {
                vitalTrans.gameObject.SetActive(true);
                vitalTab.SetParent(tabContainer.transform);
                vitalTab.gameObject.SetActive(true);
                tabContainer.SwitchTab();
            }
        }
        else {
            Transform tab = tabContainer.transform.FindChild(administrationName);
            if (tab != null) {
                tab.gameObject.SetActive(false);
                tab.SetParent(tabContainer.inactiveTabs.transform);
                tabContainer.transform.FindChild(administrationName).gameObject.SetActive(false);
            }
        }
    }
}