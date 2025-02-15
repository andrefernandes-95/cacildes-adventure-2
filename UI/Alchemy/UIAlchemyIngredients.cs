namespace AF
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using AF.Inventory;
    using AF.Music;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class UIAlchemyIngredients : MonoBehaviour
    {

        [Header("UI Components")]
        public UIDocumentAlchemy uIDocumentAlchemy;

        [Header("Components")]
        public Soundbank soundbank;

        [HideInInspector] public bool returnToBonfire = false;

        [Header("Databases")]
        public InventoryDatabase inventoryDatabase;

        // Last scroll position
        int lastScrollElementIndex = -1;

        [Header("Prefabs")]
        public VisualTreeAsset ingredientEntry;
        public VisualTreeAsset chosenIngredientPlaceholderEntry;

        public void DrawUI()
        {
            PopulateIngredientsListScrollView();
            PopulateChosenIngredientsListScrollView();
        }

        VisualElement GetRoot()
        {
            return uIDocumentAlchemy.uIDocument.rootVisualElement;
        }

        void PopulateIngredientsListScrollView()
        {
            var ingredientsListScrollView = GetRoot().Q<ScrollView>("IngredientsListScrollView");
            ingredientsListScrollView.Clear();

            // Not sure why, but suspect clear is not working as intended
            IEnumerator Test()
            {
                yield return new WaitForEndOfFrame();

                DrawIngredientsList(ingredientsListScrollView);
            }

            StartCoroutine(Test());

            if (lastScrollElementIndex == -1 && ingredientsListScrollView.childCount > 0)
            {
                ingredientsListScrollView.ScrollTo(ingredientsListScrollView.Children().ElementAt(0));
            }
            else
            {
                Invoke(nameof(GiveFocus), 0f);
            }
        }

        void DrawIngredientsList(ScrollView scrollView)
        {
            int i = 0;

            Dictionary<CraftingMaterial, int> visitedItems = new();

            foreach (var craftingMaterialInstance in inventoryDatabase.FilterByType<CraftingMaterialInstance>())
            {
                CraftingMaterial ingredient = craftingMaterialInstance.GetItem();

                int amountSelected = uIDocumentAlchemy.selectedIngredients
                    .Where(selectedIngredient => selectedIngredient == ingredient).Count();

                int currentIndex = i;
                Button scrollItem = ingredientEntry.Instantiate().Q<Button>("CraftButtonItem");
                scrollItem.SetEnabled(uIDocumentAlchemy.selectedIngredients.Count < uIDocumentAlchemy.maxAllowedIngredients);

                int nextAmount = inventoryDatabase.GetItemAmount(ingredient) - amountSelected;

                scrollItem.Q<VisualElement>("ItemIcon").style.backgroundImage = new StyleBackground(ingredient.sprite);
                scrollItem.Q<VisualElement>("Info").Q<Label>("ItemName").text = ingredient.GetName() + $" ({nextAmount})";

                var addIndicator = scrollItem.Q<VisualElement>("AddIngredient");
                addIndicator.style.display = DisplayStyle.None;
                scrollItem.Q<VisualElement>("RemoveIngredient").style.display = DisplayStyle.None;

                UIUtils.SetupButton(scrollItem,
                () =>
                {
                    lastScrollElementIndex = currentIndex;

                    uIDocumentAlchemy.SelectIngredient(ingredient, nextAmount >= amountSelected);
                },
                () =>
                {
                    scrollView.ScrollTo(scrollItem);
                    addIndicator.style.display = DisplayStyle.Flex;
                },
                () =>
                {
                    addIndicator.style.display = DisplayStyle.None;
                },
                false,
                soundbank);

                if (visitedItems.ContainsKey(ingredient))
                {
                    visitedItems[ingredient]++;
                }
                else
                {
                    scrollView.Add(scrollItem);
                    visitedItems.Add(ingredient, 1);
                }

                i++;
            }
        }

        void GiveFocus()
        {
            UIUtils.ScrollToLastPosition(
                lastScrollElementIndex,
                GetRoot().Q<ScrollView>(),
                () =>
                {
                    lastScrollElementIndex = -1;
                }
            );
        }


        void PopulateChosenIngredientsListScrollView()
        {
            var chosenIngredientsListScrollView = GetRoot().Q<ScrollView>("ChosenIngredientsScrollView");
            chosenIngredientsListScrollView.Clear();

            // Not sure why, but suspect clear is not working as intended
            IEnumerator Test()
            {
                yield return new WaitForEndOfFrame();

                DrawChosenIngredientsList(chosenIngredientsListScrollView);
            }

            StartCoroutine(Test());
        }

        void DrawChosenIngredientsList(ScrollView scrollView)
        {
            int i = 0;
            foreach (var ingredient in uIDocumentAlchemy.selectedIngredients)
            {
                int currentIndex = i;
                Button scrollItem = ingredientEntry.Instantiate().Q<Button>("CraftButtonItem");

                scrollItem.Q<VisualElement>("ItemIcon").style.backgroundImage = new StyleBackground(ingredient.sprite);
                scrollItem.Q<VisualElement>("Info").Q<Label>("ItemName").text = ingredient.GetName();

                var removeIndicator = scrollItem.Q<VisualElement>("RemoveIngredient");
                removeIndicator.style.display = DisplayStyle.None;
                scrollItem.Q<VisualElement>("AddIngredient").style.display = DisplayStyle.None;


                UIUtils.SetupButton(scrollItem,
                () =>
                {
                    lastScrollElementIndex = currentIndex;

                    uIDocumentAlchemy.UnselectIngredient(ingredient);
                },
                () =>
                {
                    scrollView.ScrollTo(scrollItem);
                    removeIndicator.style.display = DisplayStyle.Flex;
                },
                () =>
                {
                    removeIndicator.style.display = DisplayStyle.None;
                },
                false,
                soundbank);

                scrollView.Add(scrollItem);

                i++;
            }

            for (int j = 0; j < uIDocumentAlchemy.maxAllowedIngredients - uIDocumentAlchemy.selectedIngredients.Count; j++)
            {
                scrollView.Add(chosenIngredientPlaceholderEntry.Instantiate());
            }
        }

    }
}
