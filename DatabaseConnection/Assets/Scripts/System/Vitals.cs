using UnityEngine;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;


[XmlRoot("Vitals")]
public class Vitals {
    [XmlArray("VitalsTimeFrame")]
    [XmlArrayItem("Vital")]
    public List<Vital> vitalsTimeFrame = new List<Vital>();

    public static Vitals Load(string path) {
        TextAsset _xml = Resources.Load<TextAsset>(path);
        XmlSerializer serializer = new XmlSerializer(typeof(Vitals));
        StringReader reader = new StringReader(_xml.text);
        Vitals vitalsTimeFrame = serializer.Deserialize(reader) as Vitals;
        reader.Close();
        return vitalsTimeFrame;
    }
}
