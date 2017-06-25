using System;
using System.Xml.Serialization;
using System.Collections.Generic;


[XmlRoot("Condition")]
[Serializable]
public class Condition{
    // name of the condition
    [XmlAttribute("Name")]
    public string name;
    // duration of the condition in seconds
    [XmlAttribute("Duration")]
    public int duration;
    // list of vital timelines
    [XmlArray("VitalsData")]
    [XmlArrayItem("VitalData")]
     public List<VitalData> vitalsData = new List<VitalData>();


}

[Serializable]
public class VitalData {
    [XmlElement("Vital")]
    public Vital vital;
    [XmlElement("TimeLine")]
    public TimeLine timeline;
}

[Serializable]
public class TimeLine {
    // the values of the timeline
    [XmlArray("VitalValues")]
    [XmlArrayItem("Value")]
    public List<Value> vitalValues = new List<Value>();
    // the upper threshold values of the vital
    [XmlArray("UpperThreshold")]
    [XmlArrayItem("Value")]
    public List<Value> upperThresholdValues = new List<Value>();
    // the lower threshold values of the vital
    [XmlArray("LowerThreshold")]
    [XmlArrayItem("Value")]
    public List<Value> lowerThresholdValues = new List<Value>();
}


[Serializable]
public class Value {
    [XmlAttribute("second")]
    public float second;
    [XmlElement("Value")]
    public float value;
}


//[DataContract]
//public class SerialisableDictionary {
//    // need a parameterless constructor for serialization
//    public SerialisableDictionary() {
//        vitals = new Dictionary<string, TimeLine>();
//    }
//    [DataMember]
//    public Dictionary<string, TimeLine> vitals { get; set; }
//}

//[Serializable]
//public class DictionaryVitalKeyByTimelineValue : SerializableDictionary<string, TimeLine> { }



//[Serializable]
//public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver {
//    [SerializeField]
//    private List<TKey> keys = new List<TKey>();

//    [SerializeField]
//    private List<TValue> values = new List<TValue>();

//    // save the dictionary to lists
//    public void OnBeforeSerialize() {
//        keys.Clear();
//        values.Clear();
//        foreach (KeyValuePair<TKey, TValue> pair in this) {
//            keys.Add(pair.Key);
//            values.Add(pair.Value);
//        }
//    }

//    // load dictionary from lists
//    public void OnAfterDeserialize() {
//        this.Clear();

//        if (keys.Count != values.Count)
//            throw new System.Exception(string.Format("there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable."));

//        for (int i = 0; i < keys.Count; i++)
//            this.Add(keys[i], values[i]);
//    }
//}
