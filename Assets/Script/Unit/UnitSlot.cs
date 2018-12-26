using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitSlot : MonoBehaviour, IDropHandler {

    public GameObject unit {
        get {
            if (transform.childCount > 0)
                return transform.GetChild(0).gameObject;

            return null;
        }        
    }

    public void OnDrop (PointerEventData eventData) {

        if(!unit) {
            DragHandler.unit.transform.SetParent(transform);
            //ExecuteEvents.ExecuteHierarchy<IHasChanged>(gameObject, null, (x, y) => x.HasChanged());
        }        
    }	
}
