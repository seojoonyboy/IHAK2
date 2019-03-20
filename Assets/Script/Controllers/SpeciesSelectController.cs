using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeciesSelectController : MonoBehaviour {
    private int index;
    public int Index {
        get { return index; }
        set {
            index = value;
            OnChangeSpecies();
        }
    }

    public void ToggleModal(bool toggle) {
        gameObject.SetActive(toggle);
    }

    private void OnChangeSpecies() {

    }

    public void UnableToSelect() {
        Modal.instantiate("아직 준비중입니다.", Modal.Type.CHECK);
    }
}
