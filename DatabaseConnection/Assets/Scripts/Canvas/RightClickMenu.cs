using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightClickMenu : MonoBehaviour {

    public static RightClickMenu instance { get; private set; }

    public GameObject child;

    private void Start() {
        if (instance) {
            DestroyImmediate(this);
        }
        else {
            instance = this;
        }
    }

    public void SetMenuPosition(Vector3 position) {
        Vector3 pos = Camera.main.ScreenToWorldPoint(position);
        child.SetActive(true);
        transform.localPosition = Vector3.zero;
        transform.position = Vector3.zero;
        transform.localScale = Vector3.one;
        transform.position = new Vector3(pos.x, pos.y, 0);
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, - 900);
    }

    public void PopulateOptions() {

    }
}
