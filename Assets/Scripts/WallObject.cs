using UnityEngine;
using UnityEngine.Tilemaps;

public class WallObject : CellObject
{
   [SerializeField] private Tile ObstacleTile;
   [SerializeField, Range(1, 20)] private int MaxHealth = 3;

   private int m_HealthPoint;
   private Tile m_OriginalTile;
  
   public override void Init(Vector2Int cell)
   {
       base.Init(cell);

       m_HealthPoint = MaxHealth;
      
       m_OriginalTile = GameManager.Instance.BoardManager.GetCellTile(cell);
       GameManager.Instance.BoardManager.SetCellTile(cell, ObstacleTile);
   }

   public override bool PlayerWantsToEnter()
   {
       m_HealthPoint -= 1;
       GameManager.Instance.PlayOneShot(GameManager.Instance.WallAttackSound);
       if (m_HealthPoint > 0)
       {
           GameManager.Instance.PlayerController.PlayAttackAnimation();
           return false;
       }

       GameManager.Instance.BoardManager.SetCellTile(m_Cell, m_OriginalTile);
       GameManager.Instance.PlayWallDestructionParticles(GameManager.Instance.BoardManager.CellToWorld(m_Cell));
       GameManager.Instance.BoardManager.RecycleObject(this);
       return true;
   }
}