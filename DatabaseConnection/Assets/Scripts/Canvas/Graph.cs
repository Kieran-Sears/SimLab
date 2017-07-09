using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;




public class Graph : MonoBehaviour {

    #region Public Variables
    public GameObject graph;
    public GameObject grid;
    public GameObject thresholds;
    public GameObject xAxis;
    public GameObject yAxis;
    public GameObject graphContent;
    public GameObject xAxisContent;
    public GameObject yAxisContent;
    public GameObject xAxisLabel;
    public GameObject yAxisLabel;
    public RectTransform graphViewport;
    public RectTransform xAxisViewport;
    public RectTransform yAxisViewport;
    public ScrollRect GraphScrollRect;
    public ScrollRect xAxisScrollRect;
    public ScrollRect yAxisScrollRect;

    public GameObject xDashMarkerPrefab;
    public GameObject yDashMarkerPrefab;
    public GameObject graphPointPrefab;
    public GameObject lineRendererPrefab;
    public GameObject lineRendererThresholdPrefab;
    public GameObject coordinateSystem;
    public InputField coordinateX;
    public InputField coordinateY;


    public SortedList<float, Slider> sortedGraphPointsList = new SortedList<float, Slider>();
    public SortedList<float, Slider> pointsUpperThreshold = new SortedList<float, Slider>();
    public SortedList<float, Slider> pointsLowerThreshold = new SortedList<float, Slider>();

    public int duration;
    public int xScale;
    public int xStart;
    public int xEnd;
    public int yScale;
    public int yStart;
    public int yEnd;
    #endregion

    #region Private Variables
    // private float scrollWheel = 1;
    // private float newScrollWheel;
    private Vector2 localpoint;
    private RectTransform graphContentRectTrans;
    private LineRenderer pointLine;
    private LineRenderer thresholdLineLower;
    private LineRenderer thresholdLineUpper;

    private float mouseHold;
    private bool leftMouseButtonIsDown;
    private Transform sliderHandleTransform;
    private Transform previousSliderHandleTransform;
    private Vector3 newPos;
    private float previousFrameTime;
    private int counter = 0;
    private Slider currentlySelectedSlider;
    private int indexOfSliderMinipulated;
    private float newSliderTime;
    private bool allowChangingPosition;
    private float tempXCoordinate;
    private float tempYCoordinate;
    private bool placeHandleBack;
    #endregion

    #region Unity Methods

    private void LateUpdate() {

        if (coordinateSystem != null && coordinateSystem.activeSelf && Input.GetKeyDown(KeyCode.Return)) {
            float x = float.Parse(coordinateX.text);
            float y = float.Parse(coordinateY.text);
            if ((x > xStart && x < xEnd) && (y > yStart && y < yEnd)) {
                if (sortedGraphPointsList.ContainsKey(x)) {
                    Destroy(sortedGraphPointsList[x].gameObject);
                    sortedGraphPointsList.Remove(x);
                }
                coordinateSystem.SetActive(false);
                sortedGraphPointsList.RemoveAt(sortedGraphPointsList.IndexOfValue(sliderHandleTransform.parent.parent.GetComponent<Slider>()));
                Destroy(sliderHandleTransform.gameObject);
                AddPoint(x, y);
                coordinateX.text = "";
                coordinateY.text = "";
                DrawLinkedPointLines();
            }
            else {
                Error.instance.informMessageText.text = "Ensure coordinates are within range limits";
                Error.instance.informPanel.SetActive(true);
                Error.instance.informOkButton.onClick.AddListener(SelectCoordinateXValue);
            }
        }

        // check for if cursor is over graph area
        if (graphViewport != null && graphViewport.GetComponent<CusorSensor>().mouseOver) {

            // if left mouse click held record time it is held for
            if (Input.GetMouseButton(0)) {
                mouseHold += Time.deltaTime;
            }

            // find the object the cursor is over
            RaycastHit hit1;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit1, 800)) {
                string tag = hit1.collider.tag;

                // if over graph check for a mouse up to place a new point else deactivate coordinate system
                // else
                if (tag == "Graph") {
                    if (Input.GetMouseButtonUp(0) && mouseHold < 0.3) {
                        if (coordinateSystem.activeSelf) {
                            coordinateSystem.SetActive(false);
                            sliderHandleTransform.GetComponent<Image>().color = currentlySelectedSlider.colors.normalColor;
                        }
                        else {
                            AddPoint(Camera.main.WorldToScreenPoint(hit1.point));
                        }
                    }
                }

                // if over handle 
                else if (tag == "Handle") {
                 
                    sliderHandleTransform = hit1.transform;
                    currentlySelectedSlider = sliderHandleTransform.parent.parent.GetComponent<Slider>();
                    indexOfSliderMinipulated = sortedGraphPointsList.IndexOfValue(currentlySelectedSlider);

                    if (indexOfSliderMinipulated == -1 || graph == null) {
                        return;
                    }

                    // if isn't the first or last point in the graph -> setup the coordinate system if left mouse is clicked
                    if (indexOfSliderMinipulated != 0 || indexOfSliderMinipulated != sortedGraphPointsList.Count - 1) {

                        if (Input.GetMouseButtonUp(0) && mouseHold < 0.5) {
                            if (previousSliderHandleTransform != null) {
                                previousSliderHandleTransform.GetComponent<Image>().color = currentlySelectedSlider.colors.normalColor;
                            }
                            if (coordinateSystem.activeSelf) {
                                coordinateSystem.SetActive(false);
                                sliderHandleTransform.GetComponent<Image>().color = currentlySelectedSlider.colors.normalColor;
                            }
                            else {
                                if (previousSliderHandleTransform != null) {
                                    previousSliderHandleTransform.GetComponent<Image>().color = currentlySelectedSlider.colors.normalColor;
                                }
                                previousSliderHandleTransform = sliderHandleTransform;
                                sliderHandleTransform.GetComponent<Image>().color = Color.red;
                                coordinateSystem.SetActive(true);
                                coordinateSystem.transform.localPosition = Vector3.zero;
                                coordinateSystem.transform.localScale = new Vector3(0.5f, 0.5f, 1);
                                coordinateSystem.transform.position = sliderHandleTransform.position + new Vector3(0.3f, 0.3f, 0);
                                coordinateSystem.transform.localPosition = new Vector3(coordinateSystem.transform.localPosition.x, coordinateSystem.transform.localPosition.y, 0);
                                coordinateX.text = Mathf.CeilToInt(sortedGraphPointsList.Keys[sortedGraphPointsList.IndexOfValue(currentlySelectedSlider)]).ToString();
                                coordinateY.text = Mathf.CeilToInt(currentlySelectedSlider.value).ToString();
                                tempXCoordinate = sortedGraphPointsList.Keys[sortedGraphPointsList.IndexOfValue(currentlySelectedSlider)];
                                tempYCoordinate = currentlySelectedSlider.value;
                            }
                            mouseHold = 0;
                        }
                    }

                    // the mouse has been clicked reset the time limit required to spawn the coordinate system and setup the point to be dragged
                    if (Input.GetMouseButtonDown(0)) {
                        leftMouseButtonIsDown = true;
                    }

                    // if right click detected and not first or last graph point then delete point
                    if (Input.GetMouseButtonUp(1)) {
                        if (indexOfSliderMinipulated == -1 || indexOfSliderMinipulated == 0 || indexOfSliderMinipulated == sortedGraphPointsList.Count - 1) return;
                        sortedGraphPointsList.RemoveAt(indexOfSliderMinipulated);
                        Destroy(currentlySelectedSlider.gameObject);
                        DrawLinkedPointLines();
                    }

                }

            }
            else {
                // no raycast hit detected
                return;
            }

            #region scrolling
            //  newScrollWheel = scrollWheel + Input.GetAxis("Mouse ScrollWheel");

            //if (scrollWheel != newScrollWheel) {
            //    scrollWheel = newScrollWheel;
            //    if (scrollWheel >= 1f) {
            //        SetZoom(scrollWheel);
            //    } else if (scrollWheel < 1f) {
            //        scrollWheel = 1f;
            //    }
            //}
            #endregion

            #region point dragging
            if (leftMouseButtonIsDown) {
                // if the first or last point in the graph allow only movement of sliders value
                if (indexOfSliderMinipulated == 0 || indexOfSliderMinipulated == sortedGraphPointsList.Count - 1) {
                    allowChangingPosition = false;
                    newPos = currentlySelectedSlider.transform.position;
                }
                // if not the first or last point allow dragging in x direction as well as slider value (y direction)
                else {
                    allowChangingPosition = true;
                    newPos = new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, currentlySelectedSlider.transform.position.y, currentlySelectedSlider.transform.position.z);
                }
                // record the new time for the slider based on its x axis position
                newSliderTime = (currentlySelectedSlider.transform.localPosition.x + (graph.GetComponent<RectTransform>().rect.width / 2)) / (graph.GetComponent<RectTransform>().rect.width / xScale);
                if (((newSliderTime <= 0) || newSliderTime >= duration) && !(indexOfSliderMinipulated == 0 || indexOfSliderMinipulated == sortedGraphPointsList.Count - 1)) {
                    print("Crossover duration extremes detected");
                    placeHandleBack = true;
                }
                else {
                    placeHandleBack = false;
                    // if the user isnt just holding the handle still
                    if (newSliderTime != previousFrameTime) {
                        // ensure there isn't overwriting of existing points and if it isn't an end point slider
                        if (!sortedGraphPointsList.ContainsKey(newSliderTime) && allowChangingPosition) {
                            sortedGraphPointsList.RemoveAt(indexOfSliderMinipulated);
                            sortedGraphPointsList.Add(newSliderTime, currentlySelectedSlider);
                            indexOfSliderMinipulated = sortedGraphPointsList.IndexOfKey(newSliderTime);
                        }

                        previousFrameTime = newSliderTime;
                    }
                    currentlySelectedSlider.transform.position = newPos;
                    DrawLinkedPointLines();
                    DrawThresholds();
                }
            }
            #endregion
        }

        // extra check incase user releases mouse click outside of graph area after beginning within graph area
        if (Input.GetMouseButtonUp(0)) {
            if (placeHandleBack) {
                currentlySelectedSlider.transform.localPosition = new Vector3(0, currentlySelectedSlider.transform.localPosition.y, currentlySelectedSlider.transform.localPosition.z);
                newSliderTime = (currentlySelectedSlider.transform.localPosition.x + (graph.GetComponent<RectTransform>().rect.width / 2)) / (graph.GetComponent<RectTransform>().rect.width / xScale);
                if (!sortedGraphPointsList.ContainsKey(newSliderTime) && allowChangingPosition) {
                    sortedGraphPointsList.RemoveAt(indexOfSliderMinipulated);
                    sortedGraphPointsList.Add(newSliderTime, currentlySelectedSlider);
                    indexOfSliderMinipulated = sortedGraphPointsList.IndexOfKey(newSliderTime);
                }

                previousFrameTime = newSliderTime;
                DrawLinkedPointLines();
            }
            leftMouseButtonIsDown = false;
            mouseHold = 0;

        }

    }

    public void OnEnable() {
        DrawLinkedPointLines();
        DrawThresholds();
    }
    #endregion

    #region Private Methods

    private void DrawGrid() {
        Vector2 size = new Vector2(graphContentRectTrans.rect.width, graphContentRectTrans.rect.height);
        GameObject dashMarker;
        LineRenderer lineRenderer;

        for (int i = 0; i <= xScale; i++) {
            dashMarker = Instantiate(lineRendererPrefab, grid.transform);
            dashMarker.transform.localScale = Vector3.one;
            dashMarker.transform.localPosition = Vector3.zero;

            lineRenderer = dashMarker.GetComponent<LineRenderer>();
            lineRenderer.transform.localScale = new Vector3(1f, 1f, 1f);
            lineRenderer.startWidth = 0.01f;
            lineRenderer.endWidth = 0.01f;
            lineRenderer.SetPosition(0, new Vector3((((size.x / xScale) * i) - (size.x / 2)), (-size.y / 2), 0));
            lineRenderer.SetPosition(1, new Vector3((((size.x / xScale) * i) - (size.x / 2)), (size.y / 2), 0));

        }
        for (int n = 0; n <= yScale; n++) {
            dashMarker = Instantiate(lineRendererPrefab, grid.transform);
            dashMarker.transform.localScale = Vector3.one;
            dashMarker.transform.localPosition = new Vector3(0, 0, 0);

            lineRenderer = dashMarker.GetComponent<LineRenderer>();
            lineRenderer.transform.localScale = new Vector3(1f, 1f, 1f);
            lineRenderer.startWidth = 0.01f;
            lineRenderer.endWidth = 0.01f;
            lineRenderer.SetPosition(0, new Vector3((-size.x / 2), (((size.y / yScale) * n) - (size.y / 2)), 0));
            lineRenderer.SetPosition(1, new Vector3((size.x / 2), (((size.y / yScale) * n) - (size.y / 2)), 0));
        }

    }

    private void InitialiseXScale() {
        //if (xScale > 300) {
        //    xAxisLabel.GetComponent<Text>().text = "Duration (Minutes)";
        //    xScale /= 60;
        //}
        for (int i = 0; i <= xScale; i++) {
            GameObject dashMarker = Instantiate(xDashMarkerPrefab, xAxis.transform);
            dashMarker.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            dashMarker.transform.localPosition = Vector3.zero;
            dashMarker.transform.localPosition += new Vector3((-graphContentRectTrans.rect.width / 2), 0, 0);
            dashMarker.transform.localPosition += new Vector3((((graphContentRectTrans.rect.width) / xScale) * i), 0, 0);
            // duration numbering format 1
            // dashMarker.GetComponent<Text>().text = ((i / 60)).ToString() + ":" + ((i % 60)).ToString();
            // dashMarker.transform.localScale = Vector3.one / (xScale / 30);
            // duration numbering format 2
            if (i % 60 == 0) {
                dashMarker.GetComponent<Text>().text = ((i / 60)).ToString();
            }
            else {
                dashMarker.GetComponent<Text>().text = ((i % 60)).ToString();
                dashMarker.transform.localScale = Vector3.one / 2f;

            }
        }
    }

    private void InitialiseYScale() {
        GameObject dashMarker;
        Text text;

        for (int i = yStart; i <= yEnd; i++) {
            dashMarker = Instantiate(yDashMarkerPrefab, yAxis.transform);
            dashMarker.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            dashMarker.transform.localPosition = Vector3.zero;
            dashMarker.transform.localPosition += new Vector3(0, (-graphContentRectTrans.rect.height / 2), 0);
            dashMarker.transform.localPosition += new Vector3(0, (((graphContentRectTrans.rect.height) / yScale) * (i - yStart)), 0);
            text = dashMarker.GetComponent<Text>();
            text.text = (i).ToString();
            text.fontSize = 1;
        }
    }

    private void DrawThresholds() {
        GameObject upperLineWriter;
        GameObject lowerLineWriter;
        if (thresholdLineLower == null) {
            if (graph == null) return;
            lowerLineWriter = Instantiate(lineRendererThresholdPrefab, graphContent.transform);
            lowerLineWriter.transform.localScale = Vector3.one;
            lowerLineWriter.transform.localPosition = Vector3.zero;
            lowerLineWriter.name = "LowerThreshold";

            thresholdLineLower = lowerLineWriter.GetComponent<LineRenderer>();
            thresholdLineLower.startColor = Color.red;
            thresholdLineLower.endColor = Color.red;
            thresholdLineLower.startWidth = 0.1f;
            thresholdLineLower.endWidth = 0.1f;
        }
        thresholdLineLower.numPositions = pointsLowerThreshold.Count;
        counter = 0;
        foreach (KeyValuePair<float, Slider> item in pointsLowerThreshold) {
            thresholdLineLower.SetPosition(counter, Camera.main.WorldToScreenPoint(item.Value.handleRect.transform.position) - Camera.main.WorldToScreenPoint(graphContent.transform.position));
            counter++;
        }


        if (thresholdLineUpper == null) {
            if (graph == null) return;
            upperLineWriter = Instantiate(lineRendererThresholdPrefab, graphContent.transform);
            upperLineWriter.transform.localScale = Vector3.one;
            upperLineWriter.transform.localPosition = Vector3.zero;
            upperLineWriter.name = "UpperThreshold";

            thresholdLineUpper = upperLineWriter.GetComponent<LineRenderer>();
            thresholdLineUpper.startColor = Color.red;
            thresholdLineUpper.endColor = Color.red;
            thresholdLineUpper.startWidth = 0.1f;
            thresholdLineUpper.endWidth = 0.1f;
        }
        thresholdLineUpper.numPositions = pointsUpperThreshold.Count;
        counter = 0;
        foreach (KeyValuePair<float, Slider> item in pointsUpperThreshold) {
            thresholdLineUpper.SetPosition(counter, Camera.main.WorldToScreenPoint(item.Value.handleRect.transform.position) - Camera.main.WorldToScreenPoint(graphContent.transform.position));
            counter++;
        }
    }

    private void SetZoom(float targetSize) {
        if (graph != null) {
            //grid : setting the pivot point to be at the location of the cursor
            if (graphContentRectTrans == null) return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(graphViewport, Input.mousePosition, GetComponentInParent<Canvas>().worldCamera, out localpoint);
            Vector2 pivot = new Vector2(localpoint.x / graphViewport.rect.size.x, (localpoint.y / graphViewport.rect.size.y) + 1);

            Vector2 deltaPivot = graphContentRectTrans.pivot - pivot;
            Vector2 size = graphContentRectTrans.rect.size;
            Vector3 deltaPosition = new Vector3(deltaPivot.x * size.x, deltaPivot.y * size.y);
            graphContentRectTrans.pivot = pivot;
            graphContentRectTrans.localPosition -= deltaPosition;
            graphContent.transform.localScale = new Vector3(targetSize, targetSize, 1);
            //xAxis
            RectTransform xRectTrans = xAxisContent.GetComponent<RectTransform>();
            size = xRectTrans.rect.size;
            deltaPivot = xRectTrans.pivot - (Vector2.right * pivot.x);
            deltaPosition = new Vector3(deltaPivot.x * size.x, deltaPivot.y * size.y);
            xRectTrans.pivot = xRectTrans.pivot + (Vector2.right * pivot.x);
            xRectTrans.localPosition -= deltaPosition;
            xAxisContent.transform.localScale = new Vector3(targetSize, 1, 1);
            //yAxis
            RectTransform yRectTrans = yAxisContent.GetComponent<RectTransform>();
            size = yRectTrans.rect.size;
            deltaPivot = yRectTrans.pivot - (Vector2.up * pivot.y);
            deltaPosition = new Vector3(deltaPivot.x * size.x, deltaPivot.y * size.y);
            yRectTrans.pivot = yRectTrans.pivot + (Vector2.up * pivot.y);
            yRectTrans.localPosition -= deltaPosition;
            yAxisContent.transform.localScale = new Vector3(1, targetSize, 1);

            pointLine.transform.localScale = new Vector3(1 / targetSize, 1 / targetSize, 1);

            LayoutXScale();
            LayoutYScale();


            for (int i = 0; i < sortedGraphPointsList.Count; i++) {
                pointLine.SetPosition(i, (Camera.main.WorldToScreenPoint(sortedGraphPointsList.Values[i].handleRect.position) - Camera.main.WorldToScreenPoint(graphContent.transform.position)));
            }

            for (int i = 0; i < pointsUpperThreshold.Count; i++) {
                thresholdLineUpper.SetPosition(i, (Camera.main.WorldToScreenPoint(pointsUpperThreshold.Values[i].handleRect.position) - Camera.main.WorldToScreenPoint(graphContent.transform.position)));
            }

            for (int i = 0; i < pointsLowerThreshold.Count; i++) {
                thresholdLineLower.SetPosition(i, (Camera.main.WorldToScreenPoint(pointsLowerThreshold.Values[i].handleRect.position) - Camera.main.WorldToScreenPoint(graphContent.transform.position)));
            }

            for (int i = 0; i < xAxisContent.transform.GetChild(0).childCount; i++) {
                xAxisContent.transform.GetChild(0).GetChild(i).localScale = (Vector3.one - Vector3.right) + (Vector3.right / targetSize);
            }

            for (int i = 0; i < yAxisContent.transform.GetChild(0).childCount; i++) {
                yAxisContent.transform.GetChild(0).GetChild(i).localScale = (Vector3.one - Vector3.up) + (Vector3.up / targetSize);
            }
        }
    }

    private void LayoutXScale() {

        // 480
        if (xScale >= 720) {
            for (int i = 0; i < xScale; i++) {
                if (i % (60) == 0) {
                    xAxis.transform.GetChild(i).gameObject.SetActive(true);
                    xAxis.transform.GetChild(i).localScale = Vector3.one / 2;
                    grid.transform.GetChild(i).GetComponent<LineRenderer>().enabled = true;

                }
                else {
                    xAxis.transform.GetChild(i).gameObject.SetActive(false);
                    xAxis.transform.GetChild(i).localScale = Vector3.one;
                    grid.transform.GetChild(i).GetComponent<LineRenderer>().enabled = false;
                }
            }
        }
        else if (xScale >= 180) {
            for (int i = 0; i < xScale; i++) {
                if (i % (30) == 0) {
                    xAxis.transform.GetChild(i).gameObject.SetActive(true);
                    xAxis.transform.GetChild(i).localScale = Vector3.one / 2;
                    grid.transform.GetChild(i).GetComponent<LineRenderer>().enabled = true;

                }
                else {
                    xAxis.transform.GetChild(i).gameObject.SetActive(false);
                    xAxis.transform.GetChild(i).localScale = Vector3.one;
                    grid.transform.GetChild(i).GetComponent<LineRenderer>().enabled = false;
                }
            }
        }
        else if (xScale >= 60) {
            for (int i = 0; i < xScale; i++) {
                if (i % (10) == 0) {
                    xAxis.transform.GetChild(i).gameObject.SetActive(true);
                    xAxis.transform.GetChild(i).localScale = Vector3.one / 2;
                    grid.transform.GetChild(i).GetComponent<LineRenderer>().enabled = true;
                }
                else {
                    xAxis.transform.GetChild(i).gameObject.SetActive(false);
                    xAxis.transform.GetChild(i).localScale = Vector3.one;
                    grid.transform.GetChild(i).GetComponent<LineRenderer>().enabled = false;
                }
            }

        }
        else if (xScale >= 30) {
            for (int i = 0; i < xScale; i++) {
                if (i % (2) == 0) {
                    xAxis.transform.GetChild(i).gameObject.SetActive(true);
                    xAxis.transform.GetChild(i).localScale = Vector3.one / 2;
                    grid.transform.GetChild(i).GetComponent<LineRenderer>().enabled = true;
                }
                else {
                    xAxis.transform.GetChild(i).gameObject.SetActive(false);
                    xAxis.transform.GetChild(i).localScale = Vector3.one;
                    grid.transform.GetChild(i).GetComponent<LineRenderer>().enabled = false;
                }
            }
        }
        else {
            for (int i = 0; i < xScale; i++) {
                xAxis.transform.GetChild(i).gameObject.SetActive(true);
                grid.transform.GetChild(i).GetComponent<LineRenderer>().enabled = true;
            }
        }
        xAxis.transform.GetChild(0).gameObject.SetActive(true);
        grid.transform.GetChild(0).GetComponent<LineRenderer>().enabled = true;
        xAxis.transform.GetChild(xScale).gameObject.SetActive(true);
        grid.transform.GetChild(xScale).GetComponent<LineRenderer>().enabled = true;
    }

    private void LayoutYScale() {
        // yAxisContent.GetComponent<RectTransform>().sizeDelta = new Vector2(yAxisContent.GetComponent<RectTransform>().sizeDelta.x, graphContentRectTrans.rect.height);
        int diff = 0;


        if (yStart % 10 != 0) {
            diff = 10 - (yStart % 10);
        }

        for (int i = 0; i < yScale; i++) {
            if (i % 10 == diff) {
                yAxis.transform.GetChild(i).gameObject.SetActive(true);
                grid.transform.GetChild(xScale + i + 1).GetComponent<LineRenderer>().enabled = true;
            }
            else {
                yAxis.transform.GetChild(i).gameObject.SetActive(false);
                grid.transform.GetChild(xScale + i + 1).GetComponent<LineRenderer>().enabled = false;
            }
        }
        yAxis.transform.GetChild(0).gameObject.SetActive(true);
        grid.transform.GetChild(xScale + 1).GetComponent<LineRenderer>().enabled = true;
        yAxis.transform.GetChild(yScale).gameObject.SetActive(true);
        grid.transform.GetChild(xScale + 1 + (yScale)).GetComponent<LineRenderer>().enabled = true;
    }

    private void ChangeLinkedPointLineWithSlider(float value) {
        Slider slider = EventSystem.current.currentSelectedGameObject.GetComponent<Slider>();
        float yPos = (Camera.main.WorldToScreenPoint(slider.handleRect.position) - Camera.main.WorldToScreenPoint(graph.transform.position / graphContent.transform.localScale.y)).y / graphContent.transform.localScale.y;
        Vector3 pos = new Vector3(pointLine.GetPosition(sortedGraphPointsList.IndexOfValue(slider)).x, yPos, -1);
        pointLine.SetPosition(sortedGraphPointsList.IndexOfValue(slider), pos);
    }

    private void ChangeUpperThresholdLineWithSlider(float value) {
        Slider slider = EventSystem.current.currentSelectedGameObject.GetComponent<Slider>();
        float yPos = (Camera.main.WorldToScreenPoint(slider.handleRect.position) - Camera.main.WorldToScreenPoint(graph.transform.position / graphContent.transform.localScale.y)).y / graphContent.transform.localScale.y;
        Vector3 pos = new Vector3(thresholdLineUpper.GetPosition(pointsUpperThreshold.IndexOfValue(slider)).x, yPos, -1);
        thresholdLineUpper.SetPosition(pointsUpperThreshold.IndexOfValue(slider), pos);
    }

    private void ChangeLowerThresholdLineWithSlider(float value) {
        Slider slider = EventSystem.current.currentSelectedGameObject.GetComponent<Slider>();
        float yPos = (Camera.main.WorldToScreenPoint(slider.handleRect.position) - Camera.main.WorldToScreenPoint(graph.transform.position / graphContent.transform.localScale.y)).y / graphContent.transform.localScale.y;
        Vector3 pos = new Vector3(thresholdLineLower.GetPosition(pointsLowerThreshold.IndexOfValue(slider)).x, yPos, -1);
        thresholdLineLower.SetPosition(pointsLowerThreshold.IndexOfValue(slider), pos);
    }

    private Vector3[] MakeSmoothCurve(Vector3[] arrayToCurve, float smoothness) {
        List<Vector3> pointy;
        List<Vector3> curvedPoints;
        int pointsLength = 0;
        int curvedLength = 0;

        if (smoothness < 1.0f) smoothness = 1.0f;

        pointsLength = arrayToCurve.Length;

        curvedLength = (pointsLength * Mathf.RoundToInt(smoothness)) - 1;


        if (curvedLength == -1) return null;
        curvedPoints = new List<Vector3>(curvedLength);

        float t = 0.0f;
        for (int pointInTimeOnCurve = 0; pointInTimeOnCurve < curvedLength + 1; pointInTimeOnCurve++) {
            t = Mathf.InverseLerp(0, curvedLength, pointInTimeOnCurve);

            pointy = new List<Vector3>(arrayToCurve);

            for (int j = pointsLength - 1; j > 0; j--) {
                for (int i = 0; i < j; i++) {
                    pointy[i] = (1 - t) * pointy[i] + t * pointy[i + 1];
                }
            }

            curvedPoints.Add(pointy[0]);
        }

        return (curvedPoints.ToArray());
    }

    private void HierachyPositionSearch(GameObject point) {
        for (int i = 0; i < graph.transform.childCount; i++) {
            if (graph.transform.GetChild(i).localPosition.x > point.transform.localPosition.x) {
                point.transform.SetSiblingIndex(i);
                return;
            }

        }
    }

    private void DrawLinkedPointLines() {
        GameObject LineWriter;
        Vector3[] arrayToCurve = new Vector3[sortedGraphPointsList.Count];

        counter = 0;
        float previous = -1;
        foreach (KeyValuePair<float, Slider> item in sortedGraphPointsList) {
            if (item.Key < previous) { print("Alert"); }
            previous = item.Key;
            arrayToCurve[counter] = Camera.main.WorldToScreenPoint(item.Value.handleRect.transform.position) - Camera.main.WorldToScreenPoint(graphContent.transform.position);
            counter++;
        }

        // MakeSmoothCurve(arrayToCurve, 30);
        if (pointLine == null) {
            if (graph == null) return;
            LineWriter = Instantiate(lineRendererPrefab, graphContent.transform);
            LineWriter.transform.localScale = Vector3.one;
            LineWriter.transform.localPosition = Vector3.zero;

            pointLine = LineWriter.GetComponent<LineRenderer>();
            pointLine.startColor = Color.red;
            pointLine.endColor = Color.red;
            pointLine.startWidth = 0.1f;
            pointLine.endWidth = 0.1f;
        }

        pointLine.transform.SetAsLastSibling();
        pointLine.numPositions = sortedGraphPointsList.Count;

        for (int i = 0; i < sortedGraphPointsList.Count; i++) {
            pointLine.SetPosition(i, arrayToCurve[i] - (Vector3.forward * arrayToCurve[i].z));
        }
    }

    private void DrawLinkedPointLineUpperThreshold() {
        GameObject LineWriter;
        Vector3[] arrayToCurve = new Vector3[sortedGraphPointsList.Count];

        int counter = 0;
        foreach (KeyValuePair<float, Slider> item in sortedGraphPointsList) {
            arrayToCurve[counter] = Camera.main.WorldToScreenPoint(item.Value.handleRect.transform.position) - Camera.main.WorldToScreenPoint(graphContent.transform.position);
            counter++;
        }

        MakeSmoothCurve(arrayToCurve, 30);
        if (thresholdLineUpper == null) {
            if (graph == null) return;
            LineWriter = Instantiate(lineRendererThresholdPrefab, graphContent.transform);
            LineWriter.transform.localScale = Vector3.one;
            LineWriter.transform.localPosition = Vector3.zero;

            thresholdLineUpper = LineWriter.GetComponent<LineRenderer>();
            thresholdLineUpper.startColor = Color.red;
            thresholdLineUpper.endColor = Color.red;
            thresholdLineUpper.startWidth = 0.1f;
            thresholdLineUpper.endWidth = 0.1f;
        }

        thresholdLineUpper.transform.SetAsLastSibling();
        thresholdLineUpper.numPositions = sortedGraphPointsList.Count;

        for (int i = 0; i < sortedGraphPointsList.Count; i++) {
            thresholdLineUpper.SetPosition(i, arrayToCurve[i]);
        }
    }

    private void DrawLinkedPointLinesLowerThreshold() {
        GameObject LineWriter;
        Vector3[] arrayToCurve = new Vector3[sortedGraphPointsList.Count];

        int counter = 0;
        foreach (KeyValuePair<float, Slider> item in sortedGraphPointsList) {
            arrayToCurve[counter] = Camera.main.WorldToScreenPoint(item.Value.handleRect.transform.position) - Camera.main.WorldToScreenPoint(graphContent.transform.position);
            counter++;
        }

        MakeSmoothCurve(arrayToCurve, 30);
        if (thresholdLineLower == null) {
            if (graph == null) return;
            LineWriter = Instantiate(lineRendererThresholdPrefab, graphContent.transform);
            LineWriter.transform.localScale = Vector3.one;
            LineWriter.transform.localPosition = Vector3.zero;

            thresholdLineLower = LineWriter.GetComponent<LineRenderer>();
            thresholdLineLower.startColor = Color.red;
            thresholdLineLower.endColor = Color.red;
            thresholdLineLower.startWidth = 0.1f;
            thresholdLineLower.endWidth = 0.1f;
        }

        thresholdLineLower.transform.SetAsLastSibling();
        thresholdLineLower.numPositions = sortedGraphPointsList.Count;

        for (int i = 0; i < sortedGraphPointsList.Count; i++) {
            thresholdLineLower.SetPosition(counter, arrayToCurve[i]);
        }
    }
    #endregion

    #region Public Methods
    public void GenerateGraph(int _xStart, int _xEnd, int _yStart, int _yEnd, string yLabel) {
        print("generating graph " + name);
        // using the scrollview viewport as the basis for the graphs size
        Rect viewportRect = graphViewport.GetComponent<RectTransform>().rect;
        // "-20" is for padding so that there is no clipping at the edges of the graph for graphlines or numbers
        graphContentRectTrans = graphContent.GetComponent<RectTransform>();
        graphContentRectTrans.sizeDelta = new Vector2(viewportRect.width - 20, viewportRect.height - 20);
        graphContentRectTrans.localPosition = Vector3.zero;

        // resizing both axis to account for any changes in the new size
        xAxisContent.GetComponent<RectTransform>().sizeDelta = new Vector2(viewportRect.width - 20, xAxisContent.GetComponent<RectTransform>().sizeDelta.y);
        yAxisContent.GetComponent<RectTransform>().sizeDelta = new Vector2(yAxisContent.GetComponent<RectTransform>().sizeDelta.x, viewportRect.height - 20);

        // adjusting the collider which will detect mouseover for the graph in the Update function
        graph.GetComponent<BoxCollider>().size = new Vector2(viewportRect.width - 20, viewportRect.height - 20);
        coordinateSystem.GetComponent<BoxCollider>().size = new Vector3(coordinateSystem.GetComponent<RectTransform>().rect.width * 1.5f, coordinateSystem.GetComponent<RectTransform>().rect.height * 1.5f, 1);

        // adjusting the placement of the axis labels
        yAxisLabel.transform.localPosition += Vector3.left * 50;
        xAxisLabel.transform.localPosition += Vector3.down * 50;
        // as well as label text
        yAxisLabel.GetComponent<Text>().text = yLabel;
        xAxisLabel.GetComponent<Text>().text = "Duration (Minutes : Seconds)";

        // record of initial values based on time (x) against units (y)
        xStart = _xStart;
        xEnd = _xEnd;
        yStart = _yStart;
        yEnd = _yEnd;
        xScale = _xEnd - _xStart;
        yScale = _yEnd - _yStart;

        // duration set to difference, slightly redundant but in place in case the simulation were 
        // ever to start from anything other than 0
        duration = xScale;

        // placement of all axis markers
        InitialiseXScale();
        InitialiseYScale();

        // creation of gridlines in seporate gameobject which shares the same size and position properties as the
        // gameobject which will hold the vital values via points along the graph.
        DrawGrid();

        // set all axis markers to inactive other than the ones which make sense to show
        // based on the size and scale of the graph
        LayoutXScale();
        LayoutYScale();

        // this ensures there are no descrepencies between the axis scrollareas and the main
        // graph area scroll rect
        xAxisScrollRect.horizontalNormalizedPosition = 0;
        xAxisScrollRect.scrollSensitivity = 0;
        yAxisScrollRect.verticalNormalizedPosition = 0;
        yAxisScrollRect.scrollSensitivity = 0;
        GraphScrollRect.scrollSensitivity = 0;
        GraphScrollRect.normalizedPosition = new Vector2(0, 0);

        // functionality which allows the axis markers and the graph movement to remain syncronised
        // (providing the sizes of each are the exact same)
        // GraphScrollRect.onValueChanged.AddListener(ListenerMethod);

    }

    // for dragging of the content within the viewport of the scrollRect, to ensure axis follow where the graph is dragged
    //public void ListenerMethod(Vector2 value) {
    //    xAxisScrollRect.normalizedPosition = GraphScrollRect.normalizedPosition;
    //    yAxisScrollRect.normalizedPosition = GraphScrollRect.normalizedPosition;
    //}

    public void AddPoint(Vector3 screenPoint) {
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(screenPoint);
        if (graph == null) return;
        GameObject point = Instantiate(graphPointPrefab, graph.transform);
        Slider slider = point.GetComponent<Slider>();
        point.transform.localScale = Vector3.one;
        point.GetComponent<RectTransform>().sizeDelta = new Vector2(20, graph.GetComponent<RectTransform>().rect.height + slider.handleRect.sizeDelta.y);
        point.transform.localPosition = Vector3.zero;
        slider.minValue = yStart;
        slider.maxValue = yEnd;
        slider.value = yStart;
        float minValue = slider.handleRect.transform.position.y;
        slider.value = yEnd;
        float maxValue = slider.handleRect.transform.position.y - minValue;
        float currentValue = worldPoint.y - minValue;
        float percent = (currentValue / maxValue) * 100;
        point.transform.position = new Vector3(worldPoint.x, point.transform.position.y, 0);
        slider.value = (((slider.maxValue - slider.minValue) / 100) * percent);
        float pointTime = (slider.transform.localPosition.x + (graph.GetComponent<RectTransform>().rect.width / 2)) / (graph.GetComponent<RectTransform>().rect.width / xScale);
        HierachyPositionSearch(point);
        point.transform.localPosition += (Vector3.back * point.transform.localPosition.z) - (Vector3.forward * 50);
        if (sortedGraphPointsList.ContainsKey(pointTime)) {
            sortedGraphPointsList.Remove(pointTime);
        }
        sortedGraphPointsList.Add(pointTime, slider);
        slider.onValueChanged.AddListener(ChangeLinkedPointLineWithSlider);
        DrawLinkedPointLines();
    }

    public void AddThresholdPointUpper(Vector3 screenPoint) {
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(screenPoint);
        if (graph == null) return;
        GameObject point = Instantiate(graphPointPrefab, graph.transform);
        Slider slider = point.GetComponent<Slider>();
        point.transform.localScale = Vector3.one;
        point.GetComponent<RectTransform>().sizeDelta = new Vector2(20, graph.GetComponent<RectTransform>().rect.height + slider.handleRect.sizeDelta.y);
        point.transform.localPosition = Vector3.zero;
        slider.minValue = yStart;
        slider.maxValue = yEnd;
        slider.value = yStart;
        float minValue = slider.handleRect.transform.position.y;
        slider.value = yEnd;
        float maxValue = slider.handleRect.transform.position.y - minValue;
        float currentValue = worldPoint.y - minValue;
        float percent = (currentValue / maxValue) * 100;
        point.transform.position = new Vector3(worldPoint.x, point.transform.position.y, 1);
        slider.value = (((slider.maxValue - slider.minValue) / 100) * percent);
        float pointTime = (slider.transform.localPosition.x + (graph.GetComponent<RectTransform>().rect.width / 2)) / (graph.GetComponent<RectTransform>().rect.width / xScale);
        HierachyPositionSearch(point);
        pointsUpperThreshold.Add(pointTime, slider);
        slider.onValueChanged.AddListener(ChangeUpperThresholdLineWithSlider);
        DrawThresholds();
    }

    public void AddThresholdPointLower(Vector3 screenPoint) {
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(screenPoint);
        if (graph == null) return;
        GameObject point = Instantiate(graphPointPrefab, graph.transform);
        Slider slider = point.GetComponent<Slider>();
        point.transform.localScale = Vector3.one;
        point.GetComponent<RectTransform>().sizeDelta = new Vector2(20, graph.GetComponent<RectTransform>().rect.height + slider.handleRect.sizeDelta.y);
        point.transform.localPosition = Vector3.zero;
        slider.minValue = yStart;
        slider.maxValue = yEnd;
        slider.value = yStart;
        float minValue = slider.handleRect.transform.position.y;
        slider.value = yEnd;
        float maxValue = slider.handleRect.transform.position.y - minValue;
        float currentValue = worldPoint.y - minValue;
        float percent = (currentValue / maxValue) * 100;
        point.transform.position = new Vector3(worldPoint.x, point.transform.position.y, 1);
        slider.value = (((slider.maxValue - slider.minValue) / 100) * percent);
        float pointTime = (slider.transform.localPosition.x + (graph.GetComponent<RectTransform>().rect.width / 2)) / (graph.GetComponent<RectTransform>().rect.width / xScale);
        HierachyPositionSearch(point);
        pointsUpperThreshold.Add(pointTime, slider);
        slider.onValueChanged.AddListener(ChangeLowerThresholdLineWithSlider);
        DrawThresholds();
    }

    public void AddPoint(float xValue, float yValue) {
        GameObject point = Instantiate(graphPointPrefab, graph.transform);
        Slider slider = point.GetComponent<Slider>();
        point.transform.localScale = Vector3.one;
        point.transform.localPosition = Vector3.zero;
        point.transform.localPosition += new Vector3((-graphContentRectTrans.rect.width / 2), (-graphContentRectTrans.rect.height / 2), 0);
        point.transform.localPosition += new Vector3(((graphContentRectTrans.rect.width / xScale) * xValue), graphContentRectTrans.rect.height / 2, -50);
        point.GetComponent<RectTransform>().sizeDelta = new Vector2(20, graphContentRectTrans.rect.height + slider.handleRect.sizeDelta.y);
        slider.minValue = yStart;
        slider.maxValue = yEnd;
        slider.value = yValue;
        if (sortedGraphPointsList.ContainsKey(xValue)) {
            sortedGraphPointsList.Remove(xValue);
        }
        sortedGraphPointsList.Add(xValue, slider);
        slider.onValueChanged.AddListener(ChangeLinkedPointLineWithSlider);
        DrawLinkedPointLines();
    }

    public void AddThresholdPointUpper(float xValue, float yValue) {
        GameObject point = Instantiate(graphPointPrefab, thresholds.transform);
        Slider slider = point.GetComponent<Slider>();
        point.transform.localScale = Vector3.one;
        point.transform.localPosition = Vector3.zero;
        point.transform.localPosition += new Vector3((-graphContentRectTrans.rect.width / 2), (-graphContentRectTrans.rect.height / 2), 0);
        point.transform.localPosition += new Vector3(((graphContentRectTrans.rect.width / xScale) * xValue), graphContentRectTrans.rect.height / 2, -1);
        point.GetComponent<RectTransform>().sizeDelta = new Vector2(20, graphContentRectTrans.rect.height + slider.handleRect.sizeDelta.y);
        slider.minValue = yStart;
        slider.maxValue = yEnd;
        slider.value = yValue;
        pointsUpperThreshold.Add(xValue, slider);
        slider.onValueChanged.AddListener(ChangeUpperThresholdLineWithSlider);
        DrawThresholds();
    }

    public void AddThresholdPointLower(float xValue, float yValue) {
        GameObject point = Instantiate(graphPointPrefab, thresholds.transform);
        Slider slider = point.GetComponent<Slider>();
        point.transform.localScale = Vector3.one;
        point.transform.localPosition = Vector3.zero;
        point.transform.localPosition += new Vector3((-graphContentRectTrans.rect.width / 2), (-graphContentRectTrans.rect.height / 2), 0);
        point.transform.localPosition += new Vector3(((graphContentRectTrans.rect.width / xScale) * xValue), graphContentRectTrans.rect.height / 2, -1);
        point.GetComponent<RectTransform>().sizeDelta = new Vector2(20, graphContentRectTrans.rect.height + slider.handleRect.sizeDelta.y);
        slider.minValue = yStart;
        slider.maxValue = yEnd;
        slider.value = yValue;
        pointsLowerThreshold.Add(xValue, slider);
        slider.onValueChanged.AddListener(ChangeLowerThresholdLineWithSlider);
        DrawThresholds();
    }

    public void SelectCoordinateXValue() {
        coordinateX.Select();
        coordinateX.ActivateInputField();
        Error.instance.informOkButton.onClick.RemoveAllListeners();
    }

    #endregion



}
