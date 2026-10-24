using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class BoardManager : MonoBehaviour
{
    [SerializeField] private ExitCellObject ExitCellPrefab;


   public class CellData
   {
       public bool Passable;
   public CellObject ContainedObject;

   }
   [SerializeField] private Enemy EnemyPrefab;
   [SerializeField] private FoodObject FoodPrefab;
   [SerializeField] private WallObject WallPrefab;
   private CellData[,] m_BoardData;
   private Tilemap m_Tilemap;
   private Grid m_Grid;
   private List<Vector2Int> m_EmptyCellsList;
   private readonly List<Enemy> m_EnemyPool = new List<Enemy>();
   private readonly List<FoodObject> m_FoodPool = new List<FoodObject>();
   private readonly List<WallObject> m_WallPool = new List<WallObject>();
  
   [SerializeField, Range(4, 32)] private int Width = 8;
   [SerializeField, Range(4, 32)] private int Height = 8;
   [SerializeField] private Tile[] GroundTiles;
   [SerializeField] private Tile[] WallTiles;
   [SerializeField, Range(0, 10), Tooltip("Maximum number of enemies spawned per level.")]
   private int EnemyCount = 1;
   [SerializeField, Range(0, 20), Tooltip("Maximum number of food objects spawned per level.")]
   private int FoodCount = 5;
   [SerializeField, Range(0, 20)] private int MinWallCount = 6;
   [SerializeField, Range(0, 20)] private int MaxWallCount = 9;
   private int m_BaseWidth;
   private int m_BaseHeight;
   private int m_BaseEnemyCount;
   private int m_BaseMinWallCount;
   private int m_BaseMaxWallCount;
   public int BoardWidth => Width;
   public int BoardHeight => Height;

   private void Awake()
   {
       m_BaseWidth = Width;
       m_BaseHeight = Height;
       m_BaseEnemyCount = EnemyCount;
       m_BaseMinWallCount = MinWallCount;
       m_BaseMaxWallCount = MaxWallCount;
   }

   public void SetLevel(int level)
   {
       int levelOffset = Mathf.Max(1, level) - 1;
       Width = Mathf.Min(32, m_BaseWidth + levelOffset * 2);
       Height = Mathf.Min(32, m_BaseHeight + levelOffset * 2);
       EnemyCount = Mathf.Min(10, m_BaseEnemyCount + levelOffset);
       MinWallCount = Mathf.Min(20, m_BaseMinWallCount + levelOffset);
       MaxWallCount = Mathf.Min(20, m_BaseMaxWallCount + levelOffset);
   }
  
   public void Init()
{
   m_Tilemap = GetComponentInChildren<Tilemap>();
   m_Grid = GetComponentInChildren<Grid>();
   InitializePools();
   //Initialize the list
   m_EmptyCellsList = new List<Vector2Int>();
  
   m_BoardData = new CellData[Width, Height];


   for (int y = 0; y < Height; ++y)
   {
       for(int x = 0; x < Width; ++x)
       {
           Tile tile;
           m_BoardData[x, y] = new CellData();
          
           if(x == 0 || y == 0 || x == Width - 1 || y == Height - 1)
           {
               tile = WallTiles[Random.Range(0, WallTiles.Length)];
               m_BoardData[x, y].Passable = false;
           }
           else
           {
               tile = GroundTiles[Random.Range(0, GroundTiles.Length)];
               m_BoardData[x, y].Passable = true;
              
               //this is a passable empty cell, add it to the list!
               m_EmptyCellsList.Add(new Vector2Int(x, y));
           }
          
           m_Tilemap.SetTile(new Vector3Int(x, y, 0), tile);
       }
   }
    m_EmptyCellsList.Remove(new Vector2Int(1, 1));

   Vector2Int endCoord = new Vector2Int(Width - 2, Height - 2);
   AddObject(Instantiate(ExitCellPrefab), endCoord);
   m_EmptyCellsList.Remove(endCoord);
  
   GenerateEnemy();
   GenerateWall();
   GenerateFood();
}

   public Vector3 CellToWorld(Vector2Int cellIndex)
   {
       return m_Grid.GetCellCenterWorld((Vector3Int)cellIndex);
   }

   public CellData GetCellData(Vector2Int cellIndex)
   {
       if (cellIndex.x < 0 || cellIndex.x >= Width
           || cellIndex.y < 0 || cellIndex.y >= Height)
       {
           return null;
       }

       return m_BoardData[cellIndex.x, cellIndex.y];
   }
       void AddObject(CellObject obj, Vector2Int coord)
{
   CellData data = m_BoardData[coord.x, coord.y];
   obj.transform.position = CellToWorld(coord);
   data.ContainedObject = obj;
   obj.Init(coord);
}
 
void GenerateFood()
{
   int foodCount = Mathf.Min(FoodCount, m_EmptyCellsList.Count);
   for (int i = 0; i < foodCount; ++i)
   {
       int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
       Vector2Int coord = m_EmptyCellsList[randomIndex];

       m_EmptyCellsList.RemoveAt(randomIndex);
       FoodObject newFood = GetPooledObject(m_FoodPool, FoodPrefab);
       AddObject(newFood, coord);
   }
}
void GenerateEnemy()
{
   int enemyCount = Mathf.Min(EnemyCount, m_EmptyCellsList.Count);
   for (int i = 0; i < enemyCount; ++i)
   {
       int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
       Vector2Int coord = m_EmptyCellsList[randomIndex];

       m_EmptyCellsList.RemoveAt(randomIndex);
       Enemy newEnemy = GetPooledObject(m_EnemyPool, EnemyPrefab);
       AddObject(newEnemy, coord);
   }
}
void GenerateWall()
{
   int minWallCount = Mathf.Min(MinWallCount, MaxWallCount);
   int maxWallCount = Mathf.Max(MinWallCount, MaxWallCount);
   int wallCount = Mathf.Min(Random.Range(minWallCount, maxWallCount + 1), m_EmptyCellsList.Count);
   for (int i = 0; i < wallCount; ++i)
   {
       int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
       Vector2Int coord = m_EmptyCellsList[randomIndex];

       m_EmptyCellsList.RemoveAt(randomIndex);
       WallObject newWall = GetPooledObject(m_WallPool, WallPrefab);
       AddObject(newWall, coord);
   }
}
void InitializePools()
{
   EnsurePool(m_EnemyPool, EnemyPrefab, Mathf.Max(EnemyCount, m_BaseEnemyCount + 9));
   EnsurePool(m_FoodPool, FoodPrefab, FoodCount);
   EnsurePool(m_WallPool, WallPrefab, 20);
}
void EnsurePool<T>(List<T> pool, T prefab, int capacity) where T : CellObject
{
   while (pool.Count < capacity)
   {
       T pooledObject = Instantiate(prefab, transform);
       pooledObject.gameObject.SetActive(false);
       pool.Add(pooledObject);
   }
}
T GetPooledObject<T>(List<T> pool, T prefab) where T : CellObject
{
   foreach (T pooledObject in pool)
   {
       if (!pooledObject.gameObject.activeSelf)
       {
           pooledObject.gameObject.SetActive(true);
           return pooledObject;
       }
   }

   T newObject = Instantiate(prefab, transform);
   pool.Add(newObject);
   return newObject;
}
public void RecycleObject(CellObject cellObject)
{
   CellData cellData = GetCellData(cellObject.Cell);
   if (cellData != null && cellData.ContainedObject == cellObject)
   {
       cellData.ContainedObject = null;
   }

   if ((cellObject is Enemy enemy && m_EnemyPool.Contains(enemy))
       || (cellObject is FoodObject food && m_FoodPool.Contains(food))
       || (cellObject is WallObject wall && m_WallPool.Contains(wall)))
   {
       cellObject.gameObject.SetActive(false);
   }
   else
   {
       Destroy(cellObject.gameObject);
   }
}
public void SetCellTile(Vector2Int cellIndex, Tile tile)
{
   m_Tilemap.SetTile(new Vector3Int(cellIndex.x, cellIndex.y, 0), tile);
}
public Tile GetCellTile(Vector2Int cellIndex)
    {
        return m_Tilemap.GetTile<Tile>(new Vector3Int(cellIndex.x,     cellIndex.y, 0));
    }
    public void Clean()
{
    //no board data, so exit early, nothing to clean
   if(m_BoardData == null)
       return;


   for (int y = 0; y < Height; ++y)
   {
       for (int x = 0; x < Width; ++x)
       {
           var cellData = m_BoardData[x, y];

           if (cellData.ContainedObject != null)
           {
               RecycleObject(cellData.ContainedObject);
           }

           SetCellTile(new Vector2Int(x,y), null);
       }
   }
   m_BoardData = null;
}}