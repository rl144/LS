using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using SharpDX;


using Color = System.Drawing.Color;

namespace ALL_In_One.champions
{
    class Graves
    {
        
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

      
        static Spell Q, W, E, R;

        public static void Load()
        {
            
            Q = new Spell(SpellSlot.Q, 720f);
            W = new Spell(SpellSlot.W, 850f);
            E = new Spell(SpellSlot.E, 425f);
            R = new Spell(SpellSlot.R, 1100f);

           
            Q.SetSkillshot(0.25f, 15f * (float)Math.PI / 180, 2000f, false, SkillshotType.SkillshotCone);
            W.SetSkillshot(0.25f, 250f, 1650f, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.25f, 100f, 2100f, false, SkillshotType.SkillshotLine);


            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseR();


            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addIfMana(70);

            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addIfMana(80);

            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addIfMana(80);
            
            AIO_Menu.Champion.Flee.addUseE();
            AIO_Menu.Champion.Flee.addIfMana();

            AIO_Menu.Champion.Misc.addItem("Use E on Gap Closer", false);
            AIO_Menu.Champion.Misc.addUseKillsteal();

            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addErange();
            AIO_Menu.Champion.Drawings.addRrange();

           
            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);


            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
        }

        static void Game_OnUpdate(EventArgs args)
        {

            if (Player.IsDead)
                return;

 
            if (Orbwalking.CanMove(10))
            {
                AIO_Func.FleeToPosition(E);
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                    Combo();

                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
                    Harass();

                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                {
                    Laneclear();
                    Jungleclear();
                }
            }

           
            if (AIO_Menu.Champion.Misc.UseKillsteal)
                Killsteal();
        }

        static void Drawing_OnDraw(EventArgs args)
        {

            if (Player.IsDead)
                return;

            var drawQ = AIO_Menu.Champion.Drawings.Qrange;
            var drawW = AIO_Menu.Champion.Drawings.Wrange;
            var drawE = AIO_Menu.Champion.Drawings.Erange;
            var drawR = AIO_Menu.Champion.Drawings.Rrange;

            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);

            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);

            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);

            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);
        }

        static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (target.Type != GameObjectType.obj_AI_Hero)
                return;

            var Target = (Obj_AI_Base)target;

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                if (AIO_Menu.Champion.Combo.UseQ && Q.CanCast(Target) && !Player.IsDashing())
                {
                    Q.Cast(Target);
                }
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                if (AIO_Menu.Champion.Combo.UseQ && Q.CanCast(Target) && !Player.IsDashing() && !(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Harass.IfMana))
                    Q.Cast(Target);
            }
        }

        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser) 
            // cradits to Asuna & ijabba
        {
            if (!AIO_Menu.Champion.Misc.getBoolValue("Use E on Gap Closer") || Player.IsDead)
                return;

            var extended = gapcloser.Start.Extend(Player.Position, gapcloser.Start.Distance(Player.ServerPosition) + E.Range);

            if (IsSafePosition(extended))
            {
                E.Cast(extended);
            }
        }

        static void Combo()
        {
            

            if (AIO_Menu.Champion.Combo.UseQ && Q.IsReady())
            {
               
                var qTarget = TargetSelector.GetTarget(Q.Range, Q.DamageType, true);

        
                if (qTarget != null && !Player.IsDashing())
                    Q.Cast(qTarget);       
            }

            if (AIO_Menu.Champion.Combo.UseW && W.IsReady())
            {
                var wTarget = TargetSelector.GetTarget(W.Range, W.DamageType, true);

                if (wTarget != null && !Player.IsDashing())
                    W.Cast(wTarget);
            }

            if (AIO_Menu.Champion.Combo.UseR && R.IsReady())
            {
                var Rtarget = HeroManager.Enemies.Where(x => R.CanCast(x) && x.Health + (x.HPRegenRate / 2) <= R.GetDamage2(x) && R.GetPrediction(x).Hitchance >= HitChance.VeryHigh).OrderByDescending(x => x.Health).FirstOrDefault();

                if (R.CanCast(Rtarget))
                    R.Cast(Rtarget);
            }
        }

        static void Harass()
        {
           
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Harass.IfMana))
                return;

            if (AIO_Menu.Champion.Harass.UseQ && Q.IsReady())
            {
                var qTarget = TargetSelector.GetTarget(Q.Range, Q.DamageType, true);


                if (qTarget != null && !Player.IsDashing())
                    Q.Cast(qTarget);       
            }
        }

        static void Laneclear()
        {

            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Laneclear.IfMana))
                return;

            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (AIO_Menu.Champion.Laneclear.UseQ && Q.IsReady())
            {
                var Farmloc = Q.GetLineFarmLocation(Minions);

                if (Farmloc.MinionsHit >= 6)
                    Q.Cast(Farmloc.Position);
            }

        }

        static void Jungleclear()
        {
            
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Jungleclear.IfMana))
                return;


            var Mobs = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (AIO_Menu.Champion.Jungleclear.UseQ && Q.IsReady())
            {
               
                if (Q.CanCast(Mobs.FirstOrDefault()))
                    Q.Cast(Mobs.FirstOrDefault());
            }

        }

        static void Killsteal()
        {
           
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                
                if (Q.CanCast(target) && AIO_Func.isKillable(target, Q))
                    Q.Cast(target);

                if (W.CanCast(target) && AIO_Func.isKillable(target, W))
                    W.Cast(target);

                if (R.CanCast(target) && AIO_Func.isKillable(target, R))
                    R.Cast(target);
            }
        }

        public static bool IsSafePosition(Vector3 position) // cradits to Asuna & ijabba
        {
            if (position.UnderTurret(true) && !Player.UnderTurret(true))
            {
                return false;
            }
            var allies = position.CountAlliesInRange(Orbwalking.GetRealAutoAttackRange(Player));
            var enemies = position.CountEnemiesInRange(Orbwalking.GetRealAutoAttackRange(Player));
            var lhEnemies = GetLhEnemiesNearPosition(position, Orbwalking.GetRealAutoAttackRange(Player)).Count();

            if (enemies == 1) 
            {
                return true;
            }

           
            return (allies + 1 > enemies - lhEnemies);
        }
        public static List<Obj_AI_Hero> GetLhEnemiesNearPosition(Vector3 position, float range)
        // cradits to Asuna & ijabba
        {
            return
                HeroManager.Enemies.Where(
                    hero => hero.IsValidTarget(range, true, position) && AIO_Func.getHealthPercent(hero) <= 15).ToList();
        }


        static float getComboDamage(Obj_AI_Base enemy)
        {
            
            float damage = 0;


            if (R.IsReady())
                damage += R.GetDamage2(enemy);

            return damage;
        }
    }
}
