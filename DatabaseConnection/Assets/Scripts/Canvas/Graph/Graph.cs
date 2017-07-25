using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;




public class Graph : MonoBehaviour {

    public delegate void OverlayPointChange();
    public static OverlayPointChange overlayPointChange;

    #region Public Variables
    public GameObject graph;
    public GameObject grid;
    public GameObject upperThresholds;
    public GameObject lowerThresholds;
    public GameObject xAxis;
    public GameObject yAxis;
    public BoxCollider graphBoxCollider;
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
    public GameObject coordinateSystem;
    public InputField coordinateX;
    public InputField coordinateY;

    public LineRenderer pointLine;
    public LineRenderer thresholdLineLower;
    public LineRenderer thresholdLineUpper;

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

    public int offset = 0;

    #endregion

    #region Private Variables
    // private float scrollWheel = 1;
    // private float newScrollWheel;
    private Vector2 localpoint;
    public RectTransform graphContentRectTrans;


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
    #endregion

    #region Unity Methods

    private void LateUpdate() {

        // check for the acceptance of the coordinate system which replaces a designated point to a new location
        if (coordinateSystem != null && coordinateSystem.activeSelf && Input.GetKeyDown(KeyCode.Return)) {
            float x;
            if (coordinateX.text.Contains(":")) {
                string[] split = coordinateX.text.Split(':');
                x = float.Parse(split[0]) * 60;
                x += float.Parse(split[1]);
            } else {
                x = float.Parse(coordinateX.text);
            }
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
            } else {
                // the values given are not within range bounds
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
                // if over graph check for a "mouse up" to place a new point, else deactivate coordinate system
                if (tag == "Graph") {
                    if (Input.GetMouseButtonUp(0) && mouseHold < 0.3) {
                        if (coordinateSystem.activeSelf) {
                            coordinateSystem.SetActive(false);
                            sliderHandleTransform.GetComponent<Image>().color = currentlySelectedSlider.colors.normalColor;
                        } else {
                            AddPoint(Camera.main.WorldToScreenPoint(hit1.point));
                        }
                    }
                }

                // if over handle 
                else if (tag == "Handle") {
                    // ensures more than one point cant be moved during a given point drag
                    if (!leftMouseButtonIsDown) {
                        sliderHandleTransform = hit1.transform;
                        currentlySelectedSlider = sliderHandleTransform.parent.parent.GetComponent<Slider>();
                        indexOfSliderMinipulated = sortedGraphPointsList.IndexOfValue(currentlySelectedSlider);
                        // placeHandleBack = false;
                    }

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
                            } else {
                                if (previousSliderHandleTransform != null) {
                                    previousSliderHandleTransform.GetComponent<Image>().color = currentlySelectedSlider.colors.normalColor;
                                }
                                previousSliderHandleTransform = sliderHandleTransform;
                                sliderHandleTransform.GetComponent<Image>().color = Color.red;
                                coordinateSystem.SetActive(true);
                                coordinateSystem.transform.localPosition = Vector3.zero;
                                coordinateSystem.transform.localScale = new Vector3(0.5f, 0.5f, 1);
                                coordinateSystem.transform.position = sliderHandleTransform.position + new Vector3(0.3f, 0.3f, 0);
                                coordinateSystem.transform.localPosition = new Vector3(coordinateSystem.transform.localPosition.x, coordinateSystem.transform.localPosition.y, -500);
                                string minutes = (Mathf.CeilToInt(sortedGraphPointsList.Keys[sortedGraphPointsList.IndexOfValue(currentlySelectedSlider)]) / 60).ToString();
                                string Seconds = (Mathf.CeilToInt(sortedGraphPointsList.Keys[sortedGraphPointsList.IndexOfValue(currentlySelectedSlider)]) % 60).ToString();

                                coordinateX.text = minutes + ":" + Seconds;
                                coordinateY.text = Mathf.CeilToInt(currentlySelectedSlider.value - offset).ToString();
                            }
                            mouseHold = 0;
                        }
                    }

                    // the mouse has been clicked reset the time limit required to spawn the coordinate system and setup the point to be dragged
                    if (Input.GetMouseButtonDown(0)) {
                        leftMouseButtonIsDown = true;
                        // currentlySelectedSlider.interactable = true;
                    }

                    // if right click detected and not first or last graph point then delete point
                    if (Input.GetMouseButtonUp(1)) {
                        if (indexOfSliderMinipulated == -1 || indexOfSliderMinipulated == 0 || indexOfSliderMinipulated == sortedGraphPointsList.Count - 1) return;
                        sortedGraphPointsList.RemoveAt(indexOfSliderMinipulated);
                        Destroy(currentlySelectedSlider.gameObject);
                        DrawLinkedPointLines();
                    }

                }

            } else {
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
                    // if the user has dragged the point beyond the first or last point then place point between the first and second, or penultimate and last
                    StandaloneInputModule input = EventSystem.current.GetComponent<StandaloneInputModule>();
                    input.DeactivateModule();
                    Slider beforePoint = sortedGraphPointsList.Values[indexOfSliderMinipulated - 1];
                    Slider afterPoint = sortedGraphPointsList.Values[indexOfSliderMinipulated + 1];
                    Vector3 newVector = (beforePoint.transform.localPosition + afterPoint.transform.localPosition) / 2;
                    currentlySelectedSlider.transform.localPosition = newVector;
                    float newValue = (beforePoint.value + afterPoint.value) / 2;
                    currentlySelectedSlider.value = newValue;
                    newSliderTime = (currentlySelectedSlider.transform.localPosition.x + (graph.GetComponent<RectTransform>().rect.width / 2)) / (graph.GetComponent<RectTransform>().rect.width / xScale);
                    if (!sortedGraphPointsList.ContainsKey(newSliderTime) && allowChangingPosition) {
                        sortedGraphPointsList.RemoveAt(indexOfSliderMinipulated);
                        sortedGraphPointsList.Add(newSliderTime, currentlySelectedSlider);
                        indexOfSliderMinipulated = sortedGraphPointsList.IndexOfKey(newSliderTime);
                    }
                    leftMouseButtonIsDown = false;
                    mouseHold = 0;
                    previousFrameTime = newSliderTime;
                    DrawLinkedPointLines();

                } else {
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
            leftMouseButtonIsDown = false;
            mouseHold = 0;
        }

    }


    public void OnEnable() {
        DrawLinkedPointLines();
        DrawThresholds();
        LerpFromView.onEnd += this.ResizeGraph;
    }
    #endregion

    #region Private Methods


    public void ResizeGraph() {
        // have graph prefab fit parent container
        GetComponent<RectTransform>().sizeDelta = transform.parent.GetComponent<RectTransform>().sizeDelta;
        GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
        GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);

        // check for null viewport as this causes crash, not sure why the delay for viewport to instantiate but this requires a frame or two.
        if (graphViewport != null) {
            Transform dashMarker;
            LineRenderer lineRenderer;
            Rect viewportRect = graphViewport.GetComponent<RectTransform>().rect;
            Vector2 padding = new Vector2(viewportRect.width / 100 * 5, viewportRect.height / 100 * 5);
            graphContentRectTrans = graphContent.GetComponent<RectTransform>();
            Vector2 sizeMarkup = new Vector2(graphContentRectTrans.sizeDelta.x / (viewportRect.width - padding.x), graphContentRectTrans.sizeDelta.y / (viewportRect.height - padding.y));

            // content resize
            graphContentRectTrans.sizeDelta = new Vector2(viewportRect.width - padding.x, viewportRect.height - padding.y);
            Vector2 size = new Vector2(graphContentRectTrans.rect.width, graphContentRectTrans.rect.height);
            graphBoxCollider.size = graphContentRectTrans.sizeDelta;

            // axis resize
            xAxisContent.GetComponent<RectTransform>().sizeDelta = new Vector2(viewportRect.width - padding.x, xAxisContent.GetComponent<RectTransform>().sizeDelta.y);
            yAxisContent.GetComponent<RectTransform>().sizeDelta = new Vector2(yAxisContent.GetComponent<RectTransform>().sizeDelta.x, viewportRect.height - padding.y);

            // grid resize
            for (int i = 0; i < grid.transform.childCount; i++) {
                dashMarker = grid.transform.GetChild(i);
                lineRenderer = dashMarker.GetComponent<LineRenderer>();
                if (i <= xScale) {
                    lineRenderer.SetPosition(0, new Vector3((((size.x / xScale) * i) - (size.x / 2)), (-size.y / 2), 20));
                    lineRenderer.SetPosition(1, new Vector3((((size.x / xScale) * i) - (size.x / 2)), (size.y / 2), 20));
                } else {
                    lineRenderer.SetPosition(0, new Vector3((-size.x / 2), (((size.y / yScale) * (i - (xScale + 1))) - (size.y / 2)), 20));
                    lineRenderer.SetPosition(1, new Vector3((size.x / 2), (((size.y / yScale) * (i - (xScale + 1))) - (size.y / 2)), 20));
                }
            }

            for (int i = 0; i < xAxis.transform.childCount; i++) {
                dashMarker = xAxis.transform.GetChild(i);
                dashMarker.transform.localPosition = Vector3.zero;
                dashMarker.transform.localPosition += new Vector3((-graphContentRectTrans.rect.width / 2), 0, 0);
                dashMarker.transform.localPosition += new Vector3((((graphContentRectTrans.rect.width) / xScale) * i), 0, 0);
            }
            for (int i = 0; i < yAxis.transform.childCount; i++) {
                dashMarker = yAxis.transform.GetChild(i);
                dashMarker.transform.localPosition = Vector3.zero;
                dashMarker.transform.localPosition += new Vector3(0, (-graphContentRectTrans.rect.height / 2), 0);
                dashMarker.transform.localPosition += new Vector3(0, (((graphContentRectTrans.rect.height) / yScale) * (i - yStart)), 0);
            }
            yAxisScrollRect.verticalNormalizedPosition = 0;
            xAxisScrollRect.horizontalNormalizedPosition = 0;


            // replacing of points
            sortedGraphPointsList.Clear();
            for (int i = 0; i < graph.transform.childCount; i++) {
                Transform sliderObject = graph.transform.GetChild(i);
                sliderObject.GetComponent<RectTransform>().sizeDelta = new Vector2(20, graph.GetComponent<RectTransform>().rect.height + sliderObject.GetComponent<Slider>().handleRect.sizeDelta.y);
                sliderObject.localPosition = new Vector3(sliderObject.localPosition.x / sizeMarkup.x, sliderObject.localPosition.y / sizeMarkup.y, -20);
                float pointTime = (sliderObject.transform.localPosition.x + (graph.GetComponent<RectTransform>().rect.width / 2)) / (graph.GetComponent<RectTransform>().rect.width / xScale);
                sortedGraphPointsList.Add(pointTime, sliderObject.GetComponent<Slider>());
            }

            pointsUpperThreshold.Clear();
            for (int i = 0; i < upperThresholds.transform.childCount; i++) {
                Transform sliderObject = upperThresholds.transform.GetChild(i);
                sliderObject.localPosition = new Vector3(sliderObject.localPosition.x / sizeMarkup.x, sliderObject.localPosition.y / sizeMarkup.y, -10);
                float pointTime = (sliderObject.transform.localPosition.x + (graph.GetComponent<RectTransform>().rect.width / 2)) / (graph.GetComponent<RectTransform>().rect.width / xScale);
                pointsUpperThreshold.Add(pointTime, sliderObject.GetComponent<Slider>());
            }
            pointsLowerThreshold.Clear();
            for (int i = 0; i < lowerThresholds.transform.childCount; i++) {
                Transform sliderObject = lowerThresholds.transform.GetChild(i);
                sliderObject.localPosition = new Vector3(sliderObject.localPosition.x / sizeMarkup.x, sliderObject.localPosition.y / sizeMarkup.y, -10);
                float pointTime = (sliderObject.transform.localPosition.x + (graph.GetComponent<RectTransform>().rect.width / 2)) / (graph.GetComponent<RectTransform>().rect.width / xScale);
                pointsLowerThreshold.Add(pointTime, sliderObject.GetComponent<Slider>());
            }
            DrawLinkedPointLines();
            DrawLinkedPointLinesLowerThreshold();
            DrawLinkedPointLineUpperThreshold();
        }
    }

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
            if (n - offset == 0) {
                lineRenderer.startWidth = 0.05f;
                lineRenderer.endWidth = 0.05f;
            } else {
                lineRenderer.startWidth = 0.01f;
                lineRenderer.endWidth = 0.01f;
            }
            lineRenderer.SetPosition(0, new Vector3((-size.x / 2), (((size.y / yScale) * n) - (size.y / 2)), 0));
            lineRenderer.SetPosition(1, new Vector3((size.x / 2), (((size.y / yScale) * n) - (size.y / 2)), 0));
        }

    }

    private void InitialiseXScale() {
        for (int i = 0; i <= xScale; i++) {
            GameObject dashMarker = Instantiate(xDashMarkerPrefab, xAxis.transform);
            dashMarker.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            dashMarker.transform.localPosition = Vector3.zero;
            dashMarker.transform.localPosition += new Vector3((-graphContentRectTrans.rect.width / 2), 0, 0);
            dashMarker.transform.localPosition += new Vector3((((graphContentRectTrans.rect.width) / xScale) * i), 0, 0);
            if (i % 60 == 0) {
                dashMarker.GetComponent<Text>().text = ((i / 60)).ToString();
            } else {
                dashMarker.GetComponent<Text>().text = ((i % 60)).ToString();
                dashMarker.transform.localScale = Vector3.one / 3f;

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
            text.text = (i - offset).ToString();
            text.fontSize = 1;
        }
    }

    private void DrawThresholds() {
        if (graph == null) return;
        thresholdLineLower.numPositions = pointsLowerThreshold.Count;
        counter = 0;
        foreach (KeyValuePair<float, Slider> item in pointsLowerThreshold) {
            thresholdLineLower.SetPosition(counter, Camera.main.WorldToScreenPoint(item.Value.handleRect.transform.position) - Camera.main.WorldToScreenPoint(graphContent.transform.position));
            counter++;
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
        float currentIncrement = -1;
        float[] increments = { 1, 2, 10, 30, 60, 120, 300, 600, 1800, 3600, 7200 };
        // go through each increment number one by one
        for (int i = 0; i < increments.Length; i++) {
            float numberOfIncrements = xScale / increments[i];
            // if xscale / current increment is > 6 && < 12
            if (numberOfIncrements >= 6 && numberOfIncrements <= 18) {
                currentIncrement = increments[i];
                i = increments.Length;
            }
        }
        if (currentIncrement != -1) {
            for (int i = 0; i < xScale; i++) {
                if (i % (currentIncrement) == 0) {
                    xAxis.transform.GetChild(i).gameObject.SetActive(true);
                    grid.transform.GetChild(i).GetComponent<LineRenderer>().enabled = true;

                } else {
                    xAxis.transform.GetChild(i).gameObject.SetActive(false);
                    grid.transform.GetChild(i).GetComponent<LineRenderer>().enabled = false;
                }
            }
        }
    }

    private void LayoutYScale() {
        // find the increment used to set the scaling
        int currentIncrement = -1;
        float numberOfIncrements = 0;
        int[] increments = { 1, 2, 10, 25, 50, 100, 250, 500, 1000, 2500, 5000, 10000 };
        for (int i = 0; i < increments.Length; i++) {
            numberOfIncrements = yScale / increments[i];
            // print(numberOfIncrements + " : " + increments[i]);  // ~~~ Print increments here
            if (numberOfIncrements >= 5 && numberOfIncrements <= 25) {
                currentIncrement = increments[i];
                i = increments.Length;
            }
        }


        // set the scaling based on the chosen increment
        if (currentIncrement != -1) {
            for (int i = 0; i < yScale; i++) {
                if ((i + (yScale % currentIncrement)) % (currentIncrement) == 0) {
                    //  print("i " + i + " % " + currentIncrement + " == " + i % currentIncrement);
                    yAxis.transform.GetChild(i).gameObject.SetActive(true);
                    grid.transform.GetChild(xScale + i + 1).GetComponent<LineRenderer>().enabled = true;

                } else {
                    yAxis.transform.GetChild(i).gameObject.SetActive(false);
                    grid.transform.GetChild(xScale + i + 1).GetComponent<LineRenderer>().enabled = false;
                }
            }
            if ((yScale % currentIncrement) != 0) {
                grid.transform.GetChild(xScale + (yScale % currentIncrement)).GetComponent<LineRenderer>().enabled = true;
            }
        } else {
            print("Error: increments are out of bounds in Graph.LayoutYScale() - print " +
                "increments to console from this method and use these to guide the setting of" +
                "range values within conditional statement.");
        }
    }

    private void ChangeLinkedPointLineWithSlider(float value) {
        if (EventSystem.current.currentSelectedGameObject != null) {
            Slider slider = EventSystem.current.currentSelectedGameObject.GetComponent<Slider>();
            float yPos = (Camera.main.WorldToScreenPoint(slider.handleRect.position) - Camera.main.WorldToScreenPoint(graph.transform.position / graphContent.transform.localScale.y)).y / graphContent.transform.localScale.y;
            Vector3 pos = new Vector3(pointLine.GetPosition(sortedGraphPointsList.IndexOfValue(slider)).x, yPos, -1);
            pointLine.SetPosition(sortedGraphPointsList.IndexOfValue(slider), pos);
        }
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

    public void DrawLinkedPointLines() {
        if (graph == null) return;

        Vector3[] arrayToCurve = new Vector3[sortedGraphPointsList.Count];

        counter = 0;
        foreach (KeyValuePair<float, Slider> item in sortedGraphPointsList) {
            arrayToCurve[counter] = Camera.main.WorldToScreenPoint(item.Value.handleRect.transform.position) - Camera.main.WorldToScreenPoint(graphContent.transform.position);
            counter++;
        }

        // MakeSmoothCurve(arrayToCurve, 30);

        pointLine.numPositions = sortedGraphPointsList.Count;
        for (int i = 0; i < sortedGraphPointsList.Count; i++) {
            pointLine.SetPosition(i, arrayToCurve[i] - (Vector3.forward * arrayToCurve[i].z));
        }

    }

    private void DrawLinkedPointLineUpperThreshold() {

        //  if (graph == null) return;
        Vector3[] arrayToCurve = new Vector3[pointsUpperThreshold.Count];

        int counter = 0;
        foreach (KeyValuePair<float, Slider> item in pointsUpperThreshold) {
            arrayToCurve[counter] = Camera.main.WorldToScreenPoint(item.Value.handleRect.transform.position) - Camera.main.WorldToScreenPoint(graphContent.transform.position);
            counter++;
        }

        // MakeSmoothCurve(arrayToCurve, 30);

        thresholdLineUpper.numPositions = pointsUpperThreshold.Count;

        for (int i = 0; i < pointsUpperThreshold.Count; i++) {
            thresholdLineUpper.SetPosition(i, arrayToCurve[i]);
        }
    }

    private void DrawLinkedPointLinesLowerThreshold() {
        //   if (graph == null) return;
        Vector3[] arrayToCurve = new Vector3[pointsLowerThreshold.Count];

        int counter = 0;
        foreach (KeyValuePair<float, Slider> item in pointsLowerThreshold) {
            arrayToCurve[counter] = Camera.main.WorldToScreenPoint(item.Value.handleRect.transform.position) - Camera.main.WorldToScreenPoint(graphContent.transform.position);
            counter++;
        }

        // MakeSmoothCurve(arrayToCurve, 30);

        thresholdLineLower.numPositions = pointsLowerThreshold.Count;

        for (int i = 0; i < pointsLowerThreshold.Count; i++) {
            thresholdLineLower.SetPosition(i, arrayToCurve[i]);
        }
    }
    #endregion

    #region Public Methods
    public void GenerateGraph(int _xStart, int _xEnd, int _yStart, int _yEnd, string yLabel) {
        print("generating graph " + name);

        // establish if y range has negative value and offset values to have range be positive
        // numbers output then have the offset added back onto them before being displayed or saved.
        if (_yStart < 0) {
            offset = _yStart * -1;
            print("offset in graph y axis = " + offset);
        }

        // as well as label text
        yAxisLabel.GetComponent<Text>().text = yLabel;
        xAxisLabel.GetComponent<Text>().text = "Duration";

        // record of initial values based on time (x) against units (y)
        xStart = _xStart;
        xEnd = _xEnd;
        yStart = _yStart + offset;
        yEnd = _yEnd + offset;
        xScale = _xEnd - _xStart;
        yScale = _yEnd - _yStart;

        // duration set to difference, slightly redundant but in place in case the simulation were 
        // ever to start from anything other than 0
        duration = xScale;

        ResizeGraph();
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

    public void AddPoint(float xValue, float yValue, bool interactable = true) {
        GameObject point = Instantiate(graphPointPrefab, graph.transform);
        Slider slider = point.GetComponent<Slider>();
        point.transform.localScale = Vector3.one;
        point.transform.localPosition = Vector3.zero;
        point.transform.localPosition += new Vector3((-graphContentRectTrans.rect.width / 2), (-graphContentRectTrans.rect.height / 2), 0);
        point.transform.localPosition += new Vector3(((graphContentRectTrans.rect.width / xScale) * xValue), graphContentRectTrans.rect.height / 2, -50);
        point.GetComponent<RectTransform>().sizeDelta = new Vector2(20, graphContentRectTrans.rect.height + slider.handleRect.sizeDelta.y);
        slider.minValue = yStart;
        slider.maxValue = yEnd;
        slider.value = yValue + offset;
        slider.interactable = interactable;
        if (sortedGraphPointsList.ContainsKey(xValue)) {
            sortedGraphPointsList.Remove(xValue);
        }
        sortedGraphPointsList.Add(xValue, slider);
        slider.onValueChanged.AddListener(ChangeLinkedPointLineWithSlider);
        DrawLinkedPointLines();
    }

    public void AddThresholdPointUpper(float xValue, float yValue) {
        GameObject point = Instantiate(graphPointPrefab, upperThresholds.transform);
        Slider slider = point.GetComponent<Slider>();
        point.transform.localScale = Vector3.one;
        point.transform.localPosition = Vector3.zero;
        point.transform.localPosition += new Vector3((-graphContentRectTrans.rect.width / 2), (-graphContentRectTrans.rect.height / 2), 0);
        point.transform.localPosition += new Vector3(((graphContentRectTrans.rect.width / xScale) * xValue), graphContentRectTrans.rect.height / 2, -51);
        point.GetComponent<RectTransform>().sizeDelta = new Vector2(20, graphContentRectTrans.rect.height + slider.handleRect.sizeDelta.y);
        slider.minValue = yStart;
        slider.maxValue = yEnd;
        slider.value = yValue;
        pointsUpperThreshold.Add(xValue, slider);
        slider.onValueChanged.AddListener(ChangeUpperThresholdLineWithSlider);
        DrawThresholds();
    }

    public void AddThresholdPointLower(float xValue, float yValue) {
        GameObject point = Instantiate(graphPointPrefab, lowerThresholds.transform);
        Slider slider = point.GetComponent<Slider>();
        point.transform.localScale = Vector3.one;
        point.transform.localPosition = Vector3.zero;
        point.transform.localPosition += new Vector3((-graphContentRectTrans.rect.width / 2), (-graphContentRectTrans.rect.height / 2), 0);
        point.transform.localPosition += new Vector3(((graphContentRectTrans.rect.width / xScale) * xValue), graphContentRectTrans.rect.height / 2, -51);
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
