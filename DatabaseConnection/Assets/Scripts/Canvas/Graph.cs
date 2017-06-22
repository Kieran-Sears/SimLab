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
    private float newScrollWheel;
    private Vector2 localpoint;
    private RectTransform graphContentRectTrans;
    private LineRenderer pointLine;
    private LineRenderer thresholdLineLower;
    private LineRenderer thresholdLineUpper;

    private float mouseHold;
    private float mouseHoverOverHandleTime;
    private RaycastHit[] cursorRaycastHits;
    private bool leftMouseButtonIsDown;
    private Transform sliderHandleTransform;
    private bool cursorIsOverGraph;
    private Vector3 cursorRaycastHitPosition;
    private Vector3 newPos;
    private float previousFrameTime;
    private int counter = 0;
    private bool activateCoordinateSystem;


    #region point dragging variables
   private Slider currentlySelectedSlider;
    private int indexOfSliderMinipulated;
    private float originalSliderTime;
   private float newSliderTime;
#endregion

    #endregion

    #region Unity Methods
    private void LateUpdate() {

        #region if cursor over graph
        if (graphViewport != null && graphViewport.GetComponent<CusorSensor>().mouseOver) {
            cursorRaycastHits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), 800);

            if (Input.GetMouseButton(0)) {
                mouseHold += UnityEngine.Time.deltaTime;
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

            #region determining cursor position
            //cycle through cursor raycast
            for (int i = cursorRaycastHits.Length - 1; i >= 0; i--) {
                // get posititon to add the point
                cursorRaycastHitPosition = cursorRaycastHits[i].point;
                // if cursor is over the graph
                if (cursorRaycastHits[i].collider.tag == "Graph") {
                    cursorIsOverGraph = true;
                    // if cursor is over handle
                } else if (cursorRaycastHits[i].collider.tag == "Handle") {
                    sliderHandleTransform = cursorRaycastHits[i].transform;
                    int inde = sortedGraphPointsList.IndexOfValue(cursorRaycastHits[i].transform.parent.parent.GetComponent<Slider>());
                    if (inde != 0 || inde != sortedGraphPointsList.Count - 1) {
                        activateCoordinateSystem = true;
                        mouseHoverOverHandleTime += UnityEngine.Time.deltaTime;
                    }
                    // and the mouse has been clicked
                    if (Input.GetMouseButtonDown(0)) {
                        leftMouseButtonIsDown = true;
                        // get the point the cursor is over
                        sliderHandleTransform = cursorRaycastHits[i].transform;
                        currentlySelectedSlider = cursorRaycastHits[i].transform.parent.parent.GetComponent<Slider>();
                        // reset the time limit required to spawn the coordinate system
                        mouseHoverOverHandleTime = 0;
                    }
                } else if (cursorRaycastHits[i].collider.tag == "CoordinateSystem") {
                    activateCoordinateSystem = true;
                    mouseHoverOverHandleTime += UnityEngine.Time.deltaTime;
                    cursorIsOverGraph = false;
                    leftMouseButtonIsDown = false;
                }
            }
            #endregion

            #region point dragging
            if (leftMouseButtonIsDown) {
               // currentlySelectedSlider = sliderHandleTransform.parent.parent.GetComponent<Slider>();
                if (sortedGraphPointsList.IndexOfValue(currentlySelectedSlider) == -1) return;
                indexOfSliderMinipulated = sortedGraphPointsList.IndexOfValue(currentlySelectedSlider);
                originalSliderTime = sortedGraphPointsList.Keys[indexOfSliderMinipulated];
                if (indexOfSliderMinipulated == -1 || indexOfSliderMinipulated == 0 || indexOfSliderMinipulated == sortedGraphPointsList.Count - 1) {
                    newPos = currentlySelectedSlider.transform.position;
                } else {
                    newPos = new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, currentlySelectedSlider.transform.position.y, currentlySelectedSlider.transform.position.z);
                }
                if (graph == null) return;
                newSliderTime = (currentlySelectedSlider.transform.localPosition.x + (graph.GetComponent<RectTransform>().rect.width / 2)) / (graph.GetComponent<RectTransform>().rect.width / xScale);
                if (newSliderTime != previousFrameTime) {
                    if (indexOfSliderMinipulated != -1 && !sortedGraphPointsList.ContainsKey(newSliderTime)) {
                        if (indexOfSliderMinipulated - 1 >= 0 && indexOfSliderMinipulated + 1 < sortedGraphPointsList.Count) {
                            if (newSliderTime > sortedGraphPointsList.Keys[indexOfSliderMinipulated + 1] || newSliderTime < sortedGraphPointsList.Keys[indexOfSliderMinipulated - 1]) {
                                sortedGraphPointsList.RemoveAt(indexOfSliderMinipulated);
                                sortedGraphPointsList.Add(newSliderTime, currentlySelectedSlider);
                                print("newSliderTime: " + graphViewport + " Slider value: " + currentlySelectedSlider.value);
                                foreach (KeyValuePair<float, Slider> item in sortedGraphPointsList) {
                                    print("Index: " + sortedGraphPointsList.IndexOfKey(item.Key) + " key: " + item.Key + " Value: " + item.Value.value);
                                }
                            }
                        }
                    }
                    previousFrameTime = newSliderTime;
                }
                currentlySelectedSlider.transform.position = newPos;
                DrawLinkedPointLines();
                DrawThresholds();

            }
            #endregion

            #region Coordinate System
            if (activateCoordinateSystem) {
                if (mouseHoverOverHandleTime > 3) {
                    coordinateSystem.SetActive(true);
                    coordinateSystem.transform.localPosition = Vector3.zero;
                    coordinateSystem.transform.localScale = new Vector3(0.5f, 0.5f, 1);
                    coordinateSystem.transform.position = sliderHandleTransform.position + new Vector3(0.1f, 0.1f, 1);
                    activateCoordinateSystem = false;
                }
            } else {
                coordinateSystem.SetActive(false);
                mouseHoverOverHandleTime = 0;
            }
            #endregion

            #region delete point
            if (Input.GetMouseButtonUp(1)) {
                foreach (RaycastHit hit in cursorRaycastHits) {
                    if (hit.collider.tag == "Handle") {
                        Slider slider = hit.transform.parent.parent.GetComponent<Slider>();
                        int index = sortedGraphPointsList.IndexOfValue(slider);
                        if (index == -1 || index == 0 || index == sortedGraphPointsList.Count - 1) return;
                        sortedGraphPointsList.RemoveAt(sortedGraphPointsList.IndexOfValue(slider));
                        Destroy(slider.gameObject);
                        DrawLinkedPointLines();
                    }
                }
            }
            #endregion

        }
        #endregion

        #region even if cursor is not over graph
        // Add point to graph
        if (Input.GetMouseButtonUp(0)) {
            leftMouseButtonIsDown = false;
            if (mouseHold < 0.3 && cursorIsOverGraph && mouseHoverOverHandleTime < 1) {
                AddPoint(Camera.main.WorldToScreenPoint(cursorRaycastHitPosition));
            }
            mouseHold = 0;
        }
        #endregion

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
        if (xScale > 300) {
            xAxisLabel.GetComponent<Text>().text = "Duration (Minutes)";
            xScale /= 60;
            print(xScale);
        }

        for (int i = 0; i <= xScale; i++) {
            GameObject dashMarker = Instantiate(xDashMarkerPrefab, xAxis.transform);
            dashMarker.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            dashMarker.transform.localPosition = Vector3.zero;
            dashMarker.transform.localPosition += new Vector3((-graphContentRectTrans.rect.width / 2), 0, 1);
            dashMarker.transform.localPosition += new Vector3((((graphContentRectTrans.rect.width) / xScale) * i), 0, 1);
            dashMarker.GetComponent<Text>().text = i.ToString();
        }
    }

    private void InitialiseYScale() {
        GameObject dashMarker;
        Text text;

        for (int i = yStart; i <= yEnd; i++) {
            dashMarker = Instantiate(yDashMarkerPrefab, yAxis.transform);
            dashMarker.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            dashMarker.transform.localPosition = Vector3.zero;
            dashMarker.transform.localPosition += new Vector3(0, (-graphContentRectTrans.rect.height / 2), 1);
            dashMarker.transform.localPosition += new Vector3(0, (((graphContentRectTrans.rect.height) / yScale) * (i - yStart)), 1);
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
            lowerLineWriter = Instantiate(lineRendererPrefab, graphContent.transform);
            lowerLineWriter.transform.localScale = Vector3.one;
            lowerLineWriter.transform.localPosition = Vector3.zero;
            lowerLineWriter.name = "LowerThreshold";

            thresholdLineLower = lowerLineWriter.GetComponent<LineRenderer>();
            thresholdLineLower.startColor = Color.red;
            thresholdLineLower.endColor = Color.red;
            thresholdLineLower.startWidth = 0.1f;
            thresholdLineLower.endWidth = 0.1f;
        }

        counter = 0;
        foreach (KeyValuePair<float, Slider> item in pointsLowerThreshold) {
            thresholdLineLower.SetPosition(counter, Camera.main.WorldToScreenPoint(item.Value.handleRect.transform.position) - Camera.main.WorldToScreenPoint(graphContent.transform.position));
            counter++;
        }


        if (thresholdLineUpper == null) {
            if (graph == null) return;
            upperLineWriter = Instantiate(lineRendererPrefab, graphContent.transform);
            upperLineWriter.transform.localScale = Vector3.one;
            upperLineWriter.transform.localPosition = Vector3.zero;
            upperLineWriter.name = "UpperThreshold";

            thresholdLineUpper = upperLineWriter.GetComponent<LineRenderer>();
            thresholdLineUpper.startColor = Color.red;
            thresholdLineUpper.endColor = Color.red;
            thresholdLineUpper.startWidth = 0.1f;
            thresholdLineUpper.endWidth = 0.1f;
        }

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
        xAxisContent.GetComponent<RectTransform>().sizeDelta = new Vector2(graphContentRectTrans.rect.width, xAxisContent.GetComponent<RectTransform>().sizeDelta.y);


        if (xScale >= 180) {
            for (int i = 0; i < xScale; i++) {
                if (i % (20) == 0) {
                    xAxis.transform.GetChild(i).gameObject.SetActive(true);
                    grid.transform.GetChild(i).GetComponent<LineRenderer>().enabled = true;
                } else {
                    xAxis.transform.GetChild(i).gameObject.SetActive(false);
                    grid.transform.GetChild(i).GetComponent<LineRenderer>().enabled = false;
                }
            }
        } else if (xScale >= 60) {
            for (int i = 0; i < xScale; i++) {
                if (i % (10) == 0) {
                    xAxis.transform.GetChild(i).gameObject.SetActive(true);
                    grid.transform.GetChild(i).GetComponent<LineRenderer>().enabled = true;
                } else {
                    xAxis.transform.GetChild(i).gameObject.SetActive(false);
                    grid.transform.GetChild(i).GetComponent<LineRenderer>().enabled = false;
                }
            }

        } else if (xScale >= 30) {
            for (int i = 0; i < xScale; i++) {
                if (i % (2) == 0) {
                    xAxis.transform.GetChild(i).gameObject.SetActive(true);
                    grid.transform.GetChild(i).GetComponent<LineRenderer>().enabled = true;
                } else {
                    xAxis.transform.GetChild(i).gameObject.SetActive(false);
                    grid.transform.GetChild(i).GetComponent<LineRenderer>().enabled = false;
                }
            }
        } else {
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
        yAxisContent.GetComponent<RectTransform>().sizeDelta = new Vector2(yAxisContent.GetComponent<RectTransform>().sizeDelta.x, graphContentRectTrans.rect.height);
        int diff = 0;


        if (yStart % 10 != 0) {
            diff = 10 - (yStart % 10);
        }

        for (int i = 0; i < yScale; i++) {
            if (i % 10 == diff) {
                yAxis.transform.GetChild(i).gameObject.SetActive(true);
                grid.transform.GetChild(xScale + i + 1).GetComponent<LineRenderer>().enabled = true;
            } else {
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

    private static Vector3[] MakeSmoothCurve(Vector3[] arrayToCurve, float smoothness) {
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

        foreach (KeyValuePair<float, Slider> item in sortedGraphPointsList) {
            arrayToCurve[counter] = Camera.main.WorldToScreenPoint(item.Value.handleRect.transform.position) - Camera.main.WorldToScreenPoint(graphContent.transform.position);
            counter++;
        }

        MakeSmoothCurve(arrayToCurve, 30);
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
            pointLine.SetPosition(i, arrayToCurve[i]);
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
            pointLine.SetPosition(counter, arrayToCurve[i]);
        }
    }
    #endregion

    #region Public Methods
    public void GenerateGraph(int _xStart, int _xEnd, string xLabel, int _yStart, int _yEnd, string yLabel) {
        print("generating graph " + name);
        graphContentRectTrans = graphContent.GetComponent<RectTransform>();
        //graphContentRectTrans.sizeDelta = new Vector2(graphViewport.GetComponent<RectTransform>().rect.x * -2, graphViewport.GetComponent<RectTransform>().rect.y * -2);
        graphContentRectTrans.sizeDelta = new Vector2(xAxis.GetComponent<RectTransform>().rect.x * -2, yAxis.GetComponent<RectTransform>().rect.y * -2);

        graphContentRectTrans.localPosition = Vector3.zero;
        graph.GetComponent<BoxCollider>().size = new Vector2(graphContentRectTrans.rect.width, graphContentRectTrans.rect.height);
        coordinateSystem.GetComponent<BoxCollider>().size = new Vector3(coordinateSystem.GetComponent<RectTransform>().rect.width * 2, coordinateSystem.GetComponent<RectTransform>().rect.height * 2, 1);

        yAxisLabel.transform.localPosition += Vector3.left * 50;
        xAxisLabel.transform.localPosition += Vector3.down * 50;

        yAxisLabel.GetComponent<Text>().text = yLabel;
        xAxisLabel.GetComponent<Text>().text = xLabel;

        xStart = _xStart;
        xEnd = _xEnd;
        yStart = _yStart;
        yEnd = _yEnd;
        xScale = _xEnd - _xStart;
        yScale = _yEnd - _yStart;

        duration = xScale;

        InitialiseXScale();
        InitialiseYScale();

        DrawGrid();

        LayoutXScale();
        LayoutYScale();
     
        xAxisScrollRect.horizontalNormalizedPosition = 0;
        xAxisScrollRect.scrollSensitivity = 0;
        yAxisScrollRect.verticalNormalizedPosition = 0;
        yAxisScrollRect.scrollSensitivity = 0;
        GraphScrollRect.scrollSensitivity = 0;
        GraphScrollRect.onValueChanged.AddListener(ListenerMethod);
        GraphScrollRect.normalizedPosition = new Vector2(0, 0);

    }

    public void ListenerMethod(Vector2 value) {
        xAxisScrollRect.normalizedPosition = GraphScrollRect.normalizedPosition;
        yAxisScrollRect.normalizedPosition = GraphScrollRect.normalizedPosition;
    }

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
        point.transform.position = new Vector3(worldPoint.x, point.transform.position.y, 50);
        slider.value = (((slider.maxValue - slider.minValue) / 100) * percent);
        float pointTime = (slider.transform.localPosition.x + (graph.GetComponent<RectTransform>().rect.width / 2)) / (graph.GetComponent<RectTransform>().rect.width / xScale);
        HierachyPositionSearch(point);
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
        //  RectTransform rectTrans = graph.GetComponent<RectTransform>();
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
        //  RectTransform rectTrans = graph.GetComponent<RectTransform>();
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

    public void SetCoordinateValue() {
        float x = -1;
        float y = -1;
        float.TryParse(coordinateX.text, out x);
        float.TryParse(coordinateY.text, out y);
        if (coordinateX.text.Length != 0 && coordinateY.text.Length != 0) {
            if ((x > xStart && x < xEnd) && (y > yStart && y < yEnd)) {
                activateCoordinateSystem = false;
                mouseHoverOverHandleTime = 0;
                coordinateSystem.SetActive(false);
                sortedGraphPointsList.RemoveAt(sortedGraphPointsList.IndexOfValue(sliderHandleTransform.parent.parent.GetComponent<Slider>()));
                Destroy(sliderHandleTransform.gameObject);
                AddPoint(x, y);
                coordinateX.text = "";
                coordinateY.text = "";
                DrawLinkedPointLines();
            } else {
                Error.instance.informMessageText.text = "Ensure coordinates are within range limits";
                Error.instance.informPanel.SetActive(true);
                Error.instance.informOkButton.onClick.AddListener(SelectCoordinateXValue);
            }
        }
    }

    public void SelectCoordinateXValue() {
        coordinateX.Select();
        coordinateX.ActivateInputField();
        Error.instance.informOkButton.onClick.RemoveAllListeners();
    }

    #endregion



}
