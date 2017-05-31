using System;
using System.Xml.Serialization;
using System.Collections.Generic;

[XmlRoot("Condition")]
[Serializable]
public class Condition{
    [XmlArray("Timeline")]
    [XmlArrayItem("Time")]
    public List<Time> timeline = new List<Time>();

}

[Serializable]
public class Time {

    [XmlAttribute("second")]
    public int second;
    [XmlArray("VitalValues")]
    [XmlArrayItem("VitalValue")]
    public List<Value> vitalValues = new List<Value>();
}

[Serializable]
public class Value {

    [XmlAttribute("vitalID")]
    public int vitalID;
    [XmlElement("Value")]
    public float value;
}

// Example XML below

//<Condition>
//<Timeline>
//  <Time second = "0" >
//    < VitalValue vitalID="0">
//      <Value>46</Value>
//    </VitalValue>
//    <VitalValue vitalID = "1" >
//      < Value > 84 </ Value >
//    </ VitalValue >
//    < VitalValue vitalID="2">
//      <Value>23</Value>
//    </VitalValue>
//    <VitalValue vitalID = "3" >
//      < Value > 34 </ Value >
//    </ VitalValue >
//  </ Time >
//</ Timeline >
//</ Condition >