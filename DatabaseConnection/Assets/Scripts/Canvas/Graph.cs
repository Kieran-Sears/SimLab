using UnityEngine;
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
    public ScrollRect scrollRectGraph;
    public ScrollRect scrollRectXAxis;
    public ScrollRect scrollRectYAxis;

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
    #endregion

    #region Inspector fields
    [SerializeField]
    float startSize = 1;
    [SerializeField]
    float minSize = 0.1f;
    [SerializeField]
    float maxSize = 10;

    [SerializeField]
    private float zoomRate = 1f;
    #endregion

    #region Unity Methods

    private void Update() {
        scrollWheel += Input.GetAxis("Mouse ScrollWheel");

        if (onObj && scrollWheel >= 1f) {
            SetZoom(scrollWheel);
        } else if (scrollWheel < 1f) {
            scrollWheel = 1f;
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
    private void ChangeZoom(float scrollWheel) {
        float rate = 1 + zoomRate * UnityEngine.Time.deltaTime;
        if (scrollWheel > 0) {
            SetZoom(Mathf.Clamp(transform.localScale.y / rate, minSize, maxSize));
        } else {
            SetZoom(Mathf.Clamp(transform.localScale.y * rate, minSize, maxSize));
        }
    }

    private void SetZoom(float targetSize) {
        if (graph != null) {
            graphContent.transform.localScale = new Vector3(targetSize, targetSize, 1);
            xAxisContent.transform.localScale = new Vector3(targetSize, 1, 1);
            yAxisContent.transform.localScale = new Vector3(1, targetSize, 1);

            for (int i = 0; i < xAxisContent.transform.childCount; i++) {
                xAxisContent.transform.GetChild(i).localScale = Vector3.one;
            }
            for (int i = 0; i < yAxisContent.transform.childCount; i++) {
                yAxisContent.transform.GetChild(i).localScale = Vector3.one;
            }
        }
    }

    private void LayoutXScale() {
        xAxisContent.GetComponent<RectTransform>().sizeDelta = new Vector2(gridRect.width, xAxisContent.GetComponent<RectTransform>().sizeDelta.y);
        for (int i = 1; i <= xScale; i++) {
            GameObject dashMarker = Instantiate(xDashMarkerPrefab, xAxis.transform);
            dashMarker.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            dashMarker.transform.localPosition = Vector3.zero;
            dashMarker.transform.localPosition += new Vector3((-gridRect.width / 2), 0, 1);
            dashMarker.transform.localPosition += new Vector3(((gridRect.width / xScale) * i), 0, 1);
            dashMarker.GetComponent<Text>().text = i.ToString();
        }
        scrollRectXAxis.verticalNormalizedPosition = 1;
        scrollRectXAxis.scrollSensitivity = 0;
    }

    private void LayoutYScale() {
        yAxisContent.GetComponent<RectTransform>().sizeDelta = new Vector2(yAxisContent.GetComponent<RectTransform>().sizeDelta.x, gridRect.height);
        float scaleRatio = 0;
        for (int i = 1; i <= yScale; i++) {
            if (yScale < 20) {
                scaleRatio = 2;
            } else if (yScale > 20 && yScale < 40) {
                scaleRatio = 4;
            } else {
                scaleRatio = 10;
            }
            if ((i + 1) % scaleRatio == 0) {
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
        scrollRectYAxis.verticalNormalizedPosition = 0;
        scrollRectYAxis.scrollSensitivity = 0;
    }

    private void DrawGrid() {
        Vector2 size = new Vector2(gridRect.width, gridRect.height);
        GameObject dashMarker;
        LineRenderer lineRenderer;

        for (int i = 0; i <= xScale; i++) {
            dashMarker = Instantiate(lineRendererPrefab);
            dashMarker.transform.SetParent(grid.transform);
            dashMarker.transform.localScale = Vector3.one;
            dashMarker.transform.localPosition = Vector3.zero;

            lineRenderer = dashMarker.GetComponent<LineRenderer>();
            lineRenderer.transform.localScale = new Vector3(1f, 1f, 1f);
            lineRenderer.startWidth = 0.01f;
            lineRenderer.endWidth = 0.01f;
            lineRenderer.SetPosition(0, new Vector3((((size.x / xScale) * i) - (size.x / 2)), (-size.y / 2), 0));
            lineRenderer.SetPosition(1, new Vector3((((size.x / xScale) * i) - (size.x / 2)), (size.y / 2), 0));

            for (int n = 0; n <= yScale; n++) {
                dashMarker = Instantiate(lineRendererPrefab);
                dashMarker.transform.SetParent(grid.transform);
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
        graphContent.GetComponent<RectTransform>().sizeDelta = new Vector2(scrollRectGraph.GetComponent<RectTransform>().rect.x * -2, scrollRectGraph.GetComponent<RectTransform>().rect.y * -2) ;
        gridRect = graphContent.GetComponent<RectTransform>().rect;
        graph.GetComponent<BoxCollider>().size = new Vector2(gridRect.width, gridRect.height);
        xStart = _xStart;
        xEnd = _xEnd;
        yStart = _yStart;
        yEnd = _yEnd;
        xScale = _xEnd - _xStart;
        yScale = _yEnd - _yStart;
        LayoutXScale();
        LayoutYScale();
        DrawGrid();
        scrollRectGraph.scrollSensitivity = 0;
        scrollRectGraph.onValueChanged.AddListener(ListenerMethod);
        scrollRectGraph.normalizedPosition = new Vector2(0,0);

    }


    public void ListenerMethod(Vector2 value) {
        scrollRectXAxis.normalizedPosition = scrollRectGraph.normalizedPosition;
        scrollRectYAxis.normalizedPosition = scrollRectGraph.normalizedPosition;
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
        point.GetComponent<RectTransform>().sizeDelta = new Vector2(20, rectTrans.rect.height + slider.handleRect.sizeDelta.y);
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
