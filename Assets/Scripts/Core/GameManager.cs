using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using SunnysideIsland.Events;
using SunnysideIsland.Core;
using SunnysideIsland.Crafting;
using SunnysideIsland.Quest;
using SunnysideIsland.UI.Quest;
using DI;
using SunnysideIsland.UI;

namespace SunnysideIsland.Core
{
    /// <summary>
    /// 게임의 전체 상태와 흐름을 관리하는 매니저
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private static bool _pendingNewGameLaunch;
        private static bool _pendingLoadGameLaunch;
        private static string _pendingLoadSaveName;

        public const string DefaultGameSceneName = "MainGame";

        [Header("=== Settings ===")]
        [SerializeField] private string _gameSceneName = "MainGame";
        [SerializeField] private string _mainMenuSceneName = "Start Scene";
        [SerializeField] private string _endSceneName = "End Scene";
        
        [Header("=== References ===")]
        [SerializeField] private TimeManager _timeManager;
        [SerializeField] private SaveSystem _saveSystem;
        
        // 현재 게임 상태
        public GameState CurrentState { get; private set; } = GameState.MainMenu;
        public static GameManager Instance { get; private set; }
        
        // 현재 활성화된 세이브 이름
        public string CurrentSaveName { get; private set; }
        public string LastPlayableSaveName { get; private set; }
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void EnsureNewGameQuestSetup()
        {
            var questSystem = FindFirstObjectByType<QuestSystem>(FindObjectsInactive.Include);
            if (questSystem == null || questSystem.GetActiveQuests().Count > 0)
            {
                return;
            }

            var chapterQuestManager = FindFirstObjectByType<ChapterQuestManager>(FindObjectsInactive.Include);
            if (chapterQuestManager == null)
            {
                questSystem.AcceptQuest("quest_crash");
                return;
            }

            chapterQuestManager.StartChapter(chapterQuestManager.CurrentChapter);

            if (questSystem.GetActiveQuests().Count == 0)
            {
                questSystem.AcceptQuest("quest_crash");
            }
        }
        
        private void Start()
        {
            Initialize();
        }
        
        /// <summary>
        /// 초기화
        /// </summary>
        private void Initialize()
        {
            // DI Container 초기화
            if (DIContainer.Global == null)
            {
                DIContainer.InitializeGlobal();
            }
            
            // 전역 서비스 등록
            RegisterGlobalServices();
            
            // 이벤트 구독
            EventBus.Subscribe<ItemCraftedEvent>(OnItemCrafted);
            EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDiedEvent);
            
        }
        
        /// <summary>
        /// 전역 서비스 등록
        /// </summary>
        private void RegisterGlobalServices()
        {
            RefreshSceneReferences();
        }
        
        /// <summary>
        /// 새 게임 시작
        /// </summary>
        public void StartNewGame()
        {
            CurrentSaveName = $"save_{DateTime.Now:yyyyMMdd_HHmmss}";
            LastPlayableSaveName = null;
            CurrentState = GameState.Loading;
            
            // 시간 초기화
            if (_timeManager != null)
            {
                _timeManager.Initialize(1, 6, 0); // Day 1, 06:00
            }
            
            // 게임 씬 로드
            LoadGameScene(() =>
            {
                if (Instance != null)
                {
                    Instance.StartCoroutine(Instance.FinishGameStart(CurrentSaveName, true));
                }
            });
        }

        public static void PrepareNewGameLaunch()
        {
            _pendingNewGameLaunch = true;
        }

        public static void PrepareLoadGame(string saveName)
        {
            _pendingLoadSaveName = saveName;
            _pendingLoadGameLaunch = true;
        }

        private void BeginNewGameFromSceneLoad()
        {
            RefreshSceneReferences();
            InjectSceneDependencies();
            EnsureNewGameQuestSetup();
            CurrentSaveName = $"save_{DateTime.Now:yyyyMMdd_HHmmss}";
            LastPlayableSaveName = null;
            CurrentState = GameState.Loading;

            if (_timeManager != null)
            {
                _timeManager.Initialize(1, 6, 0);
            }

            StartCoroutine(FinishGameStart(CurrentSaveName, true));
        }

        private void BeginLoadGameFromSceneLoad(string saveName)
        {
            RefreshSceneReferences();
            InjectSceneDependencies();
            if (string.IsNullOrWhiteSpace(saveName))
            {
                Debug.LogError("[GameManager] Pending load save name is empty.");
                CurrentState = GameState.MainMenu;
                return;
            }

            CurrentSaveName = saveName;
            if (!string.Equals(saveName, "death_autosave", StringComparison.OrdinalIgnoreCase))
            {
                LastPlayableSaveName = saveName;
            }

            CurrentState = GameState.Loading;
            Time.timeScale = 1f;
            StartCoroutine(LoadGameProcess(saveName));
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<ItemCraftedEvent>(OnItemCrafted);
            EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDiedEvent);
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        
        /// <summary>
        /// 저장된 게임 불러오기
        /// </summary>
        public void LoadGame(string saveName)
        {
            if (Instance == null)
            {
                Debug.LogError("[GameManager] No instance found to load game!");
                return;
            }

            Instance.CurrentSaveName = saveName;
            if (!string.Equals(saveName, "death_autosave", StringComparison.OrdinalIgnoreCase))
            {
                Instance.LastPlayableSaveName = saveName;
            }
            Instance.CurrentState = GameState.Loading;
            Time.timeScale = 1f;
            UIManager.Instance?.CloseAllPanels();
            
            // 씬 로드
            if (string.Equals(SceneManager.GetActiveScene().name, Instance._gameSceneName, StringComparison.OrdinalIgnoreCase))
            {
                Instance.RefreshSceneReferences();
                Instance.InjectSceneDependencies();
                Instance.StartCoroutine(Instance.LoadGameProcess(saveName));
                return;
            }

            Instance.LoadGameScene(() =>
            {
                // 씬 로드 후에는 Instance가 바뀌었을 수 있으므로 다시 Instance를 통해 호출
                if (Instance != null)
                {
                    Instance.StartCoroutine(Instance.LoadGameProcess(saveName));
                }
            });
        }

        private System.Collections.IEnumerator LoadGameProcess(string saveName)
        {
            // 한 프레임 대기하여 모든 오브젝트의 Awake/Start가 실행될 시간을 줍니다.
            yield return null;
            
            RefreshSceneReferences();
            
            if (_saveSystem != null)
            {
                bool success = _saveSystem.LoadGame(saveName);
                if (success)
                {
                    StartCoroutine(FinishGameStart(saveName, false));
                }
                else
                {
                    CurrentState = GameState.MainMenu;
                    Debug.LogError($"[GameManager] Failed to load game: {saveName}");
                }
            }
            else
            {
                Debug.LogError("[GameManager] SaveSystem is null! Cannot load game.");
            }
        }
        
        /// <summary>
        /// 게임 저장
        /// </summary>
        public void SaveGame(string saveName = null)
        {
            if (CurrentState != GameState.Playing && CurrentState != GameState.MainMenu)
            {
                Debug.LogWarning("[GameManager] Cannot save in current state: " + CurrentState);
                return;
            }
            
            string nameToSave = saveName ?? CurrentSaveName ?? "quicksave";
            
            if (_saveSystem != null)
            {
                _saveSystem.SaveGame(nameToSave);
                CurrentSaveName = nameToSave;
                if (!string.Equals(nameToSave, "death_autosave", StringComparison.OrdinalIgnoreCase))
                {
                    LastPlayableSaveName = nameToSave;
                }
            }
        }
        
        /// <summary>
        /// 게임 일시정지
        /// </summary>
        public void PauseGame()
        {
            if (CurrentState == GameState.Playing)
            {
                CurrentState = GameState.Paused;
                Time.timeScale = 0f;
                
                if (_timeManager != null)
                {
                    _timeManager.Pause();
                }
                
            }
        }
        
        /// <summary>
        /// 게임 재개
        /// </summary>
        public void ResumeGame()
        {
            if (CurrentState == GameState.Paused)
            {
                CurrentState = GameState.Playing;
                Time.timeScale = 1f;
                
                if (_timeManager != null)
                {
                    _timeManager.Resume();
                }
                
            }
        }
        
        /// <summary>
        /// 메인 메뉴로 돌아가기
        /// </summary>
        public void ReturnToMainMenu()
        {
            CurrentState = GameState.MainMenu;
            CurrentSaveName = null;
            LastPlayableSaveName = null;
            Time.timeScale = 1f;
            
            UIManager.Instance?.CloseAllPanels();
            DIContainer.ClearSceneContainers();

            EventBus.Publish(new ReturnToMainMenuEvent());
            
            SceneManager.LoadScene(_mainMenuSceneName);
        }
        
        /// <summary>
        /// 게임 종료
        /// </summary>
        public void QuitGame()
        {
            
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        
        /// <summary>
        /// 게임 씬 로드
        /// </summary>
        private void LoadGameScene(Action onComplete = null)
        {
            var asyncLoad = SceneManager.LoadSceneAsync(_gameSceneName);
            
            if (asyncLoad != null && onComplete != null)
            {
                asyncLoad.completed += (_) =>
                {
                    RefreshSceneReferences();
                    InjectSceneDependencies();
                    onComplete?.Invoke();
                };
            }
            else if (onComplete != null)
            {
                RefreshSceneReferences();
                InjectSceneDependencies();
                onComplete?.Invoke();
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // EventBus가 Clear된 후 재구독
            ResubscribeEvents();
            
            if (!string.Equals(scene.name, _gameSceneName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            RefreshSceneReferences();

            if (_pendingNewGameLaunch)
            {
                _pendingNewGameLaunch = false;
                BeginNewGameFromSceneLoad();
                return;
            }

            if (_pendingLoadGameLaunch)
            {
                _pendingLoadGameLaunch = false;
                BeginLoadGameFromSceneLoad(_pendingLoadSaveName);
            }
        }
        
        private void ResubscribeEvents()
        {
            // 기존 구독 해제 후 재구독 (중복 방지)
            EventBus.Unsubscribe<ItemCraftedEvent>(OnItemCrafted);
            EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDiedEvent);
            
            // 이벤트 재구독
            EventBus.Subscribe<ItemCraftedEvent>(OnItemCrafted);
            EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDiedEvent);
            
            Debug.Log("[GameManager] EventBus events resubscribed after scene load");
        }

        private void RefreshSceneReferences()
        {
            _timeManager = FindFirstObjectByType<TimeManager>(FindObjectsInactive.Include);

            _saveSystem = FindFirstObjectByType<SaveSystem>(FindObjectsInactive.Include);
        }

        private System.Collections.IEnumerator FinishGameStart(string saveName, bool isNewGame)
        {
            CurrentState = GameState.Playing;
            if (!string.Equals(saveName, "death_autosave", StringComparison.OrdinalIgnoreCase))
            {
                LastPlayableSaveName = saveName;
            }
            yield return null;

            EventBus.Publish(new GameStartedEvent
            {
                IsNewGame = isNewGame,
                SaveName = saveName
            });

            yield return null;
            if (isNewGame)
            {
                StartCoroutine(OpenQuestPanelAfterStartRoutine());
            }
            else
            {
                UIManager.Instance?.CloseAllPanels();
            }
        }

        private System.Collections.IEnumerator OpenQuestPanelAfterStartRoutine()
        {
            if (UIManager.Instance == null)
            {
                yield break;
            }

            Debug.Log("[GameManager] Opening QuestPanel after new game start");
            UIManager.Instance.CloseAllPanels();
            yield return WaitForQuestInitialization();

            var questPanel = UIManager.Instance.GetPanel<QuestPanel>();
            if (questPanel != null)
            {
                var questSystem = FindFirstObjectByType<QuestSystem>(FindObjectsInactive.Include);
                int activeQuestCount = questSystem?.GetActiveQuests().Count ?? 0;
                Debug.Log($"[GameManager] QuestPanel found: {questPanel.name}, IsOpen={questPanel.IsOpen}, ActiveQuests={activeQuestCount}, ActiveInHierarchy={questPanel.gameObject.activeInHierarchy}");
                UIManager.Instance.OpenPanel(questPanel);
            }
            else
            {
                Debug.LogWarning("[GameManager] QuestPanel not found on game start.");
            }
        }

        private System.Collections.IEnumerator WaitForQuestInitialization()
        {
            const float timeoutSeconds = 1.5f;
            float elapsed = 0f;

            while (elapsed < timeoutSeconds)
            {
                var questSystem = FindFirstObjectByType<QuestSystem>(FindObjectsInactive.Include);
                if (questSystem != null && questSystem.GetActiveQuests().Count > 0)
                {
                    yield break;
                }

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }
        
        /// <summary>
        /// 씬 의존성 주입
        /// </summary>
        private void InjectSceneDependencies()
        {
            // 씬 내의 모든 MonoBehaviour에 DI 주입
            var monoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var mb in monoBehaviours)
            {
                DIContainer.Inject(mb);
            }
            
        }
        
        /// <summary>
        /// 플레이어 사망 처리
        /// </summary>
        public void OnPlayerDied(string deathReason)
        {
            SaveGame("death_autosave");
            CurrentState = GameState.Dead;

            Invoke(nameof(GameOver), 2f);
        }

        private void OnPlayerDiedEvent(PlayerDiedEvent evt)
        {
            OnPlayerDied(evt?.DeathReason);
        }
        
        /// <summary>
        /// 부활 UI 표시
        /// </summary>
        private void ShowRespawnUI()
        {
            CurrentState = GameState.Playing;
            
            EventBus.Publish(new PlayerRespawnedEvent
            {
                RespawnPosition = GetRespawnPosition()
            });
            
        }
        
        /// <summary>
        /// 리스폰 위치 반환
        /// </summary>
        private Vector3 GetRespawnPosition()
        {
            return Vector3.zero;
        }
        
        /// <summary>
        /// 게임 오버
        /// </summary>
        public void GameOver()
        {
            Time.timeScale = 1f;
            UIManager.Instance?.CloseAllPanels();
            CurrentState = GameState.GameOver;
            
            EventBus.Publish(new GameOverEvent
            {
                DeathReason = "게임 오버"
            });
        }
        
        /// <summary>
        /// 배 건설 완료 (게임 클리어)
        /// </summary>
        public void OnBoatBuilt()
        {
            Debug.Log("[GameManager] OnBoatBuilt()");
            if (CurrentState == GameState.Victory)
            {
                return;
            }

            CurrentState = GameState.Victory;

            EventBus.Publish(new GameClearEvent());
            LoadEndScene();
        }

        private void LoadEndScene()
        {
            Debug.Log($"[GameManager] LoadEndScene() -> {_endSceneName}");
            Time.timeScale = 1f;
            UIManager.Instance?.CloseAllPanels();

            if (string.IsNullOrWhiteSpace(_endSceneName))
            {
                Debug.LogError("[GameManager] End scene name is not configured.");
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(_endSceneName))
            {
                Debug.LogError($"[GameManager] End scene cannot be loaded: {_endSceneName}");
                return;
            }

            SceneManager.LoadScene(_endSceneName);
        }

        private void OnItemCrafted(ItemCraftedEvent evt)
        {
            if (evt.ResultItemId == "boat")
            {
                OnBoatBuilt();
            }
        }
        
        /// <summary>
        /// 디버그: 현재 상태 출력
        /// </summary>
        [ContextMenu("Debug State")]
        private void DebugState()
        {
        }
        
        public class GameClearEvent
        {
        }
    }
    
    /// <summary>
    /// 게임 상태 열거형
    /// </summary>
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        Loading,
        GameOver,
        Dead,
        Victory
    }
}
