using System;
using System.Xml.Serialization;
using System.Collections.Generic;

[XmlRoot("Equipment")]
[Serializable]
public class Equipment {

    [XmlArray("EquipmentList")]
    [XmlArrayItem("Item")]
    public List<Item> itemList = new List<Item>();

}

[XmlRoot("frame")]
[Serializable]
public class Item {

    [XmlAttribute("name")]
    public string name;

    [XmlElement("NodeID")]
    public int nodeID { get; set; }

    [XmlElement("Units")]
    public string units { get; set; }

    [XmlArray("Vitals")]
    [XmlArrayItem("Vital")]
    public List<string> Vitals = new List<string>();


}
