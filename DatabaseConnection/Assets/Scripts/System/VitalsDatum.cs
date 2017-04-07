using System;
using UnityEngine;


[Serializable]
public class VitalsDatum : MonoBehaviour {
    public string BreathingRate;
    public string HeartRate;
    public string SystolicBloodPressure;
    public string DiastolicBloodPressure;
    public string OxygenSaturations;
    public string CapillaryRefill;
    public string Temperature;


    public void SetBreathingRate(string breathingRate) {
        BreathingRate = breathingRate;
    }
    public void SetHeartRate(string heartRate) {
        HeartRate = heartRate;
    }
    public void SetSystolicBloodPressure(string systolicBloodPressure) {
        SystolicBloodPressure = systolicBloodPressure;
    }
    public void SetDiastolicBloodPressure(string diastolicBloodPressure) {
        DiastolicBloodPressure = diastolicBloodPressure;
    }
    public void SetOxygenSaturations(string oxygenSaturations) {
        OxygenSaturations = oxygenSaturations;
    }
    public void SetCapillaryRefill(string capillaryRefill) {
        CapillaryRefill = capillaryRefill;
    }
    public void SetTemperature(string temperature) {
        Temperature = temperature;
    }
    public float GetBreathingRate() {
        return float.Parse(BreathingRate);
    }
    public float GetHeartRate() {
        return float.Parse(HeartRate);
    }
    public float GetSystolicBloodPressure() {
        return float.Parse(SystolicBloodPressure);
    }
    public float GetDiastolicBloodPressure() {
        return float.Parse(DiastolicBloodPressure);
    }
    public float GetOxygenSaturations() {
        return float.Parse(OxygenSaturations);
    }
    public float GetCapillaryRefill() {
        return float.Parse(CapillaryRefill);
    }
    public float GetTemperature() {
        return float.Parse(Temperature);
    }
}
