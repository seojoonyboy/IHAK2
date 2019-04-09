using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WarCry : Buff {
    public SpriteRenderer range_texture;
    public int duration;

    public void Init(string[] data) {
        //string[] args = data.Split(',');
        //int duration = 0;
        //int.TryParse(args[0], out duration);

        //float attackSpeed = 0;
        //float.TryParse(args[1], out attackSpeed);

        //int attackPower = 0;
        //int.TryParse(args[2], out attackPower);

        //int moveSpeed = 0;
        //int.TryParse(args[3], out moveSpeed);

        //this.attackSpeed = -attackSpeed;
        //power = attackPower;
        //this.duration = duration;
        //moveSpeed_percentage = moveSpeed;
    }

    public void StartBuff() {
        StartCoroutine(Buff(1, duration));
    }

    IEnumerator Buff(float interval, int loopCount) {
        int count = 0;
        while (count < loopCount) {
            count++;
            yield return new WaitForSeconds(interval);
        }
        Destroy(GetComponent<WarCry>());
    }

    public struct Data {
        public int moveSpeed;           //이동속도 증가량(%)
        public int attackPower;         //공격력 증가량(%)
        public float attackSpeed;       //공격속도 감소량(초)
        public int duration;            //지속시간
    }
}
