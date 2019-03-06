namespace DataModules{
    [System.Serializable]
    public class Magnification {
        public string key;
        public float mag;
        public float max_mag;
        public int max_point;

        /// <summary>
        /// 업그레이드시 분야별 배율
        /// </summary>
        /// <param name="key">분야</param>
        /// <param name="mag">1포인트당 배율 증가량</param>
        /// <param name="max_mag">최대 배율</param>
        /// <param name="max_point">포인트 투자 한계량</param>
        public Magnification(string key, float mag, float max_mag, int max_point) {
            this.key = key;
            this.mag = mag;
            this.max_mag = max_mag;
        }
    }
}
