using System.Collections;
using SunnysideIsland.Building;
using SunnysideIsland.Core;
using SunnysideIsland.Environment;
using SunnysideIsland.Farming;
using SunnysideIsland.Items;
using SunnysideIsland.Pool;
using UnityEngine;

namespace SunnysideIsland.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerInteraction : MonoBehaviour
    {
        private static readonly int AnimWater = Animator.StringToHash("Water");

        private Animator _animator;
        private PlayerMovement _movement;
        private ICropSelectionSystem _cropSelectionSystem;
        private FarmingManager _farmingManager;
        private Grid _grid;
        private IPoolManager _poolManager;
        private GameObject _plotPrefab;
        private LayerMask _interactableLayer;
        private LayerMask _farmingLayer;
        private LayerMask _treeLayer;
        private LayerMask _harvestLayer;
        private LayerMask _groundLayer;

        public void Configure(
            Animator animator,
            PlayerMovement movement,
            ICropSelectionSystem cropSelectionSystem,
            FarmingManager farmingManager,
            Grid grid,
            IPoolManager poolManager,
            GameObject plotPrefab,
            LayerMask interactableLayer,
            LayerMask farmingLayer,
            LayerMask treeLayer,
            LayerMask harvestLayer,
            LayerMask groundLayer)
        {
            _animator = animator != null ? animator : GetComponent<Animator>();
            _movement = movement != null ? movement : GetComponent<PlayerMovement>();
            _cropSelectionSystem = cropSelectionSystem;
            _farmingManager = farmingManager;
            _grid = grid;
            _poolManager = poolManager;
            _plotPrefab = plotPrefab;
            _interactableLayer = interactableLayer;
            _farmingLayer = farmingLayer;
            _treeLayer = treeLayer;
            _harvestLayer = harvestLayer;
            _groundLayer = groundLayer;
        }

        public void HandlePrimaryAction()
        {
            bool workDone = TryInteract();

            if (!workDone && _movement != null && !_movement.IsSwimming)
            {
                workDone = ExecuteWateringWithResult();
                if (!workDone)
                {
                    TryCreatePlot();
                }
            }
        }

        private void TryCreatePlot()
        {
            Vector2 origin = (Vector2)transform.position + new Vector2(0f, 0.1f);
            Vector2 rawTargetPos = origin + _movement.FacingDirection;
            Vector2 targetPos = rawTargetPos;

            if (_grid != null)
            {
                Vector3Int cellPos = _grid.WorldToCell(rawTargetPos);
                targetPos = _grid.GetCellCenterWorld(cellPos);
            }

            Collider2D treeCheck = Physics2D.OverlapCircle(targetPos, 0.4f, _treeLayer);
            Collider2D harvestCheck = Physics2D.OverlapCircle(targetPos, 0.4f, _harvestLayer);
            if (treeCheck != null || harvestCheck != null)
            {
                return;
            }

            Collider2D groundCheck = Physics2D.OverlapPoint(targetPos, _groundLayer);
            if (groundCheck == null)
            {
                return;
            }

            Collider2D overlap = Physics2D.OverlapCircle(targetPos, 0.2f, _interactableLayer);
            if (overlap != null)
            {
                return;
            }

            _animator.SetTrigger("Dig");
            StartCoroutine(DelayedCreatePlot(targetPos, 0.4f));
        }

        private bool ExecuteWateringWithResult()
        {
            Vector2 origin = (Vector2)transform.position + new Vector2(0f, 0.2f);
            RaycastHit2D hit = Physics2D.Raycast(origin, _movement.FacingDirection, 0.7f, _farmingLayer);

            if (hit.collider == null || !hit.collider.TryGetComponent(out FarmPlot _))
            {
                return false;
            }

            ExecuteWatering();
            return true;
        }

        private bool TryInteract()
        {
            Vector2 origin = (Vector2)transform.position + new Vector2(0f, 0.1f);
            const float radius = 0.8f;
            const float distance = 1.5f;

            Campfire nearestCampfire = FindNearbyCampfire(origin);
            bool isNearCampfire = nearestCampfire != null;
            IInteractable targetInteractable = FindTargetInteractable(origin, isNearCampfire, nearestCampfire);

            if (targetInteractable != null)
            {
                if (targetInteractable.CanInteract())
                {
                    targetInteractable.Interact();
                    return true;
                }

                if (isNearCampfire)
                {
                    return true;
                }
            }

            if (isNearCampfire)
            {
                return true;
            }

            if (TryHarvestPickable(origin, radius, distance))
            {
                return true;
            }

            if (TryChopTree(origin, radius, distance))
            {
                return true;
            }

            return TryHandleFarmPlot(origin);
        }

        private Campfire FindNearbyCampfire(Vector2 origin)
        {
            Collider2D[] nearbyInteractables = Physics2D.OverlapCircleAll(origin, 2.0f, _interactableLayer);
            foreach (var col in nearbyInteractables)
            {
                if (col.TryGetComponent(out Campfire campfire))
                {
                    return campfire;
                }
            }

            return null;
        }

        private IInteractable FindTargetInteractable(Vector2 origin, bool isNearCampfire, Campfire nearestCampfire)
        {
            RaycastHit2D hit = Physics2D.CircleCast(origin, 0.4f, _movement.FacingDirection, 2.0f, _interactableLayer);
            if (hit.collider != null)
            {
                if (hit.collider.TryGetComponent(out IInteractable targetInteractable))
                {
                    return targetInteractable;
                }

                return hit.collider.GetComponentInParent<IInteractable>();
            }

            return isNearCampfire ? nearestCampfire : null;
        }

        private bool TryHarvestPickable(Vector2 origin, float radius, float distance)
        {
            RaycastHit2D[] harvestHits = Physics2D.CircleCastAll(
                origin,
                radius,
                _movement.FacingDirection,
                distance,
                _harvestLayer);

            if (harvestHits.Length == 0 || !harvestHits[0].collider.TryGetComponent(out PickableItem item))
            {
                return false;
            }

            item.PickUp();
            _animator.SetTrigger("Harvest");
            return true;
        }

        private bool TryChopTree(Vector2 origin, float radius, float distance)
        {
            RaycastHit2D[] treeHits = Physics2D.CircleCastAll(origin, radius, _movement.FacingDirection, distance, _treeLayer);
            if (treeHits.Length == 0 || !treeHits[0].collider.TryGetComponent(out HarvestableTree tree))
            {
                return false;
            }

            if (tree.IsChopped)
            {
                return false;
            }

            tree.Chop();
            _animator.SetTrigger("Axe");
            return true;
        }

        private bool TryHandleFarmPlot(Vector2 origin)
        {
            RaycastHit2D farmHit = Physics2D.Raycast(origin, _movement.FacingDirection, 1.0f, _interactableLayer);
            if (farmHit.collider == null || !farmHit.collider.TryGetComponent(out FarmPlot plot))
            {
                return false;
            }

            if (plot.IsEmpty)
            {
                PlantSelectedCrop(plot);
                return true;
            }

            if (plot.IsReady)
            {
                plot.Harvest();
                _animator.SetTrigger("Harvest");
            }
            else
            {
                ExecuteWatering();
            }

            return true;
        }

        private void PlantSelectedCrop(FarmPlot plot)
        {
            var selectedCrop = _cropSelectionSystem?.SelectedCrop;
            if (selectedCrop == null)
            {
                Debug.LogWarning("[PlayerInteraction] No crop selected");
                return;
            }

            if (_cropSelectionSystem.TryConsume(_cropSelectionSystem.SelectedIndex, 1))
            {
                plot.Plant(selectedCrop.seedItemId, selectedCrop);
                return;
            }

            Debug.LogWarning(
                $"[PlayerInteraction] Not enough {_cropSelectionSystem.SelectedCrop?.cropName} seeds (x{_cropSelectionSystem.GetCount(_cropSelectionSystem.SelectedIndex)})");
        }

        private void ExecuteWatering()
        {
            Vector2 origin = (Vector2)transform.position + new Vector2(0f, 0.2f);
            RaycastHit2D hit = Physics2D.Raycast(origin, _movement.FacingDirection, 0.7f, _farmingLayer);

            if (hit.collider == null)
            {
                return;
            }

            _animator.SetTrigger(AnimWater);
            if (hit.collider.TryGetComponent(out FarmPlot plot))
            {
                StartCoroutine(DelayedWatering(plot, 0.5f));
            }
        }

        private IEnumerator DelayedWatering(FarmPlot plot, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (plot != null)
            {
                plot.Water();
            }
        }

        private IEnumerator DelayedCreatePlot(Vector2 spawnPos, float delay)
        {
            for (int i = 0; i < 5; i++)
            {
                _animator.ResetTrigger("Dig");
                _animator.SetTrigger("Dig");
                yield return new WaitForSeconds(delay);
            }

            const string poolName = "FarmPlot";
            GameObject newPlot;

            if (_poolManager != null)
            {
                if (_poolManager.GetPool(poolName) == null)
                {
                    _poolManager.CreatePool(poolName, _plotPrefab, 20, 100);
                }

                newPlot = _poolManager.Spawn(poolName, new Vector3(spawnPos.x, spawnPos.y, 0f), Quaternion.identity);
            }
            else
            {
                newPlot = Instantiate(_plotPrefab, new Vector3(spawnPos.x, spawnPos.y, 0f), Quaternion.identity);
            }

            if (newPlot != null && newPlot.TryGetComponent(out FarmPlot plotScript))
            {
                plotScript.Clear();
                _farmingManager?.RegisterPlot(plotScript);
            }
        }
    }
}
