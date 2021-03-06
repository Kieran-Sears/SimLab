﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;



[Serializable]
public class VitalsData {
    public List<VitalsDatum> data;
}


public class NetworkManager : MonoBehaviour {


    public static NetworkManager Instance { get; private set; }

    private void Awake() {
        if (Instance) {
            DestroyImmediate(this);
        } else {
            Instance = this;
        }
    }

    //public void Upload(Vitals data) {

    //    WWWForm form = new WWWForm();
    //    foreach (Vital item in data.vitalList) {
    //        form.AddField(item.name, item.value.ToString());
    //        form.AddField(item.name, item.value.ToString());
    //        form.AddField(item.name, item.value.ToString());
    //    }
     

    //    UnityWebRequest www = UnityWebRequest.Post("http://localhost:8084/VisualAnalytics/VisualAnalyticsServlet", form);
    //    www.Send();

    //    if (www.isError) {
    //        Debug.Log(www.error);
    //    } else {
    //        Debug.Log("Form upload complete!");
    //    }
    //}

    public Vital Download() {
        WWW www = new WWW("http://localhost:8084/VisualAnalytics/VisualAnalyticsServlet");
        StartCoroutine(WaitForRequest(www));
        while (!www.isDone) { }
        Vital vital = JsonUtility.FromJson<Vital>(www.text);      
        return vital;
    }

    IEnumerator WaitForRequest(WWW www) {
        yield return www;

        // check for errors
        if (www.error == null) {
            Debug.Log("WWW Ok!: " + www.text);
        } else {
            Debug.Log("WWW Error: " + www.error);
        }
    }

}
