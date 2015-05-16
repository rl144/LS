using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;


namespace ALL_In_One.champions
{
    class Sivir
    {
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }


        static Spell Q, W, E, R;

        public static void Load()
        {

            Q = new Spell(SpellSlot.Q, 1245f, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 1000f);


            Q.SetSkillshot(0.25f, 90f, 1350f, false, SkillshotType.SkillshotLine);




            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW();
            AIO_Menu.Champion.Harass.addIfMana(60);

            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseW();
            AIO_Menu.Champion.Laneclear.addIfMana();

            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW();
            AIO_Menu.Champion.Jungleclear.addIfMana();
            
            AIO_Menu.Champion.Flee.addUseR();
            AIO_Menu.Champion.Flee.addIfMana();

            AIO_Menu.Champion.Misc.addUseKillsteal();
            AIO_Menu.Champion.Misc.addItem("AutoQ", true);

            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addRrange();

        
            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.AfterAttack += Orbwalking_OnAfterAttack;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
        }

        static void Game_OnUpdate(EventArgs args)
        {
        
            if (Player.IsDead)
                return;

            
            if (Orbwalking.CanMove(10))
            {
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                    Combo();

                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
                    Harass();

                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                {
                    Laneclear();
                    Jungleclear();
                }

                AutoQ();
            }

            if (AIO_Menu.Champion.Misc.UseKillsteal)
                Killsteal();
                
            #region AfterAttack
            AIO_Func.AASkill(W);
            if(AIO_Func.AfterAttack())
            AA();
            #endregion
        }

        static void Drawing_OnDraw(EventArgs args)
        {
          
            if (Player.IsDead)
                return;

  
            var drawQ = AIO_Menu.Champion.Drawings.Qrange;
            var drawR = AIO_Menu.Champion.Drawings.Rrange;

            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);

            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);
        }
        
        static void AA()
        {
            AIO_Func.AACb(W);
        }

        static void Orbwalking_OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var Target = (Obj_AI_Base)target;
            if (!unit.IsMe || Target == null)
                return;
                
            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);
            var Mobs = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if(Minions.Count > 3 || Mobs.Count > 0)
            AIO_Func.AALcJc(W);
            if(!utility.Activator.AfterAttack.AIO)
            AA();
        }
        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!AIO_Menu.Champion.Combo.UseE || Player.IsDead)
                return;


            if (sender is Obj_AI_Hero && sender.IsEnemy && args.Target.IsMe && !args.SData.IsAutoAttack() && E.IsReady() && !args.SData.Name.Contains("summoner") && args.SData.Name != "TormentedSoil") 
                E.Cast();

        }
        static float getBuffDuration // afterattack tempfix
        {
            get
            {
                var buff = AIO_Func.getBuffInstance(Player, "sivirpassivespeed");

                return buff != null ? buff.EndTime - Game.ClockTime : 0;
            }
        }

        static void AutoQ()
        {
            if (!AIO_Menu.Champion.Misc.getBoolValue("AutoQ"))
                return;
            foreach (Obj_AI_Hero target in HeroManager.Enemies.Where(x => x.IsValidTarget(Q.Range) && !x.HasBuffOfType(BuffType.Invulnerability)))
            {
                if (target != null)
                {
                    if (Q.CanCast(target) && Q.GetPrediction(target).Hitchance >= HitChance.Immobile)
                        Q.Cast(target);
                }
            }
        }
        static void Combo()
        {
            if (AIO_Menu.Champion.Combo.UseQ && Q.IsReady())
            {
                var qTarget = TargetSelector.GetTarget(Q.Range, Q.DamageType, true);

                if (qTarget != null && Q.GetPrediction(qTarget).Hitchance >= HitChance.VeryHigh)
                    Q.Cast(qTarget);

            }
/*
            if (AIO_Menu.Champion.Combo.UseW && W.IsReady()) // afterattack tempfix
            {
                if (getBuffDuration > 1.95 && HeroManager.Enemies.Any(x => Orbwalking.InAutoAttackRange(x)))
                {
                    W.Cast();
                    Orbwalking.ResetAutoAttackTimer2();
                }
            }*/
        }

        static void Harass()
        {

            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Harass.IfMana))
                return;

            if (AIO_Menu.Champion.Harass.UseQ && Q.IsReady())
            {
                var qTarget = TargetSelector.GetTarget(Q.Range, Q.DamageType, true);

                if (qTarget != null && Q.GetPrediction(qTarget).Hitchance >= HitChance.VeryHigh)
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
                var farmloc = Q.GetLineFarmLocation(Minions);

                if (farmloc.MinionsHit >= 3)
                    Q.Cast(farmloc.Position);
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

            }
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage2(enemy);

            if (W.IsReady())
                damage += W.GetDamage2(enemy) + (float)Player.GetAutoAttackDamage2(enemy, true);
                
            if(!Player.IsWindingUp)
                damage += (float)Player.GetAutoAttackDamage2(enemy, true);

            return damage;
        }
    }
}
