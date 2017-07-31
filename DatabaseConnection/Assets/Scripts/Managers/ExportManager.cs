using UnityEngine;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Xml;



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
           
            default:
                _xml = Resources.Load<TextAsset>(path);
                if (_xml == null) {
                    Debug.Log("Unrecognised condition name. Please check corrosponding xml filename for " + path);
                } else {
                    Debug.Log("Loading " + path);
                    serializer = new XmlSerializer(typeof(Condition));
                    reader = new StringReader(_xml.text);
                    condition = serializer.Deserialize(reader) as Condition;
                    reader.Close();
                    return condition;
                }
                return null;
        }

    }



    public void SaveCondition(Condition condition, string path) {
        print("saving condition in: " + path);
        var serializer = new XmlSerializer(typeof(Condition));
        var stream = new FileStream(Path.GetFullPath(Application.dataPath) + path, FileMode.Create);
        serializer.Serialize(stream, condition);
        stream.Close();

    }

    public void SaveVital(Vital vital, string path) {
        print("saving vital ("+vital.name+") in: " + path);
        var serializer = new XmlSerializer(typeof(Vital));
        var stream = new FileStream(Path.GetFullPath(Application.dataPath) + path, FileMode.Create);
        serializer.Serialize(stream, vital);
        stream.Close();

    }

    public void SaveDrug(Drug drug, string path) {
        print("saving drug ("+drug.name+") in: " + path);
        var serializer = new XmlSerializer(typeof(Drug));
        var stream = new FileStream(Path.GetFullPath(Application.dataPath) + path, FileMode.Create);
        serializer.Serialize(stream, drug);
        stream.Close();

    }


}
