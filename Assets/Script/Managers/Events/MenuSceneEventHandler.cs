using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSceneEventHandler : Singleton<MenuSceneEventHandler>, IEventHanlder {
    protected MenuSceneEventHandler() { }

    private Dictionary<EVENT_TYPE, List<OnEvent>> Listeners = new Dictionary<EVENT_TYPE, List<OnEvent>>();

    public void AddListener(Enum Event_Type, OnEvent Listener) {
        //List of listeners for this event
        List<OnEvent> ListenList = null;

        //New item to be added. Check for existing event type key. If one exists, add to list
        if (Listeners.TryGetValue((EVENT_TYPE)Event_Type, out ListenList)) {
            //List exists, so add new item
            ListenList.Add(Listener);
            return;
        }

        //Otherwise create new list as dictionary key
        ListenList = new List<OnEvent>();
        ListenList.Add(Listener);
        Listeners.Add((EVENT_TYPE)Event_Type, ListenList); //Add to internal listeners list
    }

    public void PostNotification(Enum Event_Type, Component Sender, object Param = null) {
        //Notify all listeners of an event

        //List of listeners for this event only
        List<OnEvent> ListenList = null;

        //If no event entry exists, then exit because there are no listeners to notify
        if (!Listeners.TryGetValue((EVENT_TYPE)Event_Type, out ListenList))
            return;

        //Entry exists. Now notify appropriate listeners
        for (int i = 0; i < ListenList.Count; i++) {
            if (!ListenList[i].Equals(null)) //If object is not null, then send message via interfaces
                ListenList[i](Event_Type, Sender, Param);
        }
    }

    public void RemoveEvent(Enum Event_Type) {
        //Remove entry from dictionary
        Listeners.Remove((EVENT_TYPE)Event_Type);
    }

    public void RemoveListener(Enum Event_Type, OnEvent Listener) {
        List<OnEvent> ListenList = null;

        if (!Listeners.TryGetValue((EVENT_TYPE)Event_Type, out ListenList))
            return;

        ListenList.Remove(Listener);
    }

    public enum EVENT_TYPE {
        NONE,
        DECKLIST_CHANGED,
        SET_TILE_OBJECTS_COMPLETED
    }
}
