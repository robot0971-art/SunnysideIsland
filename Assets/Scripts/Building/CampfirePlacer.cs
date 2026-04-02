using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using SunnysideIsland.Events;
using SunnysideIsland.Pool;

namespace SunnysideIsland.Building
{
    /// <summary>
    /// Campfire 배치 시스템
    /// Preview → Hammer 애니메이션 2회 → Base 생성
    /// </summary>
    public class CampfirePlacer : MonoBehaviour
    {
        [Header("=== Tilemap References ===")]
        [SerializeField] private Tilemap _groundTilemap;
        [SerializeField] private Tilemap _sandTilemap;
        [SerializeField] private Transform _buildingParent;

        [Header("=== Layers ===")]
        [SerializeField] private LayerMask _treeLayer;
        [SerializeField] private LayerMask _buildingLayer;

        [Header("=== Preview ===")]
        [SerializeField] private Sprite _campfireBaseSprite;
        [Tooltip("미리보기 오브젝트의 스케일")]
        [SerializeField] private Vector3 _previewScale = Vector3.one;

        [Header("=== Campfire Prefab ===")]
        [SerializeField] private GameObject _campfirePrefab;
        [Tooltip("생성된 Campfire의 스케일")]
        [SerializeField] private Vector3 _campfireScale = Vector3.one;

        [Header("=== Input ===")]
        [SerializeField] private InputActionAsset _inputActions;

        [Header("=== Player ===")]
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private Animator _playerAnimator;

        public bool IsInPlacementMode => _isInPlacementMode;

        private bool _isInPlacementMode = false;
        private GameObject _previewObject;
        private SpriteRenderer _previewRenderer;
        private Vector3Int _currentGridPosition;
        private bool _canPlace;
        private string _placementFailReason;

        private InputAction _clickAction;
        private InputAction _pointAction;
        private InputAction _cancelAction;

        private BuildingSystem _buildingSystem;

        private static readonly int AnimHammer = Animator.StringToHash("Hammer");

        public void Initialize(BuildingSystem buildingSystem)
        {
            _buildingSystem = buildingSystem;

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
                _playerAnimator = player.GetComponent<Animator>();
                
                if (_playerAnimator == null)
                {
                    _playerAnimator = player.GetComponentInChildren<Animator>();
                }
                
                Debug.Log($"[CampfirePlacer] Initialize - Player: {player.name}, Animator: {(_playerAnimator != null ? _playerAnimator.gameObject.name : "NULL")}");
            }
            else
            {
                Debug.LogWarning("[CampfirePlacer] Player not found!");
            }

            SetupInputActions();
        }

        private void SetupInputActions()
        {
            if (_inputActions == null) return;

            var uiMap = _inputActions.FindActionMap("UI");
            if (uiMap != null)
            {
                _clickAction = uiMap.FindAction("Click");
                _pointAction = uiMap.FindAction("Point");
                _cancelAction = uiMap.FindAction("Cancel");
            }
        }

        private void OnEnable()
        {
            SetupInputActions();
            _clickAction?.Enable();
            _pointAction?.Enable();
            _cancelAction?.Enable();
            EventBus.Subscribe<BuildingPlaceConfirmEvent>(OnBuildingPlaceConfirm);
        }

        private void OnDisable()
        {
            _clickAction?.Disable();
            _pointAction?.Disable();
            _cancelAction?.Disable();
            EventBus.Unsubscribe<BuildingPlaceConfirmEvent>(OnBuildingPlaceConfirm);
        }

        private void OnBuildingPlaceConfirm(BuildingPlaceConfirmEvent evt)
        {
            if (evt.BuildingId.ToLower() != "campfire") return;

            Vector3 worldPos = _groundTilemap.GetCellCenterWorld(evt.GridPosition);
            CreateCampfireAt(worldPos);
        }

        private void CreateCampfireAt(Vector3 position)
        {
            if (_campfirePrefab == null)
            {
                Debug.LogError("[CampfirePlacer] Campfire prefab not assigned");
                return;
            }

            GameObject campfire;
            if (_buildingParent != null)
            {
                campfire = Instantiate(_campfirePrefab, position, Quaternion.identity, _buildingParent);
            }
            else
            {
                campfire = Instantiate(_campfirePrefab, position, Quaternion.identity);
            }

            campfire.transform.localScale = _campfireScale;
            Debug.Log($"[CampfirePlacer] Campfire created at {position} with scale {_campfireScale}");
        }

        /// <summary>
        /// Campfire 배치 모드 시작
        /// </summary>
        public void StartPlacement()
        {
            if (_groundTilemap == null)
            {
                Debug.LogError("[CampfirePlacer] Ground tilemap not assigned");
                return;
            }

            _isInPlacementMode = true;
            CreatePreview();

            Debug.Log("[CampfirePlacer] 모닥불을 놓을 위치를 선택하세요");
        }

        private void CreatePreview()
        {
            if (_previewObject != null)
            {
                Destroy(_previewObject);
            }

            _previewObject = new GameObject("CampfirePreview");
            _previewRenderer = _previewObject.AddComponent<SpriteRenderer>();

            if (_campfireBaseSprite != null)
            {
                _previewRenderer.sprite = _campfireBaseSprite;
            }

            _previewRenderer.sortingOrder = 100;
            _previewObject.transform.localScale = _previewScale;

            SetPreviewColor(false);
        }

        private void OnValidate()
        {
            if (_previewObject != null)
            {
                _previewObject.transform.localScale = _previewScale;
            }
        }

        private void Update()
        {
            if (!_isInPlacementMode || _previewObject == null) return;

            UpdatePreviewPosition();

            if (_cancelAction != null && _cancelAction.WasPressedThisFrame())
            {
                CancelPlacement();
                return;
            }

            if (_clickAction != null && _clickAction.WasPressedThisFrame())
            {
                TryPlaceCampfire();
            }
        }

        private void UpdatePreviewPosition()
        {
            if (_pointAction == null || _groundTilemap == null || _previewObject == null) return;

            Vector2 mousePosition = _pointAction.ReadValue<Vector2>();
            Vector3 worldPosition = UnityEngine.Camera.main.ScreenToWorldPoint(mousePosition);
            worldPosition.z = 0;

            Vector3Int cellPosition = _groundTilemap.WorldToCell(worldPosition);
            Vector3 worldPos = _groundTilemap.GetCellCenterWorld(cellPosition);

            _previewObject.transform.position = worldPos;
            _currentGridPosition = cellPosition;

            var result = CanPlaceAt(cellPosition);
            _canPlace = result.CanPlace;
            _placementFailReason = result.FailReason;
            SetPreviewColor(_canPlace);
        }

        private (bool CanPlace, string FailReason) CanPlaceAt(Vector3Int position)
        {
            if (_groundTilemap == null) return (false, "타일맵이 없습니다.");

            // Ground 타일 확인
            TileBase groundTile = _groundTilemap.GetTile(position);
            if (groundTile == null)
            {
                return (false, "땅 위에만 설치할 수 있습니다.");
            }

            // Sand 타일은 불가
            if (_sandTilemap != null)
            {
                TileBase sandTile = _sandTilemap.GetTile(position);
                if (sandTile != null)
                {
                    return (false, "모래 위에는 설치할 수 없습니다.");
                }
            }

            Vector3 worldCheckPos = _groundTilemap.GetCellCenterWorld(position);

            // 나무 확인
            Collider2D treeCollider = Physics2D.OverlapPoint(worldCheckPos, _treeLayer);
            if (treeCollider != null)
            {
                return (false, "나무가 있어 설치할 수 없습니다.");
            }

            // 다른 건물 확인
            Collider2D buildingCollider = Physics2D.OverlapPoint(worldCheckPos, _buildingLayer);
            if (buildingCollider != null)
            {
                return (false, "이미 건물이 있습니다.");
            }

            // Player 거리 확인
            if (_playerTransform != null)
            {
                float distance = Vector3.Distance(_playerTransform.position, worldCheckPos);
                if (distance > 4f)
                {
                    return (false, "너무 멀리 있습니다.");
                }
            }

            return (true, null);
        }

        private void SetPreviewColor(bool canPlace)
        {
            if (_previewRenderer == null) return;

            // BuildingPlacer와 동일한 색상
            Color color = canPlace ? new Color(0f, 1f, 0f, 0.5f) : new Color(1f, 0f, 0f, 0.5f);
            _previewRenderer.color = color;
        }

        private void TryPlaceCampfire()
        {
            if (!_canPlace)
            {
                if (!string.IsNullOrEmpty(_placementFailReason))
                {
                    Debug.Log($"[CampfirePlacer] {_placementFailReason}");
                }
                return;
            }

            // 직접 생성 대신 BuildingSystem과 동일한 이벤트 발행
            // 이를 통해 PlayerController가 측면으로 이동하여 망치질을 시작함
            EventBus.Publish(new BuildingPlaceRequestedEvent 
            { 
                BuildingId = "campfire", 
                GridPosition = _currentGridPosition 
            });

            // 프리뷰 제거 및 모드 종료
            Cleanup();
        }

        // 기존의 PlaceCampfireSequence와 CreateCampfireBase는 시스템 이벤트가 처리하므로 삭제 가능 (또는 주석 처리)

        private void CancelPlacement()
        {
            _isInPlacementMode = false;

            if (_previewObject != null)
            {
                Destroy(_previewObject);
                _previewObject = null;
            }

            EventBus.Publish(new BuildingPlacementCancelledEvent());
        }

        public void Cleanup()
        {
            if (_previewObject != null)
            {
                Destroy(_previewObject);
                _previewObject = null;
            }
            _isInPlacementMode = false;
        }
    }
}
