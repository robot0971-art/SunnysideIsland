using System.Collections.Generic;
using UnityEngine;
using SunnysideIsland.Events;
using SunnysideIsland.Core;
using DI;
using Newtonsoft.Json.Linq;

namespace SunnysideIsland.Building
{
    /// <summary>
    /// 건물 관리자
    /// </summary>
    public class BuildingManager : MonoBehaviour, ISaveable
    {
        [Header("=== Settings ===")]
        [SerializeField] private List<Building> _buildings = new List<Building>();
        [SerializeField] private Transform _buildingParent;

        [Inject] private BuildingDatabase _buildingDatabase;

        public string SaveKey => "BuildingManager";
        
        private void Awake()
        {
            _buildings.RemoveAll(b => b == null || !b.gameObject.scene.IsValid());
        }
        
        private void Start()
        {
            DIContainer.Inject(this);
            EventBus.Subscribe<DayStartedEvent>(OnDayStarted);
        }
        
        private void OnDestroy()
        {
            EventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
        }
        
        private void OnDayStarted(DayStartedEvent evt)
        {
            foreach (var building in _buildings)
            {
                if (building != null && 
                    (building.State == BuildingState.Constructing || 
                     building.State == BuildingState.Upgrading))
                {
                    building.ProgressConstruction();
                }
            }
        }
        
        public void RegisterBuilding(Building building)
        {
            if (building != null && !_buildings.Contains(building))
            {
                _buildings.Add(building);
            }
        }
        
        public void UnregisterBuilding(Building building)
        {
            _buildings.Remove(building);
        }

        public object GetSaveData()
        {
            var dataList = new List<BuildingSaveData>();
            foreach (var building in _buildings)
            {
                if (building != null)
                {
                    dataList.Add(building.GetSaveData() as BuildingSaveData);
                }
            }
            return dataList;
        }

        public void LoadSaveData(object state)
        {
            var dataList = state as List<BuildingSaveData>;
            if (dataList == null && state is JArray jArray)
            {
                dataList = jArray.ToObject<List<BuildingSaveData>>();
            }

            if (dataList == null) return;

            // Clear ALL buildings under _buildingParent (scene instances only)
            if (_buildingParent != null)
            {
                var existingBuildings = _buildingParent.GetComponentsInChildren<Building>();
                for (int i = existingBuildings.Length - 1; i >= 0; i--)
                {
                    if (existingBuildings[i] != null && existingBuildings[i].gameObject.scene.IsValid())
                    {
                        Destroy(existingBuildings[i].gameObject);
                    }
                }
            }
            // Also clear from _buildings list
            _buildings.Clear();

            // Recreate buildings
            foreach (var data in dataList)
            {
                var buildingData = _buildingDatabase.GetBuilding(data.BuildingId);
                if (buildingData != null && buildingData.BuildingPrefab != null)
                {
                    Vector3 worldPos = new Vector3(data.GridPosition.x, data.GridPosition.y, 0); // Note: Simple pos for now, should ideally match BuildingPlacer logic
                    GameObject go = Instantiate(buildingData.BuildingPrefab, worldPos, Quaternion.identity, _buildingParent);
                    go.name = $"Building_{data.BuildingId}";
                    
                    // Apply PreviewScale like BuildingPlacer does
                    go.transform.localScale = go.transform.localScale * buildingData.PreviewScale;
                    
                    if (go.TryGetComponent(out Building building))
                    {
                        building.SetBuildingData(buildingData);
                        building.LoadSaveData(data);
                        RegisterBuilding(building);
                    }
                }
            }
        }
    }
}
