using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;




public class DatabaseManager : MonoBehaviour {

    public TabManager tabs;

    public GameObject viewTableTextArea;
    public GameObject tableEntryPrefab;

    public Button viewTableButton;
    public Button insertValuesButton;

    public InputField breathingRate;
    public InputField bloodPressure;
    public InputField heartRate;
    public InputField temperature;
    public InputField oxygenSaturations;
    public InputField capillaryRefill;


    public string GetUnitOfMeasure(string vital) {
        switch (vital) {
            case "BreathingRate":
                return "BPM";
            case "HeartRate":
                return "BPM";
            case "BloodPressure":
                return "mmHg";
            case "OxygenSaturations":
                return "SpO2 / SaO2";
            case "CapillaryRefill":
                return "ms";
            case "Temperature":
                return "Celsius";
            default:
                return null;
        }
    }


    public void SetValues() {
        VitalsDatum data = new VitalsDatum();
        data.BreathingRate = breathingRate.text;
        data.SystolicBloodPressure = bloodPressure.text;
        data.HeartRate = heartRate.text;
        data.Temperature = temperature.text;
        data.OxygenSaturations = oxygenSaturations.text;
        data.CapillaryRefill = capillaryRefill.text;
        NetworkManager.instance.Upload(data);
    }

    public void DisplayValues() {
        // get data from database
        VitalsData vitalsData = NetworkManager.instance.Download();
        RectTransform rectTrans = viewTableTextArea.GetComponent<RectTransform>();

        Type vitalsDatum = typeof(VitalsDatum);
        // get table columns
        FieldInfo[] vitalsFields = vitalsDatum.GetFields();
        // generate table with appropriate scales on axis
        viewTableTextArea.GetComponent<GridLayoutGroup>().cellSize = new Vector2(rectTrans.rect.width / vitalsFields.Length, rectTrans.rect.height / vitalsData.data.Count);
        tabs.activeGraph.GenerateGraph(1, vitalsFields.Length, 1, 100);

        for (int i = 0; i < vitalsFields.Length; i++) {
            Debug.Log(vitalsFields[i].Name + " " + GetUnitOfMeasure(vitalsFields[i].Name));
            tabs.AddTab(vitalsFields[i].Name, GetUnitOfMeasure(vitalsFields[i].Name), vitalsData.data);
        }

        //for the number of rows(i.e.timelapse of vitals where each i = 6 seconds)
        //for (int i = 0; i < vitalsData.data.Count; i++) {
        //        VitalsDatum row = vitalsData.data[i];

        //        for (int j = 0; j < vitalsFields.Length; j++) {
        //            FieldInfo field = vitalsFields[j];
        //            AddTableEntry(field.Name + "\n" + field.GetValue(row) as string);
        //            tabs.activeGraph.AddPoint(j, float.Parse(field.GetValue(row) as string));
        //        }
        //    }
    }

    private void AddTableEntry(string value) {
        GameObject Datum = Instantiate(tableEntryPrefab);
        Datum.GetComponent<Text>().text = value;
        Datum.transform.SetParent(viewTableTextArea.transform, false);
    }
}
