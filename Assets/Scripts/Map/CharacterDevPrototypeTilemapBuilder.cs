using UnityEngine;
using UnityEngine.Tilemaps;

namespace ProjectR.Map
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class CharacterDevPrototypeTilemapBuilder : MonoBehaviour
    {
        [SerializeField] private TileBase grassTile;
        [SerializeField] private TileBase grassDetailTile;
        [SerializeField] private TileBase stonePathTile;
        [SerializeField] private TileBase wallTile;
        [SerializeField, Min(5)] private int width = 17;
        [SerializeField, Min(5)] private int height = 11;
        [SerializeField] private string collisionLayerName = "PrototypeMapBounds";
        [SerializeField] private string walkableLayerName = "PrototypeWalkable";
        [SerializeField] private bool rebuildOnValidate = true;

        private const string GroundName = "Ground Tilemap";
        private const string PathName = "Path Tilemap";
        private const string WallName = "Wall Tilemap";

#if UNITY_EDITOR
        private bool rebuildQueued;
#endif

        private void OnEnable()
        {
            RequestRebuild();
        }

        private void OnValidate()
        {
            if (!rebuildOnValidate)
            {
                return;
            }

            RequestRebuild();
        }

        [ContextMenu("Rebuild Prototype Tilemap")]
        private void Rebuild()
        {
            if (grassTile == null || stonePathTile == null || wallTile == null)
            {
                return;
            }

            Grid grid = GetComponent<Grid>();
            if (grid == null)
            {
                grid = gameObject.AddComponent<Grid>();
            }

            grid.cellSize = Vector3.one;
            grid.cellGap = Vector3.zero;
            grid.cellLayout = GridLayout.CellLayout.Rectangle;
            grid.cellSwizzle = GridLayout.CellSwizzle.XYZ;

            Tilemap ground = GetOrCreateLayer(GroundName, -20, LayerRole.Walkable);
            Tilemap path = GetOrCreateLayer(PathName, -10, LayerRole.Walkable);
            Tilemap walls = GetOrCreateLayer(WallName, 0, LayerRole.Blocking);

            ground.ClearAllTiles();
            path.ClearAllTiles();
            walls.ClearAllTiles();

            int halfWidth = width / 2;
            int halfHeight = height / 2;

            for (int y = -halfHeight; y <= halfHeight; y++)
            {
                for (int x = -halfWidth; x <= halfWidth; x++)
                {
                    TileBase selectedGrass = grassDetailTile != null && Mathf.Abs((x * 17 + y * 31) % 11) == 0
                        ? grassDetailTile
                        : grassTile;
                    ground.SetTile(new Vector3Int(x, y, 0), selectedGrass);
                }
            }

            for (int x = -halfWidth + 2; x <= halfWidth - 2; x++)
            {
                path.SetTile(new Vector3Int(x, 0, 0), stonePathTile);
            }

            for (int y = -halfHeight + 2; y <= halfHeight - 2; y++)
            {
                path.SetTile(new Vector3Int(0, y, 0), stonePathTile);
            }

            for (int x = -halfWidth; x <= halfWidth; x++)
            {
                walls.SetTile(new Vector3Int(x, -halfHeight, 0), wallTile);
                walls.SetTile(new Vector3Int(x, halfHeight, 0), wallTile);
            }

            for (int y = -halfHeight + 1; y <= halfHeight - 1; y++)
            {
                walls.SetTile(new Vector3Int(-halfWidth, y, 0), wallTile);
                walls.SetTile(new Vector3Int(halfWidth, y, 0), wallTile);
            }
        }

        private Tilemap GetOrCreateLayer(string layerName, int sortingOrder, LayerRole layerRole)
        {
            Transform layerTransform = transform.Find(layerName);
            if (layerTransform == null)
            {
                GameObject layer = new GameObject(layerName);
                layerTransform = layer.transform;
                layerTransform.SetParent(transform, false);
            }

            layerTransform.gameObject.layer = layerRole switch
            {
                LayerRole.Blocking => GetLayer(collisionLayerName),
                LayerRole.Walkable => GetLayer(walkableLayerName),
                _ => gameObject.layer
            };

            Tilemap tilemap = layerTransform.GetComponent<Tilemap>();
            if (tilemap == null)
            {
                tilemap = layerTransform.gameObject.AddComponent<Tilemap>();
            }

            TilemapRenderer renderer = layerTransform.GetComponent<TilemapRenderer>();
            if (renderer == null)
            {
                renderer = layerTransform.gameObject.AddComponent<TilemapRenderer>();
            }

            renderer.sortingOrder = sortingOrder;

            TilemapCollider2D collider = layerTransform.GetComponent<TilemapCollider2D>();
            if (layerRole != LayerRole.None && collider == null)
            {
                collider = layerTransform.gameObject.AddComponent<TilemapCollider2D>();
            }
            else if (layerRole == LayerRole.None && collider != null)
            {
                DestroyComponent(collider);
            }

            if (collider != null)
            {
                collider.isTrigger = layerRole == LayerRole.Walkable;
            }

            return tilemap;
        }

        private int GetLayer(string layerName)
        {
            int layer = LayerMask.NameToLayer(layerName);
            return layer >= 0 ? layer : gameObject.layer;
        }

        private void RequestRebuild()
        {
#if UNITY_EDITOR
            if (rebuildQueued)
            {
                return;
            }

            rebuildQueued = true;
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this == null)
                {
                    return;
                }

                rebuildQueued = false;
                Rebuild();
            };
            return;
#else
            Rebuild();
#endif
        }

        private static void DestroyComponent(Component component)
        {
            if (Application.isPlaying)
            {
                Destroy(component);
                return;
            }

            DestroyImmediate(component);
        }

        private enum LayerRole
        {
            None,
            Walkable,
            Blocking
        }
    }
}
