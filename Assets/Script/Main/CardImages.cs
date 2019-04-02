using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardImages : MonoBehaviour {
    public Sprite[]
        primal_unit_cardImages,
        primal_spell_cardImages;

    public Sprite defaultImage;

    public Sprite GetImage(string race, string type, string id) {
        Sprite[] sprites;
        switch (race) {
            case "primal":
                if (type == "unit") {
                    sprites = primal_unit_cardImages;
                }
                else if (type == "spell") {
                    sprites = primal_spell_cardImages;
                }
                else sprites = new Sprite[0];
                foreach(Sprite sprite in sprites) {
                    if(sprite.name == id) {
                        return sprite;
                    }
                }
                break;
        }
        return defaultImage;
    }
}
