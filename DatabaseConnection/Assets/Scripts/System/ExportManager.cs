using UnityEngine;



public class ExportManager : MonoBehaviour {

    public const string path = "vitals";

    void Start() {
        Vitals condition = Vitals.Load(path);

        foreach (Vital item in condition.vitalsTimeFrame) {
            print(item.name);
        }

        //VitalsData vitalsData = NetworkManager.instance.Download();
        //RectTransform rectTrans = viewTableTextArea.GetComponent<RectTransform>();

        //Type vitalsDatum = typeof(VitalsDatum);
        //// get table columns
        //FieldInfo[] vitalsFields = vitalsDatum.GetFields();
        //// generate table with appropriate scales on axis
        //viewTableTextArea.GetComponent<GridLayoutGroup>().cellSize = new Vector2(rectTrans.rect.width / vitalsFields.Length, rectTrans.rect.height / vitalsData.data.Count);
        //tabs.activeGraph.GenerateGraph(1, vitalsFields.Length, 1, 100);

        //for (int i = 0; i < vitalsFields.Length; i++) {
        //    Debug.Log(vitalsFields[i].Name + " " + GetUnitOfMeasure(vitalsFields[i].Name));
        //    tabs.AddTab(vitalsFields[i].Name, GetUnitOfMeasure(vitalsFields[i].Name), vitalsData.data);
        //}
    }

    public void SaveVitals() {

    }

}
