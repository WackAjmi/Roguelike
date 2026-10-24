using UnityEngine;

public class FoodObject : CellObject
{
   [SerializeField, Range(1, 50)] private int AmountGranted = 10;
  
   public override void PlayerEntered()
   {
       GameManager.Instance.PlayOneShot(GameManager.Instance.FoodPickupSound);
       GameManager.Instance.PlayFoodCollectionParticles(GameManager.Instance.BoardManager.CellToWorld(m_Cell));
       GameManager.Instance.BoardManager.RecycleObject(this);
    
       //increase food
       GameManager.Instance.ChangeFood(AmountGranted);
   }
}