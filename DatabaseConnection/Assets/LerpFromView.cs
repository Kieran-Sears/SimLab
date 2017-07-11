using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpFromView : MonoBehaviour {

    float startTime;
    public float speed = 10;
    public Transform arrowIcon;
    public Transform startPos;
    public Transform endPos;
    public Transform frame;

    float distanceStartEnd;
    float journeyFraction;
    float currentDuration;

    bool moveIn = false;
    bool moveOut = false;

    public void Start() {
        arrowIcon.rotation = Quaternion.Euler(0, 0, 180);
        frame.position = startPos.position;
        startTime = Time.time;
        distanceStartEnd = Vector3.Distance(startPos.position, endPos.position);
    }

    void Update() {
        currentDuration = Time.time - startTime;
        journeyFraction = currentDuration / distanceStartEnd * speed;

        if (moveIn) {
            frame.position = Vector3.Lerp(endPos.position, startPos.position, journeyFraction);
            if (frame.position == startPos.position) {
                arrowIcon.rotation = Quaternion.Euler(0, 0, 180);
                print("moved in");    
                moveIn = false;
            }
        }

        if (moveOut) {
            frame.position = Vector3.Lerp(startPos.position, endPos.position, journeyFraction);
            if (frame.position == endPos.position) {
                print("moved out");
                moveOut = false;
            }
        }
    }

    public void ToggleHide() {
        startTime = Time.time;
        if (frame.position == startPos.position) {
            arrowIcon.rotation = Quaternion.Euler(0, 0, 0);
            moveOut = true;
        }
        if (frame.position == endPos.position) {
            moveIn = true;
        }
       
    }


}
