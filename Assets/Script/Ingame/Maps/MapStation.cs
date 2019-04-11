using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapStation : MapNode {
	[SerializeField] protected MapRoad[] roads;
	public List<Vector3> mapStationList;
	
	public MapStation Search(EnumMapPosition destination) {
		for(int i = 0; i < roads.Length; i++) {
			if(usedRoad.Contains(roads[i].mapPostion)) continue;
			//도착지가 길일 경우
			if(roads[i].CheckDestination(destination)) {
				MapStation goal = roads[i].NextNode(mapPostion);
				goal.SetList(mapStationList);
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

	public void SetList(List<Vector3> list) {
		mapStationList = new List<Vector3>(list);
		mapStationList.Add(transform.position);
	}
}
