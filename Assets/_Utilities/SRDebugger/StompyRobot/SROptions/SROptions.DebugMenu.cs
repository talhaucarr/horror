#define ENABLE_TEST_SROPTIONS

using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

public partial class SROptions
{
    private int _matchTime;
    // Uncomment the #define at the top of file to enable test options

#if ENABLE_TEST_SROPTIONS
    //[Category("Language")]
    //[DisplayName("Turkish")]
    //public void ChangeLanguageTr()
    //{
    //    LanguageManager.Instance.ChangeLanguage("tr");
    //}
    //[Category("Language")]
    //[DisplayName("English")]
    //public void ChangeLanguageEn()
    //{
    //    LanguageManager.Instance.ChangeLanguage("en");
    //}
    //[Category("Cheats")]
    //[DisplayName("Infinite Energy")]

    //public bool InfiniteEnergyActive
    //{
    //    get => EnergyController.IsInfiniteEnergyActive;
    //    set => EnergyController.IsInfiniteEnergyActive = value;
    //}
    //[Category("Cheats")]
    //[DisplayName("Open All Cards")]

    //public void OpenAllCards()
    //{
    //    foreach (Card card in AllCardsList.Instance.AllCards)
    //    {
    //        InventoryManager.Instance.AddItemToInventory(card.name + "Card", 2);
    //    }
    //}
    //[Category("Cheats")]
    //[DisplayName("Change Match Time")]

    //public void ChangeMatchTime()
    //{
    //    GeneralSettings.Instance.TotalRoundTime = MatchTime;
    //}
    //[Category("Cheats")]
    //[DisplayName("Match Time")]
    //[NumberRange(0, 15)]
    //public int MatchTime
    //{
    //    get { return _matchTime; }
    //    set{ _matchTime = value * 60; }
    //}
    //[Category("Cheats")]
    //[DisplayName("Gain Random Card")]
    //public void GainRandomCard()
    //{
    //    Card randomCard = AllCardsList.Instance.AllCards[Random.Range(0, AllCardsList.Instance.AllCards.Count)];
    //    InventoryManager.Instance.AddItemToInventory(randomCard.name+"Card", 3);
    //}
    //[Category("Cheats")]
    //[DisplayName("Clear PlayerPrefs")]
    //public void ClearPlayerPrefs()
    //{
    //    PlayerPrefs.DeleteAll();
    //}

    //[Category("Units (Server)")]
    //[DisplayName("Spawn Unit")]
    //public void SpawnUnit()
    //{
    //    if (BoltNetwork.IsServer)
    //    {
    //        //GameObject unitPrefab = AllCardsList.Instance.AllCards
    //          //  .FirstOrDefault(card => card.serverUnitPrefab.name == UnitName)?.serverUnitPrefab;

    //          PrefabId unitID = new PrefabId() {Value = UnitPrefabID};
    //          ServerUnitManager.Instance.SummonUnit(unitID, Vector3.zero, Quaternion.identity, UnitTeamID);
    //    }
    //}

    //[Category("Units (Server)")] [DisplayName("Unit Prefab ID")]
    //public int UnitPrefabID { get; set; }

    //[Category("Units (Server)")]
    //[DisplayName("Unit Team")]
    //public int UnitTeamID { get; set; } = 1;


    //[Category("Quality")]
    //[DisplayName("Set Quality Low")]
    //public void SetQualityLow()
    //{
    //    QualitySettings.SetQualityLevel(0);
    //}
    
    //[Category("Quality")]
    //[DisplayName("Set Quality High")]
    //public void SetQualityHigh()
    //{
    //    QualitySettings.SetQualityLevel(1);
    //}

    
  
    //[Category("Controls")]
    //[DisplayName("Swipe Acceleration Base")]
    //[NumberRange(10, 50)]
    //public float SwipeLogBase
    //{
    //    get { return CommonSettingsManager.CommonSettingsInstance.swipeAccelerationLogBase * 10; }
    //    set
    //    {
    //        CommonSettingsManager.CommonSettingsInstance.swipeAccelerationLogBase = (float) value / 10;
    //        OnPropertyChanged(nameof(SwipeLogBase));
    //        SaveSettings();
    //    }
    //}

    //[Category("Device Options")]
    //[DisplayName("CrashMode")]
    //public ForcedCrashCategory CrashMode
    //{
    //    get { return crashMode; }
    //    set
    //    {
    //        crashMode = value;
    //        OnPropertyChanged(nameof(CrashMode));
    //    }
    //}

    //[Category("Device Options")]
    //[DisplayName("Crash")]
    //public void Crash()
    //{
    //    Utils.ForceCrash(crashMode);
    //}

    //[Category("Device Options")]
    //[DisplayName("Enable Weapon Card Crash")]
    //public void WeaponCardCrash()
    //{
    //    LootboxController controller = GameObject.FindObjectOfType<LootboxController>();
    //    if (controller) controller.EnableArmoryCrash = true;
    //}

    //[Category("Device Options")]
    //[DisplayName("Max Fps 30")]
    //public void ChangeMaxFps30()
    //{
    //    DeviceManager.Instance.SetMaxFramerate(30);
    //}

    //[Category("Device Options")]
    //[DisplayName("Max Fps 60")]
    //public void ChangeMaxFps60()
    //{
    //    DeviceManager.Instance.SetMaxFramerate(60);
    //}

    //[Category("Debug Options")]
    //[DisplayName("Show Graphy")]
    //public bool ShowGraphs
    //{
    //    get { return CommonSettingsManager.CommonSettingsInstance.showGraphs; }
    //    set
    //    {
    //        CommonSettingsManager.CommonSettingsInstance.showGraphs = value;
    //        if (CommonSettingsManager.CommonSettingsInstance.showGraphs) GraphyManager.Instance.Enable();
    //        else GraphyManager.Instance.Disable();
    //        OnPropertyChanged(nameof(ShowGraphs));
    //        SaveSettings();
    //    }
    //}

    //[Category("Debug Options")]
    //[DisplayName("Show Difficulty Graph")]
    //public bool ShowDifficultyGraphs
    //{
    //    get { return CommonSettingsManager.CommonSettingsInstance.showDifficultyGraphs; }
    //    set
    //    {
    //        CommonSettingsManager.CommonSettingsInstance.showDifficultyGraphs = value;
    //        OnPropertyChanged(nameof(ShowDifficultyGraphs));
    //        SaveSettings();
    //    }
    //}

    //[Category("Debug Options")]
    //[DisplayName("Reset Tutorial")]
    //public void ResetTutorial()
    //{
    //    LevelProgressManager.Instance.beachTutorialCompleted = false;
    //    LevelProgressManager.Instance.bridgeTutorialCompleted = false;
    //    LevelProgressManager.Instance.tutorialPart1Completed = 0;
    //    LevelProgressManager.Instance.tutorialPart2Completed = 0;
    //}

    //[Category("Debug Options")]
    //[DisplayName("Set Tutorial Completed")]
    //public void SetTutorialCompleted()
    //{
    //    GameManager.Instance.SaveGameData();
    //    LevelProgressManager.Instance.beachTutorialCompleted = true;
    //    LevelProgressManager.Instance.bridgeTutorialCompleted = true;
    //    LevelProgressManager.Instance.tutorialPart1Completed = 1;
    //    LevelProgressManager.Instance.tutorialPart2Completed = 1;
    //    WeaponItem browning = Resources.Load<WeaponItem>("ScriptObjects/Weapon/M2Browning");
    //    UnlockManager.Instance.UnlockAndBuyWeapon(browning);
    //    Loadout.GetLoadout(WeaponItem.WeaponType.MachineGun).ChosenWeaponID = browning.id;
    //    GameManager.Instance.LoadScene(GameManager.Instance.MainSceneName());
    //}

    //[Category("Debug Options")]
    //[DisplayName("Send Test Notification")]
    //public void SendNotification()
    //{
    //    NotificationManager.Instance.SendNotification("test_0");
    //}
    //[Category("Debug Options")]
    //[DisplayName("Cancel Test Notification")]
    //public void CancelNotification()
    //{
    //    NotificationManager.Instance.CancelNotification("test_0");
    //}

    //[Category("Controls")]
    //[DisplayName("Sensitivity")]
    //[NumberRange(0,20)]
    //public float Sensitivity
    //{
    //    get { return CommonSettingsManager.CommonSettingsInstance.Sensitivity*5; }
    //    set
    //    {
    //        CommonSettingsManager.CommonSettingsInstance.Sensitivity = (float)value/5;
    //        OnPropertyChanged(nameof(Sensitivity));
    //        SaveSettings();
    //    }
    //}

    //[Category("Controls")]
    //[DisplayName("Counter Drag Enabled")]
    //public bool CounterDragEnabled
    //{
    //    get { return CommonSettingsManager.CommonSettingsInstance.CounterDragEnabled; }
    //    set
    //    {
    //        CommonSettingsManager.CommonSettingsInstance.CounterDragEnabled = value;
    //        OnPropertyChanged(nameof(CounterDragEnabled));
    //        SaveSettings();
    //    }
    //}

    //[Category("Controls")]
    //[DisplayName("Counter Drag Power")]
    //[NumberRange(0, 25)]
    //public float CounterDragPower
    //{
    //    get { return CommonSettingsManager.CommonSettingsInstance.CounterDragPower *2.5f; }
    //    set
    //    {
    //        CommonSettingsManager.CommonSettingsInstance.CounterDragPower = value / 2.5f;
    //        OnPropertyChanged(nameof(CounterDragPower));
    //        SaveSettings();
    //    }
    //}

    //[Category("Controls")]
    //[DisplayName("Counter Drag Fall Off Speed")]
    //[NumberRange(5, 15)]
    //public float CounterDragFallOff
    //{
    //    get { return CommonSettingsManager.CommonSettingsInstance.CounterDragFallOffSpeed * 2.5f; }
    //    set
    //    {
    //        CommonSettingsManager.CommonSettingsInstance.CounterDragFallOffSpeed = value / 2.5f;
    //        OnPropertyChanged(nameof(CounterDragFallOff));
    //        SaveSettings();
    //    }
    //}

    //[Category("Controls")]
    //[DisplayName("Play Haptic Medium")]
    //public void PlayHaptic()
    //{
    //    VibrationManager.Instance.HapticMedium();
    //}

    //[Category("Controls")]
    //[DisplayName("Play Vibrate")]
    //public void PlayVibrate()
    //{
    //    //VibrationManager.Instance.Vibration();
    //    Vibration.Vibration.Vibrate();
    //} 
    //[DisplayName("Play Vibrate Peek")]
    //public void PlayVibratePeek()
    //{
    //    //VibrationManager.Instance.Vibration();
    //    Vibration.Vibration.VibratePeek();
    //} 

    //[DisplayName("Play Vibrate Nope")]
    //public void PlayVibrateNope()
    //{
    //    //VibrationManager.Instance.Vibration();
    //    Vibration.Vibration.VibrateNope();
    //}

    //[DisplayName("Play Vibrate Pop")]
    //public void PlayVibratePop()
    //{
    //    //VibrationManager.Instance.Vibration();
    //    Vibration.Vibration.VibratePop();
    //}

    //[DisplayName("Show Int Ad")]
    //public void IntAd()
    //{
    //    AdManager.Instance.ShowAdWithActionCallback(null, AdManager.Instance.New_3Levels, AdManager.AdType.Interstitial);
    //}

    //[DisplayName("Show Rewarded Ad")]
    //public void RewardedAd()
    //{
    //    AdManager.Instance.ShowAdWithActionCallback(null, AdManager.Instance.Double_Coin, AdManager.AdType.Rewarded);
    //}

    //[Category("Cheats")]
    //[DisplayName("Complete All Daily Objectives")]
    //public void CompleteAllDailyMissions()
    //{
    //    ObjectiveManager.Instance.TestCompleteAllDailyObjectives();
    //}

    //[Category("Cheats")]
    //[DisplayName("Gain Next Daily Reward")]
    //public void GainNextDaily()
    //{
    //    StatisticsManager.Instance.SetInteger("WeeklyLoginRewardsReceived", StatisticsManager.Instance.GetInteger("WeeklyLoginRewardsReceived") + 1);
    //    ObjectiveManager.Instance.DailyRewards.ReceiveReward(StatisticsManager.Instance.GetInteger("WeeklyLoginRewardsReceived"));
    //}

    //[Category("Cheats")]
    //[DisplayName("Unlock All Levels")]
    //public void UnlockAllLevels()
    //{
    //    foreach (Level level in LevelProgressManager.Instance.AllLevels) level.SavedData.Status = LevelSaveData.LevelStatus.NotPlayed;
    //    GameManager.Instance.LoadScene(GameManager.Instance.MainSceneName());
    //}

    //[Category("Cheats")]
    //[DisplayName("Chosen Level Id")]
    //[NumberRange(0, 100)]
    //public int ChosenLevelId
    //{
    //    get { return chosenLevelId; }
    //    set
    //    {
    //        chosenLevelId = value;
    //        OnPropertyChanged(nameof(ChosenLevelId));
    //    }
    //}

    //[Category("Cheats")]
    //[DisplayName("Open Chosen Level")]
    //public void OpenLevel()
    //{
    //    SceneManager.LoadScene(chosenLevelId);
    //}

    //[Category("Cheats")]
    //[DisplayName("Gain Energy")]
    //public void GainEnergy()
    //{
    //    UIDataMediator.Instance.AddLive();
    //}


    //[Category("Cheats")]
    //[DisplayName("100k Coin")]
    //public void GainCoins()
    //{
    //    UIDataMediator.Instance.AddCoins(100000, Vector3.zero);
    //  //  EventManager.Instance.InvokeEarnCurrency(10000, Currency.CurrencyType.Coins.ToString(), "cheat");
    //}

    //[Category("Cheats")]
    //[DisplayName("1000 Diamond")]
    //public void GainDiamonds()
    //{
    //    UIDataMediator.Instance.AddDiamonds(1000,Vector3.zero);
    //  //  EventManager.Instance.InvokeEarnCurrency(100, Currency.CurrencyType.Gems.ToString(), "cheat");
    //}

    //[Category("Cheats")]
    //[DisplayName("Add PowerUps")]
    //public void GainPowerUps()
    //{
    //    Item[] items = Resources.LoadAll<Item>("ScriptObjects/Items/PowerUps/");

    //    foreach(Item item in items)
    //    {
    //        for (int i = 0; i < 10; i++)
    //        {
    //            GeneralInventory.Instance.AddItem(item);
    //            GeneralInventory.Instance.SaveInventoryState();
    //            PowerUpButtonManager.Instance.UpdatePowerupButtons();
    //        }
    //    }
    //}
    //[Category("Cheats")]
    //[DisplayName("Gain Xp 100K")]
    //public void GainXp1M()
    //{
    //    XpManager.Instance.TestAddXpGained(100000);
    //}
    //[Category("Cheats")]
    //[DisplayName("ThreeStarsEveryLevel")]
    //public void ThreeStarsEveryLevel()
    //{
    //    foreach (Level level in LevelProgressManager.Instance.AllLevels) level.SavedData.Status = LevelSaveData.LevelStatus.ThreeStar;
    //    GameManager.Instance.LoadScene(GameManager.Instance.MainSceneName());
    //}

    //[Category("Cheats")]
    //[DisplayName("Win Level")]
    //public void WinLevel()
    //{
    //    LevelManager.Instance.OnAllGoalsCompletedPauseMenu();
    //}

    //[Category("Cheats")]
    //[DisplayName("Kill Player")]
    //public void KillPlayer()
    //{
    //    LevelManager.Instance.OnPlayerDied();
    //}

    //[Category("Cheats")]
    //[DisplayName("Deplete ammo")]
    //public void DepleteAmmo()
    //{
    //    LevelManager.Instance.OnAmmoFinished();
    //}

    //[Category("Cheats")]
    //[DisplayName("Complete All Objectives")]
    //public void CompleteRandomObjectives()
    //{
    //    ObjectiveManager.Instance.SRCompleteAllObjectives();
    //}

    //[Category("Cheats")]
    //[DisplayName("Give Common Chest")]
    //public void GiveCommonChest()
    //{
    //    Chest commonChest = (Chest) ItemHelper.GetChests(Item.ItemGrade.Tier1).First();
    //    RewardManager.Instance.RegisterChest(commonChest);
    //    RefreshChestList();
    //}

    //[Category("Cheats")]
    //[DisplayName("Give Rare Chest")]
    //public void GiveRareChest()
    //{
    //    Chest rareChest = (Chest) ItemHelper.GetChests(Item.ItemGrade.Tier2).First();
    //    RewardManager.Instance.RegisterChest(rareChest);
    //    RefreshChestList();
    //}

    //[Category("Cheats")]
    //[DisplayName("Give Epic Chest")]
    //public void GiveEpicChest()
    //{
    //    Chest epicChest = (Chest) ItemHelper.GetChests(Item.ItemGrade.Tier3).First();
    //    RewardManager.Instance.RegisterChest(epicChest);
    //    RefreshChestList();
    //}

    //[Category("Cheats")]
    //[DisplayName("Freeze Time")]
    //public void FreezeTime()
    //{
    //    Time.timeScale = 0;
    //}

    //[Category("Cheats")]
    //[DisplayName("Unfreeze Time")]
    //public void UnfreezeTime()
    //{
    //    Time.timeScale = 1;
    //}

    //[Category("Cheats")]
    //[DisplayName("Give 100 weapon cards")]
    //public void GiveWeaponCards()
    //{
    //    ItemHelper.TestGiveTonsOfWeaponCards();
    //}

    //[Category("Cheats")]
    //[DisplayName("Give premium")]
    //public void GivePremium()
    //{
    //    ItemHelper.TestGivePremiumBundle();
    //    GameManager.Instance.LoadScene(GameManager.Instance.MainSceneName());
    //}

    //[Category("Cheats")]
    //[DisplayName("Cancel premium")]
    //public void CancelPremium()
    //{
    //    PremiumManager.Instance.EndPremium();

    //}

    //[Category("Dynamic Difficulty")]
    //[DisplayName("Activate")]
    //public bool ActivateDynamicDifficulty
    //{
    //    get { return CommonSettingsManager.CommonSettingsInstance.activateDynamicDifficulty; }
    //    set
    //    {
    //        CommonSettingsManager.CommonSettingsInstance.activateDynamicDifficulty = value;
    //        OnPropertyChanged(nameof(ActivateDynamicDifficulty));
    //        SaveSettings();
    //    }
    //}

    //[Category("Dynamic Difficulty")]
    //[DisplayName("Graph")]
    //public GraphChooser.GraphName DifficultyGraph
    //{
    //    get { return CommonSettingsManager.CommonSettingsInstance.graphName; }
    //    set
    //    {
    //        CommonSettingsManager.CommonSettingsInstance.graphName = value;
    //        OnPropertyChanged(nameof(DifficultyGraph));
    //        SaveSettings();
    //    }
    //}

    //[Category("Dynamic Difficulty")]
    //[DisplayName("Intensity")]
    //public DifficultyEvaluator.DifficultyProfiles ChangeDifficultyProfile
    //{
    //    get { return CommonSettingsManager.CommonSettingsInstance.difficultyProfile; }
    //    set
    //    {
    //        CommonSettingsManager.CommonSettingsInstance.difficultyProfile = value;
    //        OnPropertyChanged(nameof(ChangeDifficultyProfile));
    //        SaveSettings();
    //    }
    //}

    //[Category("Token")]
    //[DisplayName("Copy")]
    //public void CopyToken()
    //{
    //    var token= HttpUtils.GetToken();
    //    GUIUtility.systemCopyBuffer = token;
    //}
    //[Category("Host Url")]
    //[DisplayName("Host Url Input")]
    //public string HostUrlString
    //{
    //    get { return hostUrl; }
    //    set
    //    {
    //        hostUrl = value;
    //        OnPropertyChanged(nameof(HostUrlString));
    //    }
    //} 
    //[Category("Api Host")]
    //[DisplayName("Change")]
    //public void ChangeApiHost()
    //{
    //    string clipBoard = GUIUtility.systemCopyBuffer;
    //    PlayerPrefs.SetString("MddHttpBaseUrl", clipBoard);
    //    HttpUtils.BASE_URL = HostUrlString;

    //    Debug.Log("Base Url: " + HttpUtils.BASE_URL);
    //}

    //private async Task RefreshChestList()
    //{
    //    await Task.Delay(1000);
    //    ChestManager.Instance.ListChests();
    //}


    //private void SaveSettings()
    //{
    //    Debug.Log("Settings Saved");
    //    SceneSettings.Instance.DoSave();
    //    SceneSettings.Instance.InvokeDataChanged();
    //}

#endif
}
