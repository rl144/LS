using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

using Color = System.Drawing.Color;

namespace ALL_In_One.champions
{
    class Sion
    {
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static Spell Q, W, E, R;

        static Vector2 QCastPos = new Vector2();

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 1050f, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W, 500f, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 800f, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R, 7500f);

            Q.SetSkillshot(0.6f, 100f, float.MaxValue, false, SkillshotType.SkillshotLine);
            Q.SetCharged("SionQ", "SionQ", 500, 720, 0.5f);
            E.SetSkillshot(0.25f, 80f, 1800f, false, SkillshotType.SkillshotLine);

            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();

            AIO_Menu.Champion.Misc.addHitchanceSelector();

            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addWrange();
            AIO_Menu.Champion.Drawings.addErange();

            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalking.CanMove(100))
            {
                switch (Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                        Combo();
                        break;
                    case Orbwalking.OrbwalkingMode.None:
                        break;
                }
            }

            E.MinHitChance = AIO_Menu.Champion.Misc.SelectedHitchance;
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = AIO_Menu.Champion.Drawings.Qrange;
            var drawW = AIO_Menu.Champion.Drawings.Wrange;
            var drawE = AIO_Menu.Champion.Drawings.Erange;

            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color, 3);

            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color, 3);

            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color, 3);
        }

        static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.SData.Name == Q.Instance.SData.Name)
                QCastPos = args.End.To2D();
        }

        static void Combo()
        {

            if (AIO_Menu.Champion.Combo.UseQ && Q.IsReady())
            {
                var qTarget = TargetSelector.GetTarget(Q.ChargedMaxRange, Q.DamageType);

                if (Q.IsCharging)
                {
                    //------Code from TC-CREW Sion--------
                    var start = ObjectManager.Player.ServerPosition.To2D();
                    var end = start.Extend(QCastPos, Q.Range);
                    var direction = (end - start).Normalized();
                    var normal = direction.Perpendicular();

                    var points = new List<Vector2>();
                    var hitBox = qTarget.BoundingRadius;
                    points.Add(start + normal * (Q.Width + hitBox));
                    points.Add(start - normal * (Q.Width + hitBox));
                    points.Add(end + Q.ChargedMaxRange * direction - normal * (Q.Width + hitBox));
                    points.Add(end + Q.ChargedMaxRange * direction + normal * (Q.Width + hitBox));

                    for (var i = 0; i <= points.Count - 1; i++)
                    {
                        var A = points[i];
                        var B = points[i == points.Count - 1 ? 0 : i + 1];

                        if (qTarget.ServerPosition.To2D().Distance(A, B, true, true) < 50 * 50)
                            Q.Cast(qTarget);
                    }
                    //-------------------------------------
                }
                else
                    Q.StartCharging(qTarget.ServerPosition);
            }

            if (AIO_Menu.Champion.Combo.UseW && W.IsReady())
            {
                if (HeroManager.Enemies.Any(x => x.IsValidTarget(W.Range)))
                    W.Cast();
            }

            if (AIO_Menu.Champion.Combo.UseE && E.IsReady())
                E.CastOnBestTarget();
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage2(enemy);

            if (W.IsReady())
                damage += W.GetDamage2(enemy);

            if (E.IsReady())
                damage += E.GetDamage2(enemy);

            if (R.IsReady())
                damage += R.GetDamage2(enemy);

            return damage;
        }
    }
}
