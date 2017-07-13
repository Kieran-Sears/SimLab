using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpFromView : MonoBehaviour {



    public delegate void OnEnd();
    public static OnEnd onEnd;

    public float speed = 10;
    public Transform arrowIcon;
    public Transform startPos;
    public Transform endPos;
    public Transform frame;
    public Transform background;

    private float startTime;
    private Vector3 originalRotation;

    private float distanceStartEnd;
    private float journeyFraction;
    private float currentDuration;
    private float framePercentSize;

    bool moveIn = false;
    bool moveOut = false;


    public void OnDisable() {
        distanceStartEnd = Vector3.Distance(startPos.position, endPos.position);
        framePercentSize = frame.GetComponent<RectTransform>().rect.width / GetComponent<RectTransform>().rect.width;
        background.GetComponent<RectTransform>().sizeDelta += new Vector2(GetComponent<RectTransform>().rect.width * framePercentSize, 0);
        if (name == "Left Lerp Panel") {
            background.position -= new Vector3(distanceStartEnd / 2, 0, 0);
        }
        if (name == "Right Lerp Panel") {
            background.position += new Vector3(distanceStartEnd / 2, 0, 0);
        }
        if (onEnd != null) {
            onEnd();
        }
    }

    public void OnEnable() {
        distanceStartEnd = Vector3.Distance(startPos.position, endPos.position);
        framePercentSize = frame.GetComponent<RectTransform>().rect.width / GetComponent<RectTransform>().rect.width;
        background.GetComponent<RectTransform>().sizeDelta -= new Vector2(GetComponent<RectTransform>().rect.width * framePercentSize, 0);
        if (name == "Left Lerp Panel") {
            background.position += new Vector3(distanceStartEnd / 2, 0, 0);
        }
        if (name == "Right Lerp Panel") {
            background.position -= new Vector3(distanceStartEnd / 2, 0, 0);
        }
        if (onEnd != null) {
            onEnd();
        }
    }

    public void Start() {
        originalRotation = arrowIcon.eulerAngles;
        frame.position = startPos.position;
    }

    void Update() {
        currentDuration = Time.time - startTime;
        journeyFraction = currentDuration / distanceStartEnd * speed;

        if (moveIn) {
            frame.position = Vector3.Lerp(endPos.position, startPos.position, journeyFraction);
            if (frame.position == startPos.position) {
                if (name == "Left Lerp Panel") {
                    background.GetComponent<RectTransform>().sizeDelta -= new Vector2(GetComponent<RectTransform>().rect.width * framePercentSize, 0);
                    background.position += new Vector3(distanceStartEnd / 2, 0, 0);
                }
                if (name == "Right Lerp Panel") {
                    background.GetComponent<RectTransform>().sizeDelta -= new Vector2(GetComponent<RectTransform>().rect.width * framePercentSize, 0);
                    background.position -= new Vector3(distanceStartEnd / 2, 0, 0);
                }
                arrowIcon.eulerAngles = originalRotation;
                if (onEnd != null) {
                    onEnd();
                }
                print("moved in");
                moveIn = false;
            }
        }

        if (moveOut) {
            frame.position = Vector3.Lerp(startPos.position, endPos.position, journeyFraction);
            if (frame.position == endPos.position) {
                if (name == "Left Lerp Panel") {
                    background.GetComponent<RectTransform>().sizeDelta += new Vector2(GetComponent<RectTransform>().rect.width * framePercentSize, 0);
                    background.position -= new Vector3(distanceStartEnd / 2, 0, 0);
                }
                if (name == "Right Lerp Panel") {
                    background.GetComponent<RectTransform>().sizeDelta += new Vector2(GetComponent<RectTransform>().rect.width * framePercentSize, 0);
                    background.position += new Vector3(distanceStartEnd / 2, 0, 0);
                }
                arrowIcon.eulerAngles = new Vector3(0, 0, originalRotation.z + 180);
                if (onEnd != null) {
                    onEnd();
                }
                print("moved out");
                moveOut = false;
            }
        }
    }

    public void ToggleHide() {
        startTime = Time.time;
        if (frame.position == startPos.position) {
            moveOut = true;
        }
        if (frame.position == endPos.position) {
            moveIn = true;
        }

    }


}
