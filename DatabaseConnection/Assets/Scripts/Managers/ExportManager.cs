using UnityEngine;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;




public class ExportManager : MonoBehaviour {

    public static ExportManager instance { get; private set; }

    TextAsset _xml;
    XmlSerializer serializer;
    StringReader reader;
    Vitals vitals;
    Drugs drugs;
    Equipment equipment;
    Condition condition;

    private void Awake() {
        if (instance) {
            DestroyImmediate(this);
        } else {
            instance = this;
        }
    }

   public System.Object Load(string path) {

        switch (path) {
            case "vitals":
                 _xml = Resources.Load<TextAsset>(path);
                 serializer = new XmlSerializer(typeof(Vitals));
                 reader = new StringReader(_xml.text);
                 vitals = serializer.Deserialize(reader) as Vitals;
                reader.Close();
                return vitals;
            case "drugs":
                 _xml = Resources.Load<TextAsset>(path);
                 serializer = new XmlSerializer(typeof(Drugs));
                 reader = new StringReader(_xml.text);
                 drugs = serializer.Deserialize(reader) as Drugs;
                reader.Close();
                return drugs;
            case "equipment":
                _xml = Resources.Load<TextAsset>(path);
                serializer = new XmlSerializer(typeof(Equipment));
                reader = new StringReader(_xml.text);
                equipment = serializer.Deserialize(reader) as Equipment;
                reader.Close();
                return equipment;
            default:
                Debug.Log(path);
                _xml = Resources.Load<TextAsset>(path);
                if (_xml == null) {
                    Debug.Log("Unrecognised condition name. Please check corrosponding xml filename for " + path);
                } else {
                    serializer = new XmlSerializer(typeof(Condition));
                    reader = new StringReader(_xml.text);
                    condition = serializer.Deserialize(reader) as Condition;
                    reader.Close();
                    return condition;
                }
                return null;
        }

    }



    public void SaveCondition(Dictionary<string, SortedList<float, float>> conditionValues) {
        Condition condition = new Condition();
        foreach (var item in conditionValues.Values) {

        }
        Time time = new Time();
        Value value = new Value();
    


    }

    public void AddVital(Vital vital) {

    }

    public void AddDrug() {

    }

    public void AddEquipment() {

    }


}
