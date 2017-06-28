using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDrag : MonoBehaviour {



    private float offsetX;
    private float offsetY;


    public void BeginDrag() {
        offsetX = transform.position.x - Camera.main.ScreenToWorldPoint(Input.mousePosition).x;
        offsetY = transform.position.y - Camera.main.ScreenToWorldPoint(Input.mousePosition).y;

    }

    public void OnDrag() {
        transform.position = new Vector3(offsetX + Camera.main.ScreenToWorldPoint(Input.mousePosition).x, offsetY + Camera.main.ScreenToWorldPoint(Input.mousePosition).y, 100);
    }
}
