using UnityEngine;
using DataModules;
using Spine.Unity;
public class BuildingObject : MonoBehaviour {
    [SerializeField] public Building data;
    public Sprite icon;
    public Sprite mainSprite;
    public SkeletonDataAsset spine;
    public int setTileLocation;
}
