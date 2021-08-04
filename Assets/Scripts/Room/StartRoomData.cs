using UnityEngine;
using UnityEngine.Tilemaps;

public class StartRoomData : MonoBehaviour
{
    public int id;
    public Transform endPoint;
    public Transform entrance;
    public Transform exit;
    public Transform hunterSpawn;
    public Transform escapeSpawn;
    public PolygonCollider2D polygonCollider2D;
    public Tilemap startRoomTilemap;
}
