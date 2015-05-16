using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace ALL_In_One.champions
{
    class _TwistedFate
    {
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }//메뉴에있는 오브워커(ALL_IN_ONE_Menu.Orbwalker)를 쓰기편하게 오브젝트명 Orbwalker로 단축한것.
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }//Player오브젝트 = 말그대로 플레이어 챔피언입니다. 이 오브젝트로 챔피언을 움직인다던지 스킬을 쓴다던지 다 됩니다.

        //**********************************************************
        //공동개발자용 주석 문제가 있으면 언제든지 Skype: LSxcsoft
        //***********************************************************

        //스펠 변수 선언.
        static Spell Q, W, E, R;

        public static void Load()//챔피언 로드부분. 게임 로딩이 끝나자마자 제일먼저 실행되는 부분입니다.
        {
            //스펠 설정

            //스펠슬롯, 스펠사거리, 데미지타입(마뎀, 물뎀, 고정뎀)
            Q = new Spell(SpellSlot.Q, 500f, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 600f, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R);

            //스펠 프리딕션 설정

            //Q가 스킬샷(투사체)인경우 설정하는 예제
            //스킬시전전 딜레이, 스킬샷범위(두께), 투사체속도, 미니언에 막히는가안막히는가(막히면 true,안막히면 false)
            Q.SetSkillshot(0.25f, 50f, 2000f, true, SkillshotType.SkillshotLine);

            Q.SetCharged(" ", " ", 750, 1550, 1.5f);
            Q.SetTargetted(0.25f, 2000f);

            //메뉴에 아이템추가. ALL_IN_ONE_Menu 클래스로 간편하게 만들어놨음 아래처럼 필요한 옵션만 추가하면 되고, 문제있으면 저한테 물어보세요.

            //메인메뉴.서브메뉴.메소드 혹은 함수명();

            AIO_Menu.Champion.Combo.addUseQ();//Combo서브메뉴에 Use Q 옵션 추가
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();

            AIO_Menu.Champion.Harass.addUseQ();//Harass서브메뉴에 Use Q 옵션 추가
            AIO_Menu.Champion.Harass.addUseW();
            AIO_Menu.Champion.Harass.addUseE();
            AIO_Menu.Champion.Harass.addUseR();

            AIO_Menu.Champion.Laneclear.addUseQ();//..위와 같음
            AIO_Menu.Champion.Laneclear.addUseW();
            AIO_Menu.Champion.Laneclear.addUseE();
            AIO_Menu.Champion.Laneclear.addUseR();

            AIO_Menu.Champion.Jungleclear.addUseQ();//..위와 같음
            AIO_Menu.Champion.Jungleclear.addUseW();
            AIO_Menu.Champion.Jungleclear.addUseE();
            AIO_Menu.Champion.Jungleclear.addUseR();

            AIO_Menu.Champion.Misc.addUseKillsteal();//Misc서브메뉴에 Use Killsteal 옵션 추가
            AIO_Menu.Champion.Misc.addUseAntiGapcloser();//Misc서브메뉴에 Use Anti-Gapcloser 옵션추가
            AIO_Menu.Champion.Misc.addUseInterrupter();//Misc서브메뉴에 Use Interrupter 옵션 추가.

            AIO_Menu.Champion.Drawings.addQrange();//Drawings서브메뉴에 Q Range 옵션추가.
            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addErange();
            AIO_Menu.Champion.Drawings.addRrange();

            // Drawings 서브메뉴에 데미지표시기 추가하는 메소드.
            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            //이벤트들 추가.
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            //0.01초 마다 발동하는 이벤트. 여기에 코드를 쓰면 0.01초마다 실행됩니다

            //플레이어가 죽어있는상태면 리턴 (return코드 아래부분 실행안한다는 뜻.)
            if (Player.IsDead)
                return;

            //이 부분은 건드릴 필요가 없음. 현재 사용자가 누르고있는 오브워커 버튼에따른 함수 호출.
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
            }

            //메인메뉴->Misc서브메뉴에서 Use Killsteal 옵션이 On인경우 킬스틸 함수 호출.
            if (AIO_Menu.Champion.Misc.UseKillsteal)
                Killsteal();
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            //그리기 이벤트입니다. 1초에 프레임수만큼 실행됨

            //플레이어가 죽어있는상태면 리턴 (return코드 아래부분 실행안한다는 뜻.)
            if (Player.IsDead)
                return;

            //Drawings 설정 정보를 변수에 불러오는겁니다.
            //사용하지 않는 옵션은 지우세요 인게임에서 오류납니다.
            var drawQ = AIO_Menu.Champion.Drawings.Qrange;
            var drawW = AIO_Menu.Champion.Drawings.Wrange;
            var drawE = AIO_Menu.Champion.Drawings.Erange;
            var drawR = AIO_Menu.Champion.Drawings.Rrange;

            //Q스펠이 준비상태(쿨타임아닌상태)이고 Q Range옵션이 On 이면 Q사거리를 플레이어 챔피언위치에다가 그리는겁니다. 이하동문
            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);

            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);

            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);

            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);
        }

        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            //안티갭클로저 이벤트. 적챔피언이 달라붙는 스킬을 사용할때마다 발동합니다.

            //misc서브메뉴에 Use Anti-Gapcloser옵션이 On이 아니거나, 플레이어가 죽은상태면 리턴
            if (!AIO_Menu.Champion.Misc.UseAntiGapcloser || Player.IsDead)
                return;

            //Q스펠을 gapcloser.Sender(달라붙는스킬을 시전한 챔피언)에게 사용할 수 있으면 Q스펠을 gapcloser.Sender에게 시전.
            if (Q.CanCast(gapcloser.Sender))
                Q.Cast(gapcloser.Sender);
        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            //인터럽터 이벤트. 카타리나R 피들스틱W 이런 채널링스킬들이 발동할때 이부분이 실행됩니다.

            //Misc서브메뉴에 Use Interrupter옵션이 On이 아니거나, 플레이어가 죽은상태이면 리턴
            if (!AIO_Menu.Champion.Misc.UseInterrupter || Player.IsDead)
                return;

            //Q스펠을 sender(채널링스킬을 시전한 챔피언)에게 사용할 수 있으면 Q스펠을 sender에게 시전.
            if (Q.CanCast(sender))
                Q.Cast(sender);
        }

        static void Combo()
        {
            //콤보모드. 인게임에서 스페이스바키를 누르면 아래코드가 실행되는겁니다.
            if (AIO_Menu.Champion.Combo.UseQ && Q.IsReady())
            { }

            if (AIO_Menu.Champion.Combo.UseW && W.IsReady())
            { }

            if (AIO_Menu.Champion.Combo.UseE && E.IsReady())
            { }

            if (AIO_Menu.Champion.Combo.UseR && R.IsReady())
            { }
        }

        static void Harass()
        {
            //하래스모드. 인게임에서 C키를 누르면 아래코드가 실행되는겁니다.
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Harass.IfMana))
                return;

            if (AIO_Menu.Champion.Harass.UseQ && Q.IsReady())
            { }

            if (AIO_Menu.Champion.Harass.UseW && W.IsReady())
            { }

            if (AIO_Menu.Champion.Harass.UseE && E.IsReady())
            { }

            if (AIO_Menu.Champion.Harass.UseR && R.IsReady())
            { }
        }

        static void Laneclear()
        {
            //래인클리어모드. 인게임에서 V키를 누르면 아래코드가 실행되는겁니다.
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Laneclear.IfMana))
                return;

            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (AIO_Menu.Champion.Laneclear.UseQ && Q.IsReady())
            { }

            if (AIO_Menu.Champion.Laneclear.UseW && W.IsReady())
            { }

            if (AIO_Menu.Champion.Laneclear.UseE && E.IsReady())
            { }

            if (AIO_Menu.Champion.Laneclear.UseR && R.IsReady())
            { }
        }

        static void Jungleclear()
        {
            //정글클리어모드. 인게임에서 V키를 누르면 아래코드가 실행되는겁니다.
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

            if (AIO_Menu.Champion.Jungleclear.UseW && W.IsReady())
            {
                if (W.CanCast(Mobs.FirstOrDefault()))
                    W.Cast(Mobs.FirstOrDefault());
            }

            if (AIO_Menu.Champion.Jungleclear.UseE && E.IsReady())
            { }

            if (AIO_Menu.Champion.Jungleclear.UseR && R.IsReady())
            { }
        }

        static void Killsteal()
        {
            //킬스틸부분 적챔프가 킬각일때 스펠을 시전합니다.
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                //데미지가 있고 적챔프에게 시전할 수 있는 스펠만 남겨두고 지우세요. 인게임에서 오류납니다.

                //Q스펠을 target한테 사용할 수 있고 target이 Q데미지를 입으면 죽는 체력일 경우 Q스펠 target에게 시전. 이하동문
                if (Q.CanCast(target) && AIO_Func.isKillable(target, Q))
                    Q.Cast(target);

                if (W.CanCast(target) && AIO_Func.isKillable(target, W))
                    W.Cast(target);

                if (E.CanCast(target) && AIO_Func.isKillable(target, E))
                    E.Cast(target);

                if (R.CanCast(target) && AIO_Func.isKillable(target, R))
                    R.Cast(target);
            }
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            //콤보데미지 계산부분입니다. 여기에서 계산한 데미지가 데미지표시기에 출력되는겁니다.
            float damage = 0;

            //Q스펠이 준비상태일때 적 챔프에게 Q스펠 시전했을경우 입혀지는 데미지 추가. 이하동문
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
