﻿using UnityEngine;

public class LerpFromView : MonoBehaviour {

    public delegate void OnEnd();
    public static OnEnd onEnd;

    public float speed = 15;
    public Transform arrowIcon;
    public Transform frame;
    public Transform content;
    public Transform background;

    private float startTime;

    private Vector3 originalRotation;
    private Vector2 resize;
    private Vector3 startPos;
    private Vector3 endPos;

    private float distanceStartEnd;
    private float journeyFraction;
    private float currentDuration;
    private float framePercentSize;
  

    bool moveIn = false;
    bool moveOut = false;

    int count;

    public void Start() {
        startPos = frame.localPosition;
        endPos =  (Vector3.left * content.GetComponent<RectTransform>().rect.width);
        distanceStartEnd = Vector3.Distance(startPos, endPos);
        originalRotation = arrowIcon.eulerAngles;
        framePercentSize = frame.GetComponent<RectTransform>().rect.width / GetComponent<RectTransform>().rect.width;
        resize = new Vector2(GetComponent<RectTransform>().rect.width * framePercentSize, 0);
    }

    public void OnDisable() {
        background.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        background.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
        background.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
        background.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
        if (onEnd != null) {
            onEnd();
        }
    }

    public void OnEnable() {
        background.GetComponent<RectTransform>().anchorMin = new Vector2(0.2f, 0);
        background.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
        background.GetComponent<RectTransform>().offsetMin = new Vector2(0,0);
        background.GetComponent<RectTransform>().offsetMax = new Vector2(0,0);
        if (onEnd != null) {
            onEnd();
        }
    }

    void FixedUpdate() {
       
      

        if (moveIn) {
            currentDuration = Time.time - startTime;
            journeyFraction = (currentDuration / distanceStartEnd) * speed;
            frame.localPosition = Vector3.Lerp(endPos, startPos, journeyFraction * speed);
            if (frame.localPosition == startPos) { 
                background.GetComponent<RectTransform>().anchorMin = new Vector2(0.2f, 0);
                background.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
                arrowIcon.eulerAngles = originalRotation;
                if (onEnd != null) {
                    onEnd();
                }
                moveIn = false;
            }
        }

        if (moveOut) {
            currentDuration = Time.time - startTime;
            journeyFraction = (currentDuration / distanceStartEnd) * speed;
            frame.localPosition = Vector3.Lerp(startPos, endPos, journeyFraction * speed);
            if (frame.localPosition == endPos) {
                background.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
                background.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);

                arrowIcon.eulerAngles = new Vector3(0, 0, originalRotation.z + 180);
                if (onEnd != null) {
                    onEnd();
                }
                moveOut = false;
            }
        }
    }


    public void ToggleHide() {
        startTime = Time.time;
        print(frame.localPosition);
        print("start " + startPos + " end " + endPos);
        if (frame.localPosition == startPos) {
            print("moving out");
            count = 0;
            moveOut = true;
        }
        if (frame.localPosition == endPos) {
            print("moving in");
            moveIn = true;
        }

    }


}
