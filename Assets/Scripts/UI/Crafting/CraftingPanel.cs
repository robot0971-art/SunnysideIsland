using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DI;
using SunnysideIsland.Crafting;
using SunnysideIsland.Inventory;
using SunnysideIsland.GameData;
using SunnysideIsland.Events;

namespace SunnysideIsland.UI.Crafting
{
    public class CraftingPanel : UIPanel
    {
        [Header("=== Crafting System ===")]
        [Inject(Optional = true)]
        [SerializeField] private CraftingSystem _craftingSystem;
        [Inject(Optional = true)]
        [SerializeField] private InventorySystem _inventorySystem;
        [Inject(Optional = true)]
        [SerializeField] private SunnysideIsland.GameData.GameData _gameData;

        [Header("=== Recipe List ===")]
        [SerializeField] private Transform _recipeListContent;
        [SerializeField] private GameObject _recipeItemPrefab;

        [Header("=== Recipe Detail ===")]
        [SerializeField] private TextMeshProUGUI _recipeNameText;
        [SerializeField] private TextMeshProUGUI _recipeDescriptionText;
        [SerializeField] private Image _resultItemImage;
        [SerializeField] private TextMeshProUGUI _resultAmountText;
        [SerializeField] private Transform _ingredientsListContent;
        [SerializeField] private GameObject _ingredientItemPrefab;

        [Header("=== Crafting Action ===")]
        [SerializeField] private Button _craftButton;
        [SerializeField] private TextMeshProUGUI _craftButtonText;
        [SerializeField] private Slider _craftProgressSlider;

        [Header("=== Navigation ===")]
        [SerializeField] private Button _closeButton;

        private CraftingRecipe _selectedRecipe = default!;
        private List<GameObject> _recipeItems = new List<GameObject>();
        private List<GameObject> _ingredientItems = new List<GameObject>();

        protected override void Awake()
        {
            base.Awake();

            DIContainer.Inject(this);
            
            if (_craftingSystem == null)
                DIContainer.TryResolve(out _craftingSystem);
            if (_inventorySystem == null)
                DIContainer.TryResolve(out _inventorySystem);
            if (_gameData == null)
                DIContainer.TryResolve(out _gameData);
            
            // UI 李몄“媛 ?놁쑝硫??숈쟻 UI ?앹꽦
            if (_recipeListContent == null || _recipeItemPrefab == null || 
                _recipeNameText == null || _craftButton == null || _closeButton == null)
            {
                CreateDynamicUI();
            }
            
            // UIManager???깅줉
            UIManager.Instance?.RegisterPanel(this);
        }

        private void OnEnable()
        {
            SubscribeEvents();
            SetupButtons();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            EventBus.Subscribe<ItemCraftedEvent>(OnItemCrafted);
            EventBus.Subscribe<ItemPickedUpEvent>(OnItemChanged);
            EventBus.Subscribe<ItemMovedEvent>(OnItemChanged);
        }

        private void UnsubscribeEvents()
        {
            EventBus.Unsubscribe<ItemCraftedEvent>(OnItemCrafted);
            EventBus.Unsubscribe<ItemPickedUpEvent>(OnItemChanged);
            EventBus.Unsubscribe<ItemMovedEvent>(OnItemChanged);
        }

        private void SetupButtons()
        {
            if (_closeButton != null)
                _closeButton.onClick.AddListener(Close);
            
            if (_craftButton != null)
                _craftButton.onClick.AddListener(OnCraftButtonClicked);
        }

        protected override void OnOpened()
        {
            base.OnOpened();
            RefreshRecipeList();
            ClearRecipeDetail();
        }

        private void RefreshRecipeList()
        {
            ClearRecipeList();
            
            if (_craftingSystem == null || _recipeListContent == null || _recipeItemPrefab == null)
                return;
            
            var allRecipes = _craftingSystem.GetAllRecipes();
            
            foreach (var recipe in allRecipes)
            {
                var recipeItem = Instantiate(_recipeItemPrefab, _recipeListContent);
                var recipeUI = recipeItem.GetComponent<RecipeItemUI>();
                
                if (recipeUI != null)
                {
                    string recipeName = GetItemName(recipe.ResultItemId);
                    Sprite icon = GetItemIcon(recipe.ResultItemId);
                    bool canCraft = _craftingSystem.CanCraft(recipe.RecipeId);
                    
                    recipeUI.Setup(recipe.RecipeId, recipeName, icon, canCraft);
                    recipeUI.OnClicked += OnRecipeSelected;
                }
                
                _recipeItems.Add(recipeItem);
            }
        }

        private void ClearRecipeList()
        {
            foreach (var item in _recipeItems)
            {
                if (item != null)
                    Destroy(item);
            }
            _recipeItems.Clear();
        }

        private void OnRecipeSelected(string recipeId)
        {
            _selectedRecipe = FindRecipe(recipeId);
            UpdateRecipeDetail();
        }

        private void UpdateRecipeDetail()
        {
            if (_selectedRecipe == null)
            {
                ClearRecipeDetail();
                return;
            }
            
            if (_recipeNameText != null)
                _recipeNameText.text = GetItemName(_selectedRecipe.ResultItemId);
            
            if (_recipeDescriptionText != null)
            {
                var resultItem = GetItemData(_selectedRecipe.ResultItemId);
                _recipeDescriptionText.text = resultItem != null ? resultItem.description : "";
            }
            
            if (_resultItemImage != null)
            {
                _resultItemImage.sprite = GetItemIcon(_selectedRecipe.ResultItemId);
                _resultItemImage.gameObject.SetActive(_resultItemImage.sprite != null);
            }
            
            if (_resultAmountText != null)
                _resultAmountText.text = $"x{_selectedRecipe.ResultAmount}";
            
            UpdateIngredientsList();
            UpdateCraftButton();
        }

        private void UpdateIngredientsList()
        {
            ClearIngredientsList();
            
            if (_selectedRecipe == null || _ingredientsListContent == null || _ingredientItemPrefab == null)
                return;
            
            foreach (var ingredient in _selectedRecipe.Ingredients)
            {
                var ingredientItem = Instantiate(_ingredientItemPrefab, _ingredientsListContent);
                var ingredientUI = ingredientItem.GetComponent<IngredientItemUI>();
                
                if (ingredientUI != null)
                {
                    string itemName = GetItemName(ingredient.Key);
                    Sprite icon = GetItemIcon(ingredient.Key);
                    int owned = _inventorySystem.CountItem(ingredient.Key);
                    int required = ingredient.Value;
                    bool hasEnough = owned >= required;
                    
                    ingredientUI.Setup(ingredient.Key, itemName, icon, owned, required, hasEnough);
                }
                
                _ingredientItems.Add(ingredientItem);
            }
        }

        private void ClearIngredientsList()
        {
            foreach (var item in _ingredientItems)
            {
                if (item != null)
                    Destroy(item);
            }
            _ingredientItems.Clear();
        }

        private void UpdateCraftButton()
        {
            if (_craftButton == null) return;
            
            bool canCraft = _selectedRecipe != null && _craftingSystem.CanCraft(_selectedRecipe.RecipeId);
            _craftButton.interactable = canCraft;
            
            if (_craftButtonText != null)
                _craftButtonText.text = canCraft ? "Craft" : "Not enough materials";
        }

        private void ClearRecipeDetail()
        {
            _selectedRecipe = null;
            
            if (_recipeNameText != null)
                _recipeNameText.text = "";
            if (_recipeDescriptionText != null)
                _recipeDescriptionText.text = "";
            if (_resultItemImage != null)
                _resultItemImage.gameObject.SetActive(false);
            if (_resultAmountText != null)
                _resultAmountText.text = "";
            
            ClearIngredientsList();
            UpdateCraftButton();
        }

        private void OnCraftButtonClicked()
        {
            if (_selectedRecipe == null) return;
            
            bool success = _craftingSystem.Craft(_selectedRecipe.RecipeId);
            
            if (success)
            {
                Debug.Log($"[CraftingPanel] ?쒖옉 ?깃났: {_selectedRecipe.ResultItemId}");
                // ?쒖옉 ?깃났 ??UI ?낅뜲?댄듃??ItemCraftedEvent?먯꽌 泥섎━
            }
            else
            {
                Debug.LogWarning($"[CraftingPanel] ?쒖옉 ?ㅽ뙣: {_selectedRecipe.RecipeId}");
            }
        }

        private void OnItemCrafted(ItemCraftedEvent evt)
        {
            Debug.Log($"[CraftingPanel] ?쒖옉 ?꾨즺 ?대깽???섏떊: {evt.ResultItemId}");
            
            // ?덉떆??紐⑸줉 ?덈줈怨좎묠 (?쒖옉 媛???щ? 蹂寃?
            RefreshRecipeList();
            
            // ?꾩옱 ?좏깮???덉떆???낅뜲?댄듃
            if (_selectedRecipe != null && _selectedRecipe.RecipeId == evt.RecipeId)
            {
                UpdateRecipeDetail();
            }
        }

        private void OnItemChanged(ItemPickedUpEvent evt)
        {
            // ?꾩씠???띾뱷/?대룞 ???щ즺 紐⑸줉 ?낅뜲?댄듃
            if (_selectedRecipe != null)
            {
                UpdateIngredientsList();
                UpdateCraftButton();
            }
        }

        private void OnItemChanged(ItemMovedEvent evt)
        {
            OnItemChanged(new ItemPickedUpEvent { ItemId = evt.ItemId, Quantity = 0 });
        }

        private CraftingRecipe FindRecipe(string recipeId)
        {
            if (_craftingSystem == null) return null;
            
            foreach (var recipe in _craftingSystem.GetAllRecipes())
            {
                if (recipe.RecipeId == recipeId)
                    return recipe;
            }
            return null;
        }

        private string GetItemName(string itemId)
        {
            if (string.IsNullOrEmpty(itemId) || _gameData == null) return itemId;
            
            var itemData = _gameData.GetItem(itemId);
            return itemData?.itemName ?? itemId;
        }

        private Sprite GetItemIcon(string itemId)
        {
            if (string.IsNullOrEmpty(itemId) || _gameData == null) return null;
            
            var itemData = _gameData.GetItem(itemId);
            return itemData?.GetIcon();
        }

        private SunnysideIsland.GameData.ItemData GetItemData(string itemId)
        {
            if (string.IsNullOrEmpty(itemId) || _gameData == null) return null;
            return _gameData.GetItem(itemId);
        }
        
        private void CreateDynamicUI()
        {
            // 罹붾쾭???뺤씤 (?놁쑝硫??앹꽦)
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasGO = new GameObject("CraftingCanvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;
                canvasGO.AddComponent<CanvasScaler>();
                canvasGO.AddComponent<GraphicRaycaster>();
            }
            
            // 硫붿씤 ?⑤꼸
            GameObject panelGO = new GameObject("CraftingPanel");
            panelGO.transform.SetParent(canvas.transform, false);
            Image panelImage = panelGO.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            RectTransform panelRect = panelGO.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.2f, 0.2f);
            panelRect.anchorMax = new Vector2(0.8f, 0.8f);
            panelRect.sizeDelta = Vector2.zero;
            
            // ?ㅽ겕濡ㅻ럭 (?덉떆??紐⑸줉)
            GameObject scrollGO = new GameObject("RecipeScroll");
            scrollGO.transform.SetParent(panelGO.transform, false);
            ScrollRect scrollRect = scrollGO.AddComponent<ScrollRect>();
            Image scrollImage = scrollGO.AddComponent<Image>();
            scrollImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            RectTransform scrollRectTransform = scrollGO.GetComponent<RectTransform>();
            scrollRectTransform.anchorMin = new Vector2(0, 0.3f);
            scrollRectTransform.anchorMax = new Vector2(0.4f, 1);
            scrollRectTransform.sizeDelta = Vector2.zero;
            
            // 肄섑뀗痢?
            GameObject contentGO = new GameObject("Content");
            contentGO.transform.SetParent(scrollGO.transform, false);
            RectTransform contentRect = contentGO.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.sizeDelta = new Vector2(0, 300);
            contentRect.pivot = new Vector2(0, 1);
            VerticalLayoutGroup vlg = contentGO.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            ContentSizeFitter fitter = contentGO.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            _recipeListContent = contentRect;
            
            // ?덉떆???꾩씠???꾨━???앹꽦 (媛꾨떒??踰꾪듉)
            GameObject recipeItemPrefab = new GameObject("RecipeItemPrefab");
            Button button = recipeItemPrefab.AddComponent<Button>();
            Image image = recipeItemPrefab.AddComponent<Image>();
            image.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            TextMeshProUGUI text = CreateText(recipeItemPrefab.transform, "RecipeName", 20);
            RectTransform itemRect = recipeItemPrefab.GetComponent<RectTransform>();
            itemRect.sizeDelta = new Vector2(0, 40);
            // RecipeItemUI 而댄룷?뚰듃 異붽? (湲곕낯 援ы쁽)
            RecipeItemUI recipeItemUI = recipeItemPrefab.AddComponent<RecipeItemUI>();
            recipeItemUI.Setup("", "Default Recipe", null, false);
            
            _recipeItemPrefab = recipeItemPrefab;
            
            // ?덉떆???곸꽭 ?⑤꼸
            GameObject detailPanel = new GameObject("RecipeDetail");
            detailPanel.transform.SetParent(panelGO.transform, false);
            RectTransform detailRect = detailPanel.AddComponent<RectTransform>();
            detailRect.anchorMin = new Vector2(0.45f, 0.3f);
            detailRect.anchorMax = new Vector2(1, 1);
            detailRect.sizeDelta = Vector2.zero;
            
            _recipeNameText = CreateText(detailPanel.transform, "RecipeName", 24);
            _recipeDescriptionText = CreateText(detailPanel.transform, "RecipeDescription", 18);
            _resultAmountText = CreateText(detailPanel.transform, "ResultAmount", 20);
            
            // 寃곌낵 ?꾩씠???대?吏
            GameObject imageGO = new GameObject("ResultImage");
            imageGO.transform.SetParent(detailPanel.transform, false);
            _resultItemImage = imageGO.AddComponent<Image>();
            RectTransform imageRect = imageGO.GetComponent<RectTransform>();
            imageRect.anchorMin = new Vector2(0, 0.6f);
            imageRect.anchorMax = new Vector2(0.3f, 0.9f);
            imageRect.sizeDelta = Vector2.zero;
            
            // ?щ즺 紐⑸줉 肄섑뀗痢?
            GameObject ingredientsGO = new GameObject("IngredientsContent");
            ingredientsGO.transform.SetParent(detailPanel.transform, false);
            RectTransform ingredientsRect = ingredientsGO.AddComponent<RectTransform>();
            ingredientsRect.anchorMin = new Vector2(0.35f, 0);
            ingredientsRect.anchorMax = new Vector2(1, 0.5f);
            ingredientsRect.sizeDelta = Vector2.zero;
            VerticalLayoutGroup ingredientsVlg = ingredientsGO.AddComponent<VerticalLayoutGroup>();
            ingredientsVlg.childForceExpandWidth = true;
            ingredientsVlg.childForceExpandHeight = false;
            ContentSizeFitter ingredientsFitter = ingredientsGO.AddComponent<ContentSizeFitter>();
            ingredientsFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            _ingredientsListContent = ingredientsRect;
            
            // ?щ즺 ?꾩씠???꾨━???앹꽦
            GameObject ingredientItemPrefab = new GameObject("IngredientItemPrefab");
            TextMeshProUGUI ingredientText = CreateText(ingredientItemPrefab.transform, "IngredientText", 18);
            IngredientItemUI ingredientUI = ingredientItemPrefab.AddComponent<IngredientItemUI>();
            RectTransform ingredientRect = ingredientItemPrefab.GetComponent<RectTransform>();
            ingredientRect.sizeDelta = new Vector2(0, 30);
            
            _ingredientItemPrefab = ingredientItemPrefab;
            
            // ?쒖옉 踰꾪듉
            GameObject craftButtonGO = new GameObject("CraftButton");
            craftButtonGO.transform.SetParent(panelGO.transform, false);
            _craftButton = craftButtonGO.AddComponent<Button>();
            Image craftButtonImage = craftButtonGO.AddComponent<Image>();
            craftButtonImage.color = new Color(0.2f, 0.6f, 0.2f, 1f);
            RectTransform craftButtonRect = craftButtonGO.GetComponent<RectTransform>();
            craftButtonRect.anchorMin = new Vector2(0.1f, 0.05f);
            craftButtonRect.anchorMax = new Vector2(0.4f, 0.2f);
            craftButtonRect.sizeDelta = Vector2.zero;
            _craftButtonText = CreateText(craftButtonGO.transform, "ButtonText", 22);
            _craftButtonText.text = "?쒖옉?섍린";
            
            // ?リ린 踰꾪듉
            GameObject closeButtonGO = new GameObject("CloseButton");
            closeButtonGO.transform.SetParent(panelGO.transform, false);
            _closeButton = closeButtonGO.AddComponent<Button>();
            Image closeButtonImage = closeButtonGO.AddComponent<Image>();
            closeButtonImage.color = new Color(0.8f, 0.2f, 0.2f, 1f);
            RectTransform closeButtonRect = closeButtonGO.GetComponent<RectTransform>();
            closeButtonRect.anchorMin = new Vector2(0.85f, 0.85f);
            closeButtonRect.anchorMax = new Vector2(0.95f, 0.95f);
            closeButtonRect.sizeDelta = Vector2.zero;
            TextMeshProUGUI closeText = CreateText(closeButtonGO.transform, "X", 20);
            closeText.text = "X";
            
            // ?꾨━??鍮꾪솢?깊솕 (Instantiate ???ъ슜)
            recipeItemPrefab.SetActive(false);
            ingredientItemPrefab.SetActive(false);
        }
        
        private TextMeshProUGUI CreateText(Transform parent, string name, int fontSize)
        {
            GameObject textGO = new GameObject(name);
            textGO.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = fontSize;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            RectTransform rect = textGO.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            return tmp;
        }
    }
}
