using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    private VisualElement m_GameOverPanel;
    private VisualElement m_MainMenuPanel;
    private VisualElement m_PausePanel;
    private Label m_GameOverMessage;
    private Button m_PlayButton;
    private Button m_MainMenuQuitButton;
    private Button m_ResumeButton;
    private Button m_RestartButton;
    private Button m_PauseQuitButton;

    private int m_CurrentLevel = 1;
    private bool m_IsGameOver;
    private bool m_IsPaused;
    private bool m_GameStarted;

    private int m_FoodAmount = 100;
    [SerializeField] private UIDocument m_UIDoc;
    private Label m_FoodLabel;
    [SerializeField] private AudioClip m_PlayerMoveSound;
    [SerializeField] private AudioClip m_WallAttackSound;
    [SerializeField] private AudioClip m_FoodPickupSound;
    [SerializeField] private AudioClip m_EnemyAttackSound;
    [SerializeField] private AudioClip m_EnemyDeathSound;
    [SerializeField] private AudioClip m_GameOverSound;
    [SerializeField] private ParticleSystem m_WallDestructionParticles;
    [SerializeField] private ParticleSystem m_FoodCollectionParticles;
    [SerializeField] private ParticleSystem m_EnemyDeathParticles;
    private readonly Dictionary<ParticleSystem, List<ParticleSystem>> m_ParticlePools =
        new Dictionary<ParticleSystem, List<ParticleSystem>>();

   public static GameManager Instance { get; private set; }
  
   [SerializeField] private BoardManager m_BoardManager;
   [SerializeField] private PlayerController m_PlayerController;
   public BoardManager BoardManager => m_BoardManager;
   public PlayerController PlayerController => m_PlayerController;
   public AudioClip PlayerMoveSound => m_PlayerMoveSound;
   public AudioClip WallAttackSound => m_WallAttackSound;
   public AudioClip FoodPickupSound => m_FoodPickupSound;
   public AudioClip EnemyAttackSound => m_EnemyAttackSound;
   public AudioClip EnemyDeathSound => m_EnemyDeathSound;
   public AudioClip GameOverSound => m_GameOverSound;

   public TurnManager TurnManager { get; private set;}
   public bool IsPaused => m_IsPaused;
   public bool IsGameStarted => m_GameStarted;
   private AudioSource m_AudioSource;

   private void Awake()
   {
       if (Instance != null)
       {
           Destroy(gameObject);
           return;
       }
      
       Instance = this;
       m_AudioSource = GetComponent<AudioSource>();
       if (m_AudioSource == null)
       {
           m_AudioSource = gameObject.AddComponent<AudioSource>();
       }
       PrewarmParticlePool(m_WallDestructionParticles);
       PrewarmParticlePool(m_FoodCollectionParticles);
       PrewarmParticlePool(m_EnemyDeathParticles);
   }

   public void PlayOneShot(AudioClip clip)
   {
       m_AudioSource.PlayOneShot(clip);
   }

   public void PlayWallDestructionParticles(Vector3 position)
   {
       PlayParticleEffect(m_WallDestructionParticles, position);
   }

   public void PlayFoodCollectionParticles(Vector3 position)
   {
       PlayParticleEffect(m_FoodCollectionParticles, position);
   }

   public void PlayEnemyDeathParticles(Vector3 position)
   {
       PlayParticleEffect(m_EnemyDeathParticles, position);
   }

   private void PrewarmParticlePool(ParticleSystem prefab)
   {
       if (prefab == null)
       {
           Debug.LogError("A particle effect prefab is not assigned on GameManager.", this);
           return;
       }

       List<ParticleSystem> pool = new List<ParticleSystem>();
       m_ParticlePools.Add(prefab, pool);
       pool.Add(CreateParticleInstance(prefab));
   }

   private void PlayParticleEffect(ParticleSystem prefab, Vector3 position)
   {
       if (prefab == null)
       {
           Debug.LogError("A particle effect prefab is not assigned on GameManager.", this);
           return;
       }

       if (!m_ParticlePools.TryGetValue(prefab, out List<ParticleSystem> pool))
       {
           pool = new List<ParticleSystem>();
           m_ParticlePools.Add(prefab, pool);
       }

       ParticleSystem effect = null;
       foreach (ParticleSystem pooledEffect in pool)
       {
           if (!pooledEffect.isPlaying)
           {
               effect = pooledEffect;
               break;
           }
       }

       if (effect == null)
       {
           effect = CreateParticleInstance(prefab);
           pool.Add(effect);
       }

       effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
       effect.transform.position = position;
       effect.Play();
   }

   private ParticleSystem CreateParticleInstance(ParticleSystem prefab)
   {
       ParticleSystem particles = Instantiate(prefab, transform);
       particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
       return particles;
   }
  
void Start()
{
   TurnManager = new TurnManager();
   TurnManager.OnTick += OnTurnHappen;
  
   m_FoodLabel = m_UIDoc.rootVisualElement.Q<Label>("FoodLabel");
   m_MainMenuPanel = m_UIDoc.rootVisualElement.Q<VisualElement>("MainMenuPanel");
   m_PausePanel = m_UIDoc.rootVisualElement.Q<VisualElement>("PausePanel");
   m_GameOverPanel = m_UIDoc.rootVisualElement.Q<VisualElement>("GameOverPanel");
   m_GameOverMessage = m_GameOverPanel.Q<Label>("GameOverMessage");
   m_PlayButton = m_MainMenuPanel.Q<Button>("PlayButton");
   m_MainMenuQuitButton = m_MainMenuPanel.Q<Button>("MainMenuQuitButton");
   m_ResumeButton = m_PausePanel.Q<Button>("ResumeButton");
   m_RestartButton = m_PausePanel.Q<Button>("RestartButton");
   m_PauseQuitButton = m_PausePanel.Q<Button>("PauseQuitButton");

   m_PlayButton.clicked += StartNewGame;
   m_MainMenuQuitButton.clicked += QuitGame;
   m_ResumeButton.clicked += ResumeGame;
   m_RestartButton.clicked += StartNewGame;
   m_PauseQuitButton.clicked += QuitGame;

   m_FoodLabel.style.display = DisplayStyle.None;
   m_MainMenuPanel.style.display = DisplayStyle.Flex;
   m_PausePanel.style.display = DisplayStyle.None;
   m_GameOverPanel.style.display = DisplayStyle.None;
   Time.timeScale = 1f;
}

private void Update()
{
   if (!m_GameStarted || m_IsGameOver)
   {
       return;
   }

   if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
   {
       if (m_IsPaused)
       {
           ResumeGame();
       }
       else
       {
           PauseGame();
       }
   }
}

public void StartNewGame()
{
   Time.timeScale = 1f;
   m_IsPaused = false;
   m_IsGameOver = false;
   m_GameStarted = true;
   m_CurrentLevel = 1;
   m_FoodAmount = 20;
   m_FoodLabel.text = "Food : " + m_FoodAmount;
   m_MainMenuPanel.style.display = DisplayStyle.None;
   m_PausePanel.style.display = DisplayStyle.None;
   m_GameOverPanel.style.display = DisplayStyle.None;
   m_FoodLabel.style.display = DisplayStyle.Flex;

   BoardManager.Clean();
   BoardManager.SetLevel(m_CurrentLevel);
   BoardManager.Init();
   FrameBoardCamera();
  
   PlayerController.Init();
   PlayerController.Spawn(BoardManager, new Vector2Int(1,1));
}
public void NewLevel()
{
   BoardManager.Clean();
   m_CurrentLevel++;
   BoardManager.SetLevel(m_CurrentLevel);
   BoardManager.Init();
   FrameBoardCamera();
   PlayerController.Spawn(BoardManager, new Vector2Int(1,1));
}

private void FrameBoardCamera()
{
   Camera mainCamera = Camera.main;
   if (mainCamera == null)
   {
       return;
   }

   mainCamera.transform.position = new Vector3(
       (BoardManager.BoardWidth - 1) * 0.5f,
       (BoardManager.BoardHeight - 1) * 0.5f,
       mainCamera.transform.position.z);
   mainCamera.orthographicSize = Mathf.Max(
       BoardManager.BoardHeight * 0.5f,
       BoardManager.BoardWidth / (2f * mainCamera.aspect));
}

private void PauseGame()
{
   if (!m_GameStarted || m_IsGameOver)
   {
       return;
   }

   m_IsPaused = true;
   Time.timeScale = 0f;
   m_PausePanel.style.display = DisplayStyle.Flex;
}

private void ResumeGame()
{
   m_IsPaused = false;
   Time.timeScale = 1f;
   m_PausePanel.style.display = DisplayStyle.None;
}

private static void QuitGame()
{
   Application.Quit();
}

void OnTurnHappen()
{
   ChangeFood(-1);
}

public void ChangeFood(int amount)
{
   m_FoodAmount += amount;
   m_FoodLabel.text = "Food : " + m_FoodAmount;
     if (m_FoodAmount <= 0 && !m_IsGameOver)
   {
        m_IsGameOver = true;
        PlayOneShot(m_GameOverSound);
        PlayerController.GameOver();
       m_GameOverPanel.style.display = DisplayStyle.Flex;
       m_PausePanel.style.display = DisplayStyle.None;
      m_GameOverMessage.text = "Game Over!\n\nYou traveled through " + (m_CurrentLevel - 1) + " levels\n\nPress Enter to restart";

   }
}
}