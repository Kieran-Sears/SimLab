using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class Graph : MonoBehaviour {

    public GameObject graph;
    public GameObject overlay;
    public GameObject xAxis;
    public GameObject yAxis;

    public GameObject xDashMarkerPrefab;
    public GameObject yDashMarkerPrefab;
    public GameObject graphPointPrefab;
    public GameObject lineRendererPrefab;


    private GameObject[] points;
    private GameObject[,] gridLines;
    private Vector2 size;

    public void Update() {
        if (Input.GetKeyDown(KeyCode.A)) {
            GenerateGraph(1, 11, 1, 10);
        }
        //if (Input.GetKeyDown(KeyCode.Mouse0)) {
        //    //   AddPoint();
        //    if (Input.GetMouseButtonDown(0)) {
        //        PointerEventData pointerData = new PointerEventData(EventSystem.current);

        //        pointerData.position = Input.mousePosition;

        //        RaycastHit[] hits;
        //        hits = Physics.RaycastAll(transform.position, transform.forward, 100.0F);

        //        if (hits.Length > 0) {
        //            for (int i = 0; i < hits.Length; i++) {


        //                if (hits[i]..gameObject.layer == LayerMask.NameToLayer("Graph")) {
        //                    string dbg = "Element: {0}";
        //                     Debug.Log(string.Format(dbg, hits[i].gameObject.name));

        //                    hits.Clear();
        //                }
        //            }
        //        }
        //    }
        //}
    }





    public void Start() {
        size = graph.GetComponent<RectTransform>().sizeDelta;
    }

    public void GenerateGraph(int xStart, int xEnd, int yStart, int yEnd) {
        LayoutXScale(xStart, xEnd);
        LayoutYScale(yStart, yEnd);
        GenerateGrid(xStart, xEnd, yStart, yEnd);
    }

    public void LayoutXScale(int xStart, int xEnd) {
        // xAxis.AddComponent<HorizontalLayoutGroup>();
        int x = xEnd - xStart;

        for (int i = 1; i <= x; i++) {
            GameObject dashMarker = Instantiate(xDashMarkerPrefab);
            dashMarker.transform.SetParent(xAxis.transform);
            dashMarker.transform.localScale = Vector3.one;
            dashMarker.transform.localPosition = new Vector3(((size.x / x) * i) - (size.x / 2), 0, 0);
            // dashMarker.transform.localPosition = new Vector3(((size.x / x) * i), -10, 0);
            dashMarker.GetComponent<Text>().text = i.ToString();
        }
    }

    public void LayoutYScale(int yStart, int yEnd) {
        // yAxis.AddComponent<VerticalLayoutGroup>();
        int y = yEnd - yStart;

        for (int i = 1; i <= y; i++) {
            GameObject dashMarker = Instantiate(yDashMarkerPrefab);
            dashMarker.transform.SetParent(yAxis.transform);
            dashMarker.transform.localScale = Vector3.one;
            dashMarker.transform.localPosition = new Vector3(0, ((size.y / y) * i) - (size.y / 2), 0);
            dashMarker.GetComponent<Text>().text = i.ToString();
        }
    }

    public void GenerateGrid(int xStart, int xEnd, int yStart, int yEnd) {

        int x = xEnd - xStart;
        int y = yEnd - yStart;


        Vector2 size = graph.GetComponent<RectTransform>().sizeDelta;
        GameObject dashMarker;
        LineRenderer lineRenderer;

        for (int i = 0; i <= x; i++) {

            dashMarker = Instantiate(lineRendererPrefab);
            dashMarker.transform.SetParent(overlay.transform);
            dashMarker.transform.localScale = Vector3.one;
            dashMarker.transform.localPosition = new Vector3(0, 0, 0);

            lineRenderer = dashMarker.GetComponent<LineRenderer>();
            lineRenderer.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            lineRenderer.startWidth = 0.01f;
            lineRenderer.endWidth = 0.01f;

            

            lineRenderer.SetPosition(0, new Vector3(((size.x / x) * i) * 100 - (size.x / 2) * 100, -size.y / 2 * 100, 0));
            lineRenderer.SetPosition(1, new Vector3(((size.x / x) * i) * 100 - (size.x / 2) * 100, size.y / 2 * 100, 0));


            for (int n = 0; n <= y; n++) {
                dashMarker = Instantiate(lineRendererPrefab);
                dashMarker.transform.SetParent(overlay.transform);
                dashMarker.transform.localScale = Vector3.one;
                dashMarker.transform.localPosition = new Vector3(0, 0, 0);

                lineRenderer = dashMarker.GetComponent<LineRenderer>();
                lineRenderer.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                lineRenderer.startWidth = 0.01f;
                lineRenderer.endWidth = 0.01f;
                lineRenderer.SetPosition(0, new Vector3(-size.x / 2 * 100, ((size.y / y) * n) * 100 - (size.y / 2) * 100, 0));
                lineRenderer.SetPosition(1, new Vector3(size.x / 2 * 100, ((size.y / y) * n) * 100 - (size.y / 2) * 100, 0));

            }

        }
    }

    public void AddPoint() {
        Vector3 screenPoint = Input.mousePosition;

        Debug.Log(screenPoint + " / \n" +
            graph.transform.position.x + "\n" +
            (graph.transform.position.x + size.x) + "\n" +
            graph.transform.position.y + "\n" +
            (graph.transform.position.y + size.y));

        if (
            (screenPoint.x > graph.transform.position.x - (size.x / 2) && screenPoint.x < graph.transform.position.x + (size.x / 2))
            &&
            (screenPoint.y > graph.transform.position.y - (size.y / 2) && screenPoint.y < graph.transform.position.y + (size.y / 2))
            ) {

            screenPoint.z = 1.0f; //distance of the plane from the camera
            GameObject point = Instantiate(graphPointPrefab);
            point.transform.SetParent(graph.transform);
            point.transform.localScale = Vector3.one;
            point.transform.position = Camera.main.ScreenToWorldPoint(screenPoint);
        }
    }

    public void AddPoint(float xValue, float yValue) {
        Debug.Log(xValue);
        float x = Mathf.Round((size.x / 100) * xValue) * 20;
        float y = Mathf.Round((size.y / 100) * yValue) * 20;

        GameObject point = Instantiate(graphPointPrefab, graph.transform);

        //float posX = float xPos = ((size.x / x) * i) * 100 - (size.x / 2) * 100;
        //float yPos = -size.y / 2 * 100;

        point.transform.localPosition =  new Vector3(((-size.x /2) + x), ((-size.y /2) + y), 0);// Camera.main.ScreenToWorldPoint(graphOrigin + new Vector3(x, y, 0));
       

    }

    public void DrawLines() { }

    public void DrawFailStateThresholds() { }


}
