using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace ALL_In_One.champions
{
    class Jinx
    {
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }

        static Spell Q, W, E, R;

        static bool QisActive { get { return Player.HasBuff("JinxQ", true); } }

        const int DefaultRange = 590;

        static float GetQActiveRange { get { return DefaultRange + ((25 * Q.Level) + 50); } }

        static float WLastCastedTime = 0f;

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 1450f, TargetSelector.DamageType.Physical);
            E = new Spell(SpellSlot.E, 900f);
            R = new Spell(SpellSlot.R, 2500f, TargetSelector.DamageType.Physical);

            W.SetSkillshot(0.5f, 60f, 3300f, true, SkillshotType.SkillshotLine);
            E.SetSkillshot(1.2f, 1f, 1750f, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.5f, 140f, 1700f, false, SkillshotType.SkillshotLine);

            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW();
            AIO_Menu.Champion.Harass.addIfMana();

            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addItem("Switch to FISHBONES If Hit Minion Number >=", new Slider(2, 2, 7));
            AIO_Menu.Champion.Laneclear.addIfMana();

            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW();
            AIO_Menu.Champion.Jungleclear.addIfMana();
            
            AIO_Menu.Champion.Flee.addUseE();
            AIO_Menu.Champion.Flee.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();
            AIO_Menu.Champion.Misc.addUseAntiGapcloser();
            AIO_Menu.Champion.Misc.addItem("Auto E On Immobile Targets", true);
            AIO_Menu.Champion.Misc.addItem("Switch to FISHBONES If Hit Enemy Number >=", new Slider(2, 2, 5));

            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addWrange();
            AIO_Menu.Champion.Drawings.addErange();
            AIO_Menu.Champion.Drawings.addRrange();
            AIO_Menu.Champion.Drawings.addItem("Passive Timer", new Circle(true , Color.SpringGreen));

            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            W.MinHitChance = AIO_Menu.Champion.Misc.SelectedHitchance;
            E.MinHitChance = AIO_Menu.Champion.Misc.SelectedHitchance;
            R.MinHitChance = AIO_Menu.Champion.Misc.SelectedHitchance;

            if(Orbwalking.CanMove(100))
            {
                switch (Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                        Combo();
                        break;
                    case Orbwalking.OrbwalkingMode.Mixed:
                        Harass();
                        break;
                    case Orbwalking.OrbwalkingMode.LaneClear:
                        Laneclear();
                        Jungleclear();
                        break;
                    case Orbwalking.OrbwalkingMode.None:
                        break;
                }
            }

            AutoE();
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = AIO_Menu.Champion.Drawings.Qrange;
            var drawW = AIO_Menu.Champion.Drawings.Wrange;
            var drawE = AIO_Menu.Champion.Drawings.Erange;
            var drawR = AIO_Menu.Champion.Drawings.Rrange;
            var drawP = AIO_Menu.Champion.Drawings.getCircleValue("Passive Timer");

            if (drawQ.Active && !QisActive)
                Render.Circle.DrawCircle(Player.Position, GetQActiveRange, drawQ.Color);

            if (drawW.Active && W.IsReady())
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);

            if (drawE.Active && E.IsReady())
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);

            if (drawR.Active && R.IsReady())
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);

            if (drawP.Active)
            {
                var passive = AIO_Func.getBuffInstance(Player, "jinxpassivekill");

                if (passive != null)
                {
                    var targetpos = Drawing.WorldToScreen(Player.Position);
                    Drawing.DrawText(targetpos[0] - 10, targetpos[1], drawP.Color, (passive.EndTime - Game.ClockTime).ToString("0.00"));
                }
            }

            if(QisActive)
            {
                var aaTarget = Orbwalker.GetTarget();

                if (aaTarget != null)
                    Render.Circle.DrawCircle(aaTarget.Position, 200, Color.Red);
            }
        }

        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!AIO_Menu.Champion.Misc.UseAntiGapcloser || Player.IsDead)
                return;

            if (E.IsReady() && Player.Distance(gapcloser.End, false) <= 200)
                E.Cast(gapcloser.End);
        }

        static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        {
            if (unit.IsMe && args.SData.Name == "JinxW")
                WLastCastedTime = Game.ClockTime;
        }

        static void QSwitchForUnit(AttackableUnit Unit)
        {
            if (Unit == null)
            {
                QSwitch(false);
                return;
            }

            if (AIO_Func.SelfAOE_Prediction.HitCount(0.25f, 200f, Unit.Position) >= AIO_Menu.Champion.Misc.getSliderValue("Switch to FISHBONES If Hit Enemy Number >=").Value)
            {
                QSwitch(true);
                return;
            }

            if (Unit.IsValidTarget(DefaultRange))
                QSwitch(false);
            else
                QSwitch(true);

        }

        static void QSwitch(Boolean activate)
        {
            if (!Q.IsReady())
                return;

            if (Player.IsWindingUp)
                return;

            if (activate && !QisActive)
                Q.Cast();
            else if (!activate && QisActive)
                Q.Cast();
        }

        static void AutoE()
        {
            if (!AIO_Menu.Champion.Misc.getBoolValue("Auto E On Immobile Targets"))
                return;

            foreach (Obj_AI_Hero target in HeroManager.Enemies.Where(x => x.IsValidTarget(E.Range)))
            {
                if (E.CanCast(target) && AIO_Func.UnitIsImmobileUntil(target) >= E.Delay - 0.5)
                    E.Cast(target, false, true);
            }
        }

        static Obj_AI_Base E_GetBestTarget()
        {
            return HeroManager.Enemies.Where(x => x.IsValidTarget(E.Range) && !x.HasBuffOfType(BuffType.SpellImmunity) && E.GetPrediction(x).Hitchance >= HitChance.VeryHigh && !x.IsFacing(Player) && x.IsMoving).OrderBy(x => x.Distance(Player, false)).FirstOrDefault();
        }

        static void Combo()
        {
            if (AIO_Menu.Champion.Combo.UseQ && Q.IsReady())
                QSwitchForUnit(TargetSelector.GetTarget(GetQActiveRange + 30, Q.DamageType));

            if (AIO_Menu.Champion.Combo.UseW && W.IsReady())
            {
                var Wtarget = TargetSelector.GetTarget(W.Range, W.DamageType);

                if (Wtarget != null && !Wtarget.IsValidTarget(200f))
                    W.Cast(Wtarget);
            }

            if (AIO_Menu.Champion.Combo.UseE && E.IsReady())
            {
                var Etarget = E_GetBestTarget();

                if (Etarget != null)
                    E.Cast(Etarget,false,true);
            }

            if (AIO_Menu.Champion.Combo.UseR && R.IsReady() && WLastCastedTime + 0.5 < Game.ClockTime)
            {
                if(AIO_Func.getHealthPercent(Player) > 20)
                {
                    foreach (var RT in HeroManager.Enemies.Where(x => x.IsValidTarget(R.Range) && !x.IsValidTarget(DefaultRange) && !Player.HasBuffOfType(BuffType.SpellShield) && !Player.HasBuffOfType(BuffType.Invulnerability) && Utility.GetAlliesInRange(x, 500).Where(ally => !ally.IsMe && ally.IsAlly).Count() <= 1))
                    {//이전 식대로는 징크스가 절대 궁을 안써서 식을 좀 바꿈.
                        AIO_Func.PredHealth(RT,R);
                        if (RT != null && AIO_Func.PredHealth(RT,R) + RT.HPRegenRate <= (Player.Distance(RT.ServerPosition) < 1500 ? R.GetDamage2(RT,4) : R.GetDamage2(RT,1)) && !AIO_Func.CollisionCheck(Player, RT, R.Width))
                            AIO_Func.LCast(R,RT,50f,0f,true); //R 최대 데미지는 1500범위에서. 그리고 공식 커먼의 징크스 궁 데미지 계산이 잘못되었으니 올인원 자체 데미지 계산사용.
                    }
                }
                else //현재 체력이 20퍼 미만일 경우 평타 사거리 내에도 궁을 쓰도록.
                {
                    foreach (var RT in HeroManager.Enemies.Where(x => x.IsValidTarget(R.Range) && !Player.HasBuffOfType(BuffType.SpellShield) && !Player.HasBuffOfType(BuffType.Invulnerability) && Utility.GetAlliesInRange(x, 500).Where(ally => !ally.IsMe && ally.IsAlly).Count() <= 1))
                    {//이전 식대로는 징크스가 절대 궁을 안써서 식을 좀 바꿈.
                        AIO_Func.PredHealth(RT,R);
                        if (RT != null && AIO_Func.PredHealth(RT,R) + RT.HPRegenRate <= (Player.Distance(RT.ServerPosition) < 1500 ? R.GetDamage2(RT,4) : R.GetDamage2(RT,1)) && !AIO_Func.CollisionCheck(Player, RT, R.Width))
                            AIO_Func.LCast(R,RT,50f,0f,true); //R 최대 데미지는 1500범위에서. 그리고 공식 커먼의 징크스 궁 데미지 계산이 잘못되었으니 올인원 자체 데미지 계산사용.
                    }
                }
            }
        }

        static void Harass()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Harass.IfMana))
            {
                if (AIO_Menu.Champion.Harass.UseQ)
                    QSwitch(false);

                return;
            }

            if (AIO_Menu.Champion.Harass.UseQ && Q.IsReady())
                QSwitchForUnit(TargetSelector.GetTarget(GetQActiveRange + 30, TargetSelector.DamageType.Physical, true));

            if (AIO_Menu.Champion.Harass.UseW && W.IsReady())
                W.CastOnBestTarget();
        }

        static void Laneclear()
        {
            var Minions = MinionManager.GetMinions(Player.ServerPosition, GetQActiveRange, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
            {
                QSwitch(false);
                return;
            }

            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Laneclear.IfMana))
            {
                if (AIO_Menu.Champion.Laneclear.UseQ)
                    QSwitch(false);

                return;
            }

            if (AIO_Menu.Champion.Laneclear.UseQ)
            {
                var target = Orbwalker.GetTarget();

                if (target != null)
                    QSwitch((AIO_Func.CountEnemyMinionsInRange(target.Position, 200) >= AIO_Menu.Champion.Laneclear.getSliderValue("Switch to FISHBONES If Hit Minion Number >=").Value));
            }

        }

        static void Jungleclear()
        {
            var Mobs = MinionManager.GetMinions(Player.ServerPosition, GetQActiveRange, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
            {
                QSwitch(false);
                return;
            }

            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Jungleclear.IfMana))
            {
                if (AIO_Menu.Champion.Jungleclear.UseQ)
                    QSwitch(false);

                return;
            }

            if (AIO_Menu.Champion.Jungleclear.UseQ && Q.IsReady())
            {
                var target = Orbwalker.GetTarget();

                if (target != null)
                    QSwitch((AIO_Func.CountEnemyMinionsInRange(target.Position, 200) >= 2));
            }

            if (W.CanCast(Mobs[0]) && AIO_Menu.Champion.Jungleclear.UseW)
                W.Cast(Mobs[0]);
        }
        
        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (R.IsReady())
                damage += R.GetDamage2(enemy,4);

            return damage;
        }
    }
}