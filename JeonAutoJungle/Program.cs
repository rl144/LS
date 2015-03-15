using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.IO;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
namespace JeonJunglePlay
{
    public class Program
    {
        public static Obj_AI_Hero Player = ObjectManager.Player;
        public static Spell Q, W, E, R;
        private static Vector3 spawn;
        private static Vector3 enemy_spawn;
        public static Menu JeonAutoJungleMenu;
		public static Orbwalking.Orbwalker Orbwalker; //테스트추가 오브워
		public static Orbwalking.OrbwalkingMode ActiveMode { get; set; }
        public static float gamestart = 0, pastTime = 0, pastTimeAFK, afktime = 0;
        public static List<MonsterINFO> MonsterList = new List<MonsterINFO>();
        public static int now = 1, max = 20, num = 0;
        public static float recallhp = 0;
        public static bool recall = false, IsOVER = false, IsAttackedByTurret = false, IsAttackStart = false,
        IsCastW = false;
        public static bool canBuyItems = true, IsBlueTeam, IsStart = true, IsFind = false;
        public static SpellSlot smiteSlot = SpellSlot.Unknown;
        public static Spell smite;
        public static SpellDataInst Qdata = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q);
        public static SpellDataInst Wdata = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W);
        public static SpellDataInst Edata = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E);
        public static SpellDataInst Rdata = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R);
        public static List<Spell> cast2mob = new List<Spell>();
        public static List<Spell> cast2hero = new List<Spell>();
        public static List<Spell> cast4laneclear = new List<Spell>();	
        public class MonsterINFO
        {
            public Vector3 Position;
            public string ID;
            public string name;
            public int order;
            public int respawntime;
            public int Range = 300;
            public MonsterINFO()
            {
                MonsterList.Add(this);
            }
        }
        public class ItemToShop
        {
            public int Price, index;
            public ItemId item;
            public ItemId needItem;
            public ItemToShop()
            {
                num += 1;
            }
        }
        #region 몬스터
        public static MonsterINFO Baron = new MonsterINFO
        {
            ID = "Baron",
            Position = new Vector3(4910f, 10268f, -71.24f),
            name = "SRU_BaronSpawn",
            respawntime = 420
        };
        public static MonsterINFO Dragon = new MonsterINFO
        {
            ID = "Dragon",
            Position = new Vector3(9836f, 4408f, -71.24f),
            name = "SRU_Dragon",
            respawntime = 360
        };
        public static MonsterINFO top_crab = new MonsterINFO
        {
            ID = "top_crab",
            Position = new Vector3(4266f, 9634f, -67.87f),
            name = "noneuses",
            respawntime = 180,
            Range = 3000
        };
        public static MonsterINFO BLUE_MID = new MonsterINFO
        {
            ID = "blue_MID",
            Position = new Vector3(5294.531f, 5537.924f, 50.46155f),
            name = "noneuses",
            respawntime = 180,
            Range = 3000
        };
        public static MonsterINFO PURPLE_MID = new MonsterINFO
        {
            ID = "purple_MID",
            Position = new Vector3(9443.35f, 9339.06f, 53.30994f),
            name = "noneuses",
            respawntime = 180,
            Range = 3000
        };
        public static MonsterINFO down_crab = new MonsterINFO
        {
            ID = "down_crab",
            Position = new Vector3(10524f, 5116f, -62.81f),
            name = "noneuses",
            respawntime = 180,
            Range = 3000
        };
        public static MonsterINFO bteam_Razorbeak = new MonsterINFO { ID = "bteam_Razorbeak", Position = new Vector3(6974f, 5460f, 54f), name = "SRU_Razorbeak" };
        public static MonsterINFO bteam_Red = new MonsterINFO
        {
            ID = "bteam_Red",
            Position = new Vector3(7796f, 4028f, 54f),
            name = "SRU_Red",
            respawntime = 300
        };
        public static MonsterINFO bteam_Krug = new MonsterINFO { ID = "bteam_Krug", Position = new Vector3(8394f, 2750f, 50f), name = "SRU_Krug" };
        public static MonsterINFO bteam_Blue = new MonsterINFO
        {
            ID = "bteam_Blue",
            Position = new Vector3(3832f, 7996f, 52f),
            name = "SRU_Blue",
            respawntime = 300
        };
        public static MonsterINFO bteam_Gromp = new MonsterINFO { ID = "bteam_Gromp", Position = new Vector3(2112f, 8372f, 51.7f), name = "SRU_Gromp" };
        public static MonsterINFO bteam_Wolf = new MonsterINFO { ID = "bteam_Wolf", Position = new Vector3(3844f, 6474f, 52.46f), name = "SRU_Murkwolf" };
        public static MonsterINFO pteam_Razorbeak = new MonsterINFO { ID = "pteam_Razorbeak", Position = new Vector3(7856f, 9492f, 52.33f), name = "SRU_Razorbeak" };
        public static MonsterINFO pteam_Red = new MonsterINFO
        {
            ID = "pteam_Red",
            Position = new Vector3(7124f, 10856f, 56.34f),
            name = "SRU_Red",
            respawntime = 300
        };
        public static MonsterINFO pteam_Krug = new MonsterINFO { ID = "pteam_Krug", Position = new Vector3(6495f, 12227f, 56.47f), name = "SRU_Krug" };
        public static MonsterINFO pteam_Blue = new MonsterINFO
        {
            ID = "pteam_Blue",
            Position = new Vector3(10850f, 6938f, 51.72f),
            name = "SRU_Blue",
            respawntime = 300
        };
        public static MonsterINFO pteam_Gromp = new MonsterINFO { ID = "pteam_Gromp", Position = new Vector3(12766f, 6464f, 51.66f), name = "SRU_Gromp" };
        public static MonsterINFO pteam_Wolf = new MonsterINFO { ID = "pteam_Wolf", Position = new Vector3(10958f, 8286f, 62.46f), name = "SRU_Murkwolf" };
        #endregion
        #region 아이템
        #region ap
        public static List<ItemToShop> buyThings_AP = new List<ItemToShop>
{
new ItemToShop()
{
Price = 450,
needItem = ItemId.Hunters_Machete,
item = ItemId.Rangers_Trailblazer,
index = 1
},
new ItemToShop()
{
Price = 820,
needItem = ItemId.Rangers_Trailblazer,
item = ItemId.Fiendish_Codex,
index = 2
},
new ItemToShop()
{
Price = 680,
needItem = ItemId.Fiendish_Codex,
item = ItemId.Rangers_Trailblazer_Enchantment_Magus,
index = 3
},
new ItemToShop()
{
Price = 1100,
needItem = ItemId.Rangers_Trailblazer_Enchantment_Magus,
item = ItemId.Sorcerers_Shoes,
index = 4
},
new ItemToShop()
{
Price = 820,
needItem = ItemId.Sorcerers_Shoes,
item = ItemId.Fiendish_Codex,
index = 5
},
new ItemToShop()
{
Price = 600,
needItem = ItemId.Fiendish_Codex,
item = ItemId.Forbidden_Idol,
index = 6
},
new ItemToShop()
{
Price = 880,
needItem = ItemId.Forbidden_Idol,
item = ItemId.Morellonomicon,
index = 7
},
new ItemToShop()
{
Price = 1200,
needItem = ItemId.Morellonomicon,
item = ItemId.Seekers_Armguard,
index = 8
},
new ItemToShop()
{
Price = 1600,
needItem = ItemId.Seekers_Armguard,
item = ItemId.Needlessly_Large_Rod,
index = 9
},
new ItemToShop()
{
Price = 500,
needItem = ItemId.Needlessly_Large_Rod,
item = ItemId.Zhonyas_Hourglass,
index = 10
},
new ItemToShop()
{
Price = 860,
needItem = ItemId.Zhonyas_Hourglass,
item = ItemId.Blasting_Wand,
index = 11
},
new ItemToShop()
{
Price = 1600,
needItem = ItemId.Blasting_Wand,
item = ItemId.Needlessly_Large_Rod,
index = 12
},
new ItemToShop()
{
Price = 840,
needItem = ItemId.Needlessly_Large_Rod,
item = ItemId.Rabadons_Deathcap,
index = 13
},
new ItemToShop()
{
Price = 860,
needItem = ItemId.Rabadons_Deathcap,
item = ItemId.Blasting_Wand,
index = 14
},
new ItemToShop()
{
Price = 1435,
needItem = ItemId.Blasting_Wand,
item = ItemId.Void_Staff,
index = 15
},
new ItemToShop()
{
Price = 2750,
needItem = ItemId.Void_Staff,
item = ItemId.Banshees_Veil,
index = 16
}
};
        #endregion
        #region ad = default
        public static List<ItemToShop> buyThings = new List<ItemToShop>
{
new ItemToShop()
{
Price = 450,
needItem = ItemId.Hunters_Machete,
item = ItemId.Rangers_Trailblazer,
index = 1
},
new ItemToShop()
{
Price = 450,
needItem = ItemId.Rangers_Trailblazer,
item = ItemId.Dagger,
index = 2
},
new ItemToShop()
{
Price = 1050,
needItem = ItemId.Dagger,
item = ItemId.Rangers_Trailblazer_Enchantment_Devourer,
index = 3
},
new ItemToShop()
{
Price = 1000,
needItem = ItemId.Rangers_Trailblazer_Enchantment_Devourer,
item = ItemId.Berserkers_Greaves,
index = 4
},
new ItemToShop()
{
Price = 1400,
needItem = ItemId.Berserkers_Greaves,
item = ItemId.Bilgewater_Cutlass,
index = 5
},
new ItemToShop()
{
Price = 1800,
needItem = ItemId.Bilgewater_Cutlass,
item = ItemId.Blade_of_the_Ruined_King,
index = 6
},
new ItemToShop()
{
Price = 875,
needItem = ItemId.Blade_of_the_Ruined_King,
item = ItemId.Pickaxe,
index = 7
},
new ItemToShop()
{
Price = 1025,
needItem = ItemId.Pickaxe,
item = ItemId.Tiamat_Melee_Only,
index = 8
},
new ItemToShop()
{
Price = 1100,
needItem = ItemId.Tiamat_Melee_Only,
item = ItemId.Zeal,
index = 9
},
new ItemToShop()
{
Price = 1700,
needItem = ItemId.Zeal,
item = ItemId.Phantom_Dancer,
index = 10
},
new ItemToShop()
{
Price = 1550,
needItem = ItemId.Phantom_Dancer,
item = ItemId.B_F_Sword,
index = 11
},
new ItemToShop()
{
Price = 2250,
needItem = ItemId.B_F_Sword,
item = ItemId.Infinity_Edge,
index = 12
},
new ItemToShop()
{
Price = 2900,
needItem = ItemId.Infinity_Edge,
item = ItemId.Last_Whisper,
index = 13
}
};
        #endregion
        #region as
        public static List<ItemToShop> buyThings_AS = new List<ItemToShop>
{
new ItemToShop()
{
Price = 450,
needItem = ItemId.Hunters_Machete,
item = ItemId.Rangers_Trailblazer,
index = 1
},
new ItemToShop()
{
Price = 450,
needItem = ItemId.Rangers_Trailblazer,
item = ItemId.Dagger,
index = 2
},
new ItemToShop()
{
Price = 1050,
needItem = ItemId.Dagger,
item = ItemId.Rangers_Trailblazer_Enchantment_Devourer,
index = 3
},
new ItemToShop()
{
Price = 1000,
needItem = ItemId.Rangers_Trailblazer_Enchantment_Devourer,
/*item = ItemId.Boots_of_Speed,
index = 4
},
new ItemToShop()
{
Price = 675,
needItem = ItemId.Boots_of_Speed,
*/item = ItemId.Boots_of_Swiftness,
index = 4
},
new ItemToShop()
{
Price = 1400,
needItem = ItemId.Boots_of_Swiftness,
item = ItemId.Bilgewater_Cutlass,
index = 5
},
new ItemToShop()
{
Price = 1800,
needItem = ItemId.Bilgewater_Cutlass,
item = ItemId.Blade_of_the_Ruined_King,
index = 6
},
new ItemToShop()
{
Price = 900,
needItem = ItemId.Blade_of_the_Ruined_King,
item = ItemId.Recurve_Bow,
index = 7
},
new ItemToShop()
{
Price = 500+750+450,
needItem = ItemId.Recurve_Bow,
item = ItemId.Wits_End,
index = 8
},
new ItemToShop()
{
Price = 1900,
needItem = ItemId.Wits_End,
item = ItemId.Tiamat_Melee_Only,
index = 9
},
new ItemToShop()
{
Price = 800+600,
needItem = ItemId.Tiamat_Melee_Only,
item = ItemId.Ravenous_Hydra_Melee_Only,
index = 10
},
new ItemToShop()
{
Price = 2900,
needItem = ItemId.Ravenous_Hydra_Melee_Only,
item = ItemId.Last_Whisper,
index = 11
}
};
        #endregion
        #region tanky
        public static List<ItemToShop> buyThings_TANK = new List<ItemToShop>
{
new ItemToShop()
{
Price = 450,
needItem = ItemId.Hunters_Machete,
item = ItemId.Rangers_Trailblazer,
index = 1
},
new ItemToShop()
{
Price = 450,
needItem = ItemId.Rangers_Trailblazer,
item = ItemId.Dagger,
index = 2
},
new ItemToShop()
{
Price = 1050,
needItem = ItemId.Dagger,
item = ItemId.Rangers_Trailblazer_Enchantment_Devourer,
index = 3
},
new ItemToShop()
{
Price = 1000,
needItem = ItemId.Rangers_Trailblazer_Enchantment_Devourer,
item = ItemId.Ionian_Boots_of_Lucidity,
index = 4
},
new ItemToShop()
{
Price = 500,
needItem = ItemId.Ionian_Boots_of_Lucidity,
item = ItemId.Null_Magic_Mantle,
index = 5
},
new ItemToShop()
{
Price = 900,
needItem = ItemId.Null_Magic_Mantle,
item = ItemId.Recurve_Bow,
index = 6
},
new ItemToShop()
{
Price = 1200,
needItem = ItemId.Recurve_Bow,
item = ItemId.Wits_End,
index = 7
},
new ItemToShop()
{
Price = 950,
needItem = ItemId.Wits_End,
item = ItemId.Glacial_Shroud,
index = 8
},
new ItemToShop()
{
Price = 1050+450,
needItem = ItemId.Glacial_Shroud,
item = ItemId.Frozen_Heart,
index = 9
},
new ItemToShop()
{
Price = 500,
needItem = ItemId.Frozen_Heart,
item = ItemId.Null_Magic_Mantle,
index = 10
},
new ItemToShop()
{
Price = 400+1150,
needItem = ItemId.Null_Magic_Mantle,
item = ItemId.Banshees_Veil,
index = 11
},
new ItemToShop()
{
Price = 1200,
needItem = ItemId.Banshees_Veil,
item = ItemId.Sheen,
index = 12
},
new ItemToShop()
{
Price = 1325,
needItem = ItemId.Sheen,
item = ItemId.Phage,
index = 13
},
new ItemToShop()
{
Price = 1178,
needItem = ItemId.Phage,
item = ItemId.Trinity_Force,
index = 14
},
};
        #endregion
        #endregion
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }
        private static void Game_OnGameLoad(EventArgs args)
        {
            ////////////////////customizing//////////////////
            var dir = new DirectoryInfo(Config.LeagueSharpDirectory.ToString() + @"\JeonAutoJungle");
            var setFile = new FileInfo(dir + "/" + Player.ChampionName + ".ini");
            #region File Stream
            try
            {
                if (!dir.Exists)
                    dir.Create();
                if (!setFile.Exists)
                {
                    Readini.Setini(setFile.FullName);
                }
            }
            catch
            { }
            #endregion
            ////////////////////////////////////////////////
            JeonAutoJungleMenu = new Menu("JeonAutoJungle", "JeonAutoJungle", true);
            JeonAutoJungleMenu.AddItem(new MenuItem("isActive", "Activate")).SetValue(true);
            JeonAutoJungleMenu.AddItem(new MenuItem("maxstacks", "Max Stacks").SetValue(new Slider(9, 1, 150)));
            JeonAutoJungleMenu.AddItem(new MenuItem("maxlv", "Max level").SetValue(new Slider(9, 1, 18)));
            JeonAutoJungleMenu.AddItem(new MenuItem("autorecallheal", "Recall[for heal]")).SetValue(true);
            JeonAutoJungleMenu.AddItem(new MenuItem("hpper", "Recall on HP(%)").SetValue(new Slider(50, 0, 100)));
            JeonAutoJungleMenu.AddItem(new MenuItem("autorecallitem", "Recall[for item]")).SetValue(true);
            JeonAutoJungleMenu.AddItem(new MenuItem("evading", "Detect TurretAttack")).SetValue(true);
            JeonAutoJungleMenu.AddItem(new MenuItem("Invade", "InvadeEnemyJungle?")).SetValue(true);
            JeonAutoJungleMenu.AddItem(new MenuItem("k_dragon", "Add Dragon to Route on Lv").SetValue(new Slider(10, 1, 18)));
            if (Player.ChampionName == "MasterYi")
                JeonAutoJungleMenu.AddItem(new MenuItem("yi_W", "Cast MasterYi-W(%)").SetValue(new Slider(60, 0, 100)));
            Orbwalker = new Orbwalking.Orbwalker(JeonAutoJungleMenu.AddSubMenu(new Menu(Player.ChampionName + ": Orbwalker", "Orbwalker")));
            TargetSelector.AddToMenu(JeonAutoJungleMenu.AddSubMenu(new Menu(Player.ChampionName + ": Target Selector", "Target Selector")));
				JeonAutoJungleMenu.AddToMainMenu();
				//메뉴
				setSmiteSlot();
            #region 스펠설정
            Q = new Spell(SpellSlot.Q, GetSpellRange(Qdata));
            W = new Spell(SpellSlot.W, GetSpellRange(Wdata));
            E = new Spell(SpellSlot.E, GetSpellRange(Edata));
            R = new Spell(SpellSlot.R, GetSpellRange(Rdata));
            #endregion
            #region 지점 설정
            if (Player.Team.ToString() == "Chaos")
            {
                spawn = new Vector3(14318f, 14354, 171.97f);
                enemy_spawn = new Vector3(415.33f, 453.38f, 182.66f);
                Game.PrintChat("Set PurpleTeam Spawn");
                IsBlueTeam = false;
                MonsterList.First(temp => temp.ID == pteam_Krug.ID).order = 1;
                MonsterList.First(temp => temp.ID == pteam_Red.ID).order = 2;
                MonsterList.First(temp => temp.ID == pteam_Razorbeak.ID).order = 3;
                MonsterList.First(temp => temp.ID == bteam_Gromp.ID).order = 4;
                MonsterList.First(temp => temp.ID == bteam_Blue.ID).order = 5;
                MonsterList.First(temp => temp.ID == bteam_Wolf.ID).order = 6;
                MonsterList.First(temp => temp.ID == top_crab.ID).order = 7;
                MonsterList.First(temp => temp.ID == PURPLE_MID.ID).order = 8;
                MonsterList.First(temp => temp.ID == pteam_Wolf.ID).order = 9;
                MonsterList.First(temp => temp.ID == pteam_Blue.ID).order = 10;
                MonsterList.First(temp => temp.ID == pteam_Gromp.ID).order = 11;
                MonsterList.First(temp => temp.ID == bteam_Razorbeak.ID).order = 12;
                MonsterList.First(temp => temp.ID == bteam_Red.ID).order = 13;
                MonsterList.First(temp => temp.ID == bteam_Krug.ID).order = 14;
                MonsterList.First(temp => temp.ID == down_crab.ID).order = 15;
            }
            else
            {
                spawn = new Vector3(415.33f, 453.38f, 182.66f);
                enemy_spawn = new Vector3(14318f, 14354, 171.97f);
                Game.PrintChat("Set BlueTeam Spawn");
                IsBlueTeam = true;
                MonsterList.First(temp => temp.ID == bteam_Gromp.ID).order = 1;
                MonsterList.First(temp => temp.ID == bteam_Blue.ID).order = 2;
                MonsterList.First(temp => temp.ID == bteam_Wolf.ID).order = 3;
                MonsterList.First(temp => temp.ID == bteam_Razorbeak.ID).order = 8;
                MonsterList.First(temp => temp.ID == bteam_Red.ID).order = 9;
                MonsterList.First(temp => temp.ID == bteam_Krug.ID).order = 10;
                MonsterList.First(temp => temp.ID == pteam_Razorbeak.ID).order = 6;
                MonsterList.First(temp => temp.ID == pteam_Red.ID).order = 5;
                MonsterList.First(temp => temp.ID == pteam_Krug.ID).order = 4;
                MonsterList.First(temp => temp.ID == top_crab.ID).order = 7;
                MonsterList.First(temp => temp.ID == BLUE_MID.ID).order = 15;
                MonsterList.First(temp => temp.ID == down_crab.ID).order = 14;
                MonsterList.First(temp => temp.ID == pteam_Gromp.ID).order = 11;
                MonsterList.First(temp => temp.ID == pteam_Blue.ID).order = 12;
                MonsterList.First(temp => temp.ID == pteam_Wolf.ID).order = 13;
            }
            max = MonsterList.OrderByDescending(h => h.order).First().order;
            #endregion
            #region 챔피언 설정
            if (Player.ChampionName.ToUpper() == "NUNU")
            {
                GetItemTree(setFile);
                Game.PrintChat("NUNU BOT ACTIVE");
                Readini.GetSpelltree(setFile.FullName);
            }
            else if (Player.ChampionName.ToUpper() == "WARWICK")
            {
                GetItemTree(setFile);
                Game.PrintChat("WARWICK BOT ACTIVE");
                Readini.GetSpelltree(setFile.FullName);
            }
            else if (Player.ChampionName.ToUpper() == "MASTERYI")
            {
                GetItemTree(setFile);
                Game.PrintChat("MASTER YI BOT ACTIVE");
                Readini.GetSpelltree(setFile.FullName);
            }
            else if (Player.ChampionName.ToUpper() == "CHOGATH")
            {
                GetItemTree(setFile);
                Game.PrintChat("CHOGATH BOT ACTIVE");
                Readini.GetSpelltree(setFile.FullName);
            }
            else if (Player.ChampionName.ToUpper() == "MAOKAI")
            {
                GetItemTree(setFile);
                Game.PrintChat("MAOKAI BOT ACTIVE");
                Readini.GetSpelltree(setFile.FullName);
            }
            else if (Player.ChampionName.ToUpper() == "NASUS")
            {
                GetItemTree(setFile);
                Game.PrintChat("NASUS BOT ACTIVE");
                Readini.GetSpelltree(setFile.FullName);
            }
            else
            {
                #region Read ini
                Game.PrintChat("Read ini file");
                Readini.GetSpelltree(setFile.FullName);
                GetItemTree(setFile);
                Readini.GetSpells(setFile.FullName, ref cast2mob, ref cast2hero, ref cast4laneclear);
                #endregion readini
            }
            #endregion
            #region 현재 아이템 단계 설정 - 도중 리로드시 필요
            if (buyThings.Any(h => Items.HasItem(Convert.ToInt32(h.needItem))))
            {
                if (buyThings.First().needItem != buyThings.Last(h => Items.HasItem(Convert.ToInt32(h.needItem))).needItem)
                {
                    var lastitem = buyThings.Last(h => Items.HasItem(Convert.ToInt32(h.needItem)));
                    Game.PrintChat("Find new ItemList");
                    List<ItemToShop> newlist = buyThings.Where(t => t.index >= lastitem.index).ToList();
                    buyThings.Clear();
                    buyThings = newlist;
                }
            }
            #endregion
            gamestart = Game.Time; // 시작시간 설정
            Game.OnUpdate += Game_OnUpdate;
            GameObject.OnCreate += OnCreate;
            Obj_AI_Base.OnProcessSpellCast += OnSpell;
            if (smiteSlot == SpellSlot.Unknown)
                Game.PrintChat("YOU ARE NOT JUNGLER(NO SMITE)");
        }
        private static void Game_OnUpdate(EventArgs args) 
        {
		int maxlv = JeonAutoJungleMenu.Item("maxlv").GetValue<Slider>().Value;
		int level = Player.Level;	
            setSmiteSlot();
            if (Player.Spellbook.IsChanneling)
                return;
            if (!JeonAutoJungleMenu.Item("isActive").GetValue<Boolean>() || smiteSlot == SpellSlot.Unknown)
                return;
            #region detect afk
            if (Game.Time - pastTimeAFK >= 1 && !Player.IsDead && !Player.IsRecalling())
            {
                afktime += 1;
                if (afktime > 10) // 잠수 10초 경과
                {
                    if (Player.InShop())
                        Player.IssueOrder(GameObjectOrder.AttackTo, enemy_spawn);
                    else
                        Player.Spellbook.CastSpell(SpellSlot.Recall);
                    afktime = 0;
                }
                pastTimeAFK = Game.Time;
            }
            #endregion
            #region 0.5초마다 발동 // 오류 없애줌
            if (Environment.TickCount - pastTime <= 500) return;
            pastTime = Environment.TickCount;
            #endregion
            #region InvadeEnemyJungle
            if (!IsBlueTeam)
            {
                if (!JeonAutoJungleMenu.Item("Invade").GetValue<Boolean>())
                {
                    MonsterList.First(temp => temp.ID == bteam_Gromp.ID).order = 0;
                    MonsterList.First(temp => temp.ID == bteam_Blue.ID).order = 0;
                    MonsterList.First(temp => temp.ID == bteam_Wolf.ID).order = 0;
                    MonsterList.First(temp => temp.ID == top_crab.ID).order = 0;
                    MonsterList.First(temp => temp.ID == PURPLE_MID.ID).order = 0;
                    MonsterList.First(temp => temp.ID == down_crab.ID).order = 0;
                    MonsterList.First(temp => temp.ID == bteam_Razorbeak.ID).order = 0;
                    MonsterList.First(temp => temp.ID == bteam_Red.ID).order = 0;
                    MonsterList.First(temp => temp.ID == bteam_Krug.ID).order = 0;
					MonsterList.First(temp => temp.ID == pteam_Krug.ID).order = 1;
					MonsterList.First(temp => temp.ID == pteam_Red.ID).order = 2;
					MonsterList.First(temp => temp.ID == pteam_Razorbeak.ID).order = 3;
					MonsterList.First(temp => temp.ID == pteam_Wolf.ID).order = 4;
					MonsterList.First(temp => temp.ID == pteam_Blue.ID).order = 5;
					MonsterList.First(temp => temp.ID == pteam_Gromp.ID).order = 6;
                }
                else
                {
					MonsterList.First(temp => temp.ID == pteam_Krug.ID).order = 1;
					MonsterList.First(temp => temp.ID == pteam_Red.ID).order = 2;
					MonsterList.First(temp => temp.ID == pteam_Razorbeak.ID).order = 3;
					MonsterList.First(temp => temp.ID == bteam_Gromp.ID).order = 4;
					MonsterList.First(temp => temp.ID == bteam_Blue.ID).order = 5;
					MonsterList.First(temp => temp.ID == bteam_Wolf.ID).order = 6;
					MonsterList.First(temp => temp.ID == top_crab.ID).order = 7;
					MonsterList.First(temp => temp.ID == PURPLE_MID.ID).order = 8;
					MonsterList.First(temp => temp.ID == pteam_Wolf.ID).order = 9;
					MonsterList.First(temp => temp.ID == pteam_Blue.ID).order = 10;
					MonsterList.First(temp => temp.ID == pteam_Gromp.ID).order = 11;
					MonsterList.First(temp => temp.ID == bteam_Razorbeak.ID).order = 12;
					MonsterList.First(temp => temp.ID == bteam_Red.ID).order = 13;
					MonsterList.First(temp => temp.ID == bteam_Krug.ID).order = 14;
					MonsterList.First(temp => temp.ID == down_crab.ID).order = 15;
                }
            }
            else
            {
                if (!JeonAutoJungleMenu.Item("Invade").GetValue<Boolean>())
                {
                    MonsterList.First(temp => temp.ID == pteam_Razorbeak.ID).order = 0;
                    MonsterList.First(temp => temp.ID == pteam_Red.ID).order = 0;
                    MonsterList.First(temp => temp.ID == pteam_Krug.ID).order = 0;
                    MonsterList.First(temp => temp.ID == top_crab.ID).order = 0;
                    MonsterList.First(temp => temp.ID == BLUE_MID.ID).order = 0;
                    MonsterList.First(temp => temp.ID == down_crab.ID).order = 0;
                    MonsterList.First(temp => temp.ID == pteam_Gromp.ID).order = 0;
                    MonsterList.First(temp => temp.ID == pteam_Blue.ID).order = 0;
                    MonsterList.First(temp => temp.ID == pteam_Wolf.ID).order = 0;
					MonsterList.First(temp => temp.ID == bteam_Gromp.ID).order = 1;
					MonsterList.First(temp => temp.ID == bteam_Blue.ID).order = 2;
					MonsterList.First(temp => temp.ID == bteam_Wolf.ID).order = 3;
					MonsterList.First(temp => temp.ID == bteam_Razorbeak.ID).order = 4;
					MonsterList.First(temp => temp.ID == bteam_Red.ID).order = 5;
					MonsterList.First(temp => temp.ID == bteam_Krug.ID).order = 6;
                }
                else
                {
					MonsterList.First(temp => temp.ID == bteam_Gromp.ID).order = 1;
					MonsterList.First(temp => temp.ID == bteam_Blue.ID).order = 2;
					MonsterList.First(temp => temp.ID == bteam_Wolf.ID).order = 3;
					MonsterList.First(temp => temp.ID == bteam_Razorbeak.ID).order = 8;
					MonsterList.First(temp => temp.ID == bteam_Red.ID).order = 9;
					MonsterList.First(temp => temp.ID == bteam_Krug.ID).order = 10;
					MonsterList.First(temp => temp.ID == pteam_Razorbeak.ID).order = 6;
					MonsterList.First(temp => temp.ID == pteam_Red.ID).order = 5;
					MonsterList.First(temp => temp.ID == pteam_Krug.ID).order = 4;
					MonsterList.First(temp => temp.ID == top_crab.ID).order = 7;
					MonsterList.First(temp => temp.ID == BLUE_MID.ID).order = 15;
					MonsterList.First(temp => temp.ID == down_crab.ID).order = 14;
					MonsterList.First(temp => temp.ID == pteam_Gromp.ID).order = 11;
					MonsterList.First(temp => temp.ID == pteam_Blue.ID).order = 12;
					MonsterList.First(temp => temp.ID == pteam_Wolf.ID).order = 13;
                }
            }
            max = MonsterList.OrderByDescending(h => h.order).First().order;
            #endregion
            #region detect reload
            if (IsStart && Player.Level > 1)
            {
                Game.PrintChat("You did reload");
                IsStart = false;
            }
            #endregion
            #region check somethings about dragon
            if (Player.Level > JeonAutoJungleMenu.Item("k_dragon").GetValue<Slider>().Value)
            {
                if (MonsterList.First(temp => temp.ID == down_crab.ID).order == 12)
                {
                    MonsterList.First(temp => temp.ID == down_crab.ID).order = 0;
                    MonsterList.First(temp => temp.ID == Dragon.ID).order = 12;
                }
            }
            #endregion
            #region 오토 플레이 - auto play
            if (Player.IsMoving)
                afktime = 0;
            if (!IsOVER)
            {
                if (IsStart) // start
                {
                    if (Game.Time - gamestart >= 0)
                    {
                        Player.IssueOrder(GameObjectOrder.MoveTo, MonsterList.First(t => t.order == 1).Position);
                        afktime = 0;
                    }
                    if (Player.Distance(MonsterList.First(t => t.order == 1).Position) <= 100)
                    {
                        if (CheckMonster(MonsterList.First(t => t.order == 1).name, MonsterList.First(t => t.order == 1).Position, MonsterList.First(t => t.order == 1).Range))
                        {
                            IsStart = false;
                            now = 1;
                            Game.PrintChat("START!");
                        }
                    }
                }
                else
                {
                    if (Player.IsDead && now >= 7 && now <= 9)
                        now = 5;
                    if (Player.IsDead && now > 12)
                        now = 12;
                    MonsterINFO target = MonsterList.First(t => t.order == now);
                    if (Player.Position.Distance(target.Position) >= 400)
                    {
                        if (!recall)
                        {
                            if (Player.Position.Distance(target.Position) > Player.AttackRange)
                            {
                                Player.IssueOrder(GameObjectOrder.MoveTo, target.Position);
                                afktime = 0;
                            }
                            DoCast_Hero();
                            if (Player.HealthPercentage() < JeonAutoJungleMenu.Item("hpper").GetValue<Slider>().Value && !Player.IsDead//hpper
                            && JeonAutoJungleMenu.Item("autorecallheal").GetValue<Boolean>()) // HP LESS THAN 25%
                            {
                                Game.PrintChat("YOUR HP IS SO LOW. RECALL!");
                                Player.Spellbook.CastSpell(SpellSlot.Recall);
                                recall = true;
                                recallhp = Player.Health;
                            }
                            else if (Player.Gold > buyThings.First().Price
                            && JeonAutoJungleMenu.Item("autorecallitem").GetValue<Boolean>()
                            && Player.InventoryItems.Length < 9) // HP LESS THAN 25%
                            {
                                Game.PrintChat("CAN BUY " + buyThings.First().item.ToString() + ". RECALL!");
                                Player.Spellbook.CastSpell(SpellSlot.Recall);
                                recall = true;
                                recallhp = Player.Health;
                            }
                        }
                    }
                    else
                    {
                        if (CheckMonster(target.name, target.Position, 500)) //해당지점에 몬스터가 있는지
                        {
                            DoCast();
                            Player.IssueOrder(GameObjectOrder.AttackUnit, GetNearest(Player.Position));
                            afktime = 0;
                            if (smite.Slot != SpellSlot.Unknown && smite.IsReady())
                                DoSmite();
                        }
                        else
                        {
                            now += 1;
                            if (now > max)
                                now = 1;
                        }
                    }
                }
                if (Player.InShop())
                    recall = false;

				if (level >= maxlv || Items.HasItem(Convert.ToInt32(ItemId.Sorcerers_Shoes)))
				{
                    IsOVER = true;
                    Game.PrintChat("LEVEL " + maxlv + ". Now Going to be offense.");
				}
            }
			else
			{
				if (level < maxlv && !Items.HasItem(Convert.ToInt32(ItemId.Sorcerers_Shoes)))
				{
                    Game.PrintChat("Level " + maxlv + ". Going back to farm.");
                    IsOVER = false;
                    IsAttackStart = false;
				}
            }
			#endregion
            #region 스택이 넘는지 체크 - check ur stacks
            foreach (var buff in Player.Buffs.Where(b => b.DisplayName == "Enchantment_Slayer_Stacks"))
            {
                int maxstacks = JeonAutoJungleMenu.Item("maxstacks").GetValue<Slider>().Value;
                if (buff.Count >= maxstacks && !IsOVER || level >= maxlv || Items.HasItem(Convert.ToInt32(ItemId.Rangers_Trailblazer_Enchantment_Magus))) //--테스트
                {
                    IsOVER = true;
                    Game.PrintChat("Stacks Over " + maxstacks + ". Now Going to be offense.");
                }
                if (buff.Count < maxstacks && IsOVER && level < maxlv && !Items.HasItem(Convert.ToInt32(ItemId.Rangers_Trailblazer_Enchantment_Magus))) //-- I don't speak korean :D
                {
                    Game.PrintChat("Stacks under " + maxstacks + ". Going back to farm.");
                    IsOVER = false;
                    IsAttackStart = false;
                }
            }

			
            #endregion
            #region 공격 모드 - offensive mode
            if (IsOVER)
            {
                if (!IsAttackStart)
                {
                    if (!ObjectManager.Get<Obj_AI_Turret>().Any(t => t.Name == "Turret_T2_C_05_A") && IsBlueTeam)
                        IsAttackStart = true;
                    else if (!ObjectManager.Get<Obj_AI_Turret>().Any(t => t.Name == "Turret_T1_C_05_A") && !IsBlueTeam)
                        IsAttackStart = true;
                    else
                    {
                        if (IsBlueTeam)
                        {
                            Player.IssueOrder(GameObjectOrder.MoveTo, BLUE_MID.Position);
                            if (Player.Distance(BLUE_MID.Position) <= 100)
                                IsAttackStart = true;
                        }
                        else
                        {
                            Player.IssueOrder(GameObjectOrder.MoveTo, PURPLE_MID.Position);
                            if (Player.Distance(PURPLE_MID.Position) <= 100)
                                IsAttackStart = true;
                        }
                    }
                }
                else
                {
                    var turret = ObjectManager.Get<Obj_AI_Turret>().OrderBy(t => t.Distance(Player.Position)).First(t => t.IsEnemy);
                    if (IsOVER && !IsAttackedByTurret)
                    {
                        DoCast_Hero();
                        DoLaneClear();
                        if (turret.Distance(Player.Position) > 1200)
                        {
                            Player.IssueOrder(GameObjectOrder.AttackTo, enemy_spawn);
                        }
                            
                        else if (GetMinions(turret) > 2)
                        {
                            Player.IssueOrder(GameObjectOrder.AttackTo, enemy_spawn);
                        }

                        else
                        {
                            Player.IssueOrder(GameObjectOrder.MoveTo, Player.Position.Extend(spawn, 855));
                        }
                            
                        afktime = 0;
                    }
                    if (turret.Distance(Player.Position) > 800)
                        IsAttackedByTurret = false;
                    if (Player.IsDead)
                        IsAttackedByTurret = false;
                }
            }
            #endregion
            #region 상점이용가능할때 // when you are in shop range or dead
            #region 시작아이템 사기 // startup
            if (Utility.InShop(Player) || Player.IsDead)
            {
                if (!(Items.HasItem(Convert.ToInt32(ItemId.Hunters_Machete)) ||
                Items.HasItem(Convert.ToInt32(ItemId.Rangers_Trailblazer)) ||
                Items.HasItem(Convert.ToInt32(ItemId.Rangers_Trailblazer_Enchantment_Devourer)) ||
				Items.HasItem(Convert.ToInt32(ItemId.Rangers_Trailblazer_Enchantment_Magus))))
                {
                    if (smiteSlot != SpellSlot.Unknown)
                    {
                        Player.BuyItem(ItemId.Hunters_Machete);
                        Player.BuyItem(ItemId.Warding_Totem_Trinket);
                    }
                }
            #endregion
                //Game.PrintChat("Gold:" + Player.Gold);
                //Game.PrintChat("NeedItem:" + buyThings.First().needItem.ToString());
                //Game.PrintChat("BuyItem:" + buyThings.First().item.ToString());
                #region 아이템트리 올리기 // item build up
                if (buyThings.Any(t => t.item != ItemId.Unknown))
                {
                    if (Items.HasItem(Convert.ToInt32(buyThings.First().needItem)))
                    {
                        if (Player.Gold > buyThings.First().Price)
                        {
                            Player.BuyItem(buyThings.First().item);
                            buyThings.Remove(buyThings.First());
                        }
                    }
                }
                #endregion
                #region 포션 구매 - buy potions
                if (Player.Gold > 35f && !IsOVER && !Player.InventoryItems.Any(t => t.Id == ItemId.Health_Potion) && Player.Level <= 6)
                    Player.BuyItem(ItemId.Health_Potion);
                if (Player.InventoryItems.Any(t => t.Id == ItemId.Health_Potion))
                {
                    if (Player.InventoryItems.First(t => t.Id == ItemId.Health_Potion).Stacks <= 2 && Player.Level <= 6)
                        Player.BuyItem(ItemId.Health_Potion);
                    if (Player.Level > 6)
                        Player.SellItem(Player.InventoryItems.First(t => t.Id == ItemId.Health_Potion).Slot);
                }
                if (Player.Level > 6 && Items.HasItem(2010))
                    Player.SellItem(Player.InventoryItems.First(t => Convert.ToInt32(t.Id) == 2010).Slot);
                #endregion
            }
            #endregion
            #region 자동포션사용 - auto use potions
            if (Player.HealthPercentage() <= 60 && !Player.InShop())
            {
                ItemId item = ItemId.Health_Potion;
                if (Player.InventoryItems.Any(t => Convert.ToInt32(t.Id) == 2010))
                    item = ItemId.Unknown;
                if (Player.InventoryItems.Any(t => (t.Id == ItemId.Health_Potion || Convert.ToInt32(t.Id) == 2010)))
                {
                    if (!Player.HasBuff("ItemMiniRegenPotion") && item == ItemId.Unknown)
                        Player.Spellbook.CastSpell(Player.InventoryItems.First(t => Convert.ToInt32(t.Id) == 2010).SpellSlot);
                    if (!Player.HasBuff("Health Potion") && item == ItemId.Health_Potion)
                        Player.Spellbook.CastSpell(Player.InventoryItems.First(t => t.Id == ItemId.Health_Potion).SpellSlot);
                }
            }
            #endregion
            AutoLevel.Enabled(true);
        }
        private static void OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.IsValid<Obj_SpellMissile>())
            {
                var m = (Obj_SpellMissile)sender;
                if (m.SpellCaster.IsValid<Obj_AI_Turret>() && m.SpellCaster.IsEnemy &&
                m.Target.IsValid<Obj_AI_Hero>() && m.Target.IsMe && JeonAutoJungleMenu.Item("evading").GetValue<Boolean>())
                {
                    Player.IssueOrder(GameObjectOrder.MoveTo, spawn);
                    Game.PrintChat("OOPS YOU ARE ATTACKED BY TURRET!");
                    Player.IssueOrder(GameObjectOrder.MoveTo, Player.Position.Extend(spawn, 855));
                    IsAttackedByTurret = true;
                }
            }
        }
        private static void OnSpell(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs spell)
        {
            if (spell.Target.IsValid<Obj_AI_Hero>())
            {
                if (spell.Target.IsMe && sender.IsEnemy)
                {
                    string[] turrest =
{
"Turret_T2_C_01_A",
"Turret_T2_C_02_A",
"Turret_T2_L_01_A",
"Turret_T2_C_03_A",
"Turret_T2_R_01_A",
"Turret_T1_C_01_A",
"Turret_T1_C_02_A",
"Turret_T1_C_06_A",
"Turret_T1_C_03_A",
"Turret_T1_C_07_A"
};
                    if (turrest.Contains(sender.Name) && JeonAutoJungleMenu.Item("evading").GetValue<Boolean>())
                    {
                        Player.IssueOrder(GameObjectOrder.MoveTo, spawn);
                        Game.PrintChat("OOPS YOU ARE ATTACKED BY INHIBIT TURRET!");
                        Player.IssueOrder(GameObjectOrder.MoveTo, Player.Position.Extend(spawn, 855));
                        IsAttackedByTurret = true;
                    }
                }
            }
        }
        #region getminions around turret
        public static int GetMinions(Obj_AI_Turret Turret)
        {
            int i = 0;
            foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(t => t.Name.Contains("Minion") && t.Distance(Turret.Position) <= 855 && !t.IsEnemy))
            {
                i++;
            }
            return i;
        }
        #endregion
        #region spell methods
        public static void DoSmite()
        {
            var mob1 = GetNearest_big(Player.Position);
            if (mob1.IsValid)
                smite.CastOnUnit(mob1);
        }
        public static void DoLaneClear()
        {
            var mob1 = ObjectManager.Get<Obj_AI_Minion>().OrderBy(t => Player.Distance(t.Position)).First(t => t.IsEnemy & !t.IsDead);
            if (Player.ChampionName.ToUpper() == "NUNU" && Q.IsReady()) // 누누 Q버그수정 - Fix nunu Q bug
				Player.IssueOrder(GameObjectOrder.MoveTo, mob1.ServerPosition.Extend(Player.ServerPosition, 10));
            if (!ObjectManager.Get<Obj_AI_Hero>().Any(t => t.IsEnemy & !t.IsDead && Player.Distance(t.Position) <= 700) && ObjectManager.Get<Obj_AI_Minion>().Any(t => t.IsMinion && Player.Distance(t.Position) <= 700))
                castspell_laneclear(mob1);
        }
        public static void DoCast()
        {
            var mob1 = ObjectManager.Get<Obj_AI_Minion>().OrderBy(t => Player.Distance(t.Position)).First(t => t.IsEnemy & !t.IsDead);
            if (Player.ChampionName.ToUpper() == "NUNU" && Q.IsReady()) // 누누 Q버그수정 - Fix nunu Q bug
				Player.IssueOrder(GameObjectOrder.MoveTo, mob1.ServerPosition.Extend(Player.ServerPosition, 10));
            if (!ObjectManager.Get<Obj_AI_Hero>().Any(t => t.IsEnemy & !t.IsDead && Player.Distance(t.Position) <= 700) && ObjectManager.Get<Obj_AI_Minion>().Any(t => !t.IsMinion && Player.Distance(t.Position) <= 700))
                castspell(mob1);
        }
        public static void DoCast_Hero()
        {
            if (ObjectManager.Get<Obj_AI_Hero>().Any(t => t.IsEnemy & !t.IsDead && Player.Distance(t.Position) <= 700))
            {
                var target = ObjectManager.Get<Obj_AI_Hero>().OrderBy(t => t.Distance(Player.Position)).
                Where(target => target.IsEnemy && !target.IsMe && !target.IsDead).First(); // 플레이어와 가장 가까운타겟
				Player.IssueOrder(GameObjectOrder.MoveTo, target.ServerPosition.Extend(Player.ServerPosition, 50));
                var turret = ObjectManager.Get<Obj_AI_Turret>().OrderBy(t => t.Distance(target.Position)).
                Where(turret => turret.IsEnemy && !turret.IsDead).First(); // 타겟과 가장 가까운터렛
                if (turret.Distance(target.Position) > 755) // 터렛 사정거리 밖에있어야만 공격함.
                    castspell_hero(target);
            }
        }
        public static void castspell(Obj_AI_Base mob1)
        {
            if (Player.ChampionName.ToUpper() == "NUNU")
            {
                if (Q.IsReady())
                    Q.CastOnUnit(mob1);
                if (E.IsReady())
                    E.CastOnUnit(mob1);
                if (W.IsReady())
                    W.Cast();
            }
            else if (Player.ChampionName.ToUpper() == "CHOGATH")
            {
                if (Q.IsReady())
                    Q.Cast(mob1.Position);
                if (W.IsReady())
                    W.Cast(mob1.Position);
                if (R.IsReady() && R.GetDamage(mob1) >= mob1.Health)
                    R.CastOnUnit(mob1);
            }
            else if (Player.ChampionName.ToUpper() == "WARWICK")
            {
                if (Q.IsReady())
                    Q.CastOnUnit(mob1);
                if (W.IsReady())
                    W.Cast();
                if (R.IsReady())
                    R.CastOnUnit(mob1);
            }
            else if (Player.ChampionName.ToUpper() == "MASTERYI")
            {
                if (Q.IsReady())
                    Q.CastOnUnit(mob1);
                if (W.IsReady() && Player.HealthPercentage() < JeonAutoJungleMenu.Item("yi_W").GetValue<Slider>().Value)
                    W.Cast();
                if (E.IsReady())
                    E.Cast();
                if (R.IsReady())
                    R.Cast();
            }
            else if (Player.ChampionName.ToUpper() == "MAOKAI")
            {
                if (Q.IsReady())
                    Q.Cast(mob1.Position);
                if (E.IsReady())
                    E.Cast(mob1.Position);
                if (W.IsReady())
                    W.CastOnUnit(mob1);
            }
            else if (Player.ChampionName.ToUpper() == "NASUS")
            {
                if (Q.IsReady() && CheckNasusQDamage(mob1))
                    Q.Cast();
                if (W.IsReady() && mob1.IsValid<Obj_AI_Hero>())
                    W.CastOnUnit(mob1);
                if (E.IsReady())
                    E.Cast(mob1.Position);
            }
            else
            {
                foreach (var spell in cast2mob)
                {
                    if (spell.IsReady())
                        spell.CastOnUnit(mob1);
                    if (spell.IsReady())
                        spell.Cast();
                    if (spell.IsReady())
                        spell.Cast(mob1.Position);
                }
            }
        }
        public static void castspell_hero(Obj_AI_Base mob1)
        {
            if (Player.ChampionName.ToUpper() == "NUNU")
            {
                if (Q.IsReady())
                    Q.CastOnUnit(mob1);
                if (E.IsReady())
                    E.CastOnUnit(mob1);
                if (W.IsReady())
                    W.Cast();
            }
            else if (Player.ChampionName.ToUpper() == "CHOGATH")
            {
                if (Q.IsReady())
                    Q.Cast(mob1.Position);
                if (W.IsReady())
                    W.Cast(mob1.Position);
                if (R.IsReady() && R.GetDamage(mob1) >= mob1.Health)
                    R.CastOnUnit(mob1);
            }
            else if (Player.ChampionName.ToUpper() == "WARWICK")
            {
                if (Q.IsReady())
                    Q.CastOnUnit(mob1);
                if (W.IsReady())
                    W.Cast();
                if (R.IsReady())
                    R.CastOnUnit(mob1);
            }
            else if (Player.ChampionName.ToUpper() == "MASTERYI")
            {
                if (Q.IsReady())
                    Q.CastOnUnit(mob1);
                if (W.IsReady() && Player.HealthPercentage() < JeonAutoJungleMenu.Item("yi_W").GetValue<Slider>().Value)
                    W.Cast();
                if (E.IsReady())
                    E.Cast();
                if (R.IsReady())
                    R.Cast();
            }
            else if (Player.ChampionName.ToUpper() == "MAOKAI")
            {
                if (Q.IsReady())
                    Q.Cast(mob1.Position);
                if (E.IsReady())
                    E.Cast(mob1.Position);
                if (W.IsReady())
                    W.CastOnUnit(mob1);
            }
            else if (Player.ChampionName.ToUpper() == "NASUS")
            {
                if (Q.IsReady())
                    Q.Cast();
                if (W.IsReady() && mob1.IsValid<Obj_AI_Hero>())
                    W.CastOnUnit(mob1);
                if (E.IsReady())
                    E.Cast(mob1.Position);
                if (R.IsReady())
                    R.Cast();
            }
            else
            {
                foreach (var spell in cast2hero)
                {
                    if (spell.IsReady())
                        spell.CastOnUnit(mob1);
                    if (spell.IsReady())
                        spell.Cast();
                    if (spell.IsReady())
                        spell.Cast(mob1.Position);
                }
            }
        }
        public static void castspell_laneclear(Obj_AI_Base mob1)
        {
            if (Player.ChampionName.ToUpper() == "NUNU")
            {
                if (Q.IsReady())
                    Q.CastOnUnit(mob1);
                if (E.IsReady())
                    E.CastOnUnit(mob1);
                if (W.IsReady())
                    W.Cast();
            }
            else if (Player.ChampionName.ToUpper() == "CHOGATH")
            {
                if (Q.IsReady())
                    Q.Cast(mob1.Position);
                if (W.IsReady())
                    W.Cast(mob1.Position);
                if (R.IsReady() && R.GetDamage(mob1) >= mob1.Health)
                    R.CastOnUnit(mob1);
            }
            else if (Player.ChampionName.ToUpper() == "WARWICK")
            {
                if (W.IsReady())
                    W.Cast();
            }
            else if (Player.ChampionName.ToUpper() == "MASTERYI")
            {
                if (Q.IsReady())
                    Q.CastOnUnit(mob1);
                if (E.IsReady())
                    E.Cast();
            }
            else if (Player.ChampionName.ToUpper() == "MAOKAI")
            {
                if (Q.IsReady())
                    Q.Cast(mob1.Position);
                if (E.IsReady())
                    E.Cast(mob1.Position);
            }
            else if (Player.ChampionName.ToUpper() == "NASUS")
            {
                if (Q.IsReady() && CheckNasusQDamage(mob1))
                    Q.Cast();
                if (W.IsReady() && mob1.IsValid<Obj_AI_Hero>())
                    W.CastOnUnit(mob1);
                if (E.IsReady())
                    E.Cast(mob1.Position);
            }
            else
            {
                foreach (var spell in cast4laneclear)
                {
                    if (spell.IsReady())
                        spell.CastOnUnit(mob1);
                    if (spell.IsReady())
                        spell.Cast();
                    if (spell.IsReady())
                        spell.Cast(mob1.Position);
                }
            }
        }
        public static bool CheckNasusQDamage(Obj_AI_Base target)
        {
            float QDmg = Convert.ToSingle(Q.GetDamage(target) + Player.CalcDamage(target, Damage.DamageType.Physical, Player.BaseAttackDamage + Player.FlatPhysicalDamageMod));
            if (QDmg >= target.Health)
                return true;
            else
                return false;
        }
        public static float GetSpellRange(SpellDataInst targetSpell, bool IsChargedSkill = false)
        {
            if (targetSpell.SData.CastRangeDisplayOverride <= 0)
            {
                if (targetSpell.SData.CastRange <= 0)
                {
                    return
                    targetSpell.SData.CastRadius;
                }
                else
                {
                    if (!IsChargedSkill)
                        return
                        targetSpell.SData.CastRange;
                    else
                        return
                        targetSpell.SData.CastRadius;
                }
            }
            else
                return
                targetSpell.SData.CastRangeDisplayOverride;
        }
        #endregion spell methods
        #region 스마이트함수 - Smite Function
        public static readonly int[] SmitePurple = { 3713, 3726, 3725, 3726, 3723 };
        public static readonly int[] SmiteGrey = { 3711, 3722, 3721, 3720, 3719 };
        public static readonly int[] SmiteRed = { 3715, 3718, 3717, 3716, 3714 };
        public static readonly int[] SmiteBlue = { 3706, 3710, 3709, 3708, 3707 };
        private static readonly string[] MinionNames =
{
"SRU_Blue", "SRU_Gromp", "SRU_Murkwolf", "SRU_Razorbeak", "SRU_Red", "SRU_Krug", "SRU_Dragon", "SRU_BaronSpawn"
};
        public static void setSmiteSlot()
        {
            foreach (var spell in Player.Spellbook.Spells.Where(spell => String.Equals(spell.Name, smitetype(), StringComparison.CurrentCultureIgnoreCase)))
            {
                smiteSlot = spell.Slot;
                smite = new Spell(smiteSlot, 700);
                return;
            }
        }
        public static string smitetype()
        {
            if (Player.InventoryItems.Any(item => SmiteBlue.Any(t => t == Convert.ToInt32(item.Id))))
            {
                return "s5_summonersmiteplayerganker";
            }
            if (Player.InventoryItems.Any(item => SmiteRed.Any(t => t == Convert.ToInt32(item.Id))))
            {
                return "s5_summonersmiteduel";
            }
            if (Player.InventoryItems.Any(item => SmiteGrey.Any(t => t == Convert.ToInt32(item.Id))))
            {
                return "s5_summonersmitequick";
            }
            if (Player.InventoryItems.Any(item => SmitePurple.Any(t => t == Convert.ToInt32(item.Id))))
            {
                return "itemsmiteaoe";
            }
            return "summonersmite";
        }
        public static double setSmiteDamage()
        {
            int level = Player.Level;
            int[] damage =
{
20*level + 370,
30*level + 330,
40*level + 240,
50*level + 100
};
            return damage.Max();
        }
        public static Obj_AI_Base GetNearest(Vector3 pos)
        {
            var minions =
            ObjectManager.Get<Obj_AI_Minion>()
            .Where(minion => minion.IsValid && minion.IsEnemy && !minion.IsDead && MinionNames.Any(name => minion.Name.StartsWith(name)
            && Player.Distance(minion.Position) <= 1000));
            var objAiMinions = minions as Obj_AI_Minion[] ?? minions.ToArray();
            Obj_AI_Minion sMinion = objAiMinions.FirstOrDefault();
            double? nearest = null;
            foreach (Obj_AI_Minion minion in objAiMinions)
            {
                double distance = Vector3.Distance(pos, minion.Position);
                if (nearest == null || nearest > distance)
                {
                    nearest = distance;
                    sMinion = minion;
                }
            }
            return sMinion;
        }
        public static Obj_AI_Minion GetNearest_big(Vector3 pos)
        {
            var minions =
            ObjectManager.Get<Obj_AI_Minion>()
            .Where(minion => minion.IsValid && minion.IsEnemy && !minion.IsDead && MinionNames.Any(name => minion.Name.StartsWith(name)) && !MinionNames.Any(name => minion.Name.Contains("Mini")
            && Player.Distance(minion.Position) <= 1000));
            var objAiMinions = minions as Obj_AI_Minion[] ?? minions.ToArray();
            Obj_AI_Minion sMinion = objAiMinions.FirstOrDefault();
            double? nearest = null;
            foreach (Obj_AI_Minion minion in objAiMinions)
            {
                double distance = Vector3.Distance(pos, minion.Position);
                if (nearest == null || nearest > distance)
                {
                    nearest = distance;
                    sMinion = minion;
                }
            }
            return sMinion;
        }
        public static bool CheckMonster(String TargetName, Vector3 BasePosition, int Range = 300)
        {
            var minions = ObjectManager.Get<Obj_AI_Minion>()
            .Where(minion => minion.IsValid && !minion.IsDead && minion.Name.StartsWith(TargetName));
            var objAiMinions = minions as Obj_AI_Minion[] ?? minions.ToArray();
            if (!objAiMinions.Any(m => m.Distance(BasePosition) < Range))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion
        #region GetItemTree
        public static void GetItemTree(FileInfo setFile)
        {
            if (Readini.GetItemTreetype(setFile.FullName) == "AP")
            {
                buyThings.Clear();
                buyThings = buyThings_AP;
                Game.PrintChat("Set ItemTree for AP - Finished");
            }
            else if (Readini.GetItemTreetype(setFile.FullName) == "AS")
            {
                buyThings.Clear();
                buyThings = buyThings_AS;
                Game.PrintChat("Set ItemTree for AS - Finished");
            }
            else if (Readini.GetItemTreetype(setFile.FullName) == "TANK")
            {
                buyThings.Clear();
                buyThings = buyThings_TANK;
                Game.PrintChat("Set ItemTree for TANK - Finished");
            }
            else if (Readini.GetItemTreetype(setFile.FullName) == "X")
            {
                Game.PrintChat("PLZ TYPE VALID VALUE, SET AD ITEMTREE - ERROR");
            }
            else
            {
                Game.PrintChat("Set ItemTree for AD - Finished");
            }
        }
        #endregion
    }
}