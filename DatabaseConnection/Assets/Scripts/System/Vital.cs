using System.Xml.Serialization;


[XmlRoot("frame")]
public class Vital {
    [XmlAttribute("name")]
    public string name;

    [XmlElement("NodeID")]
    public int nodeID { get; set; }
    [XmlElement("Units")]
    public string units { get; set; }
    [XmlElement("Value")]
    public float value { get; set; }
    //[XmlElement("Affects")]
    //public string affects { get; set; }
    //[XmlElement("Administration")]
    //public string administration { get; set; }
    //[XmlElement("Duration")]
    //public float duration { get; set; }
}
