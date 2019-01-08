using DataModules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class BuildingImages : MonoBehaviour {
    [SerializeField] Sprite[] 
        product_images,
        military_images,
        special_images;

    private Dictionary<int, Sprite> images;

    /// <summary>
    /// Image를 카드의 Id값에 Mapping
    /// </summary>
    public void SetImages() {
        images = new Dictionary<int, Sprite>();
        var categories = (Building.Category[])Enum.GetValues(typeof(Building.Category));
        foreach(Building.Category category in categories) {
            var lists = GetComponent<DataManager>().GetBuildingObjects(category);
            for (int i = 0; i < lists.Count; i++) {
                Card card = lists[i].GetComponent<BuildingObject>().data;
                switch (category) {
                    case Building.Category.MILITARY:
                        try {
                            images[card.Id] = military_images[i];
                        }
                        catch (System.IndexOutOfRangeException e) {
                            images[card.Id] = military_images[0];
                        }
                        break;
                    case Building.Category.PRODUCT:
                        try {
                            images[card.Id] = product_images[i];
                        }
                        catch (System.IndexOutOfRangeException e) {
                            images[card.Id] = product_images[0];
                        }
                        break;
                    case Building.Category.SPECIAL:
                        try {
                            images[card.Id] = special_images[i];
                        }
                        catch (System.IndexOutOfRangeException e) {
                            images[card.Id] = special_images[0];
                        }
                        break;
                }
            }
        }
    }

    public Sprite GetImage(int id) {
        return images[id];
    }
}
