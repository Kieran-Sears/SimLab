using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowManager : MonoBehaviour {

    public static WindowManager instance { get; private set; }

    public GameObject login;
    public GameObject condition;
         public GameObject drug;
         public GameObject visualise;
         public GameObject error;
         public GameObject save;

    private void Awake() {
        if (instance) {
            DestroyImmediate(this);
        }
        else {
            instance = this;
        }
    }
}
