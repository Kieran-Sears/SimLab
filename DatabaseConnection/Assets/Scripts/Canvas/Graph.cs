﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class Graph : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    #region Public Variables
    public GameObject graph;
    public GameObject grid;
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
    #endregion

    #region Private Variables
    private GameObject[] points;
    private GameObject[,] gridLines;

    private int xScale;
    private int yScale;
    private int xStart;
    private int xEnd;
    private int yStart;
    private int yEnd;

    private Rect gridRect;

    private bool onObj = false;
    private float scrollWheel = 1;
    private float newScrollWheel;
    private Vector2 localpoint;
    private RectTransform graphContentRectTrans;
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
            // TODO prevent the unit markers from scaling their text when zooming
            //for (int i = 0; i < xAxisContent.transform.GetChild(0).childCount; i++) {
            //    xAxisContent.transform.GetChild(0).GetChild(i).localScale = xAxisContent.transform.localScale - Vector3.right;
            //}
            //for (int i = 0; i < yAxisContent.transform.GetChild(0).childCount; i++) {
            //    yAxisContent.transform.GetChild(0).GetChild(i).localScale = xAxisContent.transform.localScale - Vector3.up;
            //}
        }
    }

    private void LayoutXScale() {
        xAxisContent.GetComponent<RectTransform>().sizeDelta = new Vector2(gridRect.width, xAxisContent.GetComponent<RectTransform>().sizeDelta.y);
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
                    if ( (i + 1) % 2 == 0) {
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
            dashMarker.transform.localPosition += new Vector3((-gridRect.width / 2), 0, 1);
            dashMarker.transform.localPosition += new Vector3(((gridRect.width / xScale) * i), 0, 1);
            dashMarker.GetComponent<Text>().text = i.ToString();
        }
    }

    private void LayoutYScale() {
        yAxisContent.GetComponent<RectTransform>().sizeDelta = new Vector2(yAxisContent.GetComponent<RectTransform>().sizeDelta.x, gridRect.height);
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
            dashMarker.transform.localPosition += new Vector3(0, (-gridRect.height / 2), 1);
            dashMarker.transform.localPosition += new Vector3(0, ((gridRect.height / yScale) * i), 1);
            Text text = dashMarker.GetComponent<Text>();
            text.text = (i + yStart).ToString();
            text.fontSize = 1;
        }
    }

    private void DrawGrid() {
        Vector2 size = new Vector2(gridRect.width, gridRect.height);
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

    private void DrawThresholds() { }
    #endregion

    #region Public Methods
    public void GenerateGrid(int _xStart, int _xEnd, int _yStart, int _yEnd) {
        graphContentRectTrans = graphContent.GetComponent<RectTransform>();
        graphContentRectTrans.sizeDelta = new Vector2(GraphScrollRect.GetComponent<RectTransform>().rect.x * -2, GraphScrollRect.GetComponent<RectTransform>().rect.y * -2);
        graphContentRectTrans.localPosition = Vector3.zero;
        gridRect = graphContent.GetComponent<RectTransform>().rect;
        graph.GetComponent<BoxCollider>().size = new Vector2(gridRect.width, gridRect.height);
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
        RectTransform rectTrans = graph.GetComponent<RectTransform>();
        GameObject point = Instantiate(graphPointPrefab, graph.transform);
        Slider slider = point.GetComponent<Slider>();

        point.transform.localPosition -= new Vector3(0, rectTrans.rect.height / 2, 0);
        float minValue = point.transform.position.y;
        float currentValue = worldPoint.y - minValue;

        point.transform.localPosition += new Vector3(0, rectTrans.rect.height, 0);
        float maxValue = point.transform.position.y - minValue;
        float percent = (currentValue / maxValue) * 100;

        point.transform.localScale = Vector3.one;
        point.GetComponent<RectTransform>().sizeDelta = new Vector2(20, rectTrans.rect.height );
        point.transform.position = new Vector3(worldPoint.x, 0, 1);
        slider.minValue = yStart;
        slider.maxValue = yEnd;
        slider.value = ((slider.maxValue - slider.minValue) / 100) * percent + slider.minValue;
    }

    public void AddPoint(float xValue, float yValue) {
        RectTransform rectTrans = graph.GetComponent<RectTransform>();
        GameObject point = Instantiate(graphPointPrefab, graph.transform);
        Slider slider = point.GetComponent<Slider>();
        point.transform.localScale = Vector3.one;
        point.transform.localPosition = Vector3.zero;
        point.transform.localPosition += new Vector3((-gridRect.width / 2), (-gridRect.height / 2), 0);
        point.transform.localPosition += new Vector3(((gridRect.width / xScale) * xValue), gridRect.height / 2, -1);
        point.GetComponent<RectTransform>().sizeDelta = new Vector2(20, gridRect.height + slider.handleRect.sizeDelta.y);
        slider.minValue = yStart;
        slider.maxValue = yEnd;
        slider.value = yValue;
    }
    #endregion



}
