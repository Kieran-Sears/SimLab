using System;
using System.Xml.Serialization;
using System.Collections.Generic;


[XmlRoot("Vital")]
[Serializable]
public class Vital {
    [XmlAttribute("name")]
    public string name;
    [XmlElement("NodeID")]
    public int nodeID { get; set; }
    [XmlElement("Units")]
    public string units { get; set; }
    [XmlElement("Min")]
    public float min { get; set; }
    [XmlElement("Max")]
    public float max { get; set; }
}


[XmlRoot("Drugs")]
[Serializable]
public class Drugs {
    [XmlArray("DrugList")]
    [XmlArrayItem("Drug")]
    public List<Drug> drugs = new List<Drug>();
}

[Serializable]
public class Drug {

    [XmlAttribute("name")]
    public string name;

    [XmlElement("NodeID")]
    public int nodeID { get; set; }

    // For adding multiple administrations to a single drug
    // [XmlArray("Administrations")]
    // [XmlArrayItem("Administration")]
    // public List<Administration> administrations = new List<Administration>();
    [XmlElement("Administration")]
    public Administration administration;
}


[XmlRoot("Administration")]
[Serializable]
public class Administration {
    [XmlElement("Name")]
    public string name { get; set; }

    [XmlElement("Units")]
    public string dose { get; set; }

    [XmlElement("Min")]
    public float min { get; set; }

    [XmlElement("Max")]
    public float max { get; set; }

    [XmlElement("Duration")]
    public float duration { get; set; }

    // list of vital timelines
    [XmlArray("VitalsData")]
    [XmlArrayItem("VitalData")]
    public List<VitalData> vitalsData = new List<VitalData>();

}

[XmlRoot("Condition")]
[Serializable]
public class Condition {
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
