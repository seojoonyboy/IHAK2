using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapStation : MapNode {
    public enum NodeDirection {
        RightUp = 0,
        Right = 1,
        RighDown = 2,
        LeftDown = 3,
        Left = 4,
        LeftUp = 5,
        Up = 6,
        Down = 7,
    }

	[SerializeField] protected MapRoad[] roads;
    //[SerializeField] public List<NodeDirection> adjNodes;
    [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
    [SerializeField] public Dictionary<NodeDirection, MapNode> adjNodes = new Dictionary<NodeDirection, MapNode>();
    public List<Vector3> mapStationList;

    public Dictionary<NodeDirection, MapNode> AdjNodes {
        get { return adjNodes; }
    }

    public MapStation Search(EnumMapPosition destination) {
		for(int i = 0; i < roads.Length; i++) {
			if(usedRoad.Contains(roads[i].mapPostion)) continue;
			//도착지가 길일 경우
			if(roads[i].CheckDestination(destination)) {
				MapStation goal = roads[i].NextNode(mapPostion);
				goal.SetListLastRoad(mapStationList);
				return goal;
			}
			//도착지가 진지일 경우
			MapStation next = roads[i].NextNode(mapPostion);
			next.SetList(mapStationList);
			if(next.CheckDestination(destination)) {
				return next;
			}
			//둘 다 못 찾을 경우
			searchQueue.Enqueue(next);
			usedRoad.Add(roads[i].mapPostion);
		}
		return null;
	}

	public List<MapStation> AdjacentStation() {
		List<MapStation> stations = new List<MapStation>();
		for(int i = 0; i < roads.Length; i++) {
			stations.Add(roads[i].NextNode(mapPostion));
		}
		return stations;
	}

	public void SetList(List<Vector3> list) {
		if(mapStationList.Count != 0 && mapStationList.Count <= list.Count) return;
		mapStationList = new List<Vector3>(list);
		mapStationList.Add(transform.position);
	}

	public void SetListLastRoad(List<Vector3> list) {
		mapStationList = new List<Vector3>(list);
		mapStationList.Add(transform.position);
	}
}
