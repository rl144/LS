using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.champions
{
    class Nunu// By RL244
    {
        static Menu Menu { get { return AIO_Menu.MainMenu_Manual; } }
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E, R;
        static SpellSlot smiteSlot = SpellSlot.Unknown;
        static Spell smite;
        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 125f);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 550f, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R, 650, TargetSelector.DamageType.Magical);
            Q.SetTargetted(0.25f, float.MaxValue);
            E.SetTargetted(0.25f, float.MaxValue);
            
            
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();

            AIO_Menu.Champion.Harass.addUseE();
            AIO_Menu.Champion.Harass.addIfMana();

            AIO_Menu.Champion.Lasthit.addUseQ();
            AIO_Menu.Champion.Lasthit.addUseE();
            AIO_Menu.Champion.Lasthit.addIfMana(20);
            
            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseW(false);
            AIO_Menu.Champion.Laneclear.addUseE(false);
            AIO_Menu.Champion.Laneclear.addIfMana();

            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW();
            AIO_Menu.Champion.Jungleclear.addUseE();
            AIO_Menu.Champion.Jungleclear.addIfMana();

            AIO_Menu.Champion.Misc.addItem("KillstealE", true);
            AIO_Menu.Champion.Misc.addItem("ObjectSteal(Dragon/Baron)", true);
            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addErange();
            AIO_Menu.Champion.Drawings.addRrange();
        
            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalking.CanMove(35))
            {
                AIO_Func.SC(Q);
                AIO_Func.SC(E);
                AIO_Func.SC(R);
            }

            #region Killsteal
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealE"))
                KillstealE();
            #endregion
            #region ObjectSteal
            if (AIO_Menu.Champion.Misc.getBoolValue("ObjectSteal(Dragon/Baron)"))
                ObjectSteal();
            #endregion
            #region AfterAttack
            //AIO_Func.AASkill(Q);
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
        
        static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var Target = (Obj_AI_Base)target;
            if (!unit.IsMe || Target == null)
                return;
            AIO_Func.AALcJc(W);
            if(!utility.Activator.AfterAttack.AIO)
            AA();
        }

        static void ObjectSteal()
        {
            float smdmg = setSmiteDamage();
            setSmiteSlot();
            foreach (var target in MinionManager.GetMinions(Orbwalking.GetRealAutoAttackRange(Player)/2+200f, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).OrderByDescending(x => x.Health).Where(x => x.Name.ToLower().Contains("Dragon") || x.Name.ToLower().Contains("Baron")))
            {
                if (Q.CanCast(target) && AIO_Func.isKillable(target, Q.GetDamage2(target) + smdmg) && Q.IsReady() && smite.IsReady())
                    Q.Cast(target);
                if (Q.CanCast(target) && AIO_Func.isKillable(target, Q.GetDamage2(target)) && Q.IsReady())
                    Q.Cast(target);
                if (Q.CanCast(target) && AIO_Func.isKillable(target, smdmg) && smite.IsReady())
                    smite.Cast(target);
            }
        }
        public static void setSmiteSlot()
        {
            foreach (var spell in Player.Spellbook.Spells.Where(spell => spell.Name.ToLower().Contains("summonersmite")))
            {
                smiteSlot = spell.Slot;
                smite = new Spell(smiteSlot, 550);
                return;
            }
        }
        public static float setSmiteDamage() //스마이트 데미지
        {
            float level = Player.Level;
            float[] damage =
            {
            20*level + 370,
            30*level + 330,
            40*level + 240,
            50*level + 100
            };
            return damage.Max();
        }
        static void KillstealE()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (E.CanCast(target) && AIO_Func.isKillable(target, E))
                    E.Cast(target);
            }
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage2(enemy);
            if (E.IsReady())
                damage += E.GetDamage2(enemy);            
            if (R.IsReady())
                damage += R.GetDamage2(enemy)/5*2;
                
            return damage;
        }
    }
}
