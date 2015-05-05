using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;
using SharpDX;
using Color = System.Drawing.Color;

namespace IgSm
{
    internal class Program
    {
        private static Orbwalking.Orbwalker Orbwalker;
        private static Obj_AI_Hero Player;
        private static Spell SmiteSlot;
        private static Spell RSmite;
		private static Spell IgniteSlot;
        private static List<Items.Item> itemsList = new List<Items.Item>();
        private static string WelcMsg = ("<font color = '#ff3366'>IgSm</font><font color='#FFFFFF'> by RL244.</font> <font color = '#66ff33'> ~~ LOADED ~~</font> ");
        public static SpellSlot smiteSlot = SpellSlot.Unknown;
        public static SpellSlot rsmiteSlot = SpellSlot.Unknown;
		public static SpellSlot igniteSlot = SpellSlot.Unknown;
        private static Menu Menu;
        private static Items.Item s0, s1, s2, s3, s4, s5, s6, s7, s8, s9;
        private static float range = 550f;
		private static float irange = 550f;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
             Player = ObjectManager.Player;
             Game.PrintChat(WelcMsg);

             CreateMenu();
             InitializeItems();

             Game.OnUpdate += Game_OnUpdate;
             Drawing.OnDraw += Drawing_OnDraw;
      
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Menu.Item("enable").GetValue<bool>())
                return;

            if (Menu.Item("draw").GetValue<Circle>().Active)
            {
                Utility.DrawCircle(Player.ServerPosition, range, Menu.Item("draw").GetValue<Circle>().Color);
            }
            else
                return;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Menu.Item("dbbuff", true).GetValue<bool>())
			{
				foreach (var buff in ObjectManager.Player.Buffs)
				{
					Console.WriteLine("Name:{0}", buff.Name);
				}
			}
            if (!Menu.Item("enable").GetValue<bool>())
                return;
            if (Player.IsDead)
                return;
            if (!CheckInv())
                return;
				
			setIgniteSlot();
            setSmiteSlot();

            var enemys = ObjectManager.Get<Obj_AI_Hero>().Where(f => !f.IsAlly && !f.IsDead && Player.Distance(f, false) <= range);
            if (enemys == null)
                return;

            float dmg = Damage();
			float idmg = Idamage();
            foreach (var enemy in enemys)
            {
                if (enemy.Health <= dmg)
                {
                    //Game.PrintChat("KAPPA");
                    SmiteSlot.Slot = smiteSlot;
                    Player.Spellbook.CastSpell(smiteSlot, enemy);
                }
				else if (enemy.Health <= idmg)
				{
                    IgniteSlot.Slot = igniteSlot;
                    Player.Spellbook.CastSpell(igniteSlot, enemy);
				}
            }
			


        }
		
		static void Orbwalking_OnAttack(AttackableUnit unit, AttackableUnit target)
		{
            var Target = (Obj_AI_Base)target;
            
			if (!unit.IsMe || Target == null)
                return;
				
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
				{
					if (!CheckInv())
					return;
                    RSmite.Slot = rsmiteSlot;
					if(rsmiteSlot.IsReady())
                    Player.Spellbook.CastSpell(rsmiteSlot, Target);
				}
		}

        public static void setSmiteSlot()
        {
            foreach (var spell in ObjectManager.Player.Spellbook.Spells.Where(spell => String.Equals(spell.Name, "s5_summonersmiteplayerganker", StringComparison.CurrentCultureIgnoreCase)))
            {
                smiteSlot = spell.Slot;
                SmiteSlot = new Spell(smiteSlot, range);
                return;
            } //BS
            foreach (var spell in ObjectManager.Player.Spellbook.Spells.Where(spell => String.Equals(spell.Name, "s5_summonersmiteduel", StringComparison.CurrentCultureIgnoreCase)))
            {
                rsmiteSlot = spell.Slot;
                RSmite = new Spell(rsmiteSlot, range);
                return;
            } //RS
        }
		
        public static void setIgniteSlot()
        {
            foreach (var spell in ObjectManager.Player.Spellbook.Spells.Where(spell => String.Equals(spell.Name, "SummonerDot", StringComparison.CurrentCultureIgnoreCase)))
            {
                igniteSlot = spell.Slot;
				igniteSlot = Player.GetSpellSlot("SummonerDot");
                return;
            }
        }

        private static void CreateMenu()
        {
            Menu = new Menu("IgSm", "menu", true);
            Menu.AddItem(new MenuItem("enable", "Enable").SetValue(true));
            Menu.AddItem(new MenuItem("draw", "Draw Smite Range").SetValue(new Circle(true, Color.Blue)));
            Menu.AddItem(new MenuItem("dbbuff", "dbbuff", true).SetValue(true));
            Menu.AddToMainMenu();
        }

        private static bool CheckInv()
        {
            bool b = false;
            foreach(var item in itemsList)
            {
                if(Player.InventoryItems.Any(f => f.Id == (ItemId)item.Id))
                {
                    b = true;
                }
            }
            return b;
        }
        private static void InitializeItems()
        {
            s0 = new Items.Item(3710, range);
            itemsList.Add(s0);
            s1 = new Items.Item(3709, range);
            itemsList.Add(s1);
            s2 = new Items.Item(3708, range);
            itemsList.Add(s2);
            s3 = new Items.Item(3707, range);
            itemsList.Add(s3);
            s4 = new Items.Item(3706, range);
            itemsList.Add(s4);
            s0 = new Items.Item(3714, range);
            itemsList.Add(s5);
            s1 = new Items.Item(3715, range);
            itemsList.Add(s6);
            s2 = new Items.Item(3716, range);
            itemsList.Add(s7);
            s3 = new Items.Item(3717, range);
            itemsList.Add(s8);
            s4 = new Items.Item(3718, range);
            itemsList.Add(s9);
        }
        private static float Damage()
        {
            int lvl = Player.Level;
            int damage = (20 + 8 * lvl);

            return damage;
        }
		private static float Idamage()
		{
			int lvl = Player.Level;
			int idamage = (30 + 20 * lvl); // 원랜 50+20*lvl이지만 이렇게함!!)
			return idamage;
		}
	}
}
