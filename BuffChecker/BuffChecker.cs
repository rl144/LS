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

namespace BuffChecker
{
    internal class Program
    {
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } } // Player 많이 쓰는데 괜히 ObjectManager.Player라고 하는거 너무길어요~~.
        private static List<Items.Item> itemsList = new List<Items.Item>();
        private static Menu Menu;
        static float pastTime = 0; //버프 체크시 랙 덜걸리도록..
        static bool RM {get{return Menu.Item("dbbuff").GetValue<KeyBind>().Active; }}
        static float TM {get{return Menu.Item("timer").GetValue<Slider>().Value; }}

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
             CreateMenu();
             Game.OnUpdate += Game_OnUpdate;
        }


        private static void Game_OnUpdate(EventArgs args)
        {
            if(RM)
            {
                if(Environment.TickCount - pastTime > TM) //랙 줄이려고 추가함
                pastTime = Environment.TickCount;
                if(Environment.TickCount - pastTime > TM - 10f)
                {
                    var Target = TargetSelector.GetTarget(1200, TargetSelector.DamageType.Physical);
                    if(Target == null)
                    {
                        foreach (var buff in Player.Buffs)
                        {
                            Console.WriteLine("PLAYER : "+buff.Name);
                        }
                    }
                    else
                    {
                        foreach (var buff in Player.Buffs)
                        {
                            Console.WriteLine("PLAYER : "+ buff.Name);
                        }
                        foreach (var buff in Target.Buffs)
                        {
                            Console.WriteLine("TARGET : "+ buff.Name);
                        }
                    }
                }
            }
        }
        
        private static void CreateMenu()
        {
            Menu = new Menu("BuffChecker", "menu", true);
            Menu.AddItem(new MenuItem("timer", "timer")).SetValue(new Slider(500, 200, 5000));
            Menu.AddItem(new MenuItem("dbbuff", "Simple Buff Checker")).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press, false));
            Menu.AddToMainMenu();
        }
    }
}
