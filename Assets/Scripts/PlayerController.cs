using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private bool m_IsGameOver;
   private bool m_IsMoving;
    [SerializeField, Range(1f, 12f), Tooltip("Player movement speed in cells per second.")]
    private float MoveSpeed = 5.0f;

   private BoardManager m_Board;
   private Vector2Int m_CellPosition;
   private Animator m_Animator;
   public Vector2Int Cell => m_CellPosition;

   private void Awake()
   {
       m_Animator = GetComponent<Animator>();
   }
   public void Spawn(BoardManager boardManager, Vector2Int cell)
   {
       m_Board = boardManager;
       MoveTo(cell);
   }
  
   public void MoveTo(Vector2Int cell)
   {
       m_CellPosition = cell;
       transform.position = m_Board.CellToWorld(m_CellPosition);
   }
public void Init()
{
   m_IsGameOver = false;
}
   private void StartMove(Vector2Int cell)
   {
       m_CellPosition = cell;
       m_IsMoving = true;
       m_Animator.SetBool("Moving", true);
       GameManager.Instance.PlayOneShot(GameManager.Instance.PlayerMoveSound);
       StartCoroutine(MoveBetweenCells(m_Board.CellToWorld(cell)));
   }

   private IEnumerator MoveBetweenCells(Vector3 targetPosition)
   {
       Vector3 startPosition = transform.position;
       float duration = 1f / MoveSpeed;
       float elapsed = 0f;

       while (elapsed < duration)
       {
           elapsed += Time.deltaTime;
           transform.position = Vector3.Lerp(startPosition, targetPosition, Mathf.Clamp01(elapsed / duration));
           yield return null;
       }

       transform.position = targetPosition;
       m_IsMoving = false;
       m_Animator.SetBool("Moving", false);

       BoardManager.CellData cellData = m_Board.GetCellData(m_CellPosition);
       if (cellData != null && cellData.ContainedObject != null)
       {
           cellData.ContainedObject.PlayerEntered();
       }
   }

   private void Update()
   {
       if (!GameManager.Instance.IsGameStarted || GameManager.Instance.IsPaused)
       {
           return;
       }

       if (m_IsGameOver)
   {
       if (Keyboard.current.enterKey.wasPressedThisFrame)
       {
           GameManager.Instance.StartNewGame();
       }

       return;
   }
       if (m_IsMoving)
       {
           return;
       }

       Vector2Int newCellTarget = m_CellPosition;
       bool hasMoved = false;

       if(Keyboard.current.upArrowKey.wasPressedThisFrame)
       {
           newCellTarget.y += 1;
           hasMoved = true;
       }
       else if(Keyboard.current.downArrowKey.wasPressedThisFrame)
       {
           newCellTarget.y -= 1;
           hasMoved = true;
       }
       else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
       {
           newCellTarget.x += 1;
           hasMoved = true;
       }
       else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
       {
           newCellTarget.x -= 1;
           hasMoved = true;
       }

if(hasMoved)
{
   //check if the new position is passable, then move there if it is.
   BoardManager.CellData cellData = m_Board.GetCellData(newCellTarget);

if(cellData != null && cellData.Passable)
{
GameManager.Instance.TurnManager.Tick();

if (cellData.ContainedObject == null)
{
    StartMove(newCellTarget);
}
else if(cellData.ContainedObject.PlayerWantsToEnter())
{
    StartMove(newCellTarget);
}
}
}
   }
   public void GameOver()
{
   m_IsGameOver = true;
}

    public void PlayAttackAnimation()
    {
        m_Animator.SetTrigger("Attack");
    }

}