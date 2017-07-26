using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class VisualizationSetup : MonoBehaviour {

    public static VisualizationSetup instance { get; private set; }



    public Transform vitalPanel;
    public Transform drugPanel;
    public Transform drugOverlay;
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

    private void SetTimelineSlider() {
        timeLineSlider.GetComponent<RectTransform>().sizeDelta =
            new Vector2(vitalGraph.graphContent.GetComponent<RectTransform>().sizeDelta.x + timeLineSlider.handleRect.GetComponent<RectTransform>().sizeDelta.x,
            timeLineSlider.GetComponent<RectTransform>().sizeDelta.y);
        timeLineSlider.transform.position = new Vector3(vitalGraph.graphContent.transform.position.x, timeLineSlider.transform.position.y, timeLineSlider.transform.position.z);
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

    private void SetOverlay() {
        RectTransform overlayRect = drugOverlay.GetComponent<RectTransform>();
        overlayRect.anchorMax = Vector2.one / 2;
        overlayRect.anchorMin = Vector2.one / 2;
        overlayRect.sizeDelta = vitalGraph.graphContentRectTrans.sizeDelta;
        drugOverlay.position = vitalGraph.graph.transform.position;

        // get the duration which will be added to the drug points durations
        float chosenDuration = timeLineSlider.value;

        float yValue;
        float gradientBetweenPoints;

        KeyValuePair<float, Slider> drugPoint;
        KeyValuePair<float, Slider> before = new KeyValuePair<float, Slider>(-1, null);
        KeyValuePair<float, Slider> after = new KeyValuePair<float, Slider>(-1, null);

        overlayDrugLine.numPositions = drugGraph.sortedGraphPointsList.Keys.Count;

        // for each drug point 
        for (int i = 0; i < drugGraph.sortedGraphPointsList.Keys.Count; i++) {

            drugPoint = new KeyValuePair<float, Slider>(drugGraph.sortedGraphPointsList.Keys[i], drugGraph.sortedGraphPointsList[drugGraph.sortedGraphPointsList.Keys[i]]);

            // find the points on the vital graph which lie between this drug point
            for (int j = 1; j < vitalGraph.sortedGraphPointsList.Keys.Count; j++) {

                if (vitalGraph.sortedGraphPointsList.Keys[j] > (chosenDuration + drugPoint.Key)) {
                    before = new KeyValuePair<float, Slider>(vitalGraph.sortedGraphPointsList.Keys[j - 1], vitalGraph.sortedGraphPointsList[vitalGraph.sortedGraphPointsList.Keys[j - 1]]);
                    after = new KeyValuePair<float, Slider>(vitalGraph.sortedGraphPointsList.Keys[j], vitalGraph.sortedGraphPointsList[vitalGraph.sortedGraphPointsList.Keys[j]]);
                    j = vitalGraph.sortedGraphPointsList.Keys.Count;
                    print("found point should go between vital points: (" + before.Key + ", " + before.Value.value + ") and (" + after.Key + ", " + after.Value.value + ")");
                }

            }

            // calculate the gradient of the line between the vital points
            gradientBetweenPoints = (after.Value.value - before.Value.value) / (after.Key - before.Key);
            print("Gradient: " + (after.Value.value - before.Value.value) + " / " + (after.Key - before.Key) + " = " + gradientBetweenPoints);
            // use the gradient to find what the yValue should be for this drug point (y = mX + C)
            yValue = (drugPoint.Key - before.Key) * gradientBetweenPoints;// + before.Value.value;
            print("vital yValue = " + yValue);
            // then add the difference to the yValue based on what the drug point's value is
            float drugYValue = drugPoint.Value.value - drugGraph.offset;
            print("drug yValue = " + drugYValue);
            yValue += drugYValue; // do i need the offset?
            print("final overlay yValue = " + yValue);
            // add the point to the overlay ### conditional check to see if within permitted viewing area before adding
            float yCoordinate = (overlayRect.rect.height / vitalGraph.yScale) * yValue;
            // have drug point begin positioned at the start of the vital graph
            float xCoordinate = (drugOverlay.transform.position.x - (overlayRect.rect.width / 2)) + before.Value.handleRect.rect.width / 2;
            // then use the points duration added to the slider duration as the calculation for its correct position
            xCoordinate += (overlayRect.rect.width / vitalGraph.xScale) * (chosenDuration + drugPoint.Key);
            // set position on line
            Vector3 newPos = new Vector3(xCoordinate, yCoordinate, 0);
            // convert point values into world positions
            overlayDrugLine.SetPosition(i, newPos);
        }

    }

    public void ReturnGraphs() {
        vitalPanel.SetParent(ConditionSetup.instance.tabManager.contentArea.transform);
        drugPanel.SetParent(DrugSetup.instance.tabManager.contentArea.transform);
        // add here call to resize graphs
    }
}
