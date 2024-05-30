using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
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

    int current_menu_category = -1; //-1 - nothing, 0 - main, 1 - weapon types, 2 - components
    int last_menu_category = -1;
    int current_item_menu = -1;
    int last_item_menu = -1;
    uint last_weapon_hash;
    int last_weapon_component;

    Keys menuOpenKey;

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
    Prop test_w;

    Vector3 box_pos_1;
    Vector3 box_pos_2;
    Vector3 box_rot_1;
    Vector3 box_rot_2;

    List<uint> HeavyMenu_preview = new List<uint>();
    List<uint> MeleeMenu_preview = new List<uint>();
    List<uint> MachineGunsMenu_preview = new List<uint>();
    List<uint> PistolsMenu_preview = new List<uint>();
    List<uint> RiflesMenu_preview = new List<uint>();
    List<uint> ShotgunsMenu_preview = new List<uint>();
    List<uint> SMGsMenu_preview = new List<uint>();
    List<uint> SniperRiflesMenu_preview = new List<uint>();
    List<uint> StunGunMenu_preview = new List<uint>();
    List<uint> ThrownMenu_preview = new List<uint>();
    List<uint> components_preview = new List<uint>();

    string[] tints_default = new string[]
        {
            "Black Tint",
            "Army Tint",
            "Green Tint",
            "Orange Tint",
            "LSPD Tint",
            "Pink Tint",
            "Gold Tint",
            "Platinum Tint",
        };

    string[] tints_mk2 = new string[]
        {
            "Classic Black",
            "Classic Gray",
            "Classic Two-Tone",
            "Classic White",
            "Classic Beige",
            "Classic Green",
            "Classic Blue",
            "Classic Earth",
            "Classic Brown & Black",
            "Red Contrast",
            "Blue Contrast",
            "Yellow Contrast",
            "Orange Contrast",
            "Bold Pink",
            "Bold Purple & Yellow",
            "Bold Orange",
            "Bold Green & Purple",
            "Bold Red Features",
            "Bold Green Features",
            "Bold Cyan Features",
            "Bold Yellow Features",
            "Bold Red & White",
            "Bold Blue & White",
            "Metallic Gold",
            "Metallic Platinum",
            "Metallic Gray & Lilac",
            "Metallic Purple & Lime",
            "Metallic Red",
            "Metallic Green",
            "Metallic Blue",
            "Metallic White & Aqua",
            "Metallic Red & Yellow",
        };

    Model model_box = new Model(2107849419);

    private Camera weaponCamera;

    ScriptSettings config;
    ScriptSettings config_settings;

    public AddonWeapons()
    {
        SetLanguage();
        InitializeCategories();
        InitializeMenu();
        GetDlcWeaponModels();
        LoadDlcWeaponModels();
        SetMenuItems();
        LoadAmmoBoxes();
        KeyUp += onkeyup;
        Tick += OnTick;
        Aborted += OnAborted;
    }

    private void SetLanguage()
    {
        int lang = Function.Call<int>(Hash.GET_CURRENT_LANGUAGE);
        string language_file = "american.dat";

        switch(lang)
        {
            case 0:
                language_file = "american.dat";
                break;
            case 1:
                language_file = "french.dat";
                break;
            case 2:
                language_file = "german.dat";
                break;
            case 3:
                language_file = "italian.dat";
                break;
            case 4:
                language_file = "spanish.dat";
                break;
            case 5:
                language_file = "brazilian.dat";
                break;
            case 6:
                language_file = "polish.dat";
                break;
            case 7:
                language_file = "russian.dat";
                break;
            case 8:
                language_file = "korean.dat";
                break;
            case 9:
                language_file = "chinesetrad.dat";
                break;
            case 10:
                language_file = "japanese.dat";
                break;
            case 11:
                language_file = "mexican.dat";
                break;
            case 12:
                language_file = "chinesesimp.dat";
                break;
        }
        config = ScriptSettings.Load($"Scripts\\AddonWeapons\\lang\\{language_file}");
        config_settings = ScriptSettings.Load($"Scripts\\AddonWeapons\\settings.ini");
        _TITLE_MAIN = config.GetValue<string>("LANG", "TITLE_MAIN", "Exclusive weapons");
        _TITLE_HEAVY = config.GetValue<string>("LANG", "TITLE_HEAVY", "Heavy");
        _TITLE_MELEE = config.GetValue<string>("LANG", "TITLE_MELEE", "Melee");
        _TITLE_MG = config.GetValue<string>("LANG", "TITLE_MG", "Machine Guns");
        _TITLE_PISTOLS = config.GetValue<string>("LANG", "TITLE_PISTOLS", "Pistols");
        _TITLE_RIFLES = config.GetValue<string>("LANG", "TITLE_RIFLES", "Rifles");
        _TITLE_SHOTGUNS = config.GetValue<string>("LANG", "TITLE_SHOTGUNS", "Shotguns");
        _TITLE_SMG = config.GetValue<string>("LANG", "TITLE_SMG", "SMGs");
        _TITLE_SR = config.GetValue<string>("LANG", "TITLE_SR", "Sniper Rifles");
        _TITLE_SG = config.GetValue<string>("LANG", "TITLE_SG", "Stun Gun");
        _TITLE_THROWN = config.GetValue<string>("LANG", "TITLE_THROWN", "Thrown");
        _ROUNDS = config.GetValue<string>("LANG", "ROUNDS", "Rounds");
        _MAX_ROUNDS = config.GetValue<string>("LANG", "MAX_ROUNDS", "FULL");
        _HELP_MESSAGE = config.GetValue<string>("LANG", "HELP_MESSAGE", "Press ~INPUT_CONTEXT~ to open the box of exclusive weapons.");
        _NO_MONEY = config.GetValue<string>("LANG", "NO_MONEY", "You need more cash!");

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

    private void LoadDlcWeaponModels()
    {
        foreach (var category in weaponCategories)
        {
            foreach (var weapon in category.Value)
            {
                Function.Call(Hash.REQUEST_WEAPON_ASSET, weapon.WeaponData.weaponHash, 31, 0);
            }
        }
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

        if (box2 != null && box2.Exists())
        {
            box2.Delete();
        }

        if (test_w != null && test_w.Exists())
        {
            test_w.Delete();
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

    public static int LoadComponentModel(uint component)
    {
        int componentModel = Function.Call<int>(Hash.GET_WEAPON_COMPONENT_TYPE_MODEL, component);

        if (componentModel != 0)
        {
            Function.Call(Hash.REQUEST_MODEL, componentModel);

            while (!Function.Call<bool>(Hash.HAS_MODEL_LOADED, componentModel))
            {
                Wait(1);
            }

            return componentModel;
        }

        return 0;
    }

    private void SetMenuCategoryID()
    {
        NativeMenu[] menus = new NativeMenu[]
        {
            menu,
            HeavyMenu,
            MeleeMenu,
            MachineGunsMenu,
            PistolsMenu,
            RiflesMenu,
            ShotgunsMenu,
            SMGsMenu,
            SniperRiflesMenu,
            StunGunMenu,
            ThrownMenu,
            ComponentMenu,
        };

        int count = 0;
        foreach (NativeMenu menu in menus)
        {
            if (menu.Visible)
            {
                current_menu_category = count;
                current_item_menu = menu.SelectedIndex;
            }
            count++;
        }

        if (!IsMenuOpen())
        {
            current_menu_category = -1;
            current_item_menu = -1;

            if (weaponCamera != null)
            {
                weaponCamera.Delete();
                World.RenderingCamera = null;
                Game.Player.Character.IsVisible = true;
                weaponCamera = null;
            }
            
        }
    }

    private void SetWeaponPreview()
    {
        if (current_menu_category > 0 && current_menu_category < 11)
        {
            switch(current_menu_category)
            {
                case 1:
                    if (test_w != null && test_w.Exists())
                    {
                        test_w.Delete();
                    }
                    test_w = Function.Call<Prop>(Hash.CREATE_WEAPON_OBJECT, HeavyMenu_preview[HeavyMenu.SelectedIndex], 100, box1.Position.X, box1.Position.Y - 0.5f, box1.Position.Z + 2f, 1, 1065353216, 0, 0, 1);
                    Function.Call(Hash.PLACE_OBJECT_ON_GROUND_OR_OBJECT_PROPERLY, test_w);
                    last_weapon_hash = HeavyMenu_preview[HeavyMenu.SelectedIndex];
                    break;
                case 2:
                    if (test_w != null && test_w.Exists())
                    {
                        test_w.Delete();
                    }
                    test_w = Function.Call<Prop>(Hash.CREATE_WEAPON_OBJECT, MeleeMenu_preview[MeleeMenu.SelectedIndex], 100, box1.Position.X, box1.Position.Y - 0.5f, box1.Position.Z + 2f, 1, 1065353216, 0, 0, 1);
                    Function.Call(Hash.PLACE_OBJECT_ON_GROUND_OR_OBJECT_PROPERLY, test_w);
                    last_weapon_hash = MeleeMenu_preview[MeleeMenu.SelectedIndex];
                    break;
                case 3:
                    if (test_w != null && test_w.Exists())
                    {
                        test_w.Delete();
                    }
                    test_w = Function.Call<Prop>(Hash.CREATE_WEAPON_OBJECT, MachineGunsMenu_preview[MachineGunsMenu.SelectedIndex], 100, box1.Position.X, box1.Position.Y - 0.5f, box1.Position.Z + 2f, 1, 1065353216, 0, 0, 1);
                    Function.Call(Hash.PLACE_OBJECT_ON_GROUND_OR_OBJECT_PROPERLY, test_w);
                    last_weapon_hash = MachineGunsMenu_preview[MachineGunsMenu.SelectedIndex];
                    break;
                case 4:
                    if (test_w != null && test_w.Exists())
                    {
                        test_w.Delete();
                    }
                    test_w = Function.Call<Prop>(Hash.CREATE_WEAPON_OBJECT, PistolsMenu_preview[PistolsMenu.SelectedIndex], 100, box1.Position.X, box1.Position.Y - 0.5f, box1.Position.Z + 2f, 1, 1065353216, 0, 0, 1);
                    Function.Call(Hash.PLACE_OBJECT_ON_GROUND_OR_OBJECT_PROPERLY, test_w);
                    last_weapon_hash = PistolsMenu_preview[PistolsMenu.SelectedIndex];
                    break;
                case 5:
                    if (test_w != null && test_w.Exists())
                    {
                        test_w.Delete();
                    }
                    test_w = Function.Call<Prop>(Hash.CREATE_WEAPON_OBJECT, RiflesMenu_preview[RiflesMenu.SelectedIndex], 100, box1.Position.X, box1.Position.Y - 0.5f, box1.Position.Z + 2f, 1, 1065353216, 0, 0, 1);
                    Function.Call(Hash.PLACE_OBJECT_ON_GROUND_OR_OBJECT_PROPERLY, test_w);
                    last_weapon_hash = RiflesMenu_preview[RiflesMenu.SelectedIndex];
                    break;
                case 6:
                    if (test_w != null && test_w.Exists())
                    {
                        test_w.Delete();
                    }
                    test_w = Function.Call<Prop>(Hash.CREATE_WEAPON_OBJECT, ShotgunsMenu_preview[ShotgunsMenu.SelectedIndex], 100, box1.Position.X, box1.Position.Y - 0.5f, box1.Position.Z + 2f, 1, 1065353216, 0, 0, 1);
                    Function.Call(Hash.PLACE_OBJECT_ON_GROUND_OR_OBJECT_PROPERLY, test_w);
                    last_weapon_hash = ShotgunsMenu_preview[ShotgunsMenu.SelectedIndex];
                    break;
                case 7:
                    if (test_w != null && test_w.Exists())
                    {
                        test_w.Delete();
                    }
                    test_w = Function.Call<Prop>(Hash.CREATE_WEAPON_OBJECT, SMGsMenu_preview[SMGsMenu.SelectedIndex], 100, box1.Position.X, box1.Position.Y - 0.5f, box1.Position.Z + 2f, 1, 1065353216, 0, 0, 1);
                    Function.Call(Hash.PLACE_OBJECT_ON_GROUND_OR_OBJECT_PROPERLY, test_w);
                    last_weapon_hash = SMGsMenu_preview[SMGsMenu.SelectedIndex];
                    break;
                case 8:
                    if (test_w != null && test_w.Exists())
                    {
                        test_w.Delete();
                    }
                    test_w = Function.Call<Prop>(Hash.CREATE_WEAPON_OBJECT, SniperRiflesMenu_preview[SniperRiflesMenu.SelectedIndex], 100, box1.Position.X, box1.Position.Y - 0.5f, box1.Position.Z + 2f, 1, 1065353216, 0, 0, 1);
                    Function.Call(Hash.PLACE_OBJECT_ON_GROUND_OR_OBJECT_PROPERLY, test_w);
                    last_weapon_hash = SniperRiflesMenu_preview[SniperRiflesMenu.SelectedIndex];
                    break;
                case 9:
                    if (test_w != null && test_w.Exists())
                    {
                        test_w.Delete();
                    }
                    test_w = Function.Call<Prop>(Hash.CREATE_WEAPON_OBJECT, StunGunMenu_preview[StunGunMenu.SelectedIndex], 100, box1.Position.X, box1.Position.Y - 0.5f, box1.Position.Z + 2f, 1, 1065353216, 0, 0, 1);
                    Function.Call(Hash.PLACE_OBJECT_ON_GROUND_OR_OBJECT_PROPERLY, test_w);
                    last_weapon_hash = StunGunMenu_preview[StunGunMenu.SelectedIndex];
                    break;
                case 10:
                    if (test_w != null && test_w.Exists())
                    {
                        test_w.Delete();
                    }
                    test_w = Function.Call<Prop>(Hash.CREATE_WEAPON_OBJECT, ThrownMenu_preview[ThrownMenu.SelectedIndex], 100, box1.Position.X, box1.Position.Y - 0.5f, box1.Position.Z + 2f, 1, 1065353216, 0, 0, 1);
                    Function.Call(Hash.PLACE_OBJECT_ON_GROUND_OR_OBJECT_PROPERLY, test_w);
                    last_weapon_hash = ThrownMenu_preview[ThrownMenu.SelectedIndex];
                    break;
            }
        }
        else
        {
            if (current_menu_category == 11 && current_item_menu > 0)
            {
                /*/Vector3 pos = test_w.Position;
                test_w.Delete();
                var luxModel = LoadComponentModel(components_preview[current_item_menu - 1]);
                test_w = Function.Call<Prop>(Hash.CREATE_WEAPON_OBJECT, last_weapon_hash, 100, pos.X, pos.Y, pos.Z, 1, 0, luxModel, 0, 1);
                Function.Call(Hash.PLACE_OBJECT_ON_GROUND_OR_OBJECT_PROPERLY, test_w);/*/

                if (last_weapon_component != 0)
                {
                    Function.Call(Hash.REMOVE_WEAPON_COMPONENT_FROM_WEAPON_OBJECT, test_w, last_weapon_component);
                    last_weapon_component = 0;
                }

                var luxModel = LoadComponentModel(components_preview[current_item_menu - 1]);
                Function.Call(Hash.GIVE_WEAPON_COMPONENT_TO_WEAPON_OBJECT, test_w.Handle, luxModel);
            }
            else
            {
                if (current_menu_category == 11 && current_item_menu == 0)
                {
                    if (last_weapon_component != 0)
                    {
                        Function.Call(Hash.REMOVE_WEAPON_COMPONENT_FROM_WEAPON_OBJECT, test_w, last_weapon_component);
                        last_weapon_component = 0;
                    }
                }
            }
        }
    }

    private void SendWeaponPreviewThisFrame()
    {
        if (current_menu_category != last_menu_category && current_menu_category > 0)
        {
            //SetWeaponPreview();
            last_menu_category = current_menu_category;
        }
        else
        {
            if (current_item_menu != last_item_menu && current_item_menu != -1)
            {
                //SetWeaponPreview();
                last_item_menu = current_item_menu;
            }
            else
            {
                if (!IsMenuOpen() || current_menu_category < 1)
                {
                    if (test_w != null && test_w.Exists())
                    {
                        test_w.Delete();
                        current_item_menu = -1;
                        last_item_menu = -1;
                        current_menu_category = -1;
                        last_menu_category = -1;
                    }
                }
            }
        }
        
        {
            if (!IsMenuOpen() || current_menu_category < 1)
            {
                if (test_w != null && test_w.Exists())
                {
                    test_w.Delete();
                }
            }
        }
    }

    private void OnTick(object sender, EventArgs e)
    {
        pool.Process();
        CreateAmmoBoxesThisFrame();
        //SetMenuCategoryID();
        //SendWeaponPreviewThisFrame();

        if (Game.Player.Character.Position.DistanceTo(box_pos_1) < 1.5f || Game.Player.Character.Position.DistanceTo(box_pos_2) < 1.5f)
        {
            if (!IsMenuOpen())
            {
                GTA.UI.Screen.ShowHelpTextThisFrame(_HELP_MESSAGE);
            }

            if (Function.Call<bool>(Hash.IS_CONTROL_JUST_PRESSED, 0, 51))
            {
                menu.Visible = true;
                last_menu_category = -1;

                if (weaponCamera == null)
                {
                    /*/Vector3 spawnPosition = Game.Player.Character.Position + Game.Player.Character.ForwardVector * 2.0f;
                    Vector3 cameraPosition = spawnPosition + new Vector3(0, 1.5f, 0.5f);
                    Game.Player.Character.IsVisible = false;
                    weaponCamera = World.CreateCamera(new Vector3(box1.Position.X, box1.Position.Y - 2f, box1.Position.Z + 0.8f), new Vector3(0.0f, 0.0f, 0.0f), 50.0f);
                    weaponCamera.PointAt(new Vector3(box1.Position.X, box1.Position.Y, box1.Position.Z + 1f));
                    World.RenderingCamera = weaponCamera;/*/
                }
            }
        }
        
        if (Game.Player.Character.Position.DistanceTo(box_pos_1) > 1.5f && Game.Player.Character.Position.DistanceTo(box_pos_2) > 1.5f && IsMenuOpen() && menuOpenKey == Keys.None)
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

    private void SetMenuItems()
    {
        int index_menu_count = -1;

        foreach (var category in weaponCategories)
        {
            foreach (var weapon in category.Value)
            {
                string WeapLabel = weapon.WeaponData.GetNameLabel();
                string WeapName = Game.GetLocalizedString(weapon.WeaponData.GetNameLabel());
                string WeapDesc = Game.GetLocalizedString(weapon.WeaponData.GetDescLabel());
                string WeapCost = $"${weapon.WeaponData.weaponCost}";
                uint weaponHash = weapon.WeaponData.weaponHash;

                Function.Call(Hash.REQUEST_MODEL, weaponHash);

                if (WeapName == null || WeapName.Length < 3)
                {
                    WeapName = weapon.WeaponData.GetNameLabel();
                }

                BadgeSet shop_gun = new BadgeSet
                {
                    NormalDictionary = "commonmenu",
                    NormalTexture = "shop_gunclub_icon_a",
                    HoveredDictionary = "commonmenu",
                    HoveredTexture = "shop_gunclub_icon_b"
                };

                NativeItem weap_m = new NativeItem(WeapName, WeapDesc, WeapCost);

                if (Game.Player.Character.Weapons.HasWeapon((WeaponHash)weaponHash))
                {
                    weap_m.AltTitle = "";
                    weap_m.RightBadgeSet = shop_gun;
                }

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
                                components_preview.Clear();
                                int rounds = -1;
                                int tints_flag = -1;
                                string ammoCost;

                                int defaultClipSize = weapon.WeaponData.defaultClipSize;
                                int current_ammo = Function.Call<int>(Hash.GET_AMMO_IN_PED_WEAPON, Game.Player.Character, weaponHash);

                                unsafe
                                {
                                    int maxAmmo = 0;
                                    bool hasMaxAmmo = Function.Call<bool>(Hash.GET_MAX_AMMO, Game.Player.Character.Handle, (uint)weaponHash, (IntPtr)(&maxAmmo));

                                    if (maxAmmo == current_ammo)
                                    {
                                        ammoCost = _MAX_ROUNDS;
                                    }
                                    else
                                    {
                                        ammoCost = $"${weapon.WeaponData.ammoCost}";
                                    }
                                }

                                NativeItem rounds_m = new NativeItem($"{_ROUNDS} x {defaultClipSize}", "", ammoCost);
                                rounds_m.Activated += (sender3, args3) =>
                                {
                                    if (Game.Player.Money < weapon.WeaponData.ammoCost)
                                    {
                                        GTA.UI.Screen.ShowSubtitle(_NO_MONEY);
                                    }
                                    else
                                    {
                                        unsafe
                                        {
                                            int maxAmmo = 0;
                                            bool hasMaxAmmo = Function.Call<bool>(Hash.GET_MAX_AMMO, Game.Player.Character.Handle, (uint)weaponHash, (IntPtr)(&maxAmmo));

                                            if (maxAmmo > current_ammo)
                                            {
                                                Game.Player.Money -= weapon.WeaponData.ammoCost;
                                                Function.Call(Hash.ADD_AMMO_TO_PED, Game.Player.Character, weaponHash, defaultClipSize);
                                                current_ammo = Function.Call<int>(Hash.GET_AMMO_IN_PED_WEAPON, Game.Player.Character, weaponHash);

                                                hasMaxAmmo = Function.Call<bool>(Hash.GET_MAX_AMMO, Game.Player.Character.Handle, (uint)weaponHash, (IntPtr)(&maxAmmo));
                                                if (maxAmmo == current_ammo)
                                                {
                                                    rounds_m.AltTitle = _MAX_ROUNDS;

                                                }
                                            }
                                        }
                                    }
                                };
                                ComponentMenu.Add(rounds_m);
                                index_menu_count = ComponentMenu.Count();

                                if (weapon.Components.Count > 0)
                                {
                                    foreach (var component in weapon.Components)
                                    {
                                        string componentName = Game.GetLocalizedString(component.GetNameLabel());
                                        string componentCost = $"${component.componentCost}";
                                        uint componentHash = component.componentHash;
                                        defaultClipSize = weapon.WeaponData.defaultClipSize;
                                        ammoCost = $"${weapon.WeaponData.ammoCost}";
                                        components_preview.Add(componentHash);

                                        NativeItem comp_m = new NativeItem(componentName, "", componentCost);

                                        if (Function.Call<bool>(Hash.HAS_PED_GOT_WEAPON_COMPONENT, Game.Player.Character.Handle, weaponHash, componentHash))
                                        {
                                            comp_m.AltTitle = "";
                                            comp_m.RightBadgeSet = shop_gun;
                                        }

                                        comp_m.Activated += (sender2, args2) =>
                                        {
                                            if (Game.Player.Money < component.componentCost)
                                            {
                                                GTA.UI.Screen.ShowSubtitle(_NO_MONEY);
                                            }
                                            else
                                            {
                                                Game.Player.Money -= component.componentCost;
                                                Function.Call(Hash.GIVE_WEAPON_COMPONENT_TO_PED, Game.Player.Character.Handle, weaponHash, componentHash);

                                                comp_m.AltTitle = "";
                                                comp_m.RightBadgeSet = shop_gun;
                                            }
                                        };
                                        ComponentMenu.Add(comp_m);
                                        index_menu_count = ComponentMenu.Count();
                                    }

                                }

                                if (tints_flag == -1)
                                {
                                    List<string> tints = new List<string>();
                                    int count = Function.Call<int>(Hash.GET_WEAPON_TINT_COUNT, weaponHash);
                                    string filePath = $"Scripts\\AddonWeapons\\tints\\{WeapLabel}.txt";

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
                                            int temp_intex = Function.Call<int>(Hash.GET_PED_WEAPON_TINT_INDEX, Game.Player.Character, weaponHash);

                                            if (temp_intex == i)
                                            {
                                                tint_m.AltTitle = "";
                                                tint_m.RightBadgeSet = shop_gun;
                                            }

                                            tint_m.Activated += (sender2, args2) =>
                                            {
                                                if (Game.Player.Money < 1000)
                                                {
                                                    GTA.UI.Screen.ShowSubtitle(_NO_MONEY);
                                                }
                                                else
                                                {
                                                    Game.Player.Money -= 1000;
                                                    int tint_index = ComponentMenu.SelectedIndex - index_menu_count;
                                                    Function.Call(Hash.SET_PED_WEAPON_TINT_INDEX, Game.Player.Character, weaponHash, tint_index);
                                                    tint_m.AltTitle = "";
                                                    tint_m.RightBadgeSet = shop_gun;
                                                }
                                            };
                                            ComponentMenu.Add(tint_m);
                                        }
                                        tints_flag = 1;
                                    }
                                    else
                                    {
                                        if (count == tints_default.Length)
                                        {
                                            for (int i = 0; i < tints_default.Length; i++)
                                            {
                                                NativeItem tint_m = new NativeItem($"{tints_default[i]}", "", "$1000");
                                                int temp_intex = Function.Call<int>(Hash.GET_PED_WEAPON_TINT_INDEX, Game.Player.Character, weaponHash);

                                                if (temp_intex == i)
                                                {
                                                    tint_m.AltTitle = "";
                                                    tint_m.RightBadgeSet = shop_gun;
                                                }

                                                tint_m.Activated += (sender4, args4) =>
                                                {
                                                    if (Game.Player.Money < 1000)
                                                    {
                                                        GTA.UI.Screen.ShowSubtitle(_NO_MONEY);
                                                    }
                                                    else
                                                    {
                                                        Game.Player.Money -= 1000;
                                                        int tint_index = ComponentMenu.SelectedIndex - index_menu_count;
                                                        Function.Call(Hash.SET_PED_WEAPON_TINT_INDEX, Game.Player.Character, weaponHash, tint_index);
                                                        tint_m.AltTitle = "";
                                                        tint_m.RightBadgeSet = shop_gun;
                                                    }
                                                };
                                                ComponentMenu.Add(tint_m);
                                            }
                                        }
                                        else
                                        {
                                            if (count >= tints_mk2.Length)
                                            {
                                                for (int i = 0; i < tints_mk2.Length; i++)
                                                {
                                                    NativeItem tint_m = new NativeItem($"{tints_mk2[i]}", "", "$1000");
                                                    int temp_intex = Function.Call<int>(Hash.GET_PED_WEAPON_TINT_INDEX, Game.Player.Character, weaponHash);

                                                    if (temp_intex == i)
                                                    {
                                                        tint_m.AltTitle = "";
                                                        tint_m.RightBadgeSet = shop_gun;
                                                    }

                                                    tint_m.Activated += (sender4, args4) =>
                                                    {
                                                        if (Game.Player.Money < 1000)
                                                        {
                                                            GTA.UI.Screen.ShowSubtitle(_NO_MONEY);
                                                        }
                                                        else
                                                        {
                                                            Game.Player.Money -= 1000;
                                                            int tint_index = ComponentMenu.SelectedIndex - index_menu_count;
                                                            Function.Call(Hash.SET_PED_WEAPON_TINT_INDEX, Game.Player.Character, weaponHash, tint_index);
                                                            tint_m.AltTitle = "";
                                                            tint_m.RightBadgeSet = shop_gun;
                                                        }
                                                    };
                                                    ComponentMenu.Add(tint_m);
                                                }
                                            }
                                            else
                                            {
                                                for (int i = 0; i < count; i++)
                                                {
                                                    NativeItem tint_m = new NativeItem($"Tint {i + 1}", "", "$1000");
                                                    int temp_intex = Function.Call<int>(Hash.GET_PED_WEAPON_TINT_INDEX, Game.Player.Character, weaponHash);

                                                    if (temp_intex == i)
                                                    {
                                                        tint_m.AltTitle = "";
                                                        tint_m.RightBadgeSet = shop_gun;
                                                    }

                                                    tint_m.Activated += (sender4, args4) =>
                                                    {
                                                        if (Game.Player.Money < 1000)
                                                        {
                                                            GTA.UI.Screen.ShowSubtitle(_NO_MONEY);
                                                        }
                                                        else
                                                        {
                                                            Game.Player.Money -= 1000;
                                                            int tint_index = ComponentMenu.SelectedIndex - index_menu_count;
                                                            Function.Call(Hash.SET_PED_WEAPON_TINT_INDEX, Game.Player.Character, weaponHash, tint_index);
                                                            tint_m.AltTitle = "";
                                                            tint_m.RightBadgeSet = shop_gun;
                                                        }
                                                    };
                                                    ComponentMenu.Add(tint_m);
                                                }
                                            }
                                        }

                                        tints_flag = 1;
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

                uint weaponTypeGroup = Function.Call<uint>(Hash.GET_WEAPONTYPE_GROUP, weaponHash);
                switch (weaponTypeGroup)
                {
                    case GROUP_HEAVY:
                        HeavyMenu.Add(weap_m);
                        HeavyMenu_preview.Add(weaponHash);
                        break;
                    case GROUP_MELEE:
                        MeleeMenu.Add(weap_m);
                        MeleeMenu_preview.Add(weaponHash);
                        break;
                    case GROUP_MG:
                        MachineGunsMenu.Add(weap_m);
                        MachineGunsMenu_preview.Add(weaponHash);
                        break;
                    case GROUP_PISTOL:
                        PistolsMenu.Add(weap_m);
                        PistolsMenu_preview.Add(weaponHash);
                        break;
                    case GROUP_RIFLE:
                        RiflesMenu.Add(weap_m);
                        RiflesMenu_preview.Add(weaponHash);
                        break;
                    case GROUP_SHOTGUN:
                        ShotgunsMenu.Add(weap_m);
                        ShotgunsMenu_preview.Add(weaponHash);
                        break;
                    case GROUP_SMG:
                        SMGsMenu.Add(weap_m);
                        SMGsMenu_preview.Add(weaponHash);
                        break;
                    case GROUP_SNIPER:
                        SniperRiflesMenu.Add(weap_m);
                        SniperRiflesMenu_preview.Add(weaponHash);
                        break;
                    case GROUP_STUNGUN:
                        StunGunMenu.Add(weap_m);
                        StunGunMenu_preview.Add(weaponHash);
                        break;
                    case GROUP_THROWN:
                        ThrownMenu.Add(weap_m);
                        ThrownMenu_preview.Add(weaponHash);
                        break;
                }

            }
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
