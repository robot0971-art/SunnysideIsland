using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using TMPro;
using SunnysideIsland.Building;

namespace SunnysideIsland.UI.Building
{
    [ExecuteAlways]
    public class BuildingCard : MonoBehaviour
    {
        [Header("=== References ===")]
        [SerializeField] private Transform _iconContainer;
        [SerializeField] private TextMeshProUGUI _buildingNameText;
        [SerializeField] private GameObject _lockedOverlay;
        [SerializeField] private Image _selectionOverlay;
        [SerializeField] private Button _button;
        
        [Header("=== Appearance Settings ===")]
        [SerializeField] private bool _stretchIconToContainer = true;
        [SerializeField] private bool _preserveIconAspect = true;
        
        [Header("=== Icon Sprite (Optional) ===")]
        [Tooltip("Inspector에서 직접 할당하면 Addressables 대신 이 스프라이트를 사용합니다")]
        [SerializeField] private Sprite _iconSprite;
        
        public DetailedBuildingData Data { get; private set; }
        public bool IsUnlocked { get; private set; }
        
        public event Action<BuildingCard> OnClicked;
        
        private AsyncOperationHandle<GameObject> _iconHandle;
        private GameObject _currentIconInstance;

        private void Awake()
        {
            if (_button != null)
            {
                _button.onClick.AddListener(OnButtonClicked);
            }
            
            if (_iconContainer == null)
            {
                _iconContainer = transform;
            }
            
            SetSelected(false);
        }

        private void OnDestroy()
        {
            CleanupIcon();
            
#if UNITY_EDITOR
            if (UnityEditor.Selection.activeGameObject != null)
            {
                var sel = UnityEditor.Selection.activeGameObject;
                if (sel == gameObject || (transform != null && sel.transform.IsChildOf(transform)))
                {
                    UnityEditor.Selection.activeGameObject = null;
                }
            }
#endif
        }

        private void CleanupIcon()
        {
            if (_currentIconInstance != null)
            {
                if (Application.isPlaying)
                    Destroy(_currentIconInstance);
                else
                    DestroyImmediate(_currentIconInstance);
                
                _currentIconInstance = null;
            }

            // 에디터 잔상 및 추적되지 않은 모든 자식 오브젝트 제거
            if (_iconContainer != null)
            {
                for (int i = _iconContainer.childCount - 1; i >= 0; i--)
                {
                    Transform child = _iconContainer.GetChild(i);
                    if (child == null) continue;
                    
                    if (Application.isPlaying)
                        Destroy(child.gameObject);
                    else
                        DestroyImmediate(child.gameObject);
                }
            }

            // Addressables 핸들 해제
            try
            {
                if (_iconHandle.IsValid())
                {
                    Addressables.Release(_iconHandle);
                }
            }
            catch
            {
                // 핸들이 유효하지 않으면 무시
            }
        }

        
        public void Setup(DetailedBuildingData data, AssetReferenceGameObject houseRef, AssetReferenceGameObject bigHouseRef, AssetReferenceGameObject boatRef, AssetReferenceGameObject campfireRef)
        {
            Data = data;
            
            if (_buildingNameText != null)
            {
                _buildingNameText.text = data.BuildingName;
            }
            
            if (_iconContainer != null)
            {
                // 1. 데이터베이스의 스프라이트 우선 적용
                if (data.IconSprite != null)
                {
                    _iconSprite = data.IconSprite;
                }

                Debug.Log($"[BuildingCard] [{gameObject.name}] Setup: {data.BuildingName}, _iconSprite: {(_iconSprite != null ? "Yes" : "No")}");
                
                // 2. 스프라이트가 있다면 사용
                if (_iconSprite != null)
                {
                    SetupSpriteIcon();
                }
                // 3. 스프라이트가 없다면 Addressables 사용
                else
                {
                    AssetReferenceGameObject targetRef = null;
                    
                    // 판별 로직
                    if (data.BuildingId.ToLower().Contains("campfire") || data.BuildingName.Contains("모닥불"))
                    {
                        targetRef = campfireRef;
                    }
                    else if (data.BuildingId.Contains("Boat") || data.BuildingName.Contains("배"))
                    {
                        targetRef = boatRef;
                    }
                    else if (data.BuildingId.Contains("LargeHouse") || data.BuildingId.Contains("BigHouse") || data.BuildingName.Contains("큰 집"))
                    {
                        targetRef = bigHouseRef;
                    }
                    
                    // 기본값: 집
                    if (targetRef == null)
                    {
                        targetRef = houseRef;
                    }

                    if (targetRef != null && targetRef.RuntimeKeyIsValid())
                    {
                        LoadAndInstantiateIcon(targetRef);
                    }
                    else
                    {
                        Debug.LogWarning($"[BuildingCard] [{gameObject.name}] No valid icon source found for {data.BuildingName}");
                    }
                }
            }
            
            IsUnlocked = data.IsUnlockedDefault;
            if (_lockedOverlay != null) _lockedOverlay.SetActive(!IsUnlocked);
        }

        private void LoadAndInstantiateIcon(AssetReferenceGameObject assetRef)
        {
            CleanupIcon();

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                var prefab = assetRef.editorAsset as GameObject;
                
                if (prefab == null && assetRef.RuntimeKeyIsValid())
                {
                    var path = UnityEditor.AssetDatabase.GUIDToAssetPath(assetRef.ToString());
                    if (!string.IsNullOrEmpty(path))
                    {
                        prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    }
                }
                
                if (prefab != null)
                {
                    _currentIconInstance = Instantiate(prefab, _iconContainer);
                    if (_currentIconInstance != null)
                    {
                        _currentIconInstance.hideFlags = HideFlags.HideAndDontSave;
                        SetupIconInstance();
                    }
                }
                return;
            }
#endif

            _iconHandle = Addressables.InstantiateAsync(assetRef, _iconContainer);
            _iconHandle.Completed += (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    _currentIconInstance = handle.Result;
                    SetupIconInstance();
                }
            };
        }

        private void SetupIconInstance()
        {
            if (_currentIconInstance == null) return;
            
            var rect = _currentIconInstance.GetComponent<RectTransform>();
            if (rect == null)
            {
                rect = _currentIconInstance.AddComponent<RectTransform>();
            }
            
            if (_iconContainer == null)
            {
                Debug.LogError($"[{gameObject.name}] _iconContainer is null! Cannot setup icon.");
                return;
            }
            
            if (_stretchIconToContainer)
            {
                float dataIconScale = (Data != null) ? Data.IconScale : 1f;
                float margin = (1f - dataIconScale) * 0.5f;
                rect.anchorMin = new Vector2(margin, margin);
                rect.anchorMax = new Vector2(1f - margin, 1f - margin);
                rect.anchoredPosition = Vector2.zero;
                rect.localScale = Vector3.one;
                rect.offsetMin = rect.offsetMax = Vector2.zero;
                
                Debug.Log($"[BuildingCard] [{gameObject.name}] SetupIconInstance - Margin applied: {margin}, DataScale: {dataIconScale}");
            }
            else
            {
                rect.sizeDelta = new Vector2(100, 100) * (Data != null ? Data.IconScale : 1f);
                rect.anchoredPosition = Vector2.zero;
            }
            
            var sr = _currentIconInstance.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                var img = _currentIconInstance.GetComponent<Image>();
                if (img == null)
                {
                    img = _currentIconInstance.AddComponent<Image>();
                }
                img.sprite = sr.sprite;
                img.preserveAspect = _preserveIconAspect;
                img.enabled = true;
                sr.enabled = false;
            }
        }
        
        /// <summary>
        /// Inspector에서 할당한 Sprite를 아이콘으로 설정
        /// </summary>
        private void SetupSpriteIcon()
        {
            try
            {
                if (_iconSprite == null || _iconContainer == null)
                {
                    Debug.LogWarning($"[BuildingCard] [{gameObject.name}] SetupSpriteIcon failed - _iconSprite: {_iconSprite == null}, _iconContainer: {_iconContainer == null}");
                    return;
                }
                
                CleanupIcon();
                
                var iconObj = new GameObject("Icon_Sprite");
                iconObj.transform.SetParent(_iconContainer, false);
                
                var rect = iconObj.AddComponent<RectTransform>();
                if (_stretchIconToContainer)
                {
                    float dataIconScale = (Data != null) ? Data.IconScale : 1f;
                    float margin = (1f - dataIconScale) * 0.5f;
                    rect.anchorMin = new Vector2(margin, margin);
                    rect.anchorMax = new Vector2(1f - margin, 1f - margin);
                    rect.anchoredPosition = Vector2.zero;
                    rect.localScale = Vector3.one;
                    rect.offsetMin = rect.offsetMax = Vector2.zero;
                    
                    Debug.Log($"[BuildingCard] [{gameObject.name}] SetupSpriteIcon - Margin applied: {margin}, DataScale: {dataIconScale}");
                }
                else
                {
                    rect.sizeDelta = new Vector2(100, 100) * (Data != null ? Data.IconScale : 1f);
                    rect.anchoredPosition = Vector2.zero;
                }
                
                var img = iconObj.AddComponent<Image>();
                img.sprite = _iconSprite;
                img.preserveAspect = _preserveIconAspect;
                img.enabled = true;
                
                _currentIconInstance = iconObj;
                Debug.Log($"[BuildingCard] [{gameObject.name}] SetupSpriteIcon SUCCESS - Icon created.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[BuildingCard] SetupSpriteIcon EXCEPTION: {ex.Message}");
            }
        }
        
        public void SetSelected(bool selected)
        {
            if (_selectionOverlay != null)
            {
                _selectionOverlay.enabled = selected;
            }
        }
        
        public void SetUnlocked(bool unlocked)
        {
            IsUnlocked = unlocked;
            if (_lockedOverlay != null)
            {
                _lockedOverlay.SetActive(!unlocked);
            }
        }
        
        private void OnButtonClicked()
        {
            OnClicked?.Invoke(this);
        }
    }
}