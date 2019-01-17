using DataModules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(ConstructManager))]
public class BuildingImages : MonoBehaviour {
    [SerializeField]
    public Sprite[]
        buildingImages,
        buildingIcons;

    public Sprite
        defaultImage,
        defaultIcon;

    public void SetImages() {
        ConstructManager dataManager = GetComponent<ConstructManager>();
        var lists = dataManager.GetBuildingObjects();
        foreach (GameObject obj in lists) {
            BuildingObject bo = obj.GetComponent<BuildingObject>();
            bo.mainSprite = GetImage(bo.data.card.id);
            bo.icon = GetIcon(bo.data.card.id);

            obj.GetComponent<SpriteRenderer>().sprite = bo.mainSprite;
        }
    }

    public Sprite GetImage(string id) {
        foreach (Sprite sprite in buildingImages) {
            if (sprite.name == id) {
                return sprite;
            }
        }
        return defaultImage;
    }

    public Sprite GetIcon(string id) {
        foreach (Sprite sprite in buildingIcons) {
            if (sprite.name == id) {
                return sprite;
            }
        }
        return defaultImage;
    }
}
