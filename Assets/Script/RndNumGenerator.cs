using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RndNumGenerator : MonoBehaviour {
    public static int[] getRandomInt(int length, int min, int max) {
        int[] rndArray = new int[length];
        bool isSame;

        for(int i=0; i<length; ++i) {
            while (true) {
                rndArray[i] = Random.Range(min, max);
                isSame = false;

                for(int j=0; j<i; ++j) {
                    if(rndArray[j] == rndArray[i]) {
                        isSame = true;
                        break;
                    }
                }
                if (!isSame) break;
            }
        }
        return rndArray;
    }
}
