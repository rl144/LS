using System;
using System.Drawing;
using System.Reflection;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One
{
    class Initializer
    {
        internal static void initialize()
        {
            AIO_Menu.initialize();

            if (!ChampLoader.champSupportedCheck("ALL_In_One.champions."))
                return;

            AIO_Menu.addSubMenu("Champion", "AIO: " + ObjectManager.Player.ChampionName);

            AIO_Menu.Champion.addOrbwalker();
            AIO_Menu.Champion.addTargetSelector();

            ChampLoader.Load(ObjectManager.Player.ChampionName);
            utility.Activator.Load();
            utility.SetOrb.Load();

            AIO_Menu.Champion.Drawings.addItem(" ", null, false);
            AIO_Menu.Champion.Drawings.addItem("--PUBLIC OPTIONS--", null, false);

            AIO_Menu.Champion.Drawings.addItem("Auto-Attack Real Range", new Circle(true, Color.Silver), false);
            AIO_Menu.Champion.Drawings.addItem("Auto-Attack Target", new Circle(true, Color.Red), false);
            AIO_Menu.Champion.Drawings.addItem("Minion Last Hit", new Circle(true, Color.GreenYellow), false);
            AIO_Menu.Champion.Drawings.addItem("Minion Near Kill", new Circle(true, Color.Gray), false);
            AIO_Menu.Champion.Drawings.addItem("Jungle Position", true, false);

            Drawing.OnDraw += Drawing_OnDraw;
            
            AIO_Func.sendDebugMsg(ObjectManager.Player.ChampionName + " Loaded.");
            AIO_Func.sendDebugMsg("Early Access.");

        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (ObjectManager.Player.IsDead)
                return;

            var drawMinionLastHit = AIO_Menu.Champion.Drawings.getCircleValue("Minion Last Hit", false);
            var drawMinionNearKill = AIO_Menu.Champion.Drawings.getCircleValue("Minion Near Kill", false);

            if (drawMinionLastHit.Active || drawMinionNearKill.Active)
            {
                foreach (var minion in MinionManager.GetMinions(ObjectManager.Player.Position, Orbwalking.GetRealAutoAttackRange(ObjectManager.Player) + 300))
                {
                    if (drawMinionLastHit.Active && ObjectManager.Player.GetAutoAttackDamage2(minion, true) >= minion.Health)
                        Render.Circle.DrawCircle(minion.Position, minion.BoundingRadius, drawMinionLastHit.Color, 3);
                    else
                        if (drawMinionNearKill.Active && ObjectManager.Player.GetAutoAttackDamage2(minion, true) * 2 >= minion.Health)
                        Render.Circle.DrawCircle(minion.Position, minion.BoundingRadius, drawMinionNearKill.Color, 3);
                }
            }

            if (Game.MapId == (GameMapId)11 && AIO_Menu.Champion.Drawings.getBoolValue("Jungle Position", false))
            {
                const byte circleRadius = 100;

                Render.Circle.DrawCircle(new SharpDX.Vector3(7461.018f, 3253.575f, 52.57141f), circleRadius, Color.Blue, 3); // blue team: red
                Render.Circle.DrawCircle(new SharpDX.Vector3(3511.601f, 8745.617f, 52.57141f), circleRadius, Color.Blue, 3); // blue team: blue
                Render.Circle.DrawCircle(new SharpDX.Vector3(7462.053f, 2489.813f, 52.57141f), circleRadius, Color.Blue, 3); // blue team: golems
                Render.Circle.DrawCircle(new SharpDX.Vector3(3144.897f, 7106.449f, 51.89026f), circleRadius, Color.Blue, 3); // blue team: wolfs
                Render.Circle.DrawCircle(new SharpDX.Vector3(7770.341f, 5061.238f, 49.26587f), circleRadius, Color.Blue, 3); // blue team: wariaths

                Render.Circle.DrawCircle(new SharpDX.Vector3(10930.93f, 5405.83f, -68.72192f), circleRadius, Color.Yellow, 3); // Dragon

                Render.Circle.DrawCircle(new SharpDX.Vector3(7326.056f, 11643.01f, 50.21985f), circleRadius, Color.Red, 3); // red team: red
                Render.Circle.DrawCircle(new SharpDX.Vector3(11417.6f, 6216.028f, 51.00244f), circleRadius, Color.Red, 3); // red team: blue
                Render.Circle.DrawCircle(new SharpDX.Vector3(7368.408f, 12488.37f, 56.47668f), circleRadius, Color.Red, 3); // red team: golems
                Render.Circle.DrawCircle(new SharpDX.Vector3(10342.77f, 8896.083f, 51.72742f), circleRadius, Color.Red, 3); // red team: wolfs
                Render.Circle.DrawCircle(new SharpDX.Vector3(7001.741f, 9915.717f, 54.02466f), circleRadius, Color.Red, 3); // red team: wariaths                    
            }

            var drawAA = AIO_Menu.Champion.Drawings.getCircleValue("Auto-Attack Real Range", false);
            var drawTarget = AIO_Menu.Champion.Drawings.getCircleValue("Auto-Attack Target", false);

            if (drawAA.Active)
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Orbwalking.GetRealAutoAttackRange(ObjectManager.Player), drawAA.Color);

            if (drawTarget.Active)
            {
                var aaTarget = AIO_Menu.Orbwalker.GetTarget();

                if (aaTarget.IsValidTarget())
                    Render.Circle.DrawCircle(aaTarget.Position, aaTarget.BoundingRadius + 15, drawTarget.Color, 3);
            }
        }
    }
}
