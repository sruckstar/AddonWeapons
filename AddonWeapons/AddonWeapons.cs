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
using System.Globalization;
using System.Reflection;
using System.Linq;

public class AddonWeapons : Script
{
    private Dictionary<uint, List<DlcWeaponDataWithComponents>> weaponCategories = new Dictionary<uint, List<DlcWeaponDataWithComponents>>();
    private Dictionary<uint, string> groupNames = new Dictionary<uint, string>();
    private Dictionary<uint, Dictionary<uint, List<uint>>> purchased_components = new Dictionary<uint, Dictionary<uint, List<uint>>>();
    private Dictionary<uint, Dictionary<uint, List<int>>> purchased_tints = new Dictionary<uint, Dictionary<uint, List<int>>>();
    private Dictionary<uint, Dictionary<uint, List<uint>>> install_components = new Dictionary<uint, Dictionary<uint, List<uint>>>();
    private Dictionary<uint, Dictionary<uint, List<int>>> install_ammo = new Dictionary<uint, Dictionary<uint, List<int>>>();
    private Dictionary<uint, Dictionary<uint, List<int>>> install_tints = new Dictionary<uint, Dictionary<uint, List<int>>>();

    //local dict
    private Dictionary<uint, uint> current_weapon = new Dictionary<uint, uint>();

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
    NativeMenu RubberGuns;
    NativeMenu DigiScaners;
    NativeMenu FireExtinguishers;
    NativeMenu HackingDevices;
    NativeMenu MetalDetectors;
    NativeMenu NightVisions;
    NativeMenu Parachutes;
    NativeMenu PetrolCans;
    NativeMenu Tranquilizers;
    NativeMenu ComponentMenu;

    NativeMenu CurrentOpenedMenu;

    const int EMPTY_DICT = -1;
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
    const uint GROUP_RUBBERGUN = 88899580u;
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
    string _RUBBERGUN;
    string _DIGISCANNER;
    string _FIREEXTINGUISHER;
    string _HACKINGDEVICE;
    string _METALDETECTOR;
    string _NIGHTVISION;
    string _PARACHUTE;
    string _PETROLCAN;
    string _TRANQILIZER;

    Model model_box = new Model(2107849419);

    ScriptSettings config_settings;

    bool SP0_loaded = false;
    bool SP1_loaded = false;
    bool SP2_loaded = false;
    bool MP0_loaded = false;
    bool MP1_loaded = false;
    uint current_weapon_hash = 0;
    uint last_player = 0;
    int menuOpenedFlag = 0;

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

    List<int> price_tints = new List<int> { };

    private static List<Vector3> ammoBoxPositions = new List<Vector3>();
    private static List<float> ammoBoxHeading = new List<float>();
    private static List<Vector3> box_pos = new List<Vector3>()
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

    private static List<float> box_rot = new List<float>()
    {
       -18.99999f,
        0f,
        46.9999f,
        70.99976f,
        0f,
        44.99992f,
        0f,
        74.99975f,
        40.99995f,
        64.9998f,
        0f,
    };

    List<Prop> box_prop = new List<Prop>() { };

    private readonly Model[] mainCharacterModels = new Model[]
    {
        new Model(PedHash.Michael),
        new Model(PedHash.Franklin),
        new Model(PedHash.Trevor),
        new Model(PedHash.FreemodeMale01),
        new Model(PedHash.FreemodeFemale01)
    };

    List<NativeMenu> CustomMenusList = new List<NativeMenu>() { };

    List<uint> NoAmmoWeaponList = new List<uint>() { };
    List<uint> DisableComponentsList = new List<uint>() { };

    public AddonWeapons()
    {
        SetLanguage();
        InitializeCategories();
        InitializeMenu();
        GetDlcWeaponModels();
        SetMenuItems();
        BuildMenu();
        ammoBoxPositions = LoadAmmoBoxes($"Scripts\\AddonWeapons\\AmmoBoxes.txt", out ammoBoxHeading);
        Tick += OnTick;
        KeyUp += onkeyup;
        Aborted += OnAborted;
    }

    private string ExtractParameter(string command, string commandName)
    {
        int startIndex = command.IndexOf('(') + 1;
        int endIndex = command.IndexOf(')');
        if (startIndex > 0 && endIndex > startIndex)
        {
            return command.Substring(startIndex, endIndex - startIndex).Trim();
        }
        return null;
    }

    private string[] ExtractParameters(string command, string commandName, int expectedCount)
    {
        string parameterString = ExtractParameter(command, commandName);
        if (parameterString != null)
        {
            string[] parameters = parameterString.Split(',');
            if (parameters.Length == expectedCount)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    parameters[i] = parameters[i].Trim();
                }
                return parameters;
            }
        }
        return null;
    }


    private void RefreshPedInventory()
    {
        foreach (var Ped in World.GetNearbyPeds(Game.Player.Character.Position, 50f, mainCharacterModels))
        {
            if (Ped != null && Ped != Game.Player.Character)
            {
                LoadInventoryForPed(Ped);
            }
        }
    }

    private void LoadInventory(uint player)
    {
        if (Game.Player.Character.Model.Hash == new Model("mp_m_freemode_01").Hash || Game.Player.Character.Model.Hash == new Model("mp_f_freemode_01").Hash)
        {
            foreach (WeaponHash weapon in Game.Player.Character.Weapons.GetAllWeaponHashes())
            {
                if (Game.Player.Character.Weapons.HasWeapon(weapon) && weapon != WeaponHash.Unarmed)
                {
                    Game.Player.Character.Weapons.Remove(weapon);

                }
            }
        }

        purchased_components.Clear();
        purchased_tints.Clear();
        install_components.Clear();
        install_ammo.Clear();
        install_tints.Clear();

        if (!Directory.Exists($"Scripts\\AddonWeapons\\bin")) Directory.CreateDirectory($"Scripts\\AddonWeapons\\bin");
        if (File.Exists($"Scripts\\AddonWeapons\\bin\\components.bin")) purchased_components = DeserializeDictionary<uint, uint, uint>("Scripts\\AddonWeapons\\bin\\components.bin");
        if (File.Exists($"Scripts\\AddonWeapons\\bin\\tints.bin")) purchased_tints = DeserializeDictionary<uint, uint, int>("Scripts\\AddonWeapons\\bin\\tints.bin");
        if (File.Exists($"Scripts\\AddonWeapons\\bin\\install_components.bin")) install_components = DeserializeDictionary<uint, uint, uint>("Scripts\\AddonWeapons\\bin\\install_components.bin");
        if (File.Exists($"Scripts\\AddonWeapons\\bin\\install_ammo.bin")) install_ammo = DeserializeDictionary<uint, uint, int>("Scripts\\AddonWeapons\\bin\\install_ammo.bin");
        if (File.Exists($"Scripts\\AddonWeapons\\bin\\install_tints.bin")) install_tints = DeserializeDictionary<uint, uint, int>("Scripts\\AddonWeapons\\bin\\install_tints.bin");

        List<uint> players = new List<uint>()
    {
        (uint)Game.GenerateHash("player_zero"),
        (uint)Game.GenerateHash("player_one"),
        (uint)Game.GenerateHash("player_two"),
        (uint)Game.GenerateHash("mp_m_freemode_01"),
        (uint)Game.GenerateHash("mp_f_freemode_01"),
    };

        foreach (uint pl in players)
        {
            if (!purchased_components.ContainsKey(pl)) purchased_components[pl] = new Dictionary<uint, List<uint>>();
            if (!purchased_tints.ContainsKey(pl)) purchased_tints[pl] = new Dictionary<uint, List<int>>();
            if (!install_components.ContainsKey(pl)) install_components[pl] = new Dictionary<uint, List<uint>>();
            if (!install_ammo.ContainsKey(pl)) install_ammo[pl] = new Dictionary<uint, List<int>>();
            if (!install_tints.ContainsKey(pl)) install_tints[pl] = new Dictionary<uint, List<int>>();
        }

        List<uint> weaponsHashes = new List<uint>(purchased_components[player].Keys);
        if (weaponsHashes.Count == 0) return;

        foreach (var weaponHash in weaponsHashes)
        {
            if (!purchased_components[player].ContainsKey(weaponHash)) purchased_components[player][weaponHash] = new List<uint> { };

            if (!purchased_tints[player].ContainsKey(weaponHash)) purchased_tints[player][weaponHash] = new List<int> { };
            if (!install_components[player].ContainsKey(weaponHash)) install_components[player][weaponHash] = new List<uint> { };
            if (!install_ammo[player].ContainsKey(weaponHash)) install_ammo[player][weaponHash] = new List<int> { };
            if (!install_tints[player].ContainsKey(weaponHash)) install_tints[player][weaponHash] = new List<int> { };

            if (!Game.Player.Character.Weapons.HasWeapon((WeaponHash)weaponHash)) Game.Player.Character.Weapons.Give((WeaponHash)weaponHash, 0, true, true);

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

                    foreach (var componentHash in purchased_components[player][weaponHash])
                    {
                        Function.Call(Hash.SET_PED_WEAPON_COMPONENT_TINT_INDEX, Game.Player.Character.Handle, weaponHash, componentHash, tint);
                        int test = Function.Call<int>(Hash.GET_PED_WEAPON_COMPONENT_TINT_INDEX, Game.Player.Character.Handle, weaponHash, componentHash);
                    }
                }
            }

            if (install_ammo[player][weaponHash].Count > 0) Function.Call(Hash.ADD_AMMO_TO_PED, Game.Player.Character, weaponHash, install_ammo[player][weaponHash][0]);

            if (current_weapon.ContainsKey(player))
            {
                while (Function.Call<int>(Hash.GET_PLAYER_SWITCH_STATE) < 10) Script.Wait(0);
                Function.Call(Hash.SET_CURRENT_PED_WEAPON, Game.Player.Character, current_weapon[player], false);
            }

            foreach (var Ped in World.GetAllPeds())
            {
                if (Ped.Model.Hash == new Model("player_zero").Hash || Ped.Model.Hash == new Model("player_one").Hash || Ped.Model.Hash == new Model("player_two").Hash || Ped.Model.Hash == new Model("mp_m_freemode_01").Hash || Ped.Model.Hash == new Model("mp_f_freemode_01").Hash)
                {
                    if (Ped != Game.Player.Character) LoadInventoryForPed(Ped);
                }
            }
        }
    }

    private void LoadInventoryForPed(Ped npc)
    {
        uint player = (uint)npc.Model.Hash;      

        List<uint> weaponsHashes = new List<uint>(purchased_components[player].Keys);
        if (weaponsHashes.Count == 0) return;

        foreach (var weaponHash in weaponsHashes)
        {

            if (!npc.Weapons.HasWeapon((WeaponHash)weaponHash))
            {
                npc.Weapons.Give((WeaponHash)weaponHash, 0, true, true);
            }

            if (install_components[player].ContainsKey(weaponHash))
            {
                foreach (var componentHash in install_components[player][weaponHash])
                {
                    if (ValueContains(INSTALLCOMP_DICT, player, weaponHash, componentHash, 0))
                    {
                        if (!Function.Call<bool>(Hash.HAS_PED_GOT_WEAPON_COMPONENT, npc, weaponHash, componentHash))
                        {
                            Function.Call(Hash.GIVE_WEAPON_COMPONENT_TO_PED, npc.Handle, weaponHash, componentHash);
                        }
                    }
                }
            }

            if (purchased_tints[player].ContainsKey(weaponHash))
            {
                foreach (var tint in purchased_tints[player][weaponHash])
                {
                    if (ValueContains(INSTALLTINT_DICT, player, weaponHash, 0, tint))
                    {
                        Function.Call(Hash.SET_PED_WEAPON_TINT_INDEX, npc, weaponHash, tint);
                    }
                }
            }

            Function.Call(Hash.ADD_AMMO_TO_PED, npc, weaponHash, install_ammo[player][weaponHash][0]);

            if (current_weapon.ContainsKey(player))
            {
                Function.Call(Hash.SET_CURRENT_PED_WEAPON, npc, current_weapon[player], true);

            }
        }
    }

    private void WaitLoadedInventory()
    {
        if (last_player == 0)
        {
            LoadInventory((uint)Game.Player.Character.Model.Hash);
            RefreshPedInventory();
            SetCurrentLoadInventoryBool();
            last_player = (uint)Game.Player.Character.Model.Hash;
        }

        if ((uint)Game.Player.Character.Model.Hash != last_player)
        {
            SaveWeaponInInventory(last_player);
            RefreshPedInventory();
            LoadInventory((uint)Game.Player.Character.Model.Hash);
            SetCurrentLoadInventoryBool();
            last_player = (uint)Game.Player.Character.Model.Hash;
        }
    }

    private void SetCurrentLoadInventoryBool()
    {
        if (!SP0_loaded && Game.Player.Character.Model.Hash == new Model("player_zero").Hash)
        {
            SP0_loaded = true;
            SP1_loaded = false;
            SP2_loaded = false;
            MP0_loaded = false;
            MP1_loaded = false;
        }

        if (!SP1_loaded && Game.Player.Character.Model.Hash == new Model("player_one").Hash)
        {
            SP0_loaded = false;
            SP1_loaded = true;
            SP2_loaded = false;
            MP0_loaded = false;
            MP1_loaded = false;
        }

        if (!SP2_loaded && Game.Player.Character.Model.Hash == new Model("player_two").Hash)
        {
            SP0_loaded = false;
            SP1_loaded = false;
            SP2_loaded = true;
            MP0_loaded = false;
            MP1_loaded = false;
        }

        if (!MP0_loaded && Game.Player.Character.Model.Hash == new Model("mp_m_freemode_01").Hash)
        {
            SP0_loaded = false;
            SP1_loaded = false;
            SP2_loaded = false;
            MP0_loaded = true;
            MP1_loaded = false;
        }

        if (!MP1_loaded && Game.Player.Character.Model.Hash == new Model("mp_f_freemode_01").Hash)
        {
            SP0_loaded = false;
            SP1_loaded = false;
            SP2_loaded = false;
            MP0_loaded = false;
            MP1_loaded = true;
        }
    }

    private void SetCurrentWeapon()
    {
        uint player = (uint)Game.Player.Character.Model.Hash;

        uint weaponHash = 0;
        unsafe
        {
            Function.Call<bool>(Hash.GET_CURRENT_PED_WEAPON, Game.Player.Character, &weaponHash, true);
        }
        current_weapon[player] = weaponHash;
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

        _RUBBERGUN = "Less Lethal";
        _DIGISCANNER = "Digiscanners";
        _FIREEXTINGUISHER = "Fire Extinguishers";
        _HACKINGDEVICE = "Hacking Devices";
        _METALDETECTOR = "Metal Detectors";
        _NIGHTVISION = "Night Visions";
        _PARACHUTE = "Parachutes";
        _PETROLCAN = "Petrol Cans";
        _TRANQILIZER = "Tranquilizers";

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
        weaponCategories.Add(88899580u, new List<DlcWeaponDataWithComponents>()); // GROUP_RUBBERGUN
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
        RubberGuns = new NativeMenu("", _RUBBERGUN, " ", new ScaledTexture(PointF.Empty, new SizeF(0, 108), "shopui_title_gunclub", "shopui_title_gunclub"));
        ThrownMenu = new NativeMenu("", _TITLE_THROWN, " ", new ScaledTexture(PointF.Empty, new SizeF(0, 108), "shopui_title_gunclub", "shopui_title_gunclub"));
        DigiScaners = new NativeMenu("", _DIGISCANNER, " ", new ScaledTexture(PointF.Empty, new SizeF(0, 108), "shopui_title_gunclub", "shopui_title_gunclub"));
        FireExtinguishers = new NativeMenu("", _FIREEXTINGUISHER, " ", new ScaledTexture(PointF.Empty, new SizeF(0, 108), "shopui_title_gunclub", "shopui_title_gunclub"));
        HackingDevices = new NativeMenu("", _HACKINGDEVICE, " ", new ScaledTexture(PointF.Empty, new SizeF(0, 108), "shopui_title_gunclub", "shopui_title_gunclub"));
        MetalDetectors = new NativeMenu("", _METALDETECTOR, " ", new ScaledTexture(PointF.Empty, new SizeF(0, 108), "shopui_title_gunclub", "shopui_title_gunclub"));
        NightVisions = new NativeMenu("", _NIGHTVISION, " ", new ScaledTexture(PointF.Empty, new SizeF(0, 108), "shopui_title_gunclub", "shopui_title_gunclub"));
        Parachutes = new NativeMenu("", _PARACHUTE, " ", new ScaledTexture(PointF.Empty, new SizeF(0, 108), "shopui_title_gunclub", "shopui_title_gunclub"));
        PetrolCans = new NativeMenu("", _PETROLCAN, " ", new ScaledTexture(PointF.Empty, new SizeF(0, 108), "shopui_title_gunclub", "shopui_title_gunclub"));
        Tranquilizers = new NativeMenu("", _TRANQILIZER, " ", new ScaledTexture(PointF.Empty, new SizeF(0, 108), "shopui_title_gunclub", "shopui_title_gunclub"));

        ComponentMenu = new NativeMenu("", "", " ", new ScaledTexture(PointF.Empty, new SizeF(0, 108), "shopui_title_gunclub", "shopui_title_gunclub"));

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
        pool.Add(RubberGuns);
        pool.Add(ThrownMenu);
        pool.Add(ComponentMenu);
        pool.Add(DigiScaners);
        pool.Add(FireExtinguishers);
        pool.Add(HackingDevices);
        pool.Add(MetalDetectors);
        pool.Add(NightVisions);
        pool.Add(Parachutes);
        pool.Add(PetrolCans);
        pool.Add(Tranquilizers);

        string[] commands = File.ReadAllLines($"Scripts\\AddonWeapons\\commandline.txt");
        foreach (string command in commands)
        {
            string trimmedCommand = command.Trim();

            if (string.IsNullOrEmpty(trimmedCommand) || trimmedCommand.StartsWith("//"))
                continue; // Skip empty lines or comments

            if (trimmedCommand.StartsWith("CreateWeaponCategory"))
            {
                string itemName = ExtractParameter(trimmedCommand, "CreateWeaponCategory");
                NativeMenu custom = new NativeMenu("", itemName, " ", new ScaledTexture(PointF.Empty, new SizeF(0, 108), "shopui_title_gunclub", "shopui_title_gunclub"));
                CustomMenusList.Add(custom);
                menu.AddSubMenu(custom);
                pool.Add(custom);
            }

        }
    }

    public static List<Vector3> LoadAmmoBoxes(string filePath, out List<float> ammoBoxHeading)
    {
        List<Vector3> ammoBoxPositions = new List<Vector3>();
        ammoBoxHeading = new List<float>();

        if (!File.Exists($"Scripts\\AddonWeapons\\AmmoBoxes.txt"))
        {
            ammoBoxPositions = box_pos;
            ammoBoxHeading = box_rot;
            return ammoBoxPositions;
        }

        string[] lines = File.ReadAllLines($"Scripts\\AddonWeapons\\AmmoBoxes.txt");
        foreach (string line in lines)
        {
            (Vector3? position, float? heading) = ParseVector3AndHeading(line);
            if (position.HasValue && heading.HasValue)
            {
                ammoBoxPositions.Add(position.Value);
                ammoBoxHeading.Add(heading.Value);
            }
        }
        return ammoBoxPositions;
    }

    private static (Vector3?, float?) ParseVector3AndHeading(string input)
    {
        try
        {
            string[] parts = input.Split(',');
            if (parts.Length != 4) return (null, null);

            float x = float.Parse(parts[0].Trim(), CultureInfo.InvariantCulture);
            float y = float.Parse(parts[1].Trim(), CultureInfo.InvariantCulture);
            float z = float.Parse(parts[2].Trim(), CultureInfo.InvariantCulture);
            float heading = float.Parse(parts[3].Trim(), CultureInfo.InvariantCulture);

            return (new Vector3(x, y, z), heading);
        }
        catch
        {
            return (null, null);
        }
    }

    private void CreateAmmoBoxesThisFrame()
    {
        if (box_prop == null)
        {
            box_prop = new List<Prop>();
        }

        while (box_prop.Count < ammoBoxPositions.Count)
        {
            box_prop.Add(null);
        }

        for (int i = 0; i < ammoBoxPositions.Count; i++)
        {
            float distance = Game.Player.Character.Position.DistanceTo(ammoBoxPositions[i]);

            // Создаем ящик, если его нет и игрок в радиусе 10 метров
            if (distance < 10f && (box_prop[i] == null || !box_prop[i].Exists()))
            {
                box_prop[i] = World.CreateProp(model_box, ammoBoxPositions[i], new Vector3(0, 0, ammoBoxHeading[i]), false, false);
                if (box_prop[i] != null && box_prop[i].Exists())
                {
                    Function.Call(Hash.PLACE_OBJECT_ON_GROUND_PROPERLY, box_prop[i]);
                    Function.Call(Hash.FREEZE_ENTITY_POSITION, box_prop[i], true);
                }
            }

            // Удаляем ящик, если игрок слишком далеко (> 15 м)
            if (distance > 15f && box_prop[i] != null && box_prop[i].Exists())
            {
                box_prop[i].Delete();
                box_prop[i] = null;
            }

            // Взаимодействие с ящиком
            if (box_prop[i] != null && box_prop[i].Exists())
            {
                float playerDistance = Game.Player.Character.Position.DistanceTo(box_prop[i].Position);
                if (playerDistance < 1.5f)
                {
                    if (!IsMenuOpen())
                    {
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
            case EMPTY_DICT:
                if (!purchased_components[player].ContainsKey(weaponHash))
                {
                    purchased_components[player].Add(weaponHash, new List<uint> { });
                }
                break;

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
                purchased_components[player][weaponHash].RemoveAll(e => (e == componentHash));
                break;

            case INSTALLCOMP_DICT:
                if (!install_components[player].ContainsKey(weaponHash))
                {
                    install_components[player].Add(weaponHash, new List<uint> { });
                }
                install_components[player][weaponHash].RemoveAll(e => (e == componentHash));
                break;

            case TINTS_DICT:
                if (!purchased_tints[player].ContainsKey(weaponHash))
                {
                    purchased_tints[player].Add(weaponHash, new List<int> { });
                }
                purchased_tints[player][weaponHash].RemoveAll(e => (e == tint));
                break;

            case AMMO_DICT:
                if (!install_ammo[player].ContainsKey(weaponHash))
                {
                    install_ammo[player].Add(weaponHash, new List<int> { });
                }
                install_ammo[player][weaponHash].RemoveAll(e => (e == tint));
                break;

            case INSTALLTINT_DICT:
                if (!install_tints[player].ContainsKey(weaponHash))
                {
                    install_tints[player].Add(weaponHash, new List<int> { });
                }
                install_tints[player][weaponHash].RemoveAll(e => (e == tint));
                break;
        }
    }

    private bool ValueContains(int type_dict, uint player, uint weaponHash, uint componentHash, int tint)
    {
        bool result = false;

        switch (type_dict)
        {
            case COMPONENTS_DICT:

                if (!purchased_components.ContainsKey(player)) purchased_components[player] = new Dictionary<uint, List<uint>>();
                if (!purchased_components[player].ContainsKey(weaponHash)) purchased_components[player].Add(weaponHash, new List<uint> { });
                if (purchased_components[player][weaponHash].Contains(componentHash)) result = true;
                break;

            case INSTALLCOMP_DICT:
                if (!install_components.ContainsKey(player)) install_components[player] = new Dictionary<uint, List<uint>>();
                if (!install_components[player].ContainsKey(weaponHash)) install_components[player].Add(weaponHash, new List<uint> { });
                if (install_components[player][weaponHash].Contains(componentHash)) result = true;
                break;

            case TINTS_DICT:
                if (!purchased_tints.ContainsKey(player)) purchased_tints[player] = new Dictionary<uint, List<int>>();
                if (!purchased_tints[player].ContainsKey(weaponHash)) purchased_tints[player].Add(weaponHash, new List<int> { });
                if (purchased_tints[player][weaponHash].Contains(tint)) result = true;
                break;

            case AMMO_DICT:
                if (!install_ammo.ContainsKey(player)) install_ammo[player] = new Dictionary<uint, List<int>>();
                if (!install_ammo[player].ContainsKey(weaponHash)) install_ammo[player].Add(weaponHash, new List<int> { });
                if (install_ammo[player][weaponHash].Contains(tint)) result = true;
                break;

            case INSTALLTINT_DICT:
                if (!install_tints.ContainsKey(player)) install_tints[player] = new Dictionary<uint, List<int>>();
                if (!install_tints[player].ContainsKey(weaponHash)) install_tints[player].Add(weaponHash, new List<int> { });
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
        SetCurrentWeapon();
        ReOpenLastMenu();

    }

    private bool IsMenuOpen()
    {
        if (pool.AreAnyVisible && menuOpenedFlag == 1)
        {
            return true;
        }
        return false;
    }

    private void CloseAllMenus()
    {
        if (menu.Visible)
        {
            menu.Visible = false;
            CurrentOpenedMenu = menu;
        }

        if (HeavyMenu.Visible)
        {

            HeavyMenu.Visible = false;
            CurrentOpenedMenu = HeavyMenu;
        }

        if (MeleeMenu.Visible)
        {
            MeleeMenu.Visible = false;
            CurrentOpenedMenu = MeleeMenu;
        }

        if (MachineGunsMenu.Visible)
        {
            MachineGunsMenu.Visible = false;
            CurrentOpenedMenu = MachineGunsMenu;
        }

        if (PistolsMenu.Visible)
        {
            PistolsMenu.Visible = false;
            CurrentOpenedMenu = PistolsMenu;
        }

        if (RiflesMenu.Visible)
        {
            RiflesMenu.Visible = false;
            CurrentOpenedMenu = RiflesMenu;
        }

        if (ShotgunsMenu.Visible)
        {
            ShotgunsMenu.Visible = false;
            CurrentOpenedMenu = ShotgunsMenu;
        }

        if (SMGsMenu.Visible)
        {
            SMGsMenu.Visible = false;
            CurrentOpenedMenu = SMGsMenu;
        }

        if (SniperRiflesMenu.Visible)
        {
            SniperRiflesMenu.Visible = false;
            CurrentOpenedMenu = SniperRiflesMenu;
        }

        if (StunGunMenu.Visible)
        {
            StunGunMenu.Visible = false;
            CurrentOpenedMenu = StunGunMenu;
        }

        if (ThrownMenu.Visible)
        {
            ThrownMenu.Visible = false;
            CurrentOpenedMenu = ThrownMenu;
        }

        if (RubberGuns.Visible)
        {
            RubberGuns.Visible = false;
            CurrentOpenedMenu = RubberGuns;
        }

        if (DigiScaners.Visible)
        {
            DigiScaners.Visible = false;
            CurrentOpenedMenu = DigiScaners;
        }

        if (FireExtinguishers.Visible)
        {
            FireExtinguishers.Visible = false;
            CurrentOpenedMenu = FireExtinguishers;
        }

        if (HackingDevices.Visible)
        {
            HackingDevices.Visible = false;
            CurrentOpenedMenu = HackingDevices;
        }

        if (MetalDetectors.Visible)
        {
            MetalDetectors.Visible = false;
            CurrentOpenedMenu = MetalDetectors;
        }

        if (NightVisions.Visible)
        {
            NightVisions.Visible = false;
            CurrentOpenedMenu = NightVisions;
        }

        if (Parachutes.Visible)
        {
            Parachutes.Visible = false;
            CurrentOpenedMenu = Parachutes;
        }

        if (PetrolCans.Visible)
        {
            PetrolCans.Visible = false;
            CurrentOpenedMenu = PetrolCans;
        }

        if (Tranquilizers.Visible)
        {
            Tranquilizers.Visible = false;
            CurrentOpenedMenu = Tranquilizers;
        }

        if (ComponentMenu.Visible)
        {
            ComponentMenu.Visible = false;
            CurrentOpenedMenu = ComponentMenu;
        }

        foreach (NativeMenu custom in CustomMenusList)
        {
            if (custom.Visible)
            {
                custom.Visible = false;
                CurrentOpenedMenu = custom;
            }
        }
    }

    private void ReOpenLastMenu()
    {
        if (CurrentOpenedMenu != null && !ComponentMenu.Visible)
        {
            CurrentOpenedMenu.Visible = true;
            CurrentOpenedMenu = null;
        }
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

    void SaveWeaponInInventory(uint player)
    {
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

    NativeItem ActivateLivery(DlcWeaponDataWithComponents weapon, NativeItem item, int livery_id, uint weaponHash, int tintPrice)
    {
        uint player = (uint)Game.Player.Character.Model.Hash;
        item.Activated += (sender, args) =>
        {
            if (Game.Player.Money < tintPrice && Game.Player.Character.Model.Hash != new Model("mp_m_freemode_01").Hash && Game.Player.Character.Model.Hash != new Model("mp_f_freemode_01").Hash)
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
                    Game.Player.Money -= tintPrice;
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
        int current_weap_tint = Function.Call<int>(Hash.GET_PED_WEAPON_TINT_INDEX, Game.Player.Character, weaponHash);
        string filePath = $"Scripts\\AddonWeapons\\tints\\{WeapLabel}.txt";
        int tint_count = Function.Call<int>(Hash.GET_WEAPON_TINT_COUNT, weaponHash);
        string tint_name = null;

        if (File.Exists(filePath))
        {
            price_tints.Clear();
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                tint_count = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    tints.Add(line);
                    price_tints.Add(1000);
                    tint_count++;
                }
            }
        }
        else
        {
            if (tint_count == 8)
            {
                tint_name = "WM_TINT";
                price_tints = price_standard;
            }
            else
            {
                tint_count = 32;
                tint_name = "WCT_TINT_";
                price_tints = price_mk2;
            }

            for (int i = 0; i < tint_count; i++)
            {
                if (tint_name == null) tint_name = tints[i];
                string LiveryName = Game.GetLocalizedString($"{tint_name}{i}");
                if (LiveryName.Length < 2) LiveryName = "Livery " + i;
                tints.Add(LiveryName);
            }
        }

        for (int i = 0; i < tints.Count; i++)
        {
            NativeItem tint_m = new NativeItem(tints[i], "", $"${price_tints[i]}");
            tint_m = ActivateLivery(weapon, tint_m, i, weaponHash, price_tints[i]);
            ComponentMenu.Add(tint_m);
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
            if (Game.Player.Money < cost && Game.Player.Character.Model.Hash != new Model("mp_m_freemode_01").Hash && Game.Player.Character.Model.Hash != new Model("mp_f_freemode_01").Hash)
            {
                GTA.UI.Screen.ShowSubtitle(_NO_MONEY);
            }
            else
            {
                if (!IsmaxAmmo(weaponHash))
                {
                    Game.Player.Money -= cost;
                    Function.Call(Hash.ADD_PED_AMMO_BY_TYPE, Game.Player.Character, Function.Call<Hash>(Hash.GET_PED_AMMO_TYPE_FROM_WEAPON, Game.Player.Character, weaponHash), defaultClipSize);
                    uint player = (uint)Game.Player.Character.Model.Hash;
                    int current_ammo = Function.Call<int>(Hash.GET_AMMO_IN_PED_WEAPON, Game.Player.Character, weaponHash);
                    AddDictValue(AMMO_DICT, player, weaponHash, 0, current_ammo);
                    SaveWeaponInInventory(player);

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
        int max_tint;
        string filePath = $"Scripts\\AddonWeapons\\tints\\{weapon.WeaponData.GetNameLabel()}.txt";

        if (File.Exists(filePath))
        {
            max_tint = File.ReadLines(filePath).Count();
        }
        else
        {
            max_tint = Function.Call<int>(Hash.GET_WEAPON_TINT_COUNT, weapon.WeaponData.weaponHash);
            if (max_tint == 33) max_tint--;
        }

        for (int i = 0; i < max_tint; i++)
        {
            components_hashes.Add(0);
        }

        if ((WeaponHash)current_weapon_hash == WeaponHash.StunGunMultiplayer)
        {
            uint componentHash = Function.Call<uint>(Hash.GET_HASH_KEY, "COMPONENT_STUNGUN_VARMOD_BAIL");
            components_hashes.Add(componentHash);
        }
        else
        {
            foreach (var component in weapon.Components)
            {
                components_hashes.Add(component.componentHash);
            }
        }

        return components_hashes;
    }

    private List<int> GetComponentsCost(DlcWeaponDataWithComponents weapon)
    {
        List<int> components_cost = new List<int>();

        int max_tint = Function.Call<int>(Hash.GET_WEAPON_TINT_COUNT, weapon.WeaponData.weaponHash);
        if (max_tint == 33) max_tint--;

        for (int i = 0; i < Function.Call<int>(Hash.GET_WEAPON_TINT_COUNT, weapon.WeaponData.weaponHash); i++)
        {
            components_cost.Add(1000);
        }

        if ((WeaponHash)current_weapon_hash == WeaponHash.StunGunMultiplayer)
        {
            components_cost.Add(1000);
        }
        else if ((WeaponHash)current_weapon_hash == WeaponHash.Bat)
        {
            for (int i = 0; i < 11; i++)
            {
                components_cost.Add(1000);
            }
        }
        else if ((WeaponHash)current_weapon_hash == WeaponHash.Knife)
        {
            for (int i = 0; i < 11; i++)
            {
                components_cost.Add(1000);
            }
        }
        else
        {
            foreach (var component in weapon.Components)
            {
                components_cost.Add(component.componentCost);
            }
        }

        return components_cost;
    }

    private void RefreshComponentMenu(NativeMenu ComponentMenu, List<uint> components_hashes, List<int> components_cost)
    {
        uint player = (uint)Game.Player.Character.Model.Hash;
        BadgeSet shop_gun = CreateBafgeFromItem("commonmenu", "shop_gunclub_icon_a", "commonmenu", "shop_gunclub_icon_b");
        BadgeSet shop_tick = CreateBafgeFromItem("commonmenu", "shop_tick_icon", "commonmenu", "shop_tick_icon");
        List<NativeItem> items = ComponentMenu.Items;

        int i = 0;
        bool IsRounds = true;
        foreach (var item in items)
        {
            if (IsRounds) //Skip the first item (rounds)
            {
                IsRounds = false;
                continue;
            }

            if (price_tints.Count > i)
            {
                if (Function.Call<int>(Hash.GET_PED_WEAPON_TINT_INDEX, Game.Player.Character, current_weapon_hash) == i)
                {
                    if (!ValueContains(TINTS_DICT, player, current_weapon_hash, 0, i))
                    {
                        AddDictValue(TINTS_DICT, player, current_weapon_hash, 0, i);
                    }
                    item.AltTitle = "";
                    item.RightBadgeSet = shop_gun;
                }
                else
                {
                    if (ValueContains(TINTS_DICT, player, current_weapon_hash, 0, i))
                    {
                        item.AltTitle = "";
                        item.RightBadgeSet = shop_tick;
                    }
                    else
                    {
                        item.AltTitle = $"${price_tints[i]}";
                        item.RightBadgeSet = null;
                    }
                }
            }
            else if (i >= price_tints.Count)
            {
                int index = i;
                if (Function.Call<bool>(Hash.HAS_PED_GOT_WEAPON_COMPONENT, Game.Player.Character, current_weapon_hash, components_hashes[index]))
                {
                    if (ValueContains(COMPONENTS_DICT, player, current_weapon_hash, components_hashes[index], 0))
                    {
                        AddDictValue(COMPONENTS_DICT, player, current_weapon_hash, components_hashes[index], 0);
                    }

                    item.AltTitle = "";
                    item.RightBadgeSet = shop_gun;
                }
                else
                {
                    if (ValueContains(COMPONENTS_DICT, player, current_weapon_hash, components_hashes[index], 0))
                    {
                        item.AltTitle = "";
                        item.RightBadgeSet = shop_tick;
                    }
                    else
                    {
                        item.AltTitle = $"${components_cost[index]}";
                        item.RightBadgeSet = null;
                    }
                }
            }

            i++;
        }
    }

    private void RefreshComponentMenuNEW(NativeMenu ComponentMenu, List<uint> components_hashes, List<int> components_cost)
    {
        uint player = (uint)Game.Player.Character.Model.Hash;
        BadgeSet shop_gun = CreateBafgeFromItem("commonmenu", "shop_gunclub_icon_a", "commonmenu", "shop_gunclub_icon_b");
        BadgeSet shop_tick = CreateBafgeFromItem("commonmenu", "shop_tick_icon", "commonmenu", "shop_tick_icon");
        List<NativeItem> items = ComponentMenu.Items;

        int i = 0;
        bool IsRounds = true;
        foreach (var item in items)
        {
            if (IsRounds) //Skip the first item (rounds)
            {
                IsRounds = false;
                continue;
            }

            if (price_tints.Count > i)
            {
                if (Function.Call<int>(Hash.GET_PED_WEAPON_TINT_INDEX, Game.Player.Character, current_weapon_hash) == i)
                {
                    if (!ValueContains(TINTS_DICT, player, current_weapon_hash, 0, i))
                    {
                        AddDictValue(TINTS_DICT, player, current_weapon_hash, 0, i);
                    }
                    item.AltTitle = "";
                    item.RightBadgeSet = shop_gun;
                }
                else
                {
                    if (ValueContains(TINTS_DICT, player, current_weapon_hash, 0, i))
                    {
                        item.AltTitle = "";
                        item.RightBadgeSet = shop_tick;
                    }
                    else
                    {
                        item.AltTitle = $"${price_tints[i]}";
                        item.RightBadgeSet = null;
                    }
                }
            }
            else 
            {
                if (Function.Call<bool>(Hash.HAS_PED_GOT_WEAPON_COMPONENT, Game.Player.Character, current_weapon_hash, components_hashes[i]))
                {
                    if (ValueContains(COMPONENTS_DICT, player, current_weapon_hash, components_hashes[i], 0))
                    {
                        AddDictValue(COMPONENTS_DICT, player, current_weapon_hash, components_hashes[i], 0);
                    }

                    item.AltTitle = "";
                    item.RightBadgeSet = shop_gun;
                }
                else
                {
                    if (ValueContains(COMPONENTS_DICT, player, current_weapon_hash, components_hashes[i], 0))
                    {
                        item.AltTitle = "";
                        item.RightBadgeSet = shop_tick;
                    }
                    else
                    {
                        item.AltTitle = $"${components_cost[i]}";
                        item.RightBadgeSet = null;
                    }
                }
            }

            i++;
        }
    }

    private void RefreshComponentMenuOld(NativeMenu ComponentMenu, List<uint> components_hashes, List<int> components_cost)
    {
        
        uint player = (uint)Game.Player.Character.Model.Hash;
        BadgeSet shop_gun = CreateBafgeFromItem("commonmenu", "shop_gunclub_icon_a", "commonmenu", "shop_gunclub_icon_b");
        BadgeSet shop_tick = CreateBafgeFromItem("commonmenu", "shop_tick_icon", "commonmenu", "shop_tick_icon");
        List<NativeItem> items = ComponentMenu.Items;
        int index = 0;
        bool IsRounds = true;

        foreach (var item in items)
        {
            if (IsRounds) //Skip the first item (rounds)
            {
                IsRounds = false;
                continue;
            }

            int max_index = Function.Call<int>(Hash.GET_WEAPON_TINT_COUNT, current_weapon_hash);
            if (max_index == 33) max_index--;

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
                }
                else
                {
                    if (ValueContains(TINTS_DICT, player, current_weapon_hash, 0, index))
                    {
                        item.AltTitle = "";
                        item.RightBadgeSet = shop_tick;
                    }
                    else
                    {
                        item.AltTitle = $"${price_tints[index]}";
                        item.RightBadgeSet = null;
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
                }
                else
                {
                    if (ValueContains(COMPONENTS_DICT, player, current_weapon_hash, components_hashes[index], 0))
                    {
                        item.AltTitle = "";
                        item.RightBadgeSet = shop_tick;
                    }
                    else
                    {
                        item.AltTitle = $"${components_cost[index]}";
                        item.RightBadgeSet = null;
                    }
                }
            }
            index++;
        }

        SaveWeaponInInventory(player);
    }

    private void ComponentMenu_ItemClicked(object sender, ItemActivatedArgs e)
    {
        throw new NotImplementedException();
    }

    bool HashComponentsAvailable(uint weaponHash)
    {
        if (Function.Call<uint>(Hash.GET_WEAPONTYPE_GROUP, weaponHash) == GROUP_MELEE ||
            Function.Call<uint>(Hash.GET_WEAPONTYPE_GROUP, weaponHash) == GROUP_THROWN ||
            DisableComponentsList.Contains(weaponHash))
        {
            return false;
        }

        return true;
    }

    private NativeItem CreateWeaponItem(DlcWeaponDataWithComponents weapon, string WeapName, string WeapDesc, int WeapCost, uint weaponHash)
    {
        uint player = (uint)Game.Player.Character.Model.Hash;
        NativeItem weap_m = new NativeItem(WeapName, WeapDesc, $"${WeapCost}");
        Function.Call(Hash.SET_CURRENT_PED_WEAPON, Game.Player.Character, weaponHash, true);
        BadgeSet shop_gun = CreateBafgeFromItem("commonmenu", "shop_gunclub_icon_a", "commonmenu", "shop_gunclub_icon_b");

        weap_m.Activated += (sender, args) =>
        {
            ComponentMenu.Name = WeapName;
            if (Game.Player.Money < WeapCost && Game.Player.Character.Model.Hash != new Model("mp_m_freemode_01").Hash && Game.Player.Character.Model.Hash != new Model("mp_f_freemode_01").Hash)
            {
                GTA.UI.Screen.ShowSubtitle(_NO_MONEY);
            }
            else
            {
                if (Game.Player.Character.Weapons.HasWeapon((WeaponHash)weaponHash))
                {
                    if (HashComponentsAvailable(weaponHash))
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
                            if (!NoAmmoWeaponList.Contains(weaponHash))
                            {
                                rounds = 1;
                                NativeItem rounds_m = CreateAmmoItem(defaultClipSize, ammoCost, weapon.WeaponData.ammoCost, weaponHash);
                                ComponentMenu.Add(rounds_m);
                            }
                            
                            CreateWeaponLivery(weapon, WeapLabel, weaponHash);

                            if ((WeaponHash)weaponHash == WeaponHash.StunGunMultiplayer)
                            {
                                string CompName = Game.GetLocalizedString("WCT_STNGN_BAIL");
                                uint componentHash = Function.Call<uint>(Hash.GET_HASH_KEY, "COMPONENT_STUNGUN_VARMOD_BAIL");
                                string componentCost = "$1000";
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
                    else
                    {
                        Game.Player.Money -= WeapCost;
                        Function.Call(Hash.ADD_AMMO_TO_PED, Game.Player.Character, weaponHash, 1);
                    }
                }
                else
                {
                    Game.Player.Money -= WeapCost;

                    if (Function.Call<uint>(Hash.GET_WEAPONTYPE_GROUP, weaponHash) == GROUP_MELEE ||
                        Function.Call<uint>(Hash.GET_WEAPONTYPE_GROUP, weaponHash) == GROUP_THROWN)
                    {
                        Game.Player.Character.Weapons.Give((WeaponHash)weaponHash, 1, true, true);
                    }
                    else
                    {
                        weap_m.AltTitle = ""; //Create badge after weapon buy
                        weap_m.RightBadgeSet = shop_gun;
                        Game.Player.Character.Weapons.Give((WeaponHash)weaponHash, 1000, true, true);
                    }

                    AddDictValue(EMPTY_DICT, player, weaponHash, 0, 0);
                    SaveWeaponInInventory(player);
                }
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
                }
                else
                {
                    Function.Call(Hash.GIVE_WEAPON_COMPONENT_TO_PED, Game.Player.Character.Handle, weaponHash, componentHash);
                    int current_ammo = Function.Call<int>(Hash.GET_AMMO_IN_PED_WEAPON, Game.Player.Character, weaponHash);
                    if (current_ammo == 0)
                    {
                        Function.Call(Hash.ADD_PED_AMMO_BY_TYPE, Game.Player.Character, Function.Call<Hash>(Hash.GET_PED_AMMO_TYPE_FROM_WEAPON, Game.Player.Character, weaponHash), 200);
                        AddDictValue(AMMO_DICT, player, weaponHash, 0, 200);
                    }
                }
            }
            else
            {
                if (Game.Player.Money < cost_int && Game.Player.Character.Model.Hash != new Model("mp_m_freemode_01").Hash && Game.Player.Character.Model.Hash != new Model("mp_f_freemode_01").Hash)
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
            SaveWeaponInInventory(player);
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
        RubberGuns.Clear();
        ThrownMenu.Clear();
        DigiScaners.Clear();
        FireExtinguishers.Clear();
        HackingDevices.Clear();
        MetalDetectors.Clear();
        NightVisions.Clear();
        Parachutes.Clear();
        PetrolCans.Clear();
        Tranquilizers.Clear();

        foreach (NativeMenu menu in CustomMenusList)
        {
            menu.Clear();
        }

        SetMenuItems();

    }

    private void SetMenuItems()
    {
        NativeItem blocked = null;

        BadgeSet shop_gun = CreateBafgeFromItem("commonmenu", "shop_gunclub_icon_a", "commonmenu", "shop_gunclub_icon_b");

        string[] commands = File.ReadAllLines($"Scripts\\AddonWeapons\\commandline.txt");

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
                int WeapCost = weapon.WeaponData.weaponCost;
                uint weaponHash = weapon.WeaponData.weaponHash;

                if (WeapName == null || WeapName.Length < 3)
                {
                    WeapName = weapon.WeaponData.GetNameLabel();
                    if (WeapName == "WT_SNOWLAUNCHER")
                    {
                        WeapName = Game.GetLocalizedString("WT_SNOWLNCHR");
                    }
                }

                foreach (string command in commands)
                {
                    string trimmedCommand = command.Trim();
                    if (trimmedCommand.StartsWith("SetWeaponCost"))
                    {
                        string[] parameters = ExtractParameters(trimmedCommand, "SetWeaponCost", 2);
                        if (parameters != null && int.TryParse(parameters[1], out int cost) && new Model(parameters[0]).Hash == (int)weaponHash)
                        {
                            WeapCost = cost;
                        }
                    }
                }

                NativeItem weap_m = CreateWeaponItem(weapon, WeapName, WeapDesc, WeapCost, weaponHash);
                if (Game.Player.Character.Weapons.HasWeapon((WeaponHash)weaponHash) && HashComponentsAvailable(weaponHash))
                {
                    weap_m.AltTitle = "";
                    weap_m.RightBadgeSet = shop_gun;
                }

                foreach (string command in commands)
                {
                    string trimmedCommand = command.Trim();
                    if (trimmedCommand.StartsWith("PutWeaponToCategory"))
                    {
                        string[] parameters = ExtractParameters(trimmedCommand, "PutWeaponToCategory", 2);
                        if (parameters != null)
                        {
                            switch(parameters[1])
                            {
                                case "GROUP_HEAVY":
                                    if (new Model(parameters[0]).Hash == (int)weaponHash)
                                    {
                                        HeavyMenu.Add(weap_m);
                                        blocked = weap_m;
                                    }
                                    break;
                                case "GROUP_MELEE":
                                    if (new Model(parameters[0]).Hash == (int)weaponHash)
                                    {
                                        MeleeMenu.Add(weap_m);
                                        blocked = weap_m;
                                    }
                                    break;
                                case "GROUP_MG":
                                    if (new Model(parameters[0]).Hash == (int)weaponHash)
                                    {
                                        MachineGunsMenu.Add(weap_m);
                                        blocked = weap_m;
                                    }
                                    break;
                                case "GROUP_PISTOL":
                                    if (new Model(parameters[0]).Hash == (int)weaponHash)
                                    {
                                        PistolsMenu.Add(weap_m);
                                        blocked = weap_m;
                                    }
                                    break;
                                case "GROUP_RIFLE":
                                    if (new Model(parameters[0]).Hash == (int)weaponHash)
                                    {
                                        RiflesMenu.Add(weap_m);
                                        blocked = weap_m;
                                    }
                                    break;
                                case "GROUP_SHOTGUN":
                                    if (new Model(parameters[0]).Hash == (int)weaponHash)
                                    {
                                        ShotgunsMenu.Add(weap_m);
                                        blocked = weap_m;
                                    }
                                    break;
                                case "GROUP_SMG":
                                    if (new Model(parameters[0]).Hash == (int)weaponHash)
                                    {
                                        SMGsMenu.Add(weap_m);
                                        blocked = weap_m;
                                    }
                                    break;
                                case "GROUP_SNIPER":
                                    if (new Model(parameters[0]).Hash == (int)weaponHash)
                                    {
                                        SniperRiflesMenu.Add(weap_m);
                                        blocked = weap_m;
                                    }
                                    break;
                                case "GROUP_STUNGUN":
                                    if (new Model(parameters[0]).Hash == (int)weaponHash)
                                    {
                                        StunGunMenu.Add(weap_m);
                                        blocked = weap_m;
                                    }
                                    break;
                                case "GROUP_THROWN":
                                    if (new Model(parameters[0]).Hash == (int)weaponHash)
                                    {
                                        ThrownMenu.Add(weap_m);
                                        blocked = weap_m;
                                    }
                                    break;
                                case "GROUP_RUBBERGUN":
                                    if (new Model(parameters[0]).Hash == (int)weaponHash)
                                    {
                                        RubberGuns.Add(weap_m);
                                        blocked = weap_m;
                                    }
                                    break;
                                case "GROUP_DIGISCANNER":
                                    if (new Model(parameters[0]).Hash == (int)weaponHash)
                                    {
                                        DigiScaners.Add(weap_m);
                                        blocked = weap_m;
                                    }
                                    break;
                                case "GROUP_FIREEXTINGUISHER":
                                    if (new Model(parameters[0]).Hash == (int)weaponHash)
                                    {
                                        FireExtinguishers.Add(weap_m);
                                        blocked = weap_m;
                                    }
                                    break;
                                case "GROUP_HACKINGDEVICE":
                                    if (new Model(parameters[0]).Hash == (int)weaponHash)
                                    {
                                        HackingDevices.Add(weap_m);
                                        blocked = weap_m;
                                    }
                                    break;
                                case "GROUP_METALDETECTOR":
                                    if (new Model(parameters[0]).Hash == (int)weaponHash)
                                    {
                                        MetalDetectors.Add(weap_m);
                                        blocked = weap_m;
                                    }
                                    break;
                                case "GROUP_NIGHTVISION":
                                    if (new Model(parameters[0]).Hash == (int)weaponHash)
                                    {
                                        NightVisions.Add(weap_m);
                                        blocked = weap_m;
                                    }
                                    break;
                                case "GROUP_PARACHUTE":
                                    if (new Model(parameters[0]).Hash == (int)weaponHash)
                                    {
                                        Parachutes.Add(weap_m);
                                        blocked = weap_m;
                                    }
                                    break;
                                case "GROUP_PETROLCAN":
                                    if (new Model(parameters[0]).Hash == (int)weaponHash)
                                    {
                                        PetrolCans.Add(weap_m);
                                        blocked = weap_m;
                                    }
                                    break;
                                case "GROUP_TRANQUILIZER":
                                    if (new Model(parameters[0]).Hash == (int)weaponHash)
                                    {
                                        Tranquilizers.Add(weap_m);
                                        blocked = weap_m;
                                    }
                                    break;
                            }

                            foreach (NativeMenu custom in CustomMenusList)
                            {
                                if (new Model(parameters[0]).Hash == (int)weaponHash && custom.Name == parameters[1])
                                {
                                    custom.Add(weap_m);
                                    blocked = weap_m;
                                    break;
                                }
                            }
                        }
                    }
                }

                if (blocked == weap_m) continue;

                uint weaponTypeGroup = Function.Call<uint>(Hash.GET_WEAPONTYPE_GROUP, weaponHash);
                switch (weaponTypeGroup)
                {
                    case GROUP_HEAVY:
                        HeavyMenu.Add(weap_m);
                        break;
                    case GROUP_MELEE:
                        MeleeMenu.Add(weap_m);
                        NoAmmoWeaponList.Add(weaponHash);
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
                    case GROUP_RUBBERGUN:
                        RubberGuns.Add(weap_m);
                        break;
                    case GROUP_STUNGUN:
                        StunGunMenu.Add(weap_m);
                        NoAmmoWeaponList.Add(weaponHash);
                        break;
                    case GROUP_THROWN:
                        ThrownMenu.Add(weap_m);
                        break;
                    case GROUP_DIGISCANNER:
                        DigiScaners.Add(weap_m);
                        break;
                    case GROUP_FIREEXTINGUISHER:
                        FireExtinguishers.Add(weap_m);
                        break;
                    case GROUP_HACKINGDEVICE:
                        HackingDevices.Add(weap_m);
                        break;
                    case GROUP_METALDETECTOR:
                        MetalDetectors.Add(weap_m);
                        break;
                    case GROUP_NIGHTVISION:
                        NightVisions.Add(weap_m);
                        break;
                    case GROUP_PARACHUTE:
                        Parachutes.Add(weap_m);
                        break;
                    case GROUP_PETROLCAN:
                        PetrolCans.Add(weap_m);
                        break;
                    case GROUP_TRANQILIZER:
                        Tranquilizers.Add(weap_m);
                        break;
                }

            }
        }
    }

    private void BuildMenu()
    {
        if (HeavyMenu.Items.Count > 0) menu.AddSubMenu(HeavyMenu);
        if (MeleeMenu.Items.Count > 0) menu.AddSubMenu(MeleeMenu);
        if (MachineGunsMenu.Items.Count > 0) menu.AddSubMenu(MachineGunsMenu);
        if (PistolsMenu.Items.Count > 0) menu.AddSubMenu(PistolsMenu);
        if (RiflesMenu.Items.Count > 0) menu.AddSubMenu(RiflesMenu);
        if (ShotgunsMenu.Items.Count > 0) menu.AddSubMenu(ShotgunsMenu);
        if (SMGsMenu.Items.Count > 0) menu.AddSubMenu(SMGsMenu);
        if (SniperRiflesMenu.Items.Count > 0) menu.AddSubMenu(SniperRiflesMenu);
        if (StunGunMenu.Items.Count > 0) menu.AddSubMenu(StunGunMenu);
        if (ThrownMenu.Items.Count > 0) menu.AddSubMenu(ThrownMenu);
        if (RubberGuns.Items.Count > 0) menu.AddSubMenu(RubberGuns);
        if (DigiScaners.Items.Count > 0) menu.AddSubMenu(DigiScaners);
        if (FireExtinguishers.Items.Count > 0) menu.AddSubMenu(FireExtinguishers);
        if (HackingDevices.Items.Count > 0) menu.AddSubMenu(HackingDevices);
        if (MetalDetectors.Items.Count > 0) menu.AddSubMenu(MetalDetectors);
        if (NightVisions.Items.Count > 0) menu.AddSubMenu(NightVisions);
        if (Parachutes.Items.Count > 0) menu.AddSubMenu(Parachutes);
        if (PetrolCans.Items.Count > 0) menu.AddSubMenu(PetrolCans);
        if (Tranquilizers.Items.Count > 0) menu.AddSubMenu(Tranquilizers);
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