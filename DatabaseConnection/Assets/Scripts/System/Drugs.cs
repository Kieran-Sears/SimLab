using System;
using System.Xml.Serialization;
using System.Collections.Generic;


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

    [XmlArray("Administrations")]
    [XmlArrayItem("Administration")]
    public List<Administration> administrations = new List<Administration>();
}


[XmlRoot("Administration")]
[Serializable]
public class Administration {
    [XmlElement("Name")]
    public string name { get; set; }

    [XmlElement("Units")]
    public string units { get; set; }

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