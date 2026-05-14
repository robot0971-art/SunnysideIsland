using System.Collections.Generic;
using UnityEngine;
using DI;
using SunnysideIsland.Core;
using SunnysideIsland.Events;
using SunnysideIsland.Inventory;
using Newtonsoft.Json.Linq;

namespace SunnysideIsland.Cooking
{
    /// <summary>
    /// ?”ë¦¬ ?ˆì‹œ??
    /// </summary>
    [System.Serializable]
    public class CookingRecipe
    {
        public string RecipeId;
        public string ResultItemId;
        public int ResultAmount;
        public int HungerRestore;
        public Dictionary<string, int> Ingredients;
    }

    /// <summary>
    /// ?”ë¦¬ ?œìŠ¤??
    /// </summary>
    public class CookingSystem : MonoBehaviour, ISaveable
    {
        [Header("=== Settings ===")]
        [SerializeField] private List<CookingRecipe> _recipes = new List<CookingRecipe>();
        
        [Inject(Optional = true)]
        private IInventorySystem _inventorySystem = default!;
        
        public string SaveKey => "CookingSystem";
        
        private void Start()
        {
            DIContainer.Inject(this);
            if (_inventorySystem == null && !DIContainer.TryResolve(out _inventorySystem))
            {
                Debug.LogWarning("[CookingSystem] IInventorySystem is not registered.");
            }
        }
        
        /// <summary>
        /// ?”ë¦¬ ê°€???¬ë? ?•ì¸
        /// </summary>
        public bool CanCook(string recipeId)
        {
            if (_inventorySystem == null) return false;

            var recipe = FindRecipe(recipeId);
            if (recipe == null) return false;
            
            // ?¬ë£Œ ?•ì¸
            foreach (var ingredient in recipe.Ingredients)
            {
                if (_inventorySystem.CountItem(ingredient.Key) < ingredient.Value)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// ?”ë¦¬?˜ê¸°
        /// </summary>
        public bool Cook(string recipeId)
        {
            if (!CanCook(recipeId)) return false;
            
            var recipe = FindRecipe(recipeId);
            
            // ?¬ë£Œ ?Œëª¨
            foreach (var ingredient in recipe.Ingredients)
            {
                if (!_inventorySystem.RemoveItem(ingredient.Key, ingredient.Value))
                {
                    return false;
                }
            }
            
            // ?Œì‹ ?ì„±
            _inventorySystem.AddItem(recipe.ResultItemId, recipe.ResultAmount);
            
            EventBus.Publish(new FoodCookedEvent
            {
                RecipeId = recipeId,
                ResultItemId = recipe.ResultItemId,
                Amount = recipe.ResultAmount,
                HungerRestore = recipe.HungerRestore
            });
            
            return true;
        }
        
        /// <summary>
        /// ê°€?¥í•œ ?ˆì‹œ??ëª©ë¡
        /// </summary>
        public List<CookingRecipe> GetAvailableRecipes()
        {
            var available = new List<CookingRecipe>();
            foreach (var recipe in _recipes)
            {
                if (CanCook(recipe.RecipeId))
                {
                    available.Add(recipe);
                }
            }
            return available;
        }
        
        /// <summary>
        /// ëª¨ë“  ?ˆì‹œ??
        /// </summary>
        public List<CookingRecipe> GetAllRecipes()
        {
            return new List<CookingRecipe>(_recipes);
        }
        
        private CookingRecipe FindRecipe(string recipeId)
        {
            foreach (var recipe in _recipes)
            {
                if (recipe.RecipeId == recipeId)
                    return recipe;
            }
            return null;
        }
        
        public void AddRecipe(CookingRecipe recipe)
        {
            if (recipe != null && FindRecipe(recipe.RecipeId) == null)
            {
                _recipes.Add(recipe);
            }
        }
        
        public object GetSaveData()
        {
            return new CookingSaveData
            {
                Recipes = _recipes
            };
        }
        
        public void LoadSaveData(object state)
        {
            var data = state as CookingSaveData ?? (state as JObject)?.ToObject<CookingSaveData>();
            if (data != null)
            {
                _recipes = data.Recipes ?? new List<CookingRecipe>();
            }
        }
    }
    
    [System.Serializable]
    public class CookingSaveData
    {
        public List<CookingRecipe> Recipes;
    }
    
    /// <summary>
    /// ?”ë¦¬ ?„ë£Œ ?´ë²¤??
    /// </summary>
    public class FoodCookedEvent
    {
        public string RecipeId { get; set; }
        public string ResultItemId { get; set; }
        public int Amount { get; set; }
        public int HungerRestore { get; set; }
    }
}
