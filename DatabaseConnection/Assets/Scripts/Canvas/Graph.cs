using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

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
    private Vector2 size;
    private int xScale;
    private int yScale;

    public void Start() {
        size = grid.GetComponent<RectTransform>().rect.size;
    }

    public void GenerateGrid(int xStart, int xEnd, int yStart, int yEnd) {
        xScale = xEnd - xStart;
        yScale = yEnd - yStart;
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
            text.text = i.ToString();
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

    public void Update() {
        if (Input.GetMouseButton(0)) {
            Vector3 screenPoint = Input.mousePosition;
            AddPoint();
        }
    }

    public void AddPoint() {
        Debug.Log( "Graph " + graph.transform.position);
        Vector3 screenPoint = Input.mousePosition;
        Debug.Log("screenPoint " + screenPoint);
        if (
            (screenPoint.x > graph.transform.position.x - (size.x / 2) && screenPoint.x < graph.transform.position.x + (size.x / 2))
            &&
            (screenPoint.y > graph.transform.position.y - (size.y / 2) && screenPoint.y < graph.transform.position.y + (size.y / 2))
            ) {
         //   Debug.Log(screenPoint);
            screenPoint.z = 1.0f; //distance of the plane from the camera
            GameObject point = Instantiate(graphPointPrefab);
            point.transform.SetParent(graph.transform);
            point.transform.localScale = Vector3.one;
            point.transform.position = Camera.main.ScreenToWorldPoint(screenPoint);
        }
    }

    public void AddPoint(float xValue, float yValue) {
        RectTransform rectTrans = graph.GetComponent<RectTransform>();
        GameObject point = Instantiate(graphPointPrefab, graph.transform);
        point.transform.localPosition = Vector3.zero;
        // move to origin
        point.transform.localPosition += new Vector3((-rectTrans.rect.width / 2), (-rectTrans.rect.height / 2), 0);
        Debug.Log(((rectTrans.rect.width / xScale) * xValue));
        Debug.Log("rectTrans.rect.width " + rectTrans.rect.width + " xScale " + xScale + " xValue " + xValue + "   : on Graph " + gameObject.name);
        point.transform.localPosition += new Vector3(((rectTrans.rect.width / xScale) * xValue), (rectTrans.rect.height / 100) * yValue, 0);
    }

    public void DrawThresholds() { }


}
