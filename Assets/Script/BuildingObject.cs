using UnityEngine;
using DataModules;
using Spine.Unity;
public class BuildingObject : MonoBehaviour {
    [SerializeField] public Card card;
    public Sprite icon;
    public Sprite mainSprite;
    public Sprite upgradeIcon;
    public SkeletonDataAsset spine;
    public int setTileLocation;
}
