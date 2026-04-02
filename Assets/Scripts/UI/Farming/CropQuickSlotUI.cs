using System;
using UnityEngine;
using UnityEngine.UI;
using DI;
using SunnysideIsland.Farming;
using SunnysideIsland.GameData;

namespace SunnysideIsland.UI.Farming
{
    [ExecuteAlways]
    public class CropQuickSlotUI : MonoBehaviour
    {
        [Inject] private ICropSelectionSystem _selectionSystem;
        
        [Header("=== UI Elements ===")]
        [SerializeField] private GameObject[] _slotObjects;
        
        [Header("=== Slot Settings ===")]
        [SerializeField] private string _slotContainerName = "SlotContainer";
        [SerializeField] private bool _autoFindSlots = true;
        
        [Header("=== Colors ===")]
        [SerializeField] private Color _selectedBackgroundColor = new Color(1f, 0.8f, 0.2f);
        [SerializeField] private Color _normalBackgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);
        [SerializeField] private Color _emptySlotColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        
        private Image[] _slotBackgrounds;
        private Image[] _cropIcons;
        private Text[] _slotNumbers;
        private Text[] _cropNames;
        
        private void Awake()
        {
            if (_autoFindSlots)
            {
                FindSlotElements();
            }
            
            if (Application.isPlaying)
            {
                DIContainer.Inject(this);
            }
            else
            {
                // 에디터 모드 전용 초기화 (가짜 데이터로 UI 미리보기 지원)
                UpdateAllSlots();
            }
        }

        private void Reset()
        {
            // 컴포넌트가 처음 추가되거나 Reset 버튼을 눌렀을 때 현재 Image의 색상을 가져옴
            FindSlotElements();
            if (_slotBackgrounds != null && _slotBackgrounds.Length > 0 && _slotBackgrounds[0] != null)
            {
                _normalBackgroundColor = _slotBackgrounds[0].color;
                _emptySlotColor = new Color(_normalBackgroundColor.r * 0.6f, _normalBackgroundColor.g * 0.6f, _normalBackgroundColor.b * 0.6f, _normalBackgroundColor.a);
            }
        }
        
        private void FindSlotElements()
        {
            if (_slotObjects == null || _slotObjects.Length == 0)
            {
                var container = transform.Find(_slotContainerName);
                if (container != null)
                {
                    _slotObjects = new GameObject[5];
                    for (int i = 0; i < 5; i++)
                    {
                        var slot = container.Find($"Slot{i + 1}");
                        if (slot != null)
                        {
                            _slotObjects[i] = slot.gameObject;
                        }
                    }
                }
            }
            
            if (_slotObjects != null)
            {
                int count = _slotObjects.Length;
                _slotBackgrounds = new Image[count];
                _cropIcons = new Image[count];
                _slotNumbers = new Text[count];
                _cropNames = new Text[count];
                
                for (int i = 0; i < count; i++)
                {
                    if (_slotObjects[i] == null) continue;
                    
                    _slotBackgrounds[i] = _slotObjects[i].GetComponent<Image>();
                    
                    var iconTransform = _slotObjects[i].transform.Find("Icon");
                    if (iconTransform != null) _cropIcons[i] = iconTransform.GetComponent<Image>();
                    
                    var numberTransform = _slotObjects[i].transform.Find("Number");
                    if (numberTransform != null) _slotNumbers[i] = numberTransform.GetComponent<Text>();
                    
                    var nameTransform = _slotObjects[i].transform.Find("Name");
                    if (nameTransform != null) _cropNames[i] = nameTransform.GetComponent<Text>();
                }
            }
        }
        
        private void Start()
        {
            UpdateAllSlots();
            
            if (_selectionSystem != null)
            {
                _selectionSystem.OnSelectionChanged += OnSelectionChanged;
            }
        }
        
        private void OnDestroy()
        {
            if (_selectionSystem != null)
            {
                _selectionSystem.OnSelectionChanged -= OnSelectionChanged;
            }
        }
        
        private void OnSelectionChanged(int index, CropData cropData)
        {
            UpdateAllSlots();
        }

        private void UpdateAllSlots()
        {
            int slotCount = (_selectionSystem != null) ? _selectionSystem.SlotCount : (_slotObjects != null ? _slotObjects.Length : 0);
            
            for (int i = 0; i < slotCount; i++)
            {
                UpdateSlot(i);
            }
        }
        
        private void UpdateSlot(int index)
        {
            CropData cropData = (_selectionSystem != null) ? _selectionSystem.GetCropData(index) : null;
            bool isSelected = (_selectionSystem != null) ? (index == _selectionSystem.SelectedIndex) : (index == 0); 
            bool hasCrop = cropData != null;
            
            // 배경색 처리
            if (_slotBackgrounds != null && index < _slotBackgrounds.Length && _slotBackgrounds[index] != null)
            {
                if (hasCrop)
                {
                    _slotBackgrounds[index].color = isSelected ? _selectedBackgroundColor : _normalBackgroundColor;
                }
                else
                {
                    _slotBackgrounds[index].color = isSelected ? _selectedBackgroundColor : _emptySlotColor;
                }
            }
            
            // 아이콘 처리
            if (_cropIcons != null && index < _cropIcons.Length && _cropIcons[index] != null)
            {
                if (hasCrop && cropData.growthSprites != null && cropData.growthSprites.Length > 0)
                {
                    _cropIcons[index].sprite = cropData.growthSprites[0];
                    _cropIcons[index].color = Color.white;
                }
                else
                {
                    _cropIcons[index].sprite = null;
                    _cropIcons[index].color = new Color(1, 1, 1, 0); 
                }
            }
            
            // 슬롯 번호 처리
            if (_slotNumbers != null && index < _slotNumbers.Length && _slotNumbers[index] != null)
            {
                _slotNumbers[index].text = (index + 1).ToString();
            }
            
            // 농작물 이름 처리
            if (_cropNames != null && index < _cropNames.Length && _cropNames[index] != null)
            {
                if (hasCrop)
                {
                    _cropNames[index].text = cropData.cropName;
                }
                else
                {
                    _cropNames[index].text = "-";
                }
            }
        }

        [ContextMenu("Sync Colors From First Slot")]
        private void SyncColorsFromFirstSlot()
        {
            FindSlotElements();
            if (_slotBackgrounds != null && _slotBackgrounds.Length > 0 && _slotBackgrounds[0] != null)
            {
                _normalBackgroundColor = _slotBackgrounds[0].color;
                Debug.Log("[CropQuickSlotUI] Synced _normalBackgroundColor from Slot1 Image component.");
                UpdateAllSlots();
            }
        }
        
        public void SetSelectionSystem(ICropSelectionSystem system)
        {
            if (_selectionSystem != null)
            {
                _selectionSystem.OnSelectionChanged -= OnSelectionChanged;
            }
            
            _selectionSystem = system;
            
            if (_selectionSystem != null)
            {
                _selectionSystem.OnSelectionChanged += OnSelectionChanged;
                UpdateAllSlots();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                UnityEditor.EditorApplication.delayCall -= UpdateAllSlots;
                UnityEditor.EditorApplication.delayCall += UpdateAllSlots;
            }
        }
#endif
    }
}