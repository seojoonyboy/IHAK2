using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapNode : MonoBehaviour {
    public EnumMapPosition mapPostion;
    protected static Queue<MapStation> searchQueue;
    protected static List<EnumMapPosition> usedRoad;

    public static List<Vector3> SearchPosition(MapStation currentStation, EnumMapPosition destination) {
        searchQueue = new Queue<MapStation>();
        usedRoad = new List<EnumMapPosition>();
        List<MapStation> queueDrop = new List<MapStation>();

        searchQueue.Enqueue(currentStation);
        currentStation.mapStationList.Add(currentStation.transform.position);

        MapStation found = null;
        while (searchQueue.Count != 0) {
            MapStation next = searchQueue.Dequeue();
            queueDrop.Add(next);
            found = next.Search(destination);
            if (found != null) break;
        }

        MapStation[] stations = FindObjectsOfType<MapStation>();
        List<Vector3> result = new List<Vector3>(found.mapStationList);
        foreach (MapStation station in stations) station.mapStationList = new List<Vector3>();
        return result;
    }

    public bool CheckDestination(EnumMapPosition destination) {
        return mapPostion == destination;
    }
}

public enum EnumMapPosition {
    S00 = 0, S01,
    S10 = 10, S11, S12,
    S20 = 20, S21,

    R00 = 100,
    R10 = 110, R11, R12, R13,
    R20 = 120, R21, R22, R23,
    R30 = 130
}
