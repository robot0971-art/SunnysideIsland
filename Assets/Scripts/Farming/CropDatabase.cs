using System.Collections.Generic;
using UnityEngine;
using SunnysideIsland.GameData;

namespace SunnysideIsland.Farming
{
    /// <summary>
    /// 모든 CropData를 관리하는 데이터베이스
    /// </summary>
    [CreateAssetMenu(fileName = "CropDatabase", menuName = "Farming/Crop Database")]
    public class CropDatabase : ScriptableObject
    {
        [Header("=== All Crops ===")]
        [SerializeField] private List<CropData> _crops = new List<CropData>();

        public List<CropData> GetAllCrops() => _crops;

        public CropData GetCrop(string cropId)
        {
            if (string.IsNullOrEmpty(cropId)) return null;
            
            foreach (var crop in _crops)
            {
                if (crop != null && crop.cropId == cropId)
                    return crop;
            }
            return null;
        }

        public void AddCrop(CropData crop)
        {
            if (crop != null && !_crops.Contains(crop))
            {
                _crops.Add(crop);
            }
        }

        public void RemoveCrop(CropData crop)
        {
            _crops.Remove(crop);
        }
    }
}
