using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using GTA;
using GTA.Native;
using GTA.Math;
using System.Runtime.InteropServices;
using LemonUI;
using LemonUI.Scaleform;
using LemonUI.Menus;
using LemonUI.Elements;
using LemonUI.Extensions;
using LemonUI.Tools;

public class AddonWeapons : Script
{
    private Dictionary<uint, List<DlcWeaponDataWithComponents>> weaponCategories = new Dictionary<uint, List<DlcWeaponDataWithComponents>>();
    private Dictionary<uint, string> groupNames = new Dictionary<uint, string>();

    ObjectPool pool;
    NativeMenu menu;
    NativeMenu HeavyMenu;
    NativeMenu MeleeMenu;
    NativeMenu MachineGunsMenu;
    NativeMenu PistolsMenu;
    NativeMenu RiflesMenu;
    NativeMenu ShotgunsMenu;
    NativeMenu SMGsMenu;
    NativeMenu SniperRiflesMenu;
    NativeMenu StunGunMenu;
    NativeMenu ThrownMenu;
    NativeMenu ComponentMenu;

    const uint GROUP_DIGISCANNER = 3539449195u;
    const uint GROUP_FIREEXTINGUISHER = 4257178988u;
    const uint GROUP_HACKINGDEVICE = 1175761940u;
    const uint GROUP_HEAVY = 2725924767u;
    const uint GROUP_MELEE = 3566412244u;
    const uint GROUP_METALDETECTOR = 3759491383u;
    const uint GROUP_MG = 1159398588u;
    const uint GROUP_NIGHTVISION = 3493187224u;
    const uint GROUP_PARACHUTE = 431593103u;
    const uint GROUP_PETROLCAN = 1595662460u;
    const uint GROUP_PISTOL = 416676503u;
    const uint GROUP_RIFLE = 970310034u;
    const uint GROUP_SHOTGUN = 860033945u;
    const uint GROUP_SMG = 3337201093u;
    const uint GROUP_SNIPER = 3082541095u;
    const uint GROUP_STUNGUN = 690389602u;
    const uint GROUP_THROWN = 1548507267u;
    const uint GROUP_TRANQILIZER = 75159441u;
    const uint GROUP_UNARMED = 2685387236u;
    const int MAX_DLC_WEAPONS = 69;

    string _TITLE_MAIN;
    string _TITLE_HEAVY;
    string _TITLE_MELEE;
    string _TITLE_MG;
    string _TITLE_PISTOLS;
    string _TITLE_RIFLES;
    string _TITLE_SHOTGUNS;
    string _TITLE_SMG;
    string _TITLE_SR;
    string _TITLE_SG;
    string _TITLE_THROWN;
    string _ROUNDS;
    string _MAX_ROUNDS;
    string _HELP_MESSAGE;
    string _NO_MONEY;

    Hash WEAPON_BOX = Function.Call<Hash>(Hash.GET_HASH_KEY, "prop_box_ammo03a_set2");

    Prop box1;
    Prop box2;

    Vector3 box_pos_1;
    Vector3 box_pos_2;
    Vector3 box_rot_1;
    Vector3 box_rot_2;

    Model model_box = new Model(2107849419);

    ScriptSettings config;
    ScriptSettings config_settings;

    int index_menu_count = -1;

    Keys menuOpenKey;

    public AddonWeapons()
    {
        SetLanguage();
        InitializeCategories();
        InitializeMenu();
        GetDlcWeaponModels();
        SetMenuItems();
        LoadAmmoBoxes();
        Tick += OnTick;
        KeyUp += onkeyup;
        Aborted += OnAborted;
    }

    private void SetLanguage()
    {
        config_settings = ScriptSettings.Load($"Scripts\\AddonWeapons\\settings.ini");
        _TITLE_MAIN = Game.GetLocalizedString("AD_AMMU_20");
        _TITLE_HEAVY = Game.GetLocalizedString("VAULT_WMENUI_6");
        _TITLE_MELEE = Game.GetLocalizedString("VAULT_WMENUI_8");
        _TITLE_MG = Game.GetLocalizedString("VAULT_WMENUI_3");
        _TITLE_PISTOLS = Game.GetLocalizedString("VAULT_WMENUI_9");
        _TITLE_RIFLES = Game.GetLocalizedString("VAULT_WMENUI_4");
        _TITLE_SHOTGUNS = Game.GetLocalizedString("VAULT_WMENUI_2");
        _TITLE_SMG = Game.GetLocalizedString("HUD_MG_SMG");
        _TITLE_SR = Game.GetLocalizedString("VAULT_WMENUI_5");
        _TITLE_SG = Game.GetLocalizedString("VRT_B_SGUN1");
        _TITLE_THROWN = Game.GetLocalizedString("GS_GROUP_7");
        _ROUNDS = Game.GetLocalizedString("GSA_TYPE_R");
        _MAX_ROUNDS = Game.GetLocalizedString("SNK_FULL");
        _HELP_MESSAGE = "GS_BROWSE_W";
        _NO_MONEY = Game.GetLocalizedString("MPCT_SMON_04");

        menuOpenKey = config_settings.GetValue<Keys>("MENU", "MenuOpenKey", Keys.None);
    }

    private void InitializeGroupNames()
    {
        groupNames.Add(3539449195u, "GROUP_DIGISCANNER");
        groupNames.Add(4257178988u, "GROUP_FIREEXTINGUISHER");
        groupNames.Add(1175761940u, "GROUP_HACKINGDEVICE");
        groupNames.Add(2725924767u, "GROUP_HEAVY");
        groupNames.Add(3566412244u, "GROUP_MELEE");
        groupNames.Add(3759491383u, "GROUP_METALDETECTOR");
        groupNames.Add(1159398588u, "GROUP_MG");
        groupNames.Add(3493187224u, "GROUP_NIGHTVISION");
        groupNames.Add(431593103u, "GROUP_PARACHUTE");
        groupNames.Add(1595662460u, "GROUP_PETROLCAN");
        groupNames.Add(416676503u, "GROUP_PISTOL");
        groupNames.Add(970310034u, "GROUP_RIFLE");
        groupNames.Add(860033945u, "GROUP_SHOTGUN");
        groupNames.Add(3337201093u, "GROUP_SMG");
        groupNames.Add(3082541095u, "GROUP_SNIPER");
        groupNames.Add(690389602u, "GROUP_STUNGUN");
        groupNames.Add(1548507267u, "GROUP_THROWN");
        groupNames.Add(75159441u, "GROUP_TRANQILIZER");
        groupNames.Add(2685387236u, "GROUP_UNARMED");
    }

    private void InitializeCategories()
    {
        weaponCategories.Add(3539449195u, new List<DlcWeaponDataWithComponents>()); // GROUP_DIGISCANNER
        weaponCategories.Add(4257178988u, new List<DlcWeaponDataWithComponents>()); // GROUP_FIREEXTINGUISHER
        weaponCategories.Add(1175761940u, new List<DlcWeaponDataWithComponents>()); // GROUP_HACKINGDEVICE
        weaponCategories.Add(2725924767u, new List<DlcWeaponDataWithComponents>()); // GROUP_HEAVY
        weaponCategories.Add(3566412244u, new List<DlcWeaponDataWithComponents>()); // GROUP_MELEE
        weaponCategories.Add(3759491383u, new List<DlcWeaponDataWithComponents>()); // GROUP_METALDETECTOR
        weaponCategories.Add(1159398588u, new List<DlcWeaponDataWithComponents>()); // GROUP_MG
        weaponCategories.Add(3493187224u, new List<DlcWeaponDataWithComponents>()); // GROUP_NIGHTVISION
        weaponCategories.Add(431593103u, new List<DlcWeaponDataWithComponents>()); // GROUP_PARACHUTE
        weaponCategories.Add(1595662460u, new List<DlcWeaponDataWithComponents>()); // GROUP_PETROLCAN
        weaponCategories.Add(416676503u, new List<DlcWeaponDataWithComponents>()); // GROUP_PISTOL
        weaponCategories.Add(970310034u, new List<DlcWeaponDataWithComponents>()); // GROUP_RIFLE
        weaponCategories.Add(860033945u, new List<DlcWeaponDataWithComponents>()); // GROUP_SHOTGUN
        weaponCategories.Add(3337201093u, new List<DlcWeaponDataWithComponents>()); // GROUP_SMG
        weaponCategories.Add(3082541095u, new List<DlcWeaponDataWithComponents>()); // GROUP_SNIPER
        weaponCategories.Add(690389602u, new List<DlcWeaponDataWithComponents>()); // GROUP_STUNGUN
        weaponCategories.Add(1548507267u, new List<DlcWeaponDataWithComponents>()); // GROUP_THROWN
        weaponCategories.Add(75159441u, new List<DlcWeaponDataWithComponents>()); // GROUP_TRANQILIZER
        weaponCategories.Add(2685387236u, new List<DlcWeaponDataWithComponents>()); // GROUP_UNARMED
    }

    private void InitializeMenu()
    {
        pool = new ObjectPool();
        menu = new NativeMenu("", _TITLE_MAIN, " ", new ScaledTexture(PointF.Empty, new SizeF(0, 108), "shopui_title_gunclub", "shopui_title_gunclub"));
        HeavyMenu = new NativeMenu("", _TITLE_HEAVY, " ", new ScaledTexture(PointF.Empty, new SizeF(0, 108), "shopui_title_gunclub", "shopui_title_gunclub"));
        MeleeMenu = new NativeMenu("", _TITLE_MELEE, " ", new ScaledTexture(PointF.Empty, new SizeF(0, 108), "shopui_title_gunclub", "shopui_title_gunclub"));
        MachineGunsMenu = new NativeMenu("", _TITLE_MG, " ", new ScaledTexture(PointF.Empty, new SizeF(0, 108), "shopui_title_gunclub", "shopui_title_gunclub"));
        PistolsMenu = new NativeMenu("", _TITLE_PISTOLS, " ", new ScaledTexture(PointF.Empty, new SizeF(0, 108), "shopui_title_gunclub", "shopui_title_gunclub"));
        RiflesMenu = new NativeMenu("", _TITLE_RIFLES, " ", new ScaledTexture(PointF.Empty, new SizeF(0, 108), "shopui_title_gunclub", "shopui_title_gunclub"));
        ShotgunsMenu = new NativeMenu("", _TITLE_SHOTGUNS, " ", new ScaledTexture(PointF.Empty, new SizeF(0, 108), "shopui_title_gunclub", "shopui_title_gunclub"));
        SMGsMenu = new NativeMenu("", _TITLE_SMG, " ", new ScaledTexture(PointF.Empty, new SizeF(0, 108), "shopui_title_gunclub", "shopui_title_gunclub"));
        SniperRiflesMenu = new NativeMenu("", _TITLE_SR, " ", new ScaledTexture(PointF.Empty, new SizeF(0, 108), "shopui_title_gunclub", "shopui_title_gunclub"));
        StunGunMenu = new NativeMenu("", _TITLE_SG, " ", new ScaledTexture(PointF.Empty, new SizeF(0, 108), "shopui_title_gunclub", "shopui_title_gunclub"));
        ThrownMenu = new NativeMenu("", _TITLE_THROWN, " ", new ScaledTexture(PointF.Empty, new SizeF(0, 108), "shopui_title_gunclub", "shopui_title_gunclub"));

        ComponentMenu = new NativeMenu("", "", " ", new ScaledTexture(PointF.Empty, new SizeF(0, 108), "shopui_title_gunclub", "shopui_title_gunclub"));

        menu.AddSubMenu(HeavyMenu);
        menu.AddSubMenu(MeleeMenu);
        menu.AddSubMenu(MachineGunsMenu);
        menu.AddSubMenu(PistolsMenu);
        menu.AddSubMenu(RiflesMenu);
        menu.AddSubMenu(ShotgunsMenu);
        menu.AddSubMenu(SMGsMenu);
        menu.AddSubMenu(SniperRiflesMenu);
        menu.AddSubMenu(StunGunMenu);
        menu.AddSubMenu(ThrownMenu);

        pool.Add(menu);
        pool.Add(HeavyMenu);
        pool.Add(MeleeMenu);
        pool.Add(MachineGunsMenu);
        pool.Add(PistolsMenu);
        pool.Add(RiflesMenu);
        pool.Add(ShotgunsMenu);
        pool.Add(SMGsMenu);
        pool.Add(SniperRiflesMenu);
        pool.Add(StunGunMenu);
        pool.Add(ThrownMenu);
        pool.Add(ComponentMenu);
    }

    private void LoadAmmoBoxes()
    {
        box_pos_1 = new Vector3(19.04f, -1103.96f, 29.24f);
        box_pos_2 = new Vector3(814.0817f, -2159.347f, 29.04f);
        box_rot_1 = new Vector3(1.001787E-05f, 5.008956E-06f, -18.99999f);
        box_rot_2 = new Vector3(0f, 0f, 0f);
    }

    private void CreateAmmoBoxesThisFrame()
    {
        if (Game.Player.Character.Position.DistanceTo(box_pos_1) < 10f && box1 == null)
        {
            box1 = World.CreateProp(model_box, box_pos_1, box_rot_1, false, false);
            Function.Call(Hash.PLACE_OBJECT_ON_GROUND_PROPERLY, box1);
            Function.Call(Hash.FREEZE_ENTITY_POSITION, box1, true);
        }

        if (Game.Player.Character.Position.DistanceTo(box_pos_1) > 15f && box1 != null && box1.Exists())
        {
            box1.Delete();
            box1 = null;
        }

        if (Game.Player.Character.Position.DistanceTo(box_pos_2) < 10f && box2 == null)
        {
            box2 = World.CreateProp(model_box, box_pos_2, box_rot_2, false, false);
            Function.Call(Hash.PLACE_OBJECT_ON_GROUND_PROPERLY, box2);
            Function.Call(Hash.FREEZE_ENTITY_POSITION, box2, true);
        }

        if (Game.Player.Character.Position.DistanceTo(box_pos_2) > 15f && box2 != null && box2.Exists())
        {
            box2.Delete();
            box2 = null;
        }
    }

    private void DeleteAmmoBoxes()
    {
        if (box1 != null && box1.Exists())
        {
            box1.Delete();
        }
    }

    private void OnAborted(object sender, EventArgs e)
    {
        DeleteAmmoBoxes();
    }

    private void onkeyup(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == menuOpenKey)
        {
            menu.Visible = true;
        }
    }

    private void OnTick(object sender, EventArgs e)
    {
        pool.Process();
        CreateAmmoBoxesThisFrame();

        if (Game.Player.Character.Position.DistanceTo(box_pos_1) < 1.5f || Game.Player.Character.Position.DistanceTo(box_pos_2) < 1.5f)
        {
            if (!IsMenuOpen())
            {
                Function.Call(Hash.BEGIN_TEXT_COMMAND_DISPLAY_HELP, _HELP_MESSAGE);
                Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_KEYBOARD_DISPLAY, "~INPUT_CONTEXT~");
                Function.Call(Hash.END_TEXT_COMMAND_DISPLAY_HELP, 0, 0, 1, -1);
            }

            if (Function.Call<bool>(Hash.IS_CONTROL_JUST_PRESSED, 0, 51))
            {
                menu.Visible = true;
            }
        }

        if (Game.Player.Character.Position.DistanceTo(box_pos_1) > 1.5f && Game.Player.Character.Position.DistanceTo(box_pos_2) > 1.5f && IsMenuOpen())
        {
            CloseAllMenus();
        }
    }

    private bool IsMenuOpen()
    {
        if (menu.Visible || HeavyMenu.Visible || MeleeMenu.Visible || MachineGunsMenu.Visible || PistolsMenu.Visible || RiflesMenu.Visible || ShotgunsMenu.Visible || SMGsMenu.Visible || SniperRiflesMenu.Visible || StunGunMenu.Visible || ThrownMenu.Visible || ComponentMenu.Visible)
        {
            return true;
        }
        return false;
    }

    private void CloseAllMenus()
    {
        menu.Visible = false;
        HeavyMenu.Visible = false;
        MeleeMenu.Visible = false;
        MachineGunsMenu.Visible = false;
        PistolsMenu.Visible = false;
        RiflesMenu.Visible = false;
        ShotgunsMenu.Visible = false;
        SMGsMenu.Visible = false;
        SniperRiflesMenu.Visible = false;
        StunGunMenu.Visible = false;
        ThrownMenu.Visible = false;
        ComponentMenu.Visible = false;
    }

    private void GetDlcWeaponModels()
    {
        int numDlcWeapons = Function.Call<int>(Hash.GET_NUM_DLC_WEAPONS);
        for (int i = 0; i < numDlcWeapons; i++)
        {
            IntPtr outData = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(DlcWeaponData)));
            try
            {
                if (Function.Call<bool>(Hash.GET_DLC_WEAPON_DATA, i, outData))
                {
                    DlcWeaponData weaponData = Marshal.PtrToStructure<DlcWeaponData>(outData);
                    List<DlcComponentData> components = GetDlcWeaponComponents(i);
                    DlcWeaponDataWithComponents weaponDataWithComponents = new DlcWeaponDataWithComponents(weaponData, components);
                    uint weaponTypeGroup = Function.Call<uint>(Hash.GET_WEAPONTYPE_GROUP, weaponData.weaponHash);
                    CategorizeWeapon(weaponDataWithComponents, weaponTypeGroup);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(outData);
            }
        }
    }

    private List<DlcComponentData> GetDlcWeaponComponents(int dlcWeaponIndex)
    {
        List<DlcComponentData> components = new List<DlcComponentData>();
        int numComponents = Function.Call<int>(Hash.GET_NUM_DLC_WEAPON_COMPONENTS, dlcWeaponIndex);

        for (int j = 0; j < numComponents; j++)
        {
            IntPtr outData = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(DlcComponentData)));
            try
            {
                if (Function.Call<bool>(Hash.GET_DLC_WEAPON_COMPONENT_DATA, dlcWeaponIndex, j, outData))
                {
                    DlcComponentData componentData = Marshal.PtrToStructure<DlcComponentData>(outData);
                    components.Add(componentData);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(outData);
            }
        }

        return components;
    }

    private void CategorizeWeapon(DlcWeaponDataWithComponents weaponDataWithComponents, uint weaponTypeGroup)
    {
        if (!weaponCategories.ContainsKey(weaponTypeGroup))
        {
            weaponCategories.Add(weaponTypeGroup, new List<DlcWeaponDataWithComponents>());
        }

        weaponCategories[weaponTypeGroup].Add(weaponDataWithComponents);
    }

    NativeItem ActivateLivery(NativeItem item, int livery_id, uint weaponHash, BadgeSet badge)
    {
        item.Activated += (sender, args) =>
        {
            if (Game.Player.Money < 1000)
            {
                GTA.UI.Screen.ShowSubtitle(_NO_MONEY);
            }
            else
            {
                Game.Player.Money -= 1000;
                Function.Call(Hash.SET_PED_WEAPON_TINT_INDEX, Game.Player.Character, weaponHash, livery_id);
                item.AltTitle = "";
                item.RightBadgeSet = badge;
            }
        };
        return item;
    }

    private void CreateWeaponLivery(string WeapLabel, uint weaponHash)
    {
        List<string> tints = new List<string>();
        int count = Function.Call<int>(Hash.GET_WEAPON_TINT_COUNT, weaponHash);
        int temp_index = Function.Call<int>(Hash.GET_PED_WEAPON_TINT_INDEX, Game.Player.Character, weaponHash);
        string filePath = $"Scripts\\AddonWeapons\\tints\\{WeapLabel}.txt";
        BadgeSet shop_gun = CreateBafgeFromItem("commonmenu", "shop_gunclub_icon_a", "commonmenu", "shop_gunclub_icon_b");

        if (File.Exists(filePath))
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    tints.Add(line);
                }
            }

            for (int i = 0; i < tints.Count; i++)
            {
                NativeItem tint_m = new NativeItem(tints[i], "", "$1000");
                tint_m = ActivateLivery(tint_m, i, weaponHash, shop_gun);

                if (temp_index == i)
                {
                    tint_m.AltTitle = "";
                    tint_m.RightBadgeSet = shop_gun;
                }

                ComponentMenu.Add(tint_m);
            }    
        }
        else
        {
            int tint_count = Function.Call<int>(Hash.GET_WEAPON_TINT_COUNT, weaponHash);
            string tint_name;

            if (tint_count == 8)
            {
                tint_name = "WM_TINT";
            }
            else
            {
                tint_name = "WCT_TINT_";
            }

            for (int i = 0; i < tint_count; i++)
            {
                string LiveryName = Game.GetLocalizedString($"{tint_name}{i}");
                NativeItem tint_m = new NativeItem(LiveryName, "", "$1000");
                tint_m = ActivateLivery(tint_m, i, weaponHash, shop_gun);

                if (temp_index == i)
                {
                    tint_m.AltTitle = "";
                    tint_m.RightBadgeSet = shop_gun;
                }

                ComponentMenu.Add(tint_m);
            }
        }
    }

    private bool IsmaxAmmo(uint weaponHash)
    {
        int current_ammo = Function.Call<int>(Hash.GET_AMMO_IN_PED_WEAPON, Game.Player.Character, weaponHash);
        unsafe
        {
            int maxAmmo = 0;
            bool hasMaxAmmo = Function.Call<bool>(Hash.GET_MAX_AMMO, Game.Player.Character.Handle, (uint)weaponHash, (IntPtr)(&maxAmmo));
            if (maxAmmo == current_ammo)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    private BadgeSet CreateBafgeFromItem(string NormalDictionary, string NormalTexture, string HoveredDictionary, string HoveredTexture)
    {
        BadgeSet badge = new BadgeSet
        {
            NormalDictionary = NormalDictionary,
            NormalTexture = NormalTexture,
            HoveredDictionary = HoveredDictionary,
            HoveredTexture = HoveredTexture
        };

        return badge;
    }

    private NativeItem CreateAmmoItem(int defaultClipSize, string ammoCost, int cost, uint weaponHash)
    {
        if (IsmaxAmmo(weaponHash))
        {
            ammoCost = _MAX_ROUNDS;
        }

        NativeItem rounds_m = new NativeItem($"{_ROUNDS} x {defaultClipSize}", "", ammoCost);
        rounds_m.Activated += (sender3, args3) =>
        {
            if (Game.Player.Money < cost)
            {
                GTA.UI.Screen.ShowSubtitle(_NO_MONEY);
            }
            else
            {
                if (!IsmaxAmmo(weaponHash))
                {
                    Game.Player.Money -= cost;
                    Function.Call(Hash.ADD_AMMO_TO_PED, Game.Player.Character, weaponHash, defaultClipSize);
                    if (IsmaxAmmo(weaponHash))
                    {
                        rounds_m.AltTitle = _MAX_ROUNDS;
                    }
                }
            }
        };
        return rounds_m;
    }

    private NativeItem CreateUnRegisterWeapon(uint weaponHash, string WeapName, string WeapDesc, int WeapCost)
    {
        BadgeSet shop_gun = CreateBafgeFromItem("commonmenu", "shop_gunclub_icon_a", "commonmenu", "shop_gunclub_icon_b");
        string WeapCost_str = $"${WeapCost}";
        NativeItem weap_m = new NativeItem(WeapName, WeapDesc, WeapCost_str);
        weap_m.Activated += (sender, args) =>
        {
            if (Game.Player.Money < WeapCost)
            {
                GTA.UI.Screen.ShowSubtitle(_NO_MONEY);
            }
            else
            {
                Game.Player.Money -= WeapCost;
                Game.Player.Character.Weapons.Give((WeaponHash)weaponHash, 1000, true, true);
            }
            weap_m.AltTitle = "";
            weap_m.RightBadgeSet = shop_gun;
        };

        return weap_m;
    }

    private NativeItem CreateWeaponItem(DlcWeaponDataWithComponents weapon, string WeapName, string WeapDesc, string WeapCost, uint weaponHash)
    {
        BadgeSet shop_gun = CreateBafgeFromItem("commonmenu", "shop_gunclub_icon_a", "commonmenu", "shop_gunclub_icon_b");

        NativeItem weap_m = new NativeItem(WeapName, WeapDesc, WeapCost);
        weap_m.Activated += (sender, args) =>
        {
            if (Game.Player.Money < weapon.WeaponData.weaponCost)
            {
                GTA.UI.Screen.ShowSubtitle(_NO_MONEY);
            }
            else
            {
                if (Game.Player.Character.Weapons.HasWeapon((WeaponHash)weaponHash))
                {
                    if (Function.Call<uint>(Hash.GET_WEAPONTYPE_GROUP, weaponHash) != GROUP_MELEE)
                    {
                        CloseAllMenus();
                        ComponentMenu.Clear();
                        int rounds = -1;
                        if (weapon.Components.Count == 0)
                        {
                            int defaultClipSize = weapon.WeaponData.defaultClipSize;
                            string ammoCost = $"${weapon.WeaponData.ammoCost}";
                            string WeapLabel = weapon.WeaponData.GetNameLabel();
                            NativeItem rounds_m = CreateAmmoItem(defaultClipSize, ammoCost, weapon.WeaponData.ammoCost, weaponHash);
                            ComponentMenu.Add(rounds_m);
                            CreateWeaponLivery(WeapLabel, weaponHash);

                            if ((WeaponHash)weaponHash == WeaponHash.StunGunMultiplayer)
                            {
                                string CompName = Game.GetLocalizedString("WCT_STNGN_BAIL");
                                uint componentHash = Function.Call<uint>(Hash.GET_HASH_KEY, "COMPONENT_STUNGUN_VARMOD_BAIL");
                                string componentCost = $"${weapon.WeaponData.ammoCost}";
                                int componentCost_int = 1000;
                                NativeItem comp_m = CreateComponentItem(CompName, componentCost, componentCost_int, componentHash, weaponHash, defaultClipSize, ammoCost);
                                ComponentMenu.Add(comp_m);
                            }
                        }
                        else
                        {
                            foreach (var component in weapon.Components)
                            {
                                string componentName = Game.GetLocalizedString(component.GetNameLabel());
                                string componentCost = $"${component.componentCost}";
                                uint componentHash = component.componentHash;
                                int defaultClipSize = weapon.WeaponData.defaultClipSize;
                                string ammoCost = $"${weapon.WeaponData.ammoCost}";
                                string WeapLabel = weapon.WeaponData.GetNameLabel();

                                if (rounds == -1)
                                {
                                    rounds = 1;
                                    NativeItem rounds_m = CreateAmmoItem(defaultClipSize, ammoCost, weapon.WeaponData.ammoCost, weaponHash);
                                    ComponentMenu.Add(rounds_m);
                                    CreateWeaponLivery(WeapLabel, weaponHash);
                                }

                                NativeItem comp_m = CreateComponentItem(componentName, componentCost, component.componentCost, componentHash, weaponHash, defaultClipSize, ammoCost);

                                if (Function.Call<bool>(Hash.HAS_PED_GOT_WEAPON_COMPONENT, Game.Player.Character.Handle, weaponHash, componentHash))
                                {
                                    comp_m.AltTitle = "";
                                    comp_m.RightBadgeSet = shop_gun;
                                }
                                ComponentMenu.Add(comp_m);
                            }
                        }
                        ComponentMenu.Visible = true;
                    }
                }
                else
                {
                    Game.Player.Money -= weapon.WeaponData.weaponCost;
                    Game.Player.Character.Weapons.Give((WeaponHash)weaponHash, 1000, true, true);
                }

                weap_m.AltTitle = "";
                weap_m.RightBadgeSet = shop_gun;
            }
        };
        return weap_m;
    }

    private NativeItem CreateComponentItem(string componentName, string componentCost, int cost_int, uint componentHash, uint weaponHash, int defaultClipSize, string ammoCost)
    {
        BadgeSet shop_gun = CreateBafgeFromItem("commonmenu", "shop_gunclub_icon_a", "commonmenu", "shop_gunclub_icon_b");

        NativeItem comp_m = new NativeItem(componentName, "", componentCost);
        comp_m.Activated += (sender, args) =>
        {
            if (Game.Player.Money < cost_int)
            {
                GTA.UI.Screen.ShowSubtitle(_NO_MONEY);
            }
            else
            {
                Game.Player.Money -= cost_int;
                Function.Call(Hash.GIVE_WEAPON_COMPONENT_TO_PED, Game.Player.Character.Handle, weaponHash, componentHash);

                comp_m.AltTitle = "";
                comp_m.RightBadgeSet = shop_gun;
            }
        };
        return comp_m;
    }

    private void SetMenuItems()
    {
        foreach (var category in weaponCategories)
        {
            foreach (var weapon in category.Value)
            {
                if ((WeaponHash)weapon.WeaponData.weaponHash == WeaponHash.Railgun)
                {
                    continue;
                }

                string WeapName = Game.GetLocalizedString(weapon.WeaponData.GetNameLabel());
                string WeapDesc = Game.GetLocalizedString(weapon.WeaponData.GetDescLabel());
                string WeapCost = $"${weapon.WeaponData.weaponCost}";
                uint weaponHash = weapon.WeaponData.weaponHash;

                if (WeapName == null || WeapName.Length < 3)
                {
                    WeapName = weapon.WeaponData.GetNameLabel();
                    if (WeapName == "WT_SNOWLAUNCHER")
                    {
                        WeapName = Game.GetLocalizedString("WT_SNOWLNCHR");
                    }
                }

                BadgeSet shop_gun = CreateBafgeFromItem("commonmenu", "shop_gunclub_icon_a", "commonmenu", "shop_gunclub_icon_b");

                NativeItem weap_m = CreateWeaponItem(weapon, WeapName, WeapDesc, WeapCost, weaponHash);

                if (Game.Player.Character.Weapons.HasWeapon((WeaponHash)weaponHash))
                {
                    weap_m.AltTitle = "";
                    weap_m.RightBadgeSet = shop_gun;
                }

                uint weaponTypeGroup = Function.Call<uint>(Hash.GET_WEAPONTYPE_GROUP, weaponHash);
                switch (weaponTypeGroup)
                {
                    case GROUP_HEAVY:
                        HeavyMenu.Add(weap_m);
                        break;
                    case GROUP_MELEE:
                        MeleeMenu.Add(weap_m);
                        break;
                    case GROUP_MG:
                        MachineGunsMenu.Add(weap_m);
                        break;
                    case GROUP_PISTOL:
                        PistolsMenu.Add(weap_m);
                        break;
                    case GROUP_RIFLE:
                        RiflesMenu.Add(weap_m);
                        break;
                    case GROUP_SHOTGUN:
                        ShotgunsMenu.Add(weap_m);
                        break;
                    case GROUP_SMG:
                        SMGsMenu.Add(weap_m);
                        break;
                    case GROUP_SNIPER:
                        SniperRiflesMenu.Add(weap_m);
                        break;
                    case GROUP_STUNGUN:
                        StunGunMenu.Add(weap_m);
                        break;
                    case GROUP_THROWN:
                        ThrownMenu.Add(weap_m);
                        break;
                }

            }
        }

        //Unregistered weapons in weapon_shop.meta must be manually added here


    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    private struct DlcWeaponData
    {
        public int emptyCheck;
        public int padding1;
        public uint weaponHash;
        public int padding2;
        public int unk;
        public int padding3;
        public int weaponCost;
        public int padding4;
        public int ammoCost;
        public int padding5;
        public int ammoType;
        public int padding6;
        public int defaultClipSize;
        public int padding7;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public byte[] nameLabel;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public byte[] descLabel;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public byte[] desc2Label;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public byte[] upperCaseNameLabel;

        public string GetNameLabel() => GetString(nameLabel);
        public string GetDescLabel() => GetString(descLabel);
        public string GetDesc2Label() => GetString(desc2Label);
        public string GetUpperCaseNameLabel() => GetString(upperCaseNameLabel);

        private string GetString(byte[] byteArray)
        {
            int count = Array.IndexOf(byteArray, (byte)0);
            if (count == -1) count = byteArray.Length;
            return System.Text.Encoding.ASCII.GetString(byteArray, 0, count);
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    private struct DlcComponentData
    {
        public int attachBone;
        public int padding1;
        public int bActiveByDefault;
        public int padding2;
        public int unk;
        public int padding3;
        public uint componentHash;
        public int padding4;
        public int unk2;
        public int padding5;
        public int componentCost;
        public int padding6;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public byte[] nameLabel;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public byte[] descLabel;

        public string GetNameLabel() => GetString(nameLabel);
        public string GetDescLabel() => GetString(descLabel);

        private string GetString(byte[] byteArray)
        {
            int count = Array.IndexOf(byteArray, (byte)0);
            if (count == -1) count = byteArray.Length;
            return System.Text.Encoding.ASCII.GetString(byteArray, 0, count);
        }
    }

    private class DlcWeaponDataWithComponents
    {
        public DlcWeaponData WeaponData { get; }
        public List<DlcComponentData> Components { get; }

        public DlcWeaponDataWithComponents(DlcWeaponData weaponData, List<DlcComponentData> components)
        {
            WeaponData = weaponData;
            Components = components;
        }
    }
}
