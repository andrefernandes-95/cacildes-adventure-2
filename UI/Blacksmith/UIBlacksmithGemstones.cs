namespace AF
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using AF.Health;
    using AF.Inventory;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class UIBlacksmithGemstones : MonoBehaviour
    {

        [Header("UI")]
        public VisualTreeAsset gemstoneItemPrefab;

        [Header("Components")]
        public Soundbank soundbank;
        public UIDocumentCraftScreen uIDocumentCraftScreen;
        public PlayerManager playerManager;
        public UIWeaponStatsContainer uIWeaponStatsContainer;

        [Header("Databases")]
        public InventoryDatabase inventoryDatabase;
        public PlayerStatsDatabase playerStatsDatabase;

        // Last scroll position
        int lastScrollElementIndex = -1;
        GemstoneInstance selectedGemstone;

        public void ClearPreviews(VisualElement root)
        {
            selectedGemstone = null;
        }

        public void DrawUI(VisualElement root, Action onClose)
        {
            PreviewCurrentDamage(root);

            PopulateScrollView(root, onClose);
        }

        void PopulateScrollView(VisualElement root, Action onClose)
        {
            var scrollView = root.Q<ScrollView>("GemstonesScrollView");
            scrollView.Clear();

            PopulateGemstonesScrollView(root, onClose);

            if (lastScrollElementIndex != -1)
            {
                StartCoroutine(GiveFocusCoroutine(root));
            }
            else if (scrollView.childCount > 0)
            {
                scrollView.Children().FirstOrDefault().Focus();
            }
        }

        IEnumerator GiveFocusCoroutine(VisualElement root)
        {
            yield return new WaitForSeconds(0);
            GiveFocus(root);
        }

        void GiveFocus(VisualElement root)
        {
            UIUtils.ScrollToLastPosition(
                lastScrollElementIndex,
                root.Q<ScrollView>("GemstonesScrollView"),
                () =>
                {
                    lastScrollElementIndex = -1;
                }
            );
        }

        string GetEquippedText()
        {
            return Glossary.IsPortuguese() ? "Equipado" : "Equipped";
        }

        string GetEquippedToWeaponText(Weapon weapon)
        {
            if (weapon == null)
            {
                return "";
            }

            return Glossary.IsPortuguese() ? $"Equipado em {weapon.GetName()}" : $"Equipped in {weapon.GetName()}";
        }

        void PopulateGemstonesScrollView(VisualElement root, Action onClose)
        {
            var scrollView = root.Q<ScrollView>("GemstonesScrollView");

            int i = 0;
            foreach (var gemstoneInstance in GetGemstonesList())
            {
                int currentIndex = i;

                Gemstone gemstone = gemstoneInstance.GetItem();

                var scrollItem = this.gemstoneItemPrefab.CloneTree();

                scrollItem.Q<VisualElement>("ItemIcon").style.backgroundImage = new StyleBackground(gemstone.sprite);
                scrollItem.Q<Label>("Title").text = gemstone.GetName();

                WeaponInstance selectedWeaponInstance = uIDocumentCraftScreen.uIBlacksmithWeaponsList?.selectedWeaponInstance;

                bool isEquipped = selectedWeaponInstance.IsGemstoneEquipped(gemstoneInstance);

                WeaponInstance weaponThatThisGemstoneIsAttachedTo = inventoryDatabase
                    .FilterByType<WeaponInstance>().FirstOrDefault(equippedWeapon => equippedWeapon.IsGemstoneEquipped(gemstoneInstance));

                scrollItem.Q<Label>("EquippedIndicator").text = isEquipped
                    ? GetEquippedText()
                    : GetEquippedToWeaponText(weaponThatThisGemstoneIsAttachedTo?.GetItem());

                var equipGemstoneButton = scrollItem.Q<Button>();

                if (isEquipped)
                {
                    equipGemstoneButton.Q<VisualElement>("Selected").style.display = DisplayStyle.Flex;
                    equipGemstoneButton.AddToClassList("blacksmith-craft-button-active");
                }
                else
                {
                    equipGemstoneButton.Q<VisualElement>("Selected").style.display = DisplayStyle.None;
                }

                UIUtils.SetupButton(equipGemstoneButton, () =>
                {
                    lastScrollElementIndex = currentIndex;
                    PreviewGemstone(gemstone, root, isEquipped);

                    SelectGemstone(gemstoneInstance);

                    DrawUI(root, onClose);
                },
                () =>
                {
                    scrollView.ScrollTo(equipGemstoneButton);
                    PreviewGemstone(gemstone, root, isEquipped);
                },
                () =>
                {
                },
                true,
                soundbank);

                scrollView.Add(equipGemstoneButton);

                i++;
            }
        }

        void SelectGemstone(GemstoneInstance gemstoneInstance)
        {
            selectedGemstone = gemstoneInstance;

            WeaponInstance weaponInstanceToAttach = uIDocumentCraftScreen.uIBlacksmithWeaponsList.selectedWeaponInstance;

            if (weaponInstanceToAttach != null)
            {
                if (weaponInstanceToAttach.IsGemstoneEquipped(selectedGemstone))
                {
                    weaponInstanceToAttach.RemoveGemstone(selectedGemstone);
                    soundbank.PlaySound(soundbank.uiCancel);
                }
                else
                {
                    WeaponInstance weaponInstanceThatHasGemstoneEquipped = inventoryDatabase.FilterByType<WeaponInstance>()
                        .FirstOrDefault(weapon => weapon.IsGemstoneEquipped(selectedGemstone));

                    if (weaponInstanceThatHasGemstoneEquipped != null)
                    {
                        weaponInstanceThatHasGemstoneEquipped.RemoveGemstone(selectedGemstone);
                    }

                    weaponInstanceToAttach.AttachGemstone(selectedGemstone);
                    soundbank.PlaySound(soundbank.uiDecision);
                }
            }

            uIDocumentCraftScreen.UpdateUI();
        }

        List<GemstoneInstance> GetGemstonesList() => inventoryDatabase.FilterByType<GemstoneInstance>();

        void ClearPreview(VisualElement root)
        {
            root.Q<VisualElement>("WeaponStatsContainer").Clear();
            root.Q<VisualElement>("WeaponStatsContainer").style.opacity = 1;
        }

        void PreviewCurrentDamage(VisualElement root)
        {
            ClearPreview(root);
            WeaponInstance selectedWeaponInstance = uIDocumentCraftScreen.uIBlacksmithWeaponsList.selectedWeaponInstance;

            Weapon weapon = selectedWeaponInstance.GetItem();

            Damage currentWeaponDamage = weapon.weaponDamage.GetCurrentDamage(playerManager,
                playerManager.statsBonusController.GetCurrentStrength(),
                playerManager.statsBonusController.GetCurrentDexterity(),
                playerManager.statsBonusController.GetCurrentIntelligence(),
                selectedWeaponInstance);

            string gemstoneNames = string.Join(", ", selectedWeaponInstance.GetAttachedGemstones(inventoryDatabase).Select(gemstone => gemstone.GetName()));

            uIWeaponStatsContainer.PreviewWeaponDamageDifference(
                weapon.GetName() + " +" + selectedWeaponInstance.level + ", " + gemstoneNames,
                currentWeaponDamage,
                currentWeaponDamage,
                root);
        }

        void PreviewGemstone(Gemstone gemstone, VisualElement root, bool isGemstoneEquipped)
        {
            PreviewCurrentDamage(root);

            WeaponInstance selectedWeaponInstance = uIDocumentCraftScreen.uIBlacksmithWeaponsList.selectedWeaponInstance;

            if (selectedWeaponInstance == null)
            {
                return;
            }

            Weapon weapon = selectedWeaponInstance.GetItem();

            Damage currentWeaponDamage = weapon.weaponDamage.GetCurrentDamage(playerManager,
                playerManager.statsBonusController.GetCurrentStrength(),
                playerManager.statsBonusController.GetCurrentDexterity(),
                playerManager.statsBonusController.GetCurrentIntelligence(),
                selectedWeaponInstance);

            if (gemstone != null)
            {
                root.Q<VisualElement>("WeaponStatsContainer").Add(uIWeaponStatsContainer.CreateLabel(" > ", 0));

                Damage gemstoneDamage = !isGemstoneEquipped
                    ? weapon.weaponDamage.EnhanceWithGemstonesDamage(currentWeaponDamage, new List<Gemstone>() { gemstone }.ToArray())
                    : weapon.weaponDamage.GetCurrentDamage(playerManager,
                        playerManager.statsBonusController.GetCurrentStrength(),
                        playerManager.statsBonusController.GetCurrentDexterity(),
                        playerManager.statsBonusController.GetCurrentIntelligence(),
                        selectedWeaponInstance);

                uIWeaponStatsContainer.PreviewWeaponDamageDifference(
                    weapon.GetName() + " + " + gemstone.GetName(),
                    currentWeaponDamage,
                    gemstoneDamage,
                    root);
            }
        }
    }
}
