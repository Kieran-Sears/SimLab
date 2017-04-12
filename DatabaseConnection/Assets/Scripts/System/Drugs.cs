using System;
using System.Xml.Serialization;
using System.Collections.Generic;


[XmlRoot("Drugs")]
[Serializable]
public class Drugs {
    [XmlArray("DrugsList")]
    [XmlArrayItem("Drug")]
    public List<Drug> drugList = new List<Drug>();


}


[XmlRoot("frame")]
[Serializable]
public class Drug {

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

    [XmlElement("Duration")]
    public float duration { get; set; }

    [XmlArray("Vitals")]
    [XmlArrayItem("Vital")]
    public List<string> vitals = new List<string>();

    [XmlArray("Administrations")]
    [XmlArrayItem("Administration")]
    public List<string> administrations = new List<string>();

}
