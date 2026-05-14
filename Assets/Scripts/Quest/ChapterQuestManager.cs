using System;
using System.Collections.Generic;
using UnityEngine;
using DI;
using SunnysideIsland.Core;
using SunnysideIsland.Events;
using Newtonsoft.Json.Linq;

namespace SunnysideIsland.Quest
{
    /// <summary>
    /// ì±•í„° ?€??
    /// </summary>
    public enum ChapterType
    {
        Chapter1, // Day 1-3: ?ì¡´???œì‘
        Chapter2, // Day 4-7: ??ê°œì²™
        Chapter3, // Day 8-14: ë§ˆì„ ê±´ì„¤
        Chapter4  // Day 15-28: ê´€ê´??„ì‹œ ?„ì„±
    }

    /// <summary>
    /// ì±•í„° ?°ì´??
    /// </summary>
    [Serializable]
    public class ChapterData
    {
        public ChapterType Chapter;
        public string Title;
        public string Description;
        public int StartDay;
        public int EndDay;
        public string[] QuestIds;
        public string[] SubQuestIds;
        public bool IsCompleted;
    }

    /// <summary>
    /// ì±•í„° ?˜ìŠ¤??ê´€ë¦¬ì
    /// ëª¨ë“  ë©”ì¸ ?˜ìŠ¤?¸ì? ì±•í„° ì§„í–‰??ê´€ë¦?
    /// </summary>
    public class ChapterQuestManager : MonoBehaviour, ISaveable
    {
        [Header("=== Chapter Data ===")]
        [SerializeField] private List<ChapterData> _chapters = new List<ChapterData>();
        
        [Header("=== Current State ===")]
        [SerializeField] private ChapterType _currentChapter = ChapterType.Chapter1;
        [SerializeField] private int _currentDay = 1;
        
        [Inject(Optional = true)]
        private QuestSystem _questSystem = default!;

        [Inject(Optional = true)]
        private TimeManager _timeManager = default!;
        
        public string SaveKey => "ChapterQuestManager";
        public ChapterType CurrentChapter => _currentChapter;
        public int CurrentDay => _currentDay;
        
        public event Action<ChapterType> OnChapterStarted;
        public event Action<ChapterType> OnChapterCompleted;
        
        private void Awake()
        {
        }
        
        private void Start()
        {
            DIContainer.Inject(this);
            DIContainer.TryResolve(out _questSystem);
            DIContainer.TryResolve(out _timeManager);
            
            EventBus.Subscribe<DayStartedEvent>(OnDayStartedEvent);
            EventBus.Subscribe<QuestCompletedEvent>(OnQuestCompleted);
        }
        
        private void OnDestroy()
        {
            EventBus.Unsubscribe<DayStartedEvent>(OnDayStartedEvent);
            EventBus.Unsubscribe<QuestCompletedEvent>(OnQuestCompleted);
        }
        
        private void OnDayStartedEvent(DayStartedEvent evt)
        {
            _currentDay = evt.Day;
            
            // ?ˆë¡œ??ì±•í„° ?œì‘ ì²´í¬
            foreach (var chapter in _chapters)
            {
                if (chapter.StartDay == evt.Day && chapter.Chapter > _currentChapter)
                {
                    StartChapter(chapter.Chapter);
                    break;
                }
            }
            
            // ?¼ì¼ ?˜ìŠ¤???ì„±
            GenerateDailyQuests();
        }
        
        /// <summary>
        /// ì±•í„° ?œì‘
        /// </summary>
        public void StartChapter(ChapterType chapter)
        {
            if (_questSystem == null)
            {
                DI.DIContainer.TryResolve(out _questSystem);
            }

            _currentChapter = chapter;
            var chapterData = GetChapterData(chapter);
            
            if (chapterData == null) return;
            
            // ë©”ì¸ ?˜ìŠ¤???ë™ ?˜ë½
            if (chapterData.QuestIds != null)
            {
                foreach (var questId in chapterData.QuestIds)
                {
                    _questSystem?.AcceptQuest(questId);
                }
            }
            
            OnChapterStarted?.Invoke(chapter);
            
            EventBus.Publish(new ChapterStartedEvent
            {
                Chapter = chapter,
                Title = chapterData.Title,
                Description = chapterData.Description
            });
            
            Debug.Log($"[ChapterQuestManager] Chapter {chapter} started: {chapterData.Title}");
        }
        
        /// <summary>
        /// ì±•í„° ?„ë£Œ
        /// </summary>
        public void CompleteChapter(ChapterType chapter)
        {
            var chapterData = GetChapterData(chapter);
            if (chapterData == null) return;
            
            chapterData.IsCompleted = true;
            
            OnChapterCompleted?.Invoke(chapter);
            
            EventBus.Publish(new ChapterCompletedEvent
            {
                Chapter = chapter,
                Title = chapterData.Title
            });
            
            Debug.Log($"[ChapterQuestManager] Chapter {chapter} completed: {chapterData.Title}");
        }
        
        /// <summary>
        /// ?˜ìŠ¤???„ë£Œ ??ì²˜ë¦¬
        /// </summary>
        private void OnQuestCompleted(QuestCompletedEvent evt)
        {
            CheckChapterCompletion();
        }
        
        /// <summary>
        /// ì±•í„° ?„ë£Œ ì²´í¬
        /// </summary>
        private void CheckChapterCompletion()
        {
            var chapterData = GetChapterData(_currentChapter);
            if (chapterData == null || chapterData.IsCompleted) return;
            
            // ëª¨ë“  ë©”ì¸ ?˜ìŠ¤???„ë£Œ ì²´í¬
            if (chapterData.QuestIds != null)
            {
                bool allCompleted = true;
                foreach (var questId in chapterData.QuestIds)
                {
                    if (!_questSystem.IsQuestCompleted(questId))
                    {
                        allCompleted = false;
                        break;
                    }
                }
                
                if (allCompleted)
                {
                    CompleteChapter(_currentChapter);
                }
            }
        }
        
        /// <summary>
        /// ?¼ì¼ ?˜ìŠ¤???ì„±
        /// </summary>
        private void GenerateDailyQuests()
        {
            // ?„ì¬ ì±•í„°???œë¸Œ ?˜ìŠ¤??ì¤??œë¤?¼ë¡œ 1-2ê°??˜ë½
            var chapterData = GetChapterData(_currentChapter);
            if (chapterData?.SubQuestIds != null)
            {
                var availableQuests = new List<string>();
                foreach (var questId in chapterData.SubQuestIds)
                {
                    if (!_questSystem.HasQuest(questId) && !_questSystem.IsQuestCompleted(questId))
                    {
                        availableQuests.Add(questId);
                    }
                }
                
                // ?œë¤?¼ë¡œ 1-2ê°??˜ë½
                int count = UnityEngine.Random.Range(1, Mathf.Min(3, availableQuests.Count + 1));
                for (int i = 0; i < count && availableQuests.Count > 0; i++)
                {
                    int index = UnityEngine.Random.Range(0, availableQuests.Count);
                    _questSystem.AcceptQuest(availableQuests[index]);
                    availableQuests.RemoveAt(index);
                }
            }
        }
        
        /// <summary>
        /// ì±•í„° ?°ì´??ê°€?¸ì˜¤ê¸?
        /// </summary>
        private ChapterData GetChapterData(ChapterType chapter)
        {
            foreach (var data in _chapters)
            {
                if (data.Chapter == chapter)
                    return data;
            }
            return null;
        }
        
        /// <summary>
        /// ?œë¸Œ ?˜ìŠ¤???˜ë½
        /// </summary>
        public void AcceptSubQuest(string questId)
        {
            _questSystem?.AcceptQuest(questId);
        }
        
        /// <summary>
        /// ?„ì¬ ì±•í„°??ì§„í–‰??(0-1)
        /// </summary>
        public float GetChapterProgress()
        {
            var chapterData = GetChapterData(_currentChapter);
            if (chapterData?.QuestIds == null || chapterData.QuestIds.Length == 0)
                return 0f;
            
            int completedCount = 0;
            foreach (var questId in chapterData.QuestIds)
            {
                if (_questSystem.IsQuestCompleted(questId))
                    completedCount++;
            }
            
            return (float)completedCount / chapterData.QuestIds.Length;
        }
        
        public object GetSaveData()
        {
            return new ChapterQuestSaveData
            {
                CurrentChapter = _currentChapter,
                CurrentDay = _currentDay,
                ChapterCompleted = GetCompletedChaptersArray()
            };
        }
        
        public void LoadSaveData(object state)
        {
            var data = state as ChapterQuestSaveData ?? (state as JObject)?.ToObject<ChapterQuestSaveData>();
            if (data != null)
            {
                _currentChapter = data.CurrentChapter;
                _currentDay = data.CurrentDay;
                
                // ì±•í„° ?„ë£Œ ?íƒœ ë³µì›
                for (int i = 0; i < data.ChapterCompleted.Length && i < _chapters.Count; i++)
                {
                    _chapters[i].IsCompleted = data.ChapterCompleted[i];
                }
            }
        }
        
        private bool[] GetCompletedChaptersArray()
        {
            var result = new bool[_chapters.Count];
            for (int i = 0; i < _chapters.Count; i++)
            {
                result[i] = _chapters[i].IsCompleted;
            }
            return result;
        }
    }
    
    /// <summary>
    /// ì±•í„° ?˜ìŠ¤???€???°ì´??
    /// </summary>
    [Serializable]
    public class ChapterQuestSaveData
    {
        public ChapterType CurrentChapter;
        public int CurrentDay;
        public bool[] ChapterCompleted;
    }
    
    /// <summary>
    /// ì±•í„° ?œì‘ ?´ë²¤??
    /// </summary>
    public class ChapterStartedEvent
    {
        public ChapterType Chapter { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
    
    /// <summary>
    /// ì±•í„° ?„ë£Œ ?´ë²¤??
    /// </summary>
    public class ChapterCompletedEvent
    {
        public ChapterType Chapter { get; set; }
        public string Title { get; set; }
    }
}
