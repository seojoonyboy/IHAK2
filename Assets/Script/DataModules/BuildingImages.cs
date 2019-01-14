using DataModules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(ConstructManager))]
public class BuildingImages : MonoBehaviour {
    [SerializeField]
    Sprite[]
        product_images,
        product_icons,
        military_images,
        military_icons,
        special_images,
        special_icons,
        total_images,
        total_icons;

    private Dictionary<int, Sprite> images;

    /// <summary>
    /// Image를 카드의 Id값에 Mapping
    /// </summary>
    public void SetImages() {
        images = new Dictionary<int, Sprite>();
        ConstructManager dataManager = GetComponent<ConstructManager>();
        string[] categories = new string[] { "prod", "military", "special" };
        foreach(string category in categories) {
            var lists = dataManager.GetBuildingObjects(category);
            for (int i = 0; i < lists.Count; i++) {
                BuildingObject buildingObject = lists[i].GetComponent<BuildingObject>();
                Card card = buildingObject.data.card;
                switch (category) {
                    case "military":
                        try {
                            images[card.id] = military_images[i];
                            buildingObject.mainSprite = images[card.id];
                            buildingObject.icon = military_icons[i];
                        }
                        catch (System.IndexOutOfRangeException e) {
                            images[card.id] = military_images[0];
                            buildingObject.mainSprite = images[0];
                            buildingObject.icon = military_icons[0];
                        }
                        lists[i].GetComponent<SpriteRenderer>().sprite = buildingObject.mainSprite;
                        break;
                    case "prod":
                        try {
                            images[card.id] = product_images[i];
                            buildingObject.mainSprite = images[card.id];
                            buildingObject.icon = product_icons[i];
                        }
                        catch (System.IndexOutOfRangeException e) {
                            images[card.id] = product_images[0];
                            buildingObject.mainSprite = images[0];
                            buildingObject.icon = product_icons[0];
                        }
                        lists[i].GetComponent<SpriteRenderer>().sprite = buildingObject.mainSprite;
                        break;
                    case "special":
                        try {
                            images[card.id] = special_images[i];
                            buildingObject.mainSprite = images[card.id];
                            buildingObject.icon = special_icons[i];
                        }
                        catch (System.IndexOutOfRangeException e) {
                            images[card.id] = special_images[0];
                            buildingObject.mainSprite = images[0];
                            buildingObject.icon = special_icons[0];
                        }
                        lists[i].GetComponent<SpriteRenderer>().sprite = buildingObject.mainSprite;
                        break;
                }
            }
        }
    }

    public Sprite GetImage(int id) {
        return images[id];
    }
}
