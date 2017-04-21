using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Error : MonoBehaviour {

    public static Error instance { get; private set; }

    public GameObject panelError;

    private void Awake() {
        if (instance) {
            DestroyImmediate(this);
        } else {
            instance = this;
        }
    }

    public void DisplayMessage(string message) {
        panelError.GetComponentInChildren<Text>().text = message;
        panelError.SetActive(true);
    }
}
