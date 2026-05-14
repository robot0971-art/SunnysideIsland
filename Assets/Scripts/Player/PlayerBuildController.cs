using System.Collections;
using SunnysideIsland.Building;
using SunnysideIsland.Events;
using UnityEngine;

namespace SunnysideIsland.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(PlayerMovement))]
    public sealed class PlayerBuildController : MonoBehaviour
    {
        private static readonly int AnimHammer = Animator.StringToHash("Hammer");

        private Rigidbody2D _rb;
        private Animator _animator;
        private UnityEngine.AI.NavMeshAgent _navMeshAgent;
        private PlayerMovement _movement;
        private BuildingSystem _buildingSystem;
        private float _buildStandOffDistance = 0.08f;
        private Vector3 _buildTargetPosition;
        private DetailedBuildingData _pendingBuildingData;
        private Vector3Int _pendingGridPosition;
        private Coroutine _buildCoroutine;

        public bool IsBuilding { get; private set; }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
            _movement = GetComponent<PlayerMovement>();
            _navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<BuildingPlaceRequestedEvent>(OnBuildingPlaceRequested);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<BuildingPlaceRequestedEvent>(OnBuildingPlaceRequested);
        }

        public void Configure(
            Rigidbody2D rb,
            Animator animator,
            UnityEngine.AI.NavMeshAgent navMeshAgent,
            PlayerMovement movement,
            BuildingSystem buildingSystem,
            float buildStandOffDistance)
        {
            _rb = rb != null ? rb : GetComponent<Rigidbody2D>();
            _animator = animator != null ? animator : GetComponent<Animator>();
            _navMeshAgent = navMeshAgent != null ? navMeshAgent : GetComponent<UnityEngine.AI.NavMeshAgent>();
            _movement = movement != null ? movement : GetComponent<PlayerMovement>();
            _buildingSystem = buildingSystem;
            _buildStandOffDistance = buildStandOffDistance;

            if (_navMeshAgent != null)
            {
                _navMeshAgent.updateRotation = false;
                _navMeshAgent.updateUpAxis = false;
            }
        }

        public void CancelIfManualInput(Vector2 inputVector)
        {
            if (IsBuilding && inputVector.sqrMagnitude > 0.01f)
            {
                CancelBuilding();
            }
        }

        public void CancelBuilding()
        {
            if (!IsBuilding)
            {
                return;
            }

            if (_buildCoroutine != null)
            {
                StopCoroutine(_buildCoroutine);
                _buildCoroutine = null;
            }

            IsBuilding = false;
            _movement.CanMove = true;
            _pendingBuildingData = null;

            if (_navMeshAgent != null && _navMeshAgent.isOnNavMesh)
            {
                _navMeshAgent.ResetPath();
            }

            _movement.Stop();
        }

        private void OnBuildingPlaceRequested(BuildingPlaceRequestedEvent evt)
        {
            if (IsBuilding)
            {
                return;
            }

            IsBuilding = true;
            _movement.CanMove = false;
            _pendingBuildingData = _buildingSystem?.GetBuildingData(evt.BuildingId);
            _pendingGridPosition = evt.GridPosition;

            float width = _pendingBuildingData != null ? _pendingBuildingData.Size.Width : 1f;
            float height = _pendingBuildingData != null ? _pendingBuildingData.Size.Height : 1f;
            Vector3 buildingCenter = new(evt.GridPosition.x + width * 0.5f, evt.GridPosition.y + height * 0.5f, 0f);

            Vector3 dirToCenter = (buildingCenter - transform.position).normalized;
            if (dirToCenter == Vector3.zero)
            {
                dirToCenter = Vector3.down;
            }

            float rayDistX = dirToCenter.x != 0f ? (width * 0.5f) / Mathf.Abs(dirToCenter.x) : float.MaxValue;
            float rayDistY = dirToCenter.y != 0f ? (height * 0.5f) / Mathf.Abs(dirToCenter.y) : float.MaxValue;
            float boxRadius = Mathf.Min(rayDistX, rayDistY);
            float standOffDistance = Mathf.Max(0.02f, _buildStandOffDistance);
            float safeRadius = Mathf.Max(standOffDistance, boxRadius + standOffDistance);
            _buildTargetPosition = buildingCenter - dirToCenter * safeRadius;

            _movement.SetFacingDirection(dirToCenter);

            bool navMeshReady = _navMeshAgent != null && _navMeshAgent.isOnNavMesh;
            if (navMeshReady)
            {
                _navMeshAgent.SetDestination(_buildTargetPosition);
            }

            if (_buildCoroutine != null)
            {
                StopCoroutine(_buildCoroutine);
            }

            _buildCoroutine = StartCoroutine(MoveToAndBuildRoutine(navMeshReady));
        }

        private IEnumerator MoveToAndBuildRoutine(bool useNavMesh)
        {
            while (useNavMesh
                ? (_navMeshAgent.pathPending || _navMeshAgent.remainingDistance > 0.1f)
                : Vector3.Distance(transform.position, _buildTargetPosition) > 0.1f)
            {
                Vector3 direction = (_buildTargetPosition - transform.position).normalized;
                if (!useNavMesh)
                {
                    _rb.linearVelocity = direction * _movement.GetMoveSpeed();
                }

                yield return null;
            }

            _movement.Stop();

            if (useNavMesh)
            {
                _navMeshAgent.ResetPath();
            }
            else
            {
                _rb.linearVelocity = Vector2.zero;
            }

            int maxHammerCount = _pendingBuildingData != null && _pendingBuildingData.BuildingId == "campfire" ? 2 : 4;
            for (int i = 0; i < maxHammerCount; i++)
            {
                _animator.SetTrigger(AnimHammer);
                yield return new WaitForSeconds(0.5f);
                while (_animator.GetCurrentAnimatorStateInfo(0).IsName("hamering"))
                {
                    yield return null;
                }
            }

            if (_pendingBuildingData != null)
            {
                EventBus.Publish(new BuildingPlaceConfirmEvent
                {
                    BuildingId = _pendingBuildingData.BuildingId,
                    GridPosition = _pendingGridPosition
                });
            }

            IsBuilding = false;
            _movement.CanMove = true;
            _pendingBuildingData = null;
            _buildCoroutine = null;
        }
    }
}
