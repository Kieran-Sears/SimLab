using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class VisualizationSetup : MonoBehaviour {

    public static VisualizationSetup instance { get; private set; }



    public Transform vitalPanel;
    public Transform drugPanel;
    public Transform drugOverlay;
    public Text vitalDescription;
    public Text drugDescription;
    public Slider timeLineSlider;
    public LineRenderer overlayDrugLine;
    public SortedList<float, Slider> sortedGraphPointsList = new SortedList<float, Slider>();

    private Graph drugGraph;
    private Graph vitalGraph;

    private void Awake() {
        if (instance) {
            DestroyImmediate(this);
        } else {
            instance = this;
        }
    }

    public void SetVisualization() {
        GetGraphs();
        SetOverlay();
        SetTimelineSlider();
    }

    private void SetDescriptions() {
        vitalDescription.text = vitalGraph.name;
        drugDescription.text = drugGraph.name;
    }

    private void SetTimelineSlider() {
        timeLineSlider.GetComponent<RectTransform>().sizeDelta =
            new Vector2(vitalGraph.graphContent.GetComponent<RectTransform>().sizeDelta.x + timeLineSlider.handleRect.GetComponent<RectTransform>().sizeDelta.x,
            timeLineSlider.GetComponent<RectTransform>().sizeDelta.y);
        timeLineSlider.transform.position = new Vector3(vitalGraph.graphContent.transform.position.x, timeLineSlider.transform.position.y, timeLineSlider.transform.position.z);
        timeLineSlider.maxValue = vitalGraph.duration;
    }

    private void GetGraphs() {
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
        drugGraph.overlayPointChange += SetOverlay;
    }

    public void SetOverlay() {
        RectTransform overlayRect = drugOverlay.GetComponent<RectTransform>();
        overlayRect.anchorMax = Vector2.one / 2;
        overlayRect.anchorMin = Vector2.one / 2;
        overlayRect.sizeDelta = vitalGraph.graphContentRectTrans.sizeDelta;
        drugOverlay.position = vitalGraph.graph.transform.position;

        // get the duration which will be added to the drug points durations
        float chosenDuration = timeLineSlider.value;


        float yValue;
        float drugYValue;
        float vitalYValue;
        float vitalGradient;
        float drugGradient;

        KeyValuePair<float, Slider> drugPointBefore = new KeyValuePair<float, Slider>(-1, null);
        KeyValuePair<float, Slider> drugPointAfter = new KeyValuePair<float, Slider>(-1, null);
        KeyValuePair<float, Slider> vitalPointBefore = new KeyValuePair<float, Slider>(-1, null);
        KeyValuePair<float, Slider> vitalPointAfter = new KeyValuePair<float, Slider>(-1, null);

        overlayDrugLine.numPositions = drugGraph.duration + 1;
        overlayDrugLine.transform.localPosition = Vector3.down * (vitalGraph.graph.GetComponent<RectTransform>().rect.height / 2);

        int drugCounter = 1;
        int vitalCounter = 1;

        // for each second the drug is in effect for
        for (int i = 0; i <= drugGraph.duration; i++) {

            // get the drug points before and after the current second being calculated    ### may need to change the condition around to look for greater than and set "-1" to "+1"
            for (int j = drugCounter; j < drugGraph.sortedGraphPointsList.Count; j++) {
                if (drugGraph.sortedGraphPointsList.Keys[j] >= i) {
                    drugPointBefore = new KeyValuePair<float, Slider>(drugGraph.sortedGraphPointsList.Keys[j - 1], drugGraph.sortedGraphPointsList[drugGraph.sortedGraphPointsList.Keys[j - 1]]);
                    drugPointAfter = new KeyValuePair<float, Slider>(drugGraph.sortedGraphPointsList.Keys[j], drugGraph.sortedGraphPointsList[drugGraph.sortedGraphPointsList.Keys[j]]);
                    //  print("Drug Points : " + drugPointBefore.Key + ", " + drugPointBefore.Value.value + " || " + drugPointAfter.Key + ", " + drugPointAfter.Value.value);
                    drugCounter = j - 1;
                    j = drugGraph.sortedGraphPointsList.Count;
                }
            }

            // likewise with the vital points before and after current second being calculated
            for (int j = vitalCounter; j < vitalGraph.sortedGraphPointsList.Count; j++) {
                if (vitalGraph.sortedGraphPointsList.Keys[j] >= chosenDuration + i) {
                    vitalPointBefore = new KeyValuePair<float, Slider>(vitalGraph.sortedGraphPointsList.Keys[j - 1], vitalGraph.sortedGraphPointsList[vitalGraph.sortedGraphPointsList.Keys[j - 1]]);
                    vitalPointAfter = new KeyValuePair<float, Slider>(vitalGraph.sortedGraphPointsList.Keys[j], vitalGraph.sortedGraphPointsList[vitalGraph.sortedGraphPointsList.Keys[j]]);
                    //   print("Vital Points : " + vitalPointBefore.Key + ", " + vitalPointBefore.Value.value + " || " + vitalPointAfter.Key + ", " + vitalPointAfter.Value.value);
                    vitalCounter = j - 1;
                    j = vitalGraph.sortedGraphPointsList.Count;
                }
            }

            // calculate the gradient of the line between the two sets of points
            drugGradient = (drugPointAfter.Value.value - drugPointBefore.Value.value) / (drugPointAfter.Key - drugPointBefore.Key);
            vitalGradient = (vitalPointAfter.Value.value - vitalPointBefore.Value.value) / (vitalPointAfter.Key - vitalPointBefore.Key);
            // then add the difference to the yValue based on what the drug point's value is
            drugYValue = (drugPointBefore.Value.value - drugGraph.offset) + ((i - drugPointBefore.Key) * drugGradient);
            vitalYValue = vitalPointBefore.Value.value + (((chosenDuration + i) - vitalPointBefore.Key) * vitalGradient);
            // subtract one from another for the final yValue used in the overlay
            yValue = vitalYValue + drugYValue;

            // Debugging print statements
            print("drugYValue " + drugYValue);
            print("vitalYValue " + vitalYValue);
            print("yValue " + yValue);

            // add the point to the overlay ### conditional check to see if within permitted viewing area before adding
            float yCoordinate = (overlayRect.rect.height / vitalGraph.yScale) * (yValue);
            // have drug point begin positioned at the start of the vital graph
            float xCoordinate = (drugOverlay.transform.position.x - (overlayRect.rect.width / 2)) + vitalPointBefore.Value.handleRect.rect.width / 2;
            // then use the points duration added to the slider duration as the calculation for its correct position
            xCoordinate += (overlayRect.rect.width / vitalGraph.xScale) * (chosenDuration + i);
            // set position on line
            Vector3 newPos = new Vector3(xCoordinate, yCoordinate, 0);
            // convert point values into world positions
            print(i + " pos " + newPos);
            overlayDrugLine.SetPosition(i, newPos);
        }

    }

    public void ReturnGraphs() {
        vitalPanel.SetParent(ConditionSetup.instance.tabManager.contentArea.transform);
        drugPanel.SetParent(DrugSetup.instance.tabManager.contentArea.transform);
        LerpFromView.onEnd();
        // add here call to resize graphs
    }
}
