using UnityEngine;
using DataModules;

public class TileObject : MonoBehaviour {

    public int tileNum;
    Vector3 Grid2DLocation;
    public bool buildingSet = false;
    public int Tier;

    [SerializeField]  public Coord location;
    [HideInInspector] public int row, col;
}
