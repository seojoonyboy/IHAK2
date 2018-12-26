using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UnitEdit : MonoBehaviour, IHasChanged {
    [SerializeField] Transform slots;
    [SerializeField] Text inventroyText;


    void Start() {
        HasChanged();
    }

    public void HasChanged() {
        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        builder.Append(" - ");
        foreach(Transform slotTransform in slots) {
            GameObject unit = slotTransform.GetComponent<UnitSlot>().unit;
            if (unit) {
                builder.Append(unit.name);
                builder.Append(" - ");
            }
        }
        inventroyText.text = builder.ToString();
    }	
}


namespace UnityEngine.EventSystems {
    public interface IHasChanged : IEventSystemHandler {
        void HasChanged();
    }
}