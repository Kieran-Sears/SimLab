using System;
using System.Xml.Serialization;
using System.Collections.Generic;


[XmlRoot("Vitals")]
[Serializable]
public class Vitals {
    [XmlArray("VitalsList")]
    [XmlArrayItem("Vital")]
    public List<Vital> vitalList = new List<Vital>();


}

[XmlRoot("frame")]
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