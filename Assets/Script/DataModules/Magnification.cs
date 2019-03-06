namespace DataModules{
    [System.Serializable]
    public class Magnification {
        public string key;
        public float mag;

        public Magnification(string key, float mag) {
            this.key = key;
            this.mag = mag;
        }
    }
}
