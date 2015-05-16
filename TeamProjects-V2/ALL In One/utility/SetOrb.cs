using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.utility
{
    class SetOrb
    {
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        internal static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion").SubMenu("Orbwalker");}}
        internal static void Load()
        {
            Menu.AddSubMenu(new Menu("Set", "Set"));
            Menu.SubMenu("Set").AddItem(new MenuItem("SetCbMove", "SetCbMove")).SetValue(true);
            Menu.SubMenu("Set").AddItem(new MenuItem("SetHrMove", "SetHrMove")).SetValue(true);
            Menu.SubMenu("Set").AddItem(new MenuItem("SetCbAttack", "SetCbAttack")).SetValue(true);
            Game.OnUpdate += Game_OnUpdate;
        }
        
        internal static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;
            if((Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && !Menu.Item("SetCbMove").GetValue<bool>()) || (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && !Menu.Item("SetHrMove").GetValue<bool>()))
            Orbwalker.SetMovement(false);
            else
            Orbwalker.SetMovement(true);
            if(Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && !Menu.Item("SetCbAttack").GetValue<bool>())
            Orbwalker.SetAttack(false);
            else
            Orbwalker.SetAttack(true);
        }
    }
}
    
