using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapRoad : MapNode {
	[SerializeField] protected MapStation[] stations = new MapStation[2];

	public MapStation NextNode(EnumMapPosition station) {
		if(stations[0].mapPostion == station) return stations[1];
		else return stations[0];
	}
}
