using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using DataModules;

namespace AccountData {
    public class UserData : MonoBehaviour {
        [SerializeField]
        public List<List<int>> deckData;
        [SerializeField]
        public List<int> selectDeck;
    }
}