using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;




public class Graph : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {




    #region Public Variables
    public GameObject graph;
    public GameObject grid;
    public GameObject thresholds;
    public GameObject xAxis;
    public GameObject yAxis;
    public GameObject graphContent;
    public GameObject xAxisContent;
    public GameObject yAxisContent;
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

    public SortedList<float, Slider> points = new SortedList<float, Slider>();
    public SortedList<float, Slider> pointsUpperThreshold = new SortedList<float, Slider>();
    public SortedList<float, Slider> pointsLowerThreshold = new SortedList<float, Slider>();
    #endregion

    #region Private Variables
    private int xScale;
    private int yScale;
    private int xStart;
    private int xEnd;
    private int yStart;
    private int yEnd;

    private bool onObj = false;
    private float scrollWheel = 1;
    private float newScrollWheel;
    private Vector2 localpoint;
    private RectTransform graphContentRectTrans;
    private LineRenderer pointLine;
    private LineRenderer thresholdLineLower;
    private LineRenderer thresholdLineUpper;

    private float mouseHold;
    private RaycastHit[] hits;
    private bool cursorOverHandle;
    private Transform handleTrans;
    private bool addPoint;
    private Vector3 hitPoint;
    private float previousFrameTime;
    private int counter = 0;
    #endregion

    #region Unity Methods
    private void Update() {
        newScrollWheel = scrollWheel + Input.GetAxis("Mouse ScrollWheel");

        if (onObj && scrollWheel != newScrollWheel) {
            scrollWheel = newScrollWheel;
            if (scrollWheel >= 1f) {
                SetZoom(scrollWheel);
            } else if (scrollWheel < 1f) {
                scrollWheel = 1f;
            }
        }

        if (Input.GetMouseButton(0)) {
            mouseHold += UnityEngine.Time.deltaTime;
        }

        if (Input.GetMouseButtonDown(0)) {
            hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), 1000);
            foreach (RaycastHit hit in hits) {
                if (hit.collider.tag == "Handle") {
                    cursorOverHandle = true;
                    handleTrans = hit.transform;
                } else if (hit.collider.tag == "Graph") {
                    addPoint = true;
                    hitPoint = hit.point;
                }
            }
        }

        // prevent overlap of graph point line
        if (cursorOverHandle) {
            Slider slider = handleTrans.parent.parent.GetComponent<Slider>();
            if (points.IndexOfValue(slider) == -1) return;
            int indexOfSliderMinipulated = points.IndexOfValue(slider);
            float originalSliderTime = points.Keys[indexOfSliderMinipulated];
            Vector3 newPos = new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, handleTrans.parent.parent.transform.position.y, handleTrans.parent.parent.transform.position.z);
            if (graph == null) return;
            float newSliderTime = (slider.transform.localPosition.x + (graph.GetComponent<RectTransform>().rect.width / 2)) / (graph.GetComponent<RectTransform>().rect.width / xScale);
            if (newSliderTime != previousFrameTime) {
                if (indexOfSliderMinipulated != -1 && !points.ContainsKey(newSliderTime)) {
                    if (indexOfSliderMinipulated - 1 >= 0 && indexOfSliderMinipulated + 1 < points.Count) {
                        if (newSliderTime > points.Keys[indexOfSliderMinipulated + 1] || newSliderTime < points.Keys[indexOfSliderMinipulated - 1]) {
                            points.RemoveAt(indexOfSliderMinipulated);
                            points.Add(newSliderTime, slider);           
                        } 
                    }        
                }
                previousFrameTime = newSliderTime;
            }
            // TODO update the slider key for slider being minipulated in points
            handleTrans.parent.parent.transform.position = newPos;
            DrawLinkedPointLines();
            DrawThresholds();
        }

        // Add point to graph
        if (Input.GetMouseButtonUp(0)) {
            cursorOverHandle = false;
            if (mouseHold < 0.3 && addPoint) {
                AddPoint(Camera.main.WorldToScreenPoint(hitPoint));
            }
            mouseHold = 0;
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        onObj = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
        onObj = false;
    }

    public void OnDisable() {
        onObj = false;
    }

    public void OnEnable() {
        DrawLinkedPointLines();
        DrawThresholds();
    }
    #endregion

    #region Private Methods
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

            LayoutYScale();
            LayoutXScale();

            pointLine.transform.localScale = new Vector3(1 / targetSize, 1 / targetSize, 1);


            for (int i = 0; i < points.Count; i++) {
                pointLine.SetPosition(i, (Camera.main.WorldToScreenPoint(points.Values[i].handleRect.position) - Camera.main.WorldToScreenPoint(graphContent.transform.position)));        
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
        if (xAxis.transform.childCount > 25) {
            for (int i = 0; i < xAxis.transform.childCount; i++) {
                xAxis.transform.GetChild(i).gameObject.SetActive(false);
                if (graphContent.transform.localScale.x < 1.4f) {
                    if ((i + 1) % 10 == 0) {
                        xAxis.transform.GetChild(i).gameObject.SetActive(true);
                    }
                } else if (graphContent.transform.localScale.x >= 1.4f && graphContent.transform.localScale.x < 2f) {
                    if ((i + 1) % 5 == 0) {
                        xAxis.transform.GetChild(i).gameObject.SetActive(true);
                    }
                } else {
                    if ((i + 1) % 2 == 0) {
                        xAxis.transform.GetChild(i).gameObject.SetActive(true);
                    }
                }
            }
        }
    }

    private void InitialiseXScale() {
        for (int i = 1; i <= xScale; i++) {
            GameObject dashMarker = Instantiate(xDashMarkerPrefab, xAxis.transform);
            dashMarker.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            dashMarker.transform.localPosition = Vector3.zero;
            dashMarker.transform.localPosition += new Vector3((-graphContentRectTrans.rect.width / 2), 0, 1);
            dashMarker.transform.localPosition += new Vector3(((graphContentRectTrans.rect.width / xScale) * i), 0, 1);
            dashMarker.GetComponent<Text>().text = i.ToString();
        }
    }

    private void LayoutYScale() {
        yAxisContent.GetComponent<RectTransform>().sizeDelta = new Vector2(yAxisContent.GetComponent<RectTransform>().sizeDelta.x, graphContentRectTrans.rect.height);
        if (yAxis.transform.childCount > 25) {
            for (int i = 0; i < yAxis.transform.childCount; i++) {
                yAxis.transform.GetChild(i).gameObject.SetActive(false);
                if (graphContent.transform.localScale.y < 1.4f) {
                    if ((i + 2) % 10 == 0) {
                        yAxis.transform.GetChild(i).gameObject.SetActive(true);
                    }
                } else if (graphContent.transform.localScale.y >= 1.4f && graphContent.transform.localScale.y < 2f) {
                    if ((i + 2) % 5 == 0) {
                        yAxis.transform.GetChild(i).gameObject.SetActive(true);
                    }
                } else {
                    if ((i + 2) % 2 == 0) {
                        yAxis.transform.GetChild(i).gameObject.SetActive(true);
                    }
                }
            }
        }
    }

    private void InitialiseYScale() {
        for (int i = 1; i <= yScale; i++) {
            GameObject dashMarker = Instantiate(yDashMarkerPrefab, yAxis.transform);
            dashMarker.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            dashMarker.transform.localPosition = Vector3.zero;
            dashMarker.transform.localPosition += new Vector3(0, (-graphContentRectTrans.rect.height / 2), 1);
            dashMarker.transform.localPosition += new Vector3(0, ((graphContentRectTrans.rect.height / yScale) * i), 1);
            Text text = dashMarker.GetComponent<Text>();
            text.text = (i + yStart).ToString();
            text.fontSize = 1;
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

    private void ChangeLinkedPointLineWithSlider(float value) {
        Slider slider = EventSystem.current.currentSelectedGameObject.GetComponent<Slider>();
        float yPos = (Camera.main.WorldToScreenPoint(slider.handleRect.position) - Camera.main.WorldToScreenPoint(graph.transform.position / graphContent.transform.localScale.y)).y / graphContent.transform.localScale.y;
        Vector3 pos = new Vector3(pointLine.GetPosition(points.IndexOfValue(slider)).x, yPos, -1);
        pointLine.SetPosition(points.IndexOfValue(slider), pos);
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
        Vector3[] arrayToCurve = new Vector3[points.Count];

        counter = 0;

        foreach (KeyValuePair<float, Slider> item in points) {
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
        pointLine.numPositions = points.Count;

        for (int i = 0; i < points.Count; i++) {
            pointLine.SetPosition(i, arrayToCurve[i]);
        }
    }

    private void DrawLinkedPointLineUpperThreshold() {
        GameObject LineWriter;
        Vector3[] arrayToCurve = new Vector3[points.Count];

        int counter = 0;
        foreach (KeyValuePair<float, Slider> item in points) {
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
        pointLine.numPositions = points.Count;

        for (int i = 0; i < points.Count; i++) {
            pointLine.SetPosition(i, arrayToCurve[i]);
        }
    }

    private void DrawLinkedPointLinesLowerThreshold() {
        GameObject LineWriter;
        Vector3[] arrayToCurve = new Vector3[points.Count];

        int counter = 0;
        foreach (KeyValuePair<float, Slider> item in points) {
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
        pointLine.numPositions = points.Count;

        for (int i = 0; i < points.Count; i++) {
            pointLine.SetPosition(counter, arrayToCurve[i]);
        }
    }
    #endregion

    #region Public Methods
    public void GenerateGrid(int _xStart, int _xEnd, int _yStart, int _yEnd) {
        graphContentRectTrans = graphContent.GetComponent<RectTransform>();
        graphContentRectTrans.sizeDelta = new Vector2(GraphScrollRect.GetComponent<RectTransform>().rect.x * -2, GraphScrollRect.GetComponent<RectTransform>().rect.y * -2);
        graphContentRectTrans.localPosition = Vector3.zero;
        graph.GetComponent<BoxCollider>().size = new Vector2(graphContentRectTrans.rect.width, graphContentRectTrans.rect.height);
        xStart = _xStart;
        xEnd = _xEnd;
        yStart = _yStart;
        yEnd = _yEnd;
        xScale = _xEnd - _xStart;
        yScale = _yEnd - _yStart;
        InitialiseXScale();
        LayoutXScale();
        xAxisScrollRect.verticalNormalizedPosition = 1;
        xAxisScrollRect.scrollSensitivity = 0;
        InitialiseYScale();
        LayoutYScale();
        yAxisScrollRect.verticalNormalizedPosition = 0;
        yAxisScrollRect.scrollSensitivity = 0;
        DrawGrid();
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
        point.transform.position = new Vector3(worldPoint.x, point.transform.position.y, 1);
        slider.value = (((slider.maxValue - slider.minValue) / 100) * percent);
        float pointTime = (slider.transform.localPosition.x + (graph.GetComponent<RectTransform>().rect.width / 2)) / (graph.GetComponent<RectTransform>().rect.width / xScale);
        HierachyPositionSearch(point);
        points.Add(pointTime, slider);
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
        RectTransform rectTrans = graph.GetComponent<RectTransform>();
        GameObject point = Instantiate(graphPointPrefab, graph.transform);
        Slider slider = point.GetComponent<Slider>();
        point.transform.localScale = Vector3.one;
        point.transform.localPosition = Vector3.zero;
        point.transform.localPosition += new Vector3((-graphContentRectTrans.rect.width / 2), (-graphContentRectTrans.rect.height / 2), 0);
        point.transform.localPosition += new Vector3(((graphContentRectTrans.rect.width / xScale) * xValue), graphContentRectTrans.rect.height / 2, -1);
        point.GetComponent<RectTransform>().sizeDelta = new Vector2(20, graphContentRectTrans.rect.height + slider.handleRect.sizeDelta.y);
        slider.minValue = yStart;
        slider.maxValue = yEnd;
        slider.value = yValue;
        points.Add(xValue, slider);
        slider.onValueChanged.AddListener(ChangeLinkedPointLineWithSlider);
        DrawLinkedPointLines();
    }

    public void AddThresholdPointUpper(float xValue, float yValue) {
        RectTransform rectTrans = graph.GetComponent<RectTransform>();
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
        RectTransform rectTrans = graph.GetComponent<RectTransform>();
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
    #endregion



}
