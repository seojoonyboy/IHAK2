namespace DataModules{
    [System.Serializable]
    public class Magnification {
        public string key;          //타입명
        public int lv = 1;          //현재 배율 레벨 (1부터 시작)
        public uint foodCost;       //업그레이드 비용(식량)
        public uint goldCost;       //업그레이드 비용(골드)
        public float magnfication;   //최종 배율
    }
}
