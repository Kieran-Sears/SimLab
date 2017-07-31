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
        SetDescriptions();
        //if (LerpFromView.onEnd != null) {
        //    LerpFromView.onEnd();
        //}
        //if (drugGraph.overlayPointChange != null) {
        //    drugGraph.overlayPointChange();
        //}
        //if (vitalGraph.overlayPointChange != null) {
        //    vitalGraph.overlayPointChange();
        //}
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
        GameObject originalVitalGraph = ConditionSetup.Instance.tabManager.contentArea.transform.FindChild(DrugSetup.Instance.graph.name).gameObject;
        GameObject originalDrugGraph = DrugSetup.Instance.tabManager.contentArea.transform.FindChild(DrugSetup.Instance.graph.name).gameObject;
        vitalGraph = Instantiate(originalVitalGraph, vitalPanel).GetComponent<Graph>();
        drugGraph = Instantiate(originalDrugGraph, drugPanel).GetComponent<Graph>();
        vitalGraph.name = originalVitalGraph.name;
        drugGraph.name = DrugSetup.Instance.drugName.text;
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
        vitalGraph.overlayPointChange += SetOverlay;
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
            //print("drugYValue " + drugYValue);
            //print("vitalYValue " + vitalYValue);
            //print("yValue " + yValue);

            // add the point to the overlay ### conditional check to see if within permitted viewing area before adding
            float yCoordinate = (overlayRect.rect.height / vitalGraph.yScale) * (yValue);
            // have drug point begin positioned at the start of the vital graph
            float xCoordinate = (drugOverlay.transform.position.x - (overlayRect.rect.width / 2)) + vitalPointBefore.Value.handleRect.rect.width / 2;
            // then use the points duration added to the slider duration as the calculation for its correct position
            xCoordinate += (overlayRect.rect.width / vitalGraph.xScale) * (chosenDuration + i);
            // set position on line
            Vector3 newPos = new Vector3(xCoordinate, yCoordinate, 0);
            // convert point values into world positions
           // print(i + " pos " + newPos);
            if (((newPos.x > -(overlayRect.sizeDelta.x / 2)) && (newPos.x < (overlayRect.sizeDelta.x / 2))
                && (newPos.y > 0) && (newPos.y < (overlayRect.sizeDelta.y))) || i == 0) {
                overlayDrugLine.SetPosition(i, newPos);
            } else {
                print("(" + newPos.x + " > (" + -(overlayRect.sizeDelta.x / 2) + ")) = " + (newPos.x > -(overlayRect.sizeDelta.x / 2)));
                print("(" + newPos.x + " < (" + (overlayRect.sizeDelta.x / 2) + ")) = " + (newPos.x < (overlayRect.sizeDelta.x / 2)));

                overlayDrugLine.numPositions--;
            } 
        }
      
    }

    private void SaveChanges() {
        // clear the message for saving from the screen
        Error.instance.boolPanel.SetActive(false);
        Error.instance.boolMessageText.text = "";
        Error.instance.boolLeftButton.onClick.RemoveAllListeners();
        Error.instance.boolRightButton.onClick.RemoveAllListeners();

        // find the original graphs in condition and drug windows and retract their resizing delegates
        Transform originalVitalGraph = ConditionSetup.Instance.tabManager.contentArea.transform.FindChild(vitalGraph.name);
        if (originalVitalGraph != null) {
            print("found the original vital graph");
            LerpFromView.onEnd -= originalVitalGraph.GetComponent<Graph>().ResizeGraph;
        }
        Transform originalDrugGraph = DrugSetup.Instance.tabManager.contentArea.transform.FindChild(vitalGraph.name);
        if (originalVitalGraph != null) {
            print("found the original drug graph");
            LerpFromView.onEnd -= originalDrugGraph.GetComponent<Graph>().ResizeGraph;
        }
        // make sure the drug graphs name is set to the vital it is effecting and not the name of the drug
        drugGraph.name = DrugSetup.Instance.graph.name;

        // exchange the toggles for the original graphs and attach them to the newly edited graphs
        Transform toggleObject = ConditionSetup.Instance.tabManager.activeTabs.transform.FindChild(vitalGraph.name);
        if (toggleObject != null) {
            ConditionSetup.Instance.tabManager.tabGraphs[toggleObject.GetComponent<Toggle>()] = vitalGraph.gameObject;
        }
        toggleObject = DrugSetup.Instance.tabManager.activeTabs.transform.FindChild(drugGraph.name);
        if (toggleObject != null) {
            DrugSetup.Instance.tabManager.tabGraphs[toggleObject.GetComponent<Toggle>()] = drugGraph.gameObject;
        }
      
        // have the new edited graphs take their position
        vitalGraph.transform.SetParent(ConditionSetup.Instance.tabManager.contentArea.transform);
        drugGraph.transform.SetParent(DrugSetup.Instance.tabManager.contentArea.transform);
        // ensure the toggles are active and ready to be used
        for (int i = 0; i < ConditionSetup.Instance.tabManager.activeTabs.transform.childCount; i++) {
            ConditionSetup.Instance.tabManager.activeTabs.transform.GetChild(i).gameObject.SetActive(true);
        }
       // close down the visualisation window and open the drug window
        WindowManager.instance.drug.SetActive(true);
        WindowManager.instance.visualise.SetActive(false);
        // make sure the drug graph is resized in the drug window
        LerpFromView.onEnd();

        // remove the original graphs
        Destroy(originalVitalGraph.gameObject);
        Destroy(originalDrugGraph.gameObject);
    }

    private void RevertChanges() {
        LerpFromView.onEnd -= vitalGraph.ResizeGraph;
        LerpFromView.onEnd -= drugGraph.ResizeGraph;
        Destroy(vitalGraph.gameObject);
        Destroy(drugGraph.gameObject);
        vitalDescription.text = "";
        drugDescription.text = "";
        DrugSetup.Instance.ToggleActiveVisualizeWindow();
    }


    public void NavitageBackToDrugSetup() {
        Error.instance.boolPanel.SetActive(true);
        Error.instance.boolMessageText.text = "Would you like to save your changes?";
        Error.instance.boolLeftButton.onClick.AddListener(RevertChanges);
        Error.instance.boolRightButton.onClick.AddListener(SaveChanges);
        Error.instance.boolCancelButton.gameObject.SetActive(true);
        Error.instance.boolCancelButton.onClick.AddListener(Error.instance.DeactivateErrorBoolPanel);
    
    }
}
