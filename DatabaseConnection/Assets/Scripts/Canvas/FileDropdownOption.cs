using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FileDropdownOption : MonoBehaviour {

  
    public int GetIndex() {
      return  transform.GetSiblingIndex() - 1;
    }
}
