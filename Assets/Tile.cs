public class Tile {
    public int x;
    public int y;
    public int type;
    public float timeSinceLastUpdate = 0;

    public Tile(int x, int y, int type) {
        this.x = x;
        this.y = y;
        this.type = type;
    }
}
