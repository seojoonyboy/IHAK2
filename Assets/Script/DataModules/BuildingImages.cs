using DataModules;
using UnityEngine;
using Spine.Unity;

[RequireComponent(typeof(ConstructManager))]
public class BuildingImages : MonoBehaviour {
    [SerializeField]
    //원시도시 이외의 종족 추가예정
    public Sprite[]
        primal_product_buildingImages,
        primal_other_buildingImages,
        primal_product_buildingIcons,
        primal_other_buildingIcons,
        primal_upgrade_buildingIcons;
    
    public SkeletonDataAsset[] 
        primal_product_buildingSpines,
        primal_other_buildingSpines;

    public Sprite
        defaultImage,
        defaultIcon;

    public void SetImages() {
        ConstructManager dataManager = GetComponent<ConstructManager>();
        var lists = dataManager.GetBuildingObjects();
        foreach (GameObject obj in lists) {
            BuildingObject bo = obj.GetComponent<BuildingObject>();
            Card card = bo.data.card;
            bo.mainSprite = GetImage(card.race, card.type, card.id);
            bo.icon = GetIcon(card.race, card.type, card.id);
            bo.upgradeIcon = GetImage(card.race, "upgrade", card.id);
            bo.spine = GetSpine(card.race, card.type, card.id);

            if(bo.spine == null) { 
                SpriteRenderer sprite = obj.AddComponent<SpriteRenderer>();
                sprite.sprite = bo.mainSprite;
            }
            else {
                obj.AddComponent<SkeletonAnimation>();
                obj.AddComponent<TileSpineAnimation>();
            }
        }
    }

    public Sprite GetImage(string race, string type, string id) {
        Sprite[] sprites;
        switch (race) {
            case "primal" :
                if (type == "prod") sprites = primal_product_buildingImages;
                else if (type == "upgrade") sprites = primal_upgrade_buildingIcons;
                else sprites = primal_other_buildingImages;
                foreach (Sprite sprite in sprites) {
                    if (sprite.name == id) {
                        Debug.Log(sprite.name);
                        return sprite;
                    }
                }
                break;
        }
        return defaultImage;
    }

    public SkeletonDataAsset GetSpine(string race, string type, string id) {
        SkeletonDataAsset[] spines;
        switch (race) {
            case "primal" :
                if (type == "prod") spines = primal_product_buildingSpines;
                else  spines = primal_other_buildingSpines;
                foreach (SkeletonDataAsset spine in spines) {
                    if (spine.name == id) {
                        spine.GetSkeletonData(false);
                        return spine;
                    }
                }
                break;
        }
        return null;
    }

    public Sprite GetIcon(string race, string type, string id) {
        Sprite[] sprites;
        switch (race) {
            case "primal":
                if (type == "prod") sprites = primal_product_buildingIcons;
                else sprites = primal_other_buildingIcons;
                foreach (Sprite sprite in sprites) {
                    if (sprite.name == id) {
                        return sprite;
                    }
                }
                break;
        }
        return defaultImage;
    }
}
