using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualizationSetup : MonoBehaviour {

    public static VisualizationSetup instance { get; private set; }

    public Transform vitalPanel;
    public Transform drugPanel;
    public Transform drugOverlay;

    private Graph drugGraph;
    private Graph vitalGraph;
    private Graph overlay;

    private void Awake() {
        if (instance) {
            DestroyImmediate(this);
        }
        else {
            instance = this;
        }
    }

    public void GetGraphs() {
        GameObject originalVitalGraph = ConditionSetup.instance.tabManager.contentArea.transform.FindChild(DrugSetup.instance.graph.name).gameObject;
        GameObject originalDrugGraph = DrugSetup.instance.tabManager.contentArea.transform.FindChild(DrugSetup.instance.graph.name).gameObject;
        vitalGraph = Instantiate(originalVitalGraph, vitalPanel).GetComponent<Graph>();
        drugGraph = Instantiate(originalDrugGraph, drugPanel).GetComponent<Graph>();
        vitalGraph.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
        vitalGraph.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
        drugGraph.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
        drugGraph.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
        vitalGraph.sortedGraphPointsList = originalVitalGraph.GetComponent<Graph>().sortedGraphPointsList;
        vitalGraph.pointsUpperThreshold = originalVitalGraph.GetComponent<Graph>().pointsUpperThreshold;
        vitalGraph.pointsLowerThreshold = originalVitalGraph.GetComponent<Graph>().pointsLowerThreshold;
        drugGraph.sortedGraphPointsList = originalDrugGraph.GetComponent<Graph>().sortedGraphPointsList;
        drugGraph.pointsUpperThreshold = originalDrugGraph.GetComponent<Graph>().pointsUpperThreshold;
        drugGraph.pointsLowerThreshold = originalDrugGraph.GetComponent<Graph>().pointsLowerThreshold;
        LerpFromView.onEnd();
    }

    public void SetOverlay() {
        overlay = Instantiate(drugGraph.gameObject, drugOverlay).GetComponent<Graph>();
    }

    public void ReturnGraphs() {
        vitalPanel.SetParent(ConditionSetup.instance.tabManager.contentArea.transform);
        drugPanel.SetParent(DrugSetup.instance.tabManager.contentArea.transform);
    }
}
