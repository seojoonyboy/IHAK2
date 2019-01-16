using System.Collections;
using System.Collections.Generic;
using DataModules;
using UnityEngine;

public class IngameCityManager : MonoBehaviour {

    class BuildingsInfo {
        public int id;
        public bool activate;
        public int hp;
        public int maxHp;
        public Card cardInfo;
    }

    private Deck deck;
    private List<BuildingsInfo> buildingsInfo = new List<BuildingsInfo>();
    public int[] buildingList;

    // Use this for initialization
    void Start () {
        deck = AccountManager.Instance.decks[0];
        buildingList = deck.coordsSerial;
        BuildingsInfo bi = new BuildingsInfo();
        for (int i = 0; i < deck.coordsSerial.Length - 1; i++) {
            bi.id = deck.coordsSerial[i];
            bi.activate = true;
            bi.cardInfo = this.transform.GetChild(0).GetChild(i).GetChild(0).GetComponent<BuildingObject>().data.card;
            bi.hp = bi.maxHp = bi.cardInfo.hitPoint;
        }
    }
	
}
