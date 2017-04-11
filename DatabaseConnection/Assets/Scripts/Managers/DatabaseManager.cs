using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;




public class DatabaseManager : MonoBehaviour {

    public TabManager tabs;

    public GameObject viewTableTextArea;

    public Button viewTableButton;
    public Button insertValuesButton;

    // Input fields MUST be named identically to vital name
    public InputField breathingRate;
    public InputField systolicBloodPressure;
    public InputField diastolicBloodPressure;
    public InputField heartRate;
    public InputField temperature;
    public InputField oxygenSaturations;
    public InputField capillaryRefill;


    //public void SetValues() {
    //    Vitals data = new Vitals();
    //    for (int i = 0; i < data.vitalList.Count; i++) {
    //        Type databaseManager = typeof(DatabaseManager);
    //        FieldInfo field = databaseManager.GetField(data.vitalList[i].name);
    //        field.SetValue(this, data.vitalList[i].value);
    //    }
    //    NetworkManager.instance.Upload(data);
    //}



}
