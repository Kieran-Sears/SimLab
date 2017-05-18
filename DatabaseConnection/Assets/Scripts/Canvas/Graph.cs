using UnityEngine;
using UnityEngine.UI;

public class Graph : MonoBehaviour {

    public GameObject container;
    public GameObject graph;
    public GameObject grid;
    public GameObject xAxis;
    public GameObject yAxis;

    public GameObject xDashMarkerPrefab;
    public GameObject yDashMarkerPrefab;
    public GameObject graphPointPrefab;
    public GameObject lineRendererPrefab;


    private GameObject[] points;
    private GameObject[,] gridLines;

    private int xScale;
    private int yScale;

    private int xStart;
    private int xEnd;
    private int yStart;
    private int yEnd;

    public void LateUpdate() {
        var d = Input.GetAxis("Mouse ScrollWheel");
        if (d > 0f) {
            // scroll up
        } else if (d < 0f) {
            // scroll down
        }
    }

    public void GenerateGrid(int _xStart, int _xEnd, int _yStart, int _yEnd) {
        graph.GetComponent<BoxCollider>().size = new Vector2(graph.GetComponent<RectTransform>().rect.width, graph.GetComponent<RectTransform>().rect.height);
        xStart = _xStart;
        xEnd = _xEnd;
        yStart = _yStart;
        yEnd = _yEnd;
        xScale = _xEnd - _xStart;
        yScale = _yEnd - _yStart;
        LayoutXScale();
        LayoutYScale();
        DrawGrid();
    }

    public void LayoutXScale() {
        for (int i = 1; i <= xScale; i++) {
            GameObject dashMarker = Instantiate(xDashMarkerPrefab);
            dashMarker.transform.SetParent(xAxis.transform);
            dashMarker.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            dashMarker.transform.localPosition = Vector3.zero;
            dashMarker.transform.localPosition += new Vector3((-grid.GetComponent<RectTransform>().rect.width / 2), 0, 1);
            dashMarker.transform.localPosition += new Vector3(((grid.GetComponent<RectTransform>().rect.width / xScale) * i), 0, 1);
            dashMarker.GetComponent<Text>().text = i.ToString();
        }
    }

    public void LayoutYScale() {
        for (int i = 1; i <= yScale; i++) {
            GameObject dashMarker = Instantiate(yDashMarkerPrefab);
            dashMarker.transform.SetParent(yAxis.transform);
            dashMarker.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            dashMarker.transform.localPosition = Vector3.zero;
            dashMarker.transform.localPosition += new Vector3(0, (-grid.GetComponent<RectTransform>().rect.height / 2), 1);
            dashMarker.transform.localPosition += new Vector3(0, ((grid.GetComponent<RectTransform>().rect.height / yScale) * i), 1);
            Text text = dashMarker.GetComponent<Text>();
            text.text = (i + yStart).ToString();
            text.fontSize = 1;
        }
    }

    public void DrawGrid() {
        Vector2 size = new Vector2(graph.GetComponent<RectTransform>().rect.width, graph.GetComponent<RectTransform>().rect.height);
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
        point.transform.localPosition += new Vector3((-rectTrans.rect.width / 2), (-rectTrans.rect.height / 2), 0);
        point.transform.localPosition += new Vector3(((rectTrans.rect.width / xScale) * xValue), rectTrans.rect.height / 2, -1);
        point.GetComponent<RectTransform>().sizeDelta = new Vector2(20, rectTrans.rect.height + slider.handleRect.sizeDelta.y);
        slider.minValue = yStart;
        slider.maxValue = yEnd;
        slider.value = yValue;
    }

    public void DrawThresholds() { }


}
