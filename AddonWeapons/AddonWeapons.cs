using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using GTA;
using GTA.Native;
using GTA.Math;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
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
    private Dictionary<uint, Dictionary<uint, List<uint>>> purchased_components = new Dictionary<uint, Dictionary<uint, List<uint>>>();
    private Dictionary<uint, Dictionary<uint, List<int>>> purchased_tints = new Dictionary<uint, Dictionary<uint, List<int>>>();
    private Dictionary<uint, Dictionary<uint, List<uint>>> install_components = new Dictionary<uint, Dictionary<uint, List<uint>>>();
    private Dictionary<uint, Dictionary<uint, List<int>>> install_ammo = new Dictionary<uint, Dictionary<uint, List<int>>>();
    private Dictionary<uint, Dictionary<uint, List<int>>> install_tints = new Dictionary<uint, Dictionary<uint, List<int>>>();

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

    const int COMPONENTS_DICT = 0;
    const int INSTALLCOMP_DICT = 1;
    const int TINTS_DICT = 2;
    const int AMMO_DICT = 3;
    const int INSTALLTINT_DICT = 4;

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

    Model model_box = new Model(2107849419);

    ScriptSettings config_settings;

    bool SP0_loaded = false;
    bool SP1_loaded = false;
    bool SP2_loaded = false;
    uint current_weapon_hash = 0;

    int show_ammo_flag = 0;
    int save_in_progress = 0;

    Keys menuOpenKey;

    List<string> labels = new List<string>()
    {
        "GS_TITLE_0",
        "VAULT_WMENUI_6",
        "VAULT_WMENUI_8",
        "VAULT_WMENUI_3",
        "VAULT_WMENUI_9",
        "VAULT_WMENUI_4",
        "VAULT_WMENUI_2",
        "HUD_MG_SMG",
        "VAULT_WMENUI_5",
        "VRT_B_SGUN1",
        "VAULT_WMENUI_7",
        "GSA_TYPE_R",
        "SNK_FULL",
        "GS_BROWSE_W",
        "MPCT_SMON_04"
    };

    List<int> price_standard = new List<int>()
    {
        0,
        100,
        200,
        400,
        600,
        800,
        1000,
        1500,
    };

    List<int> price_mk2 = new List<int>()
    {
        20000,
        20000,
        30000,
        30000,
        30000,
        30000,
        30000,
        35000,
        35000,
        40000,
        40000,
        40000,
        40000,
        75000,
        60000,
        60000,
        60000,
        50000,
        50000,
        50000,
        50000,
        45000,
        45000,
        100000,
        100000,
        80000,
        80000,
        75000,
        75000,
        75000,
        90000,
        90000
    };

    List<Vector3> box_pos = new List<Vector3>()
    {
        new Vector3(19.04f, -1103.96f, 29.24f),
        new Vector3(814.0817f, -2159.347f, 29.04f),
        new Vector3(1691.051f, 3756.589f, 34.14f),
        new Vector3(253.34f, -45.93f, 69.27754f),
        new Vector3(846.4512f, -1033.26f, 27.63f),
        new Vector3(-333.0135f, 6080.67f, 30.89f),
        new Vector3(-666.3039f, -935.6205f, 21.26f),
        new Vector3(-1305.119f, -390.2967f, 36.12f),
        new Vector3(-1120.622f, 2695.518f, 17.99f),
        new Vector3(-3173.297f, 1083.793f, 20.28f),
        new Vector3(2572.105f, 294.635f, 108.17f),
    };

    List<Vector3> box_rot = new List<Vector3>()
    {
        new Vector3(1.001787E-05f, 5.008956E-06f, -18.99999f),
        new Vector3(0f, 0f, 0f),
        new Vector3(0f, 0f, 46.9999f),
        new Vector3(0f, 0f, 70.99976f),
        new Vector3(0f, 0f, 0f),
        new Vector3(0f, 0f, 44.99992f),
        new Vector3(0f, 0f, 0f),
        new Vector3(0f, 0f, 74.99975f),
        new Vector3(0f, 0f, 40.99995f),
        new Vector3(0f, 0f, 64.9998f),
        new Vector3(0f, 0f, 0f),
    };

    List<Prop> box_prop = new List<Prop>() { };

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

    private void LoadInventory()
    {
        uint player = (uint)Game.Player.Character.Model.Hash;

        if (!Directory.Exists($"Scripts\\AddonWeapons\\bin"))
        {
            Directory.CreateDirectory($"Scripts\\AddonWeapons\\bin");
        }    

        if (File.Exists($"Scripts\\AddonWeapons\\bin\\components.bin"))
        {
            purchased_components = DeserializeDictionary<uint, uint, uint>("Scripts\\AddonWeapons\\bin\\components.bin");
        }
        if (File.Exists($"Scripts\\AddonWeapons\\bin\\tints.bin"))
        {
            purchased_tints = DeserializeDictionary<uint, uint, int>("Scripts\\AddonWeapons\\bin\\tints.bin");
        }
        if (File.Exists($"Scripts\\AddonWeapons\\bin\\install_components.bin"))
        {
            install_components = DeserializeDictionary<uint, uint, uint>("Scripts\\AddonWeapons\\bin\\install_components.bin");
        }
        if (File.Exists($"Scripts\\AddonWeapons\\bin\\install_ammo.bin"))
        {
            install_ammo = DeserializeDictionary<uint, uint, int>("Scripts\\AddonWeapons\\bin\\install_ammo.bin");
        }
        if (File.Exists($"Scripts\\AddonWeapons\\bin\\install_tints.bin"))
        {
            install_tints = DeserializeDictionary<uint, uint, int>("Scripts\\AddonWeapons\\bin\\install_tints.bin");
        }

        if (!purchased_components.ContainsKey(player))
        {
            purchased_components[player] = new Dictionary<uint, List<uint>>();
        }
        if (!purchased_tints.ContainsKey(player))
        {
            purchased_tints[player] = new Dictionary<uint, List<int>>();
        }
        if (!install_components.ContainsKey(player))
        {
            install_components[player] = new Dictionary<uint, List<uint>>();
        }
        if (!install_ammo.ContainsKey(player))
        {
            install_ammo[player] = new Dictionary<uint, List<int>>();
        }
        if (!install_tints.ContainsKey(player))
        {
            install_tints[player] = new Dictionary<uint, List<int>>();
        }

        List<uint> weaponsHashes = new List<uint>(purchased_components[player].Keys);
        if (weaponsHashes.Count == 0) return;

        foreach (var weaponHash in weaponsHashes)
        {
            if (!purchased_components[player].ContainsKey(weaponHash))
            {
                purchased_components[player][weaponHash] = new List<uint> { };
            }
            if (!purchased_tints[player].ContainsKey(weaponHash))
            {
                purchased_tints[player][weaponHash] = new List<int> { };
            }
            if (!install_components[player].ContainsKey(weaponHash))
            {
                install_components[player][weaponHash] = new List<uint> { };
            }
            if (!install_ammo[player].ContainsKey(weaponHash))
            {
                install_ammo[player][weaponHash] = new List<int> { };
            }
            if (!install_tints[player].ContainsKey(weaponHash))
            {
                install_tints[player][weaponHash] = new List<int> { };
            }

            if (!Game.Player.Character.Weapons.HasWeapon((WeaponHash)weaponHash))
            {
                Game.Player.Character.Weapons.Give((WeaponHash)weaponHash, install_ammo[player][weaponHash][0], true, true);
            }

            foreach (var componentHash in purchased_components[player][weaponHash])
            {
                if (install_components[player][weaponHash].Contains(componentHash))
                {
                    Function.Call(Hash.GIVE_WEAPON_COMPONENT_TO_PED, Game.Player.Character.Handle, weaponHash, componentHash);
                }
            }
            foreach (var tint in purchased_tints[player][weaponHash])
            {
                if (ValueContains(INSTALLTINT_DICT, player, weaponHash, 0, tint))
                {
                    Function.Call(Hash.SET_PED_WEAPON_TINT_INDEX, Game.Player.Character, weaponHash, tint);
                }
            }
        }
    }

    private void WaitLoadedInventory()
    {
        if (Function.Call<bool>(Hash.IS_PLAYER_SWITCH_IN_PROGRESS))
        {
            if (save_in_progress == 0)
            {
                SaveWeaponInInventory();
                save_in_progress = 1;
            }
        }
        else
        {
            if (save_in_progress == 1) save_in_progress = 0;
        }

        if (!SP0_loaded && Game.Player.Character.Model.Hash == new Model("player_zero").Hash)
        {
            LoadInventory();
            SP0_loaded = true;
            SP1_loaded = false;
            SP2_loaded = false;
        }

        if (!SP1_loaded && Game.Player.Character.Model.Hash == new Model("player_one").Hash)
        {
            LoadInventory();
            SP0_loaded = false;
            SP1_loaded = true;
            SP2_loaded = false;
        }

        if (!SP2_loaded && Game.Player.Character.Model.Hash == new Model("player_two").Hash)
        {
            LoadInventory();
            SP0_loaded = false;
            SP1_loaded = false;
            SP2_loaded = true;
        }
    }

    private void SetLanguage()
    {

        config_settings = ScriptSettings.Load($"Scripts\\AddonWeapons\\settings.ini");

        _TITLE_MAIN = Game.GetLocalizedString("GS_TITLE_0");
        if (_TITLE_MAIN.Length < 3) _TITLE_MAIN = "WEAPONS";

        _TITLE_HEAVY = Game.GetLocalizedString("VAULT_WMENUI_6");
        if (_TITLE_HEAVY.Length < 3) _TITLE_HEAVY = "Heavy Weapons";

        _TITLE_MELEE = Game.GetLocalizedString("VAULT_WMENUI_8");
        if (_TITLE_MELEE.Length < 3) _TITLE_MELEE = "Melee Weapons";

        _TITLE_MG = Game.GetLocalizedString("VAULT_WMENUI_3");
        if (_TITLE_MG.Length < 3) _TITLE_MG = "Machine Guns";

        _TITLE_PISTOLS = Game.GetLocalizedString("VAULT_WMENUI_9");
        if (_TITLE_PISTOLS.Length < 3) _TITLE_PISTOLS = "Pistols";

        _TITLE_RIFLES = Game.GetLocalizedString("VAULT_WMENUI_4");
        if (_TITLE_RIFLES.Length < 3) _TITLE_RIFLES = "Rifles";

        _TITLE_SHOTGUNS = Game.GetLocalizedString("VAULT_WMENUI_2");
        if (_TITLE_SHOTGUNS.Length < 3) _TITLE_SHOTGUNS = "Shotguns";

        _TITLE_SMG = Game.GetLocalizedString("HUD_MG_SMG");
        if (_TITLE_SMG.Length < 3) _TITLE_SMG = "Submachine Guns";

        _TITLE_SR = Game.GetLocalizedString("VAULT_WMENUI_5");
        if (_TITLE_SR.Length < 3) _TITLE_SR = "Sniper Rifles";

        _TITLE_SG = Game.GetLocalizedString("VRT_B_SGUN1");
        if (_TITLE_SG.Length < 3) _TITLE_SG = "Stun Guns";

        _TITLE_THROWN = Game.GetLocalizedString("VAULT_WMENUI_7");
        if (_TITLE_THROWN.Length < 3) _TITLE_THROWN = "Explosives";

        _ROUNDS = Game.GetLocalizedString("GSA_TYPE_R");
        if (_ROUNDS.Length < 3) _ROUNDS = "Rounds";

        _MAX_ROUNDS = Game.GetLocalizedString("GS_FULL");
        if (_MAX_ROUNDS.Length < 3) _MAX_ROUNDS = "FULL";

        _HELP_MESSAGE = "GS_BROWSE_W";

        _NO_MONEY = Game.GetLocalizedString("MPCT_SMON_04");
        if (_NO_MONEY.Length < 3) _NO_MONEY = "~z~You'll need more cash to afford that.";


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
        box_prop.Add(null);
        box_prop.Add(null);
        box_prop.Add(null);
        box_prop.Add(null);
        box_prop.Add(null);
        box_prop.Add(null);
        box_prop.Add(null);
        box_prop.Add(null);
        box_prop.Add(null);
        box_prop.Add(null);
        box_prop.Add(null);
    }

    private void ShowAmmo()
    {
        if (show_ammo_flag == 1)
        {
            Function.Call(Hash.DISPLAY_AMMO_THIS_FRAME, true);
        }
    }

    private void CreateAmmoBoxesThisFrame()
    {
        for (int i = 0; i < box_pos.Count; i++)
        {
            if (Game.Player.Character.Position.DistanceTo(box_pos[i]) < 10f)
            {
                if (box_prop[i] == null)
                {
                    box_prop[i] = World.CreateProp(model_box, box_pos[i], box_rot[i], false, false);
                    Function.Call(Hash.PLACE_OBJECT_ON_GROUND_PROPERLY, box_prop[i]);
                    Function.Call(Hash.FREEZE_ENTITY_POSITION, box_prop[i], true);
                }
            }

            if (Game.Player.Character.Position.DistanceTo(box_pos[i]) > 15f && box_prop[i] != null && box_prop[i].Exists())
            {
                box_prop[i].Delete();
                box_prop[i] = null;
            }

            if (box_prop[i] != null && box_prop[i].Exists())
            {
                if (Game.Player.Character.Position.DistanceTo(box_prop[i].Position) < 1.5f)
                {
                    if (!IsMenuOpen())
                    {
                        show_ammo_flag = 0;
                        Function.Call(Hash.BEGIN_TEXT_COMMAND_DISPLAY_HELP, _HELP_MESSAGE);
                        Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_KEYBOARD_DISPLAY, "~INPUT_CONTEXT~");
                        Function.Call(Hash.END_TEXT_COMMAND_DISPLAY_HELP, 0, 0, 1, -1);
                    }

                    if (Function.Call<bool>(Hash.IS_CONTROL_JUST_PRESSED, 0, 51))
                    {
                        RefreshMenus();
                        menu.Visible = true;
                    }
                }
            }
        }    
    }

    private void AddDictValue(int type_dict, uint player, uint weaponHash, uint componentHash, int tint)
    {
        switch (type_dict)
        {
            case COMPONENTS_DICT:
                if (!purchased_components[player].ContainsKey(weaponHash))
                {
                    purchased_components[player].Add(weaponHash, new List<uint> { });
                }
                purchased_components[player][weaponHash].Add(componentHash);
                break;

            case INSTALLCOMP_DICT:
                if (!install_components[player].ContainsKey(weaponHash))
                {
                    install_components[player].Add(weaponHash, new List<uint> { });
                }
                install_components[player][weaponHash].Add(componentHash);
                break;

            case TINTS_DICT:
                if (!purchased_tints[player].ContainsKey(weaponHash))
                {
                    purchased_tints[player].Add(weaponHash, new List<int> { });
                }
                purchased_tints[player][weaponHash].Add(tint);
                break;

            case AMMO_DICT:
                if (!install_ammo[player].ContainsKey(weaponHash))
                {
                    install_ammo[player].Add(weaponHash, new List<int> { });
                }

                if (install_ammo[player][weaponHash].Count == 0)
                {
                    install_ammo[player][weaponHash].Add(tint);
                }
                else
                {
                    install_ammo[player][weaponHash][0] = tint;
                }
                break;
            case INSTALLTINT_DICT:
                if (!install_tints[player].ContainsKey(weaponHash))
                {
                    install_tints[player].Add(weaponHash, new List<int> { });
                }

                if (install_tints[player][weaponHash].Count == 0)
                {
                    install_tints[player][weaponHash].Add(tint);
                }
                else
                {
                    install_tints[player][weaponHash][0] = tint;
                }
                break;
        }
    }

    private void RemoveDictValue(int type_dict, uint player, uint weaponHash, uint componentHash, int tint)
    {
        switch (type_dict)
        {
            case COMPONENTS_DICT:
                if (!purchased_components[player].ContainsKey(weaponHash))
                {
                    purchased_components[player].Add(weaponHash, new List<uint> { });
                }
                purchased_components[player][weaponHash].Remove(componentHash);
                break;

            case INSTALLCOMP_DICT:
                if (!install_components[player].ContainsKey(weaponHash))
                {
                    install_components[player].Add(weaponHash, new List<uint> { });
                }
                install_components[player][weaponHash].Remove(componentHash);
                break;

            case TINTS_DICT:
                if (!purchased_tints[player].ContainsKey(weaponHash))
                {
                    purchased_tints[player].Add(weaponHash, new List<int> { });
                }
                purchased_tints[player][weaponHash].Remove(tint);
                break;

            case AMMO_DICT:
                if (!install_ammo[player].ContainsKey(weaponHash))
                {
                    install_ammo[player].Add(weaponHash, new List<int> { });
                }
                install_ammo[player][weaponHash].Remove(tint);
                break;

            case INSTALLTINT_DICT:
                if (!install_tints[player].ContainsKey(weaponHash))
                {
                    install_tints[player].Add(weaponHash, new List<int> { });
                }
                install_tints[player][weaponHash].Remove(tint);
                break;
        }
    }

    private bool ValueContains(int type_dict, uint player, uint weaponHash, uint componentHash, int tint)
    {
        bool result = false;
        switch (type_dict)
        {
            case COMPONENTS_DICT:
                if (!purchased_components[player].ContainsKey(weaponHash))
                {
                    purchased_components[player].Add(weaponHash, new List<uint> { });
                }
                if (purchased_components[player][weaponHash].Contains(componentHash)) result = true;
                break;

            case INSTALLCOMP_DICT:
                if (!install_components[player].ContainsKey(weaponHash))
                {
                    install_components[player].Add(weaponHash, new List<uint> { });
                }
                if (install_components[player][weaponHash].Contains(componentHash)) result = true;
                break;

            case TINTS_DICT:
                if (!purchased_tints[player].ContainsKey(weaponHash))
                {
                    purchased_tints[player].Add(weaponHash, new List<int> { });
                }
                if (purchased_tints[player][weaponHash].Contains(tint)) result = true;
                break;

            case AMMO_DICT:
                if (!install_ammo[player].ContainsKey(weaponHash))
                {
                    install_ammo[player].Add(weaponHash, new List<int> { });
                }
                if (install_ammo[player][weaponHash].Contains(tint)) result = true;
                break;

            case INSTALLTINT_DICT:
                if (!install_tints[player].ContainsKey(weaponHash))
                {
                    install_tints[player].Add(weaponHash, new List<int> { });
                }
                if (install_tints[player][weaponHash].Contains(tint)) result = true;
                break;
        }
        return result;
    }

    private void DeleteAmmoBoxes()
    {
        foreach (Prop box in box_prop)
        {
            if (box != null && box.Exists())
            {
                box.Delete();
            }
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
            RefreshMenus();
            menu.Visible = true;
        }
    }

    private void OnTick(object sender, EventArgs e)
    {
        pool.Process();
        CreateAmmoBoxesThisFrame();
        WaitLoadedInventory();
        ShowAmmo();
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

    void SaveWeaponInInventory()
    {

        uint player = (uint)Game.Player.Character.Model.Hash;
        List<uint> weaponsHashes = new List<uint>(purchased_components[player].Keys);

        foreach (var weaponHash in weaponsHashes)
        {
            if (purchased_components[player].ContainsKey(weaponHash))
            {
                int current_ammo = Function.Call<int>(Hash.GET_AMMO_IN_PED_WEAPON, Game.Player.Character, weaponHash);
                AddDictValue(AMMO_DICT, player, weaponHash, 0, current_ammo);

                foreach (var componentHash in purchased_components[player][weaponHash])
                {
                    if (Function.Call<bool>(Hash.HAS_PED_GOT_WEAPON_COMPONENT, Game.Player.Character, weaponHash, componentHash))
                    {
                        AddDictValue(INSTALLCOMP_DICT, player, weaponHash, componentHash, 0);
                    }
                }
            }
        }

        SerializeDictionary<uint, uint, uint>("Scripts\\AddonWeapons\\bin\\components.bin", purchased_components);
        SerializeDictionary<uint, uint, int>("Scripts\\AddonWeapons\\bin\\tints.bin", purchased_tints);
        SerializeDictionary<uint, uint, uint>("Scripts\\AddonWeapons\\bin\\install_components.bin", install_components);
        SerializeDictionary<uint, uint, int>("Scripts\\AddonWeapons\\bin\\install_ammo.bin", install_ammo);
        SerializeDictionary<uint, uint, int>("Scripts\\AddonWeapons\\bin\\install_tints.bin", install_tints);
    }

    NativeItem ActivateLivery(DlcWeaponDataWithComponents weapon, NativeItem item, int livery_id, uint weaponHash, BadgeSet badge)
    {
        uint player = (uint)Game.Player.Character.Model.Hash;
        item.Activated += (sender, args) =>
        {
            if (Game.Player.Money < 1000)
            {
                GTA.UI.Screen.ShowSubtitle(_NO_MONEY);
            }
            else
            {
                if (ValueContains(TINTS_DICT, player, weaponHash, 0, livery_id))
                {
                    Function.Call(Hash.SET_PED_WEAPON_TINT_INDEX, Game.Player.Character, weaponHash, livery_id);
                    AddDictValue(INSTALLTINT_DICT, player, weaponHash, 0, livery_id);
                }
                else
                {
                    Game.Player.Money -= 1000;
                    AddDictValue(TINTS_DICT, player, weaponHash, 0, livery_id);
                }

                List<uint> components_hashes = GetComponentsList(weapon);
                List<int> components_cost = GetComponentsCost(weapon);
                RefreshComponentMenu(ComponentMenu, components_hashes, components_cost);
            }
        };
        return item;
    }

    private void CreateWeaponLivery(DlcWeaponDataWithComponents weapon, string WeapLabel, uint weaponHash)
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
                tint_m = ActivateLivery(weapon, tint_m, i, weaponHash, shop_gun);

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
                List<int> price_temp = price_standard;
            }
            else
            {
                tint_name = "WCT_TINT_";
                List<int> price_temp = price_mk2;
            }

            for (int i = 0; i < tint_count; i++)
            {
                string LiveryName = Game.GetLocalizedString($"{tint_name}{i}");
                if (LiveryName.Length < 2) continue; 
                NativeItem tint_m = new NativeItem(LiveryName, "", "$1000");
                tint_m = ActivateLivery(weapon, tint_m, i, weaponHash, shop_gun);

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
                    uint player = (uint)Game.Player.Character.Model.Hash;
                    int current_ammo = Function.Call<int>(Hash.GET_AMMO_IN_PED_WEAPON, Game.Player.Character, weaponHash);
                    AddDictValue(AMMO_DICT, player, weaponHash, 0, current_ammo);

                    if (IsmaxAmmo(weaponHash))
                    {
                        rounds_m.AltTitle = _MAX_ROUNDS;
                    }
                }
            }
        };
        return rounds_m;
    }

    private List<uint> GetComponentsList(DlcWeaponDataWithComponents weapon)
    {
        List<uint> components_hashes = new List<uint>();

        for (int i = 0; i < Function.Call<int>(Hash.GET_WEAPON_TINT_COUNT, weapon.WeaponData.weaponHash); i++)
        {
            components_hashes.Add(0);
        }

        foreach (var component in weapon.Components)
        {
            components_hashes.Add(component.componentHash);
        }

        return components_hashes;
    }

    private List<int> GetComponentsCost(DlcWeaponDataWithComponents weapon)
    {
        List<int> components_cost = new List<int>();

        for (int i = 0; i < Function.Call<int>(Hash.GET_WEAPON_TINT_COUNT, weapon.WeaponData.weaponHash); i++)
        {
            components_cost.Add(1000);
        }

        foreach (var component in weapon.Components)
        {
            components_cost.Add(component.componentCost);
        }

        return components_cost;
    }

    private void RefreshComponentMenu(NativeMenu ComponentMenu, List<uint> components_hashes, List<int> components_cost)
    {
        uint player = (uint)Game.Player.Character.Model.Hash;
        BadgeSet shop_gun = CreateBafgeFromItem("commonmenu", "shop_gunclub_icon_a", "commonmenu", "shop_gunclub_icon_b");
        BadgeSet shop_tick = CreateBafgeFromItem("commonmenu", "shop_tick_icon", "commonmenu", "shop_tick_icon");
        List<NativeItem> items = ComponentMenu.Items;
        int index = 0;
        bool IsRounds = true;

        foreach (var item in items)
        {
            if (IsRounds)
            {
                IsRounds = false;
                continue;
            }

            int max_index = Function.Call<int>(Hash.GET_WEAPON_TINT_COUNT, current_weapon_hash);
            if (index < max_index)
            {
                if (Function.Call<int>(Hash.GET_PED_WEAPON_TINT_INDEX, Game.Player.Character, current_weapon_hash) == index)
                {
                    if (!ValueContains(TINTS_DICT, player, current_weapon_hash, 0, index))
                    {
                        AddDictValue(TINTS_DICT, player, current_weapon_hash, 0, index);
                    }
                    item.AltTitle = "";
                    item.RightBadgeSet = shop_gun;
                    index++;
                }
                else
                {
                    if (ValueContains(TINTS_DICT, player, current_weapon_hash, 0, index))
                    {
                        item.AltTitle = "";
                        item.RightBadgeSet = shop_tick;
                        index++;
                    }
                    else
                    {
                        item.AltTitle = "$1000";
                        item.RightBadgeSet = null;
                        index++;
                    }
                }
            }
            else
            {
                if (Function.Call<bool>(Hash.HAS_PED_GOT_WEAPON_COMPONENT, Game.Player.Character, current_weapon_hash, components_hashes[index]))
                {
                    if (ValueContains(COMPONENTS_DICT, player, current_weapon_hash, components_hashes[index], 0))
                    {
                        AddDictValue(COMPONENTS_DICT, player, current_weapon_hash, components_hashes[index], 0);
                    }

                    item.AltTitle = "";
                    item.RightBadgeSet = shop_gun;
                    index++;
                }
                else
                {
                    if (ValueContains(COMPONENTS_DICT, player, current_weapon_hash, components_hashes[index], 0))
                    {
                        item.AltTitle = "";
                        item.RightBadgeSet = shop_tick;
                        index++;
                    }
                    else
                    {
                        item.AltTitle = $"${components_cost[index]}";
                        item.RightBadgeSet = null;
                        index++;
                    }
                }
            }
        }

        SaveWeaponInInventory();
    }

    private NativeItem CreateWeaponItem(DlcWeaponDataWithComponents weapon, string WeapName, string WeapDesc, string WeapCost, uint weaponHash)
    {
        BadgeSet shop_gun = CreateBafgeFromItem("commonmenu", "shop_gunclub_icon_a", "commonmenu", "shop_gunclub_icon_b");
        uint player = (uint)Game.Player.Character.Model.Hash;
        NativeItem weap_m = new NativeItem(WeapName, WeapDesc, WeapCost);
        Function.Call(Hash.SET_CURRENT_PED_WEAPON, Game.Player.Character, weaponHash, true);
        show_ammo_flag = 1;
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
                        current_weapon_hash = weaponHash;
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
                            CreateWeaponLivery(weapon, WeapLabel, weaponHash);

                            if ((WeaponHash)weaponHash == WeaponHash.StunGunMultiplayer)
                            {
                                string CompName = Game.GetLocalizedString("WCT_STNGN_BAIL");
                                uint componentHash = Function.Call<uint>(Hash.GET_HASH_KEY, "COMPONENT_STUNGUN_VARMOD_BAIL");
                                string componentCost = $"${weapon.WeaponData.ammoCost}";
                                int componentCost_int = 1000;
                                NativeItem comp_m = CreateComponentItem(weapon, CompName, componentCost, componentCost_int, componentHash, weaponHash, defaultClipSize, ammoCost);
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
                                    CreateWeaponLivery(weapon, WeapLabel, weaponHash);
                                }

                                NativeItem comp_m = CreateComponentItem(weapon, componentName, componentCost, component.componentCost, componentHash, weaponHash, defaultClipSize, ammoCost);

                                if (Function.Call<bool>(Hash.HAS_PED_GOT_WEAPON_COMPONENT, Game.Player.Character.Handle, weaponHash, componentHash))
                                {
                                    comp_m.AltTitle = "";
                                    comp_m.RightBadgeSet = shop_gun;
                                }
                                ComponentMenu.Add(comp_m);
                            }

                            List<uint> components_hashes = GetComponentsList(weapon);
                            List<int> components_cost = GetComponentsCost(weapon);
                            RefreshComponentMenu(ComponentMenu, components_hashes, components_cost);

                        }
                        ComponentMenu.Visible = true;
                    }
                }
                else
                {
                    Game.Player.Money -= weapon.WeaponData.weaponCost;
                    Game.Player.Character.Weapons.Give((WeaponHash)weaponHash, 1000, true, true);
                    SaveWeaponInInventory();
                }

                weap_m.AltTitle = "";
                weap_m.RightBadgeSet = shop_gun;
            }
        };
        return weap_m;
    }

    private NativeItem CreateComponentItem(DlcWeaponDataWithComponents weapon, string componentName, string componentCost, int cost_int, uint componentHash, uint weaponHash, int defaultClipSize, string ammoCost)
    {
        uint player = (uint)Game.Player.Character.Model.Hash;
        NativeItem comp_m = new NativeItem(componentName, "", componentCost);
        comp_m.Activated += (sender, args) =>
        {
            if (ValueContains(COMPONENTS_DICT, player, weaponHash, componentHash, 0))
            {
                if (Function.Call<bool>(Hash.HAS_PED_GOT_WEAPON_COMPONENT, Game.Player.Character, weaponHash, componentHash))
                {
                    Function.Call(Hash.REMOVE_WEAPON_COMPONENT_FROM_PED, Game.Player.Character.Handle, weaponHash, componentHash);
                    RemoveDictValue(INSTALLCOMP_DICT, player, weaponHash, componentHash, 0);
                    SaveWeaponInInventory();
                }
                else
                {
                    if (!ValueContains(INSTALLCOMP_DICT, player, weaponHash, componentHash, 0))
                    {
                        AddDictValue(INSTALLCOMP_DICT, player, weaponHash, componentHash, 0);
                    }

                    Function.Call(Hash.GIVE_WEAPON_COMPONENT_TO_PED, Game.Player.Character.Handle, weaponHash, componentHash);
                }
            }
            else
            {
                if (Game.Player.Money < cost_int)
                {
                    GTA.UI.Screen.ShowSubtitle(_NO_MONEY);
                }
                else
                {
                    Game.Player.Money -= cost_int;
                    AddDictValue(COMPONENTS_DICT, player, weaponHash, componentHash, 0);
                }
            }
            List<uint> components_hashes = GetComponentsList(weapon);
            List<int> components_cost = GetComponentsCost(weapon);
            RefreshComponentMenu(ComponentMenu, components_hashes, components_cost);
        };
        return comp_m;
    }

    private void RefreshMenus()
    {
        HeavyMenu.Clear();
        MeleeMenu.Clear();
        MachineGunsMenu.Clear();
        PistolsMenu.Clear();
        RiflesMenu.Clear();
        ShotgunsMenu.Clear();
        SMGsMenu.Clear();
        SniperRiflesMenu.Clear();
        StunGunMenu.Clear();
        ThrownMenu.Clear();

        SetMenuItems();

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

    private void SerializeDictionary<T1, T2, T3>(string filePath, Dictionary<T1, Dictionary<T2, List<T3>>> dictionary)
    {
        using (FileStream fs = new FileStream(filePath, FileMode.Create))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(fs, dictionary);
        }
    }

    private Dictionary<T1, Dictionary<T2, List<T3>>> DeserializeDictionary<T1, T2, T3>(string filePath)
    {
        using (FileStream fs = new FileStream(filePath, FileMode.Open))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            return (Dictionary<T1, Dictionary<T2, List<T3>>>)formatter.Deserialize(fs);
        }
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