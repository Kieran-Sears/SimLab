using UnityEngine;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;

public class ExportManager : MonoBehaviour {

    public static ExportManager Instance { get; private set; }

    TextAsset _xml;
    XmlSerializer serializer;
    StringReader reader;


    private void Awake() {
        if (Instance) {
            DestroyImmediate(this);
        } else {
            Instance = this;
        }
    }

    public List<Drug> LoadAllDrugs() {
        UnityEngine.Object[] files = Resources.LoadAll("Drugs/");
        serializer = new XmlSerializer(typeof(Drug));
        List<Drug> objectList = new List<Drug>();
        foreach (TextAsset item in files) {
            reader = new StringReader(item.text);
            objectList.Add(serializer.Deserialize(reader) as Drug);
            reader.Close();
        }
        return objectList;
    }

    public List<Vital> LoadAllVitals() {
        UnityEngine.Object[] files = Resources.LoadAll("Vitals/");
        serializer = new XmlSerializer(typeof(Vital));
        List<Vital> objectList = new List<Vital>();
        foreach (TextAsset item in files) {
            reader = new StringReader(item.text);
            objectList.Add(serializer.Deserialize(reader) as Vital);
            reader.Close();
        }
        return objectList;
    }

    public List<Condition> LoadAllConditions() {
        UnityEngine.Object[] files = Resources.LoadAll("Conditions/");
        serializer = new XmlSerializer(typeof(Condition));
        List<Condition> objectList = new List<Condition>();
        foreach (TextAsset item in files) {
            reader = new StringReader(item.text);
            objectList.Add(serializer.Deserialize(reader) as Condition);
            reader.Close();
        }
        return objectList;
    }

    public void SaveCondition(Condition condition, string path) {
        print("saving condition in: " + path);
        var serializer = new XmlSerializer(typeof(Condition));
        var stream = new FileStream(Path.GetFullPath(Application.dataPath) + path, FileMode.Create);
        serializer.Serialize(stream, condition);
        stream.Close();

    }

    public void SaveVital(Vital vital, string path) {
        print("saving vital (" + vital.name + ") in: " + path);
        var serializer = new XmlSerializer(typeof(Vital));
        var stream = new FileStream(Path.GetFullPath(Application.dataPath) + path, FileMode.Create);
        serializer.Serialize(stream, vital);
        stream.Close();

    }

    public void SaveDrug(Drug drug, string path) {
        print("saving drug (" + drug.name + ") in: " + path);
        var serializer = new XmlSerializer(typeof(Drug));
        var stream = new FileStream(Path.GetFullPath(Application.dataPath) + path, FileMode.Create);
        serializer.Serialize(stream, drug);
        stream.Close();

    }


}
