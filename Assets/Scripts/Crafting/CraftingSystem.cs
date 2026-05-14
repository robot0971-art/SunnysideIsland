using System.Collections.Generic;
using UnityEngine;
using DI;
using SunnysideIsland.Core;
using SunnysideIsland.Events;
using SunnysideIsland.Inventory;
using Newtonsoft.Json.Linq;

namespace SunnysideIsland.Crafting
{
    /// <summary>
    /// ì¡°í•© ?ˆì‹œ??
    /// </summary>
    [System.Serializable]
    public class CraftingRecipe
    {
        public string RecipeId;
        public string ResultItemId;
        public int ResultAmount;
        public Dictionary<string, int> Ingredients; // ?„ì´??ID: ?˜ëŸ‰
        public float CraftTime; // ì¡°í•© ?œê°„ (ì´?
    }

    /// <summary>
    /// ì¡°í•© ?œìŠ¤??
    /// </summary>
    public class CraftingSystem : MonoBehaviour, ISaveable
    {
        [Header("=== Settings ===")]
        [SerializeField] private List<CraftingRecipe> _recipes = new List<CraftingRecipe>();
        
        [Inject(Optional = true)]
        private IInventorySystem _inventorySystem = default!;
        
        public string SaveKey => "CraftingSystem";
        
        private void Start()
        {
            DIContainer.Inject(this);
            if (_inventorySystem == null && !DIContainer.TryResolve(out _inventorySystem))
            {
                Debug.LogWarning("[CraftingSystem] IInventorySystem is not registered.");
            }
            AddDefaultRecipes();
        }

        private void AddDefaultRecipes()
        {
            // ë°??ˆì‹œ??ì¶”ê? (?†ì„ ê²½ìš°)
            if (!HasRecipe("boat"))
            {
                var boatRecipe = new CraftingRecipe
                {
                    RecipeId = "boat",
                    ResultItemId = "boat",
                    ResultAmount = 1,
                    Ingredients = new Dictionary<string, int> { { "wood", 50 } },
                    CraftTime = 3f
                };
                AddRecipe(boatRecipe);
            }
        }
        
        /// <summary>
        /// ?ˆì‹œ???•ì¸
        /// </summary>
        public bool HasRecipe(string recipeId)
        {
            return FindRecipe(recipeId) != null;
        }
        
        /// <summary>
        /// ì¡°í•© ê°€???¬ë? ?•ì¸
        /// </summary>
        public bool CanCraft(string recipeId)
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
        /// ?„ì´??ì¡°í•©
        /// </summary>
        public bool Craft(string recipeId)
        {
            if (!CanCraft(recipeId)) return false;
            
            var recipe = FindRecipe(recipeId);
            
            // ?¬ë£Œ ?Œëª¨
            foreach (var ingredient in recipe.Ingredients)
            {
                if (!_inventorySystem.RemoveItem(ingredient.Key, ingredient.Value))
                {
                    // ?¤íŒ¨ ??ë³µêµ¬ ë¡œì§ ?„ìš”
                    return false;
                }
            }
            
            // ê²°ê³¼ë¬??ì„±
            _inventorySystem.AddItem(recipe.ResultItemId, recipe.ResultAmount);
            
            EventBus.Publish(new ItemCraftedEvent
            {
                RecipeId = recipeId,
                ResultItemId = recipe.ResultItemId,
                Amount = recipe.ResultAmount
            });
            
            return true;
        }
        
        /// <summary>
        /// ?¤ì¤‘ ì¡°í•©
        /// </summary>
        public bool CraftMultiple(string recipeId, int count)
        {
            var recipe = FindRecipe(recipeId);
            if (recipe == null) return false;
            
            // ?¬ë£Œ ì¶©ë¶„?œì? ?•ì¸
            foreach (var ingredient in recipe.Ingredients)
            {
                if (_inventorySystem.CountItem(ingredient.Key) < ingredient.Value * count)
                {
                    return false;
                }
            }
            
            // ì¡°í•© ?¤í–‰
            for (int i = 0; i < count; i++)
            {
                if (!Craft(recipeId))
                {
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// ê°€?¥í•œ ?ˆì‹œ??ëª©ë¡
        /// </summary>
        public List<CraftingRecipe> GetAvailableRecipes()
        {
            var available = new List<CraftingRecipe>();
            foreach (var recipe in _recipes)
            {
                if (CanCraft(recipe.RecipeId))
                {
                    available.Add(recipe);
                }
            }
            return available;
        }
        
        /// <summary>
        /// ëª¨ë“  ?ˆì‹œ??
        /// </summary>
        public List<CraftingRecipe> GetAllRecipes()
        {
            return new List<CraftingRecipe>(_recipes);
        }
        
        private CraftingRecipe FindRecipe(string recipeId)
        {
            foreach (var recipe in _recipes)
            {
                if (recipe.RecipeId == recipeId)
                    return recipe;
            }
            return null;
        }
        
        public void AddRecipe(CraftingRecipe recipe)
        {
            if (recipe != null && FindRecipe(recipe.RecipeId) == null)
            {
                _recipes.Add(recipe);
            }
        }
        
        public object GetSaveData()
        {
            return new CraftingSaveData
            {
                Recipes = _recipes
            };
        }
        
        public void LoadSaveData(object state)
        {
            var data = state as CraftingSaveData ?? (state as JObject)?.ToObject<CraftingSaveData>();
            if (data != null)
            {
                _recipes = data.Recipes ?? new List<CraftingRecipe>();
            }
        }
    }
    
    [System.Serializable]
    public class CraftingSaveData
    {
        public List<CraftingRecipe> Recipes;
    }
    
    /// <summary>
    /// ì¡°í•© ?„ë£Œ ?´ë²¤??
    /// </summary>
    public class ItemCraftedEvent
    {
        public string RecipeId { get; set; }
        public string ResultItemId { get; set; }
        public int Amount { get; set; }
    }
}
