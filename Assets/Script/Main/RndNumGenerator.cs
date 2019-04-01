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

    public static int[] getRandomInt(int length, int[] pool) {
        int[] rndArray = new int[length];
        bool isSame;
        for (int i = 0; i < length; ++i) {
            while (true) {
                int index = Random.Range(0, pool.Length - 1);
                rndArray[i] = pool[index];
                isSame = false;

                for (int j = 0; j < i; ++j) {
                    if (rndArray[j] == rndArray[i]) {
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
