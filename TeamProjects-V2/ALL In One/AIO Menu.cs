using System;
using System.Linq;
using System.Collections.Generic;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One
{
    class AIO_Menu
    {
        internal static Menu MainMenu_Manual;
        internal static Orbwalking.Orbwalker Orbwalker;
        
        internal static void initialize()
        {
            MainMenu_Manual = new Menu("ALL In One", "Teamproject_ALLINONE", true);
            MainMenu_Manual.AddToMainMenu();
        }

        internal static void addSubMenu(string DisplayName)
        {
            MainMenu_Manual.AddSubMenu(new Menu(DisplayName, DisplayName));
        }

        internal static void addSubMenu(string Name, string DisplayName)
        {
            MainMenu_Manual.AddSubMenu(new Menu(DisplayName, Name));
        }

        internal static void addItem(string DisplayName, object Value, bool ChampUniq = false)
        {
            if(Value == null)
            {
                MainMenu_Manual.AddItem(new MenuItem(DisplayName, DisplayName, ChampUniq));
                return;
            }

            MainMenu_Manual.AddItem(new MenuItem(DisplayName, DisplayName, ChampUniq)).SetValue(Value);
        }

        internal class Champion
        {
            internal static void addItem(string DisplayName, object Value, bool ChampUniq = true)
            {
                if (Value == null)
                {
                    MainMenu_Manual.AddItem(new MenuItem(DisplayName, DisplayName, ChampUniq));
                    return;
                }

                MainMenu_Manual.SubMenu("Champion").AddItem(new MenuItem(DisplayName, DisplayName, ChampUniq)).SetValue(Value);
            }

            internal static void addOrbwalker()
            {
                Orbwalker = new Orbwalking.Orbwalker(MainMenu_Manual.SubMenu("Champion").AddSubMenu(new Menu("Orbwalker", "Orbwalker")));
            }

            internal static void addTargetSelector()
            {
                TargetSelector.AddToMenu(MainMenu_Manual.SubMenu("Champion").AddSubMenu(new Menu("Target Selector", "Target Selector")));
            }

            internal class Combo
            {
                internal static void addItem(string DisplayName, object Value, bool ChampUniq = true)
                {
                    if (Value == null)
                    {
                        MainMenu_Manual.SubMenu("Champion").SubMenu("Combo").AddItem(new MenuItem("Combo." + DisplayName, DisplayName, ChampUniq));
                        return;
                    }

                    MainMenu_Manual.SubMenu("Champion").SubMenu("Combo").AddItem(new MenuItem("Combo." + DisplayName, DisplayName, ChampUniq)).SetValue(Value);
                }

                internal static bool getBoolValue(string DisplayName, bool ChampUniq = true)
                {
                    return MainMenu_Manual.Item("Combo." + DisplayName, ChampUniq).GetValue<bool>();
                }

                internal static Slider getSliderValue(string DisplayName, bool ChampUniq = true)
                {
                    return MainMenu_Manual.Item("Combo." + DisplayName, ChampUniq).GetValue<Slider>();
                }

                internal static void addUseQ(bool Enabled = true)
                {
                    addItem("Use Q", Enabled);
                }

                internal static void addUseW(bool Enabled = true)
                {
                    addItem("Use W", Enabled);
                }

                internal static void addUseE(bool Enabled = true)
                {
                    addItem("Use E", Enabled);
                }

                internal static void addUseR(bool Enabled = true)
                {
                    addItem("Use R", Enabled);
                }

                internal static void addIfMana(int DefaultValue = 0)
                {
                    addItem("If Mana >", new Slider(DefaultValue));
                }

                internal static bool UseQ
                {
                    get
                    {
                        return getBoolValue("Use Q");
                    }
                }

                internal static bool UseW
                {
                    get
                    {
                        return getBoolValue("Use W");
                    }
                }

                internal static bool UseE
                {
                    get
                    {
                        return getBoolValue("Use E");
                    }
                }

                internal static bool UseR
                {
                    get
                    {
                        return getBoolValue("Use R");
                    }
                }

                internal static int IfMana
                {
                    get
                    {
                        return getSliderValue("If Mana >").Value;
                    }
                }
            }

            internal class Harass
            {
                internal static void addItem(string DisplayName, object Value, bool ChampUniq = true)
                {
                    if (Value == null)
                    {
                        MainMenu_Manual.SubMenu("Champion").SubMenu("Harass").AddItem(new MenuItem("Harass." + DisplayName, DisplayName, ChampUniq));
                        return;
                    }

                    MainMenu_Manual.SubMenu("Champion").SubMenu("Harass").AddItem(new MenuItem("Harass." + DisplayName, DisplayName, ChampUniq)).SetValue(Value);
                }

                internal static bool getBoolValue(string DisplayName, bool ChampUniq = true)
                {
                    return MainMenu_Manual.Item("Harass." + DisplayName, ChampUniq).GetValue<bool>();
                }

                internal static Slider getSliderValue(string DisplayName, bool ChampUniq = true)
                {
                    return MainMenu_Manual.Item("Harass." + DisplayName, ChampUniq).GetValue<Slider>();
                }

                internal static void addUseQ(bool Enabled = true)
                {
                    addItem("Use Q", Enabled);
                }

                internal static void addUseW(bool Enabled = true)
                {
                    addItem("Use W", Enabled);
                }

                internal static void addUseE(bool Enabled = true)
                {
                    addItem("Use E", Enabled);
                }

                internal static void addUseR(bool Enabled = true)
                {
                    addItem("Use R", Enabled);
                }

                internal static void addIfMana(int DefaultValue = 60)
                {
                    addItem("If Mana >", new Slider(DefaultValue));
                }

                internal static bool UseQ
                {
                    get
                    {
                        return getBoolValue("Use Q");
                    }
                }

                internal static bool UseW
                {
                    get
                    {
                        return getBoolValue("Use W");
                    }
                }

                internal static bool UseE
                {
                    get
                    {
                        return getBoolValue("Use E");
                    }
                }

                internal static bool UseR
                {
                    get
                    {
                        return getBoolValue("Use R");
                    }
                }

                internal static int IfMana
                {
                    get
                    {
                        return getSliderValue("If Mana >").Value;
                    }
                }
            }

            internal class Lasthit
            {
                internal static void addItem(string DisplayName, object Value, bool ChampUniq = true)
                {
                    if (Value == null)
                    {
                        MainMenu_Manual.SubMenu("Champion").SubMenu("Lasthit").AddItem(new MenuItem("Lasthit." + DisplayName, DisplayName, ChampUniq));
                        return;
                    }

                    MainMenu_Manual.SubMenu("Champion").SubMenu("Lasthit").AddItem(new MenuItem("Lasthit." + DisplayName, DisplayName, ChampUniq)).SetValue(Value);
                }

                internal static bool getBoolValue(string DisplayName, bool ChampUniq = true)
                {
                    return MainMenu_Manual.Item("Lasthit." + DisplayName, ChampUniq).GetValue<bool>();
                }

                internal static Slider getSliderValue(string DisplayName, bool ChampUniq = true)
                {
                    return MainMenu_Manual.Item("Lasthit." + DisplayName, ChampUniq).GetValue<Slider>();
                }

                internal static void addUseQ(bool Enabled = true)
                {
                    addItem("Use Q", Enabled);
                }

                internal static void addUseW(bool Enabled = true)
                {
                    addItem("Use W", Enabled);
                }

                internal static void addUseE(bool Enabled = true)
                {
                    addItem("Use E", Enabled);
                }

                internal static void addUseR(bool Enabled = true)
                {
                    addItem("Use R", Enabled);
                }

                internal static void addIfMana(int DefaultValue = 60)
                {
                    addItem("If Mana >", new Slider(DefaultValue));
                }

                internal static bool UseQ
                {
                    get
                    {
                        return getBoolValue("Use Q");
                    }
                }

                internal static bool UseW
                {
                    get
                    {
                        return getBoolValue("Use W");
                    }
                }

                internal static bool UseE
                {
                    get
                    {
                        return getBoolValue("Use E");
                    }
                }

                internal static bool UseR
                {
                    get
                    {
                        return getBoolValue("Use R");
                    }
                }

                internal static int IfMana
                {
                    get
                    {
                        return getSliderValue("If Mana >").Value;
                    }
                }
            }

            internal class Laneclear
            {
                internal static void addItem(string DisplayName, object Value, bool ChampUniq = true)
                {
                    if (Value == null)
                    {
                        MainMenu_Manual.SubMenu("Champion").SubMenu("Laneclear").AddItem(new MenuItem("Laneclear." + DisplayName, DisplayName, ChampUniq));
                        return;
                    }

                    MainMenu_Manual.SubMenu("Champion").SubMenu("Laneclear").AddItem(new MenuItem("Laneclear." + DisplayName, DisplayName, ChampUniq)).SetValue(Value);
                }

                internal static bool getBoolValue(string DisplayName, bool ChampUniq = true)
                {
                    return MainMenu_Manual.Item("Laneclear." + DisplayName, ChampUniq).GetValue<bool>();
                }

                internal static Slider getSliderValue(string DisplayName, bool ChampUniq = true)
                {
                    return MainMenu_Manual.Item("Laneclear." + DisplayName, ChampUniq).GetValue<Slider>();
                }

                internal static void addUseQ(bool Enabled = true)
                {
                    addItem("Use Q", Enabled);
                }

                internal static void addUseW(bool Enabled = true)
                {
                    addItem("Use W", Enabled);
                }

                internal static void addUseE(bool Enabled = true)
                {
                    addItem("Use E", Enabled);
                }

                internal static void addUseR(bool Enabled = true)
                {
                    addItem("Use R", Enabled);
                }

                internal static void addIfMana(int DefaultValue = 60)
                {
                    addItem("If Mana >", new Slider(DefaultValue));
                }

                internal static bool UseQ
                {
                    get
                    {
                        return getBoolValue("Use Q");
                    }
                }

                internal static bool UseW
                {
                    get
                    {
                        return getBoolValue("Use W");
                    }
                }

                internal static bool UseE
                {
                    get
                    {
                        return getBoolValue("Use E");
                    }
                }

                internal static bool UseR
                {
                    get
                    {
                        return getBoolValue("Use R");
                    }
                }

                internal static int IfMana
                {
                    get
                    {
                        return getSliderValue("If Mana >").Value;
                    }
                }
            }

            internal class Jungleclear
            {
                internal static void addItem(string DisplayName, object Value, bool ChampUniq = true)
                {
                    if (Value == null)
                    {
                        MainMenu_Manual.SubMenu("Champion").SubMenu("Jungleclear").AddItem(new MenuItem("Jungleclear." + DisplayName, DisplayName, ChampUniq));
                        return;
                    }

                    MainMenu_Manual.SubMenu("Champion").SubMenu("Jungleclear").AddItem(new MenuItem("Jungleclear." + DisplayName, DisplayName, ChampUniq)).SetValue(Value);
                }

                internal static bool getBoolValue(string DisplayName, bool ChampUniq = true)
                {
                    return MainMenu_Manual.Item("Jungleclear." + DisplayName, ChampUniq).GetValue<bool>();
                }

                internal static Slider getSliderValue(string DisplayName, bool ChampUniq = true)
                {
                    return MainMenu_Manual.Item("Jungleclear." + DisplayName, ChampUniq).GetValue<Slider>();
                }

                internal static void addUseQ(bool Enabled = true)
                {
                    addItem("Use Q", Enabled);
                }

                internal static void addUseW(bool Enabled = true)
                {
                    addItem("Use W", Enabled);
                }

                internal static void addUseE(bool Enabled = true)
                {
                    addItem("Use E", Enabled);
                }

                internal static void addUseR(bool Enabled = true)
                {
                    addItem("Use R", Enabled);
                }

                internal static void addIfMana(int DefaultValue = 20)
                {
                    addItem("If Mana >", new Slider(DefaultValue));
                }

                internal static bool UseQ
                {
                    get
                    {
                        return getBoolValue("Use Q");
                    }
                }

                internal static bool UseW
                {
                    get
                    {
                        return getBoolValue("Use W");
                    }
                }

                internal static bool UseE
                {
                    get
                    {
                        return getBoolValue("Use E");
                    }
                }

                internal static bool UseR
                {
                    get
                    {
                        return getBoolValue("Use R");
                    }
                }

                internal static int IfMana
                {
                    get
                    {
                        return getSliderValue("If Mana >").Value;
                    }
                }
            }
            

            internal class Flee
            {
                internal static void addItem(string DisplayName, object Value, bool ChampUniq = true)
                {
                    if (Value == null)
                    {
                        MainMenu_Manual.SubMenu("Champion").SubMenu("Flee").AddItem(new MenuItem("Flee." + DisplayName, DisplayName, ChampUniq));
                        return;
                    }

                    MainMenu_Manual.SubMenu("Champion").SubMenu("Flee").AddItem(new MenuItem("Flee." + DisplayName, DisplayName, ChampUniq)).SetValue(Value);
                }

                internal static bool getBoolValue(string DisplayName, bool ChampUniq = true)
                {
                    return MainMenu_Manual.Item("Flee." + DisplayName, ChampUniq).GetValue<bool>();
                }

                internal static Slider getSliderValue(string DisplayName, bool ChampUniq = true)
                {
                    return MainMenu_Manual.Item("Flee." + DisplayName, ChampUniq).GetValue<Slider>();
                }

                internal static void addUseQ(bool Enabled = true)
                {
                    addItem("Use Q", Enabled);
                }

                internal static void addUseW(bool Enabled = true)
                {
                    addItem("Use W", Enabled);
                }

                internal static void addUseE(bool Enabled = true)
                {
                    addItem("Use E", Enabled);
                }

                internal static void addUseR(bool Enabled = true)
                {
                    addItem("Use R", Enabled);
                }

                internal static void addIfMana(int DefaultValue = 10)
                {
                    addItem("If Mana >", new Slider(DefaultValue));
                }

                internal static bool UseQ
                {
                    get
                    {
                        return getBoolValue("Use Q");
                    }
                }

                internal static bool UseW
                {
                    get
                    {
                        return getBoolValue("Use W");
                    }
                }

                internal static bool UseE
                {
                    get
                    {
                        return getBoolValue("Use E");
                    }
                }

                internal static bool UseR
                {
                    get
                    {
                        return getBoolValue("Use R");
                    }
                }

                internal static int IfMana
                {
                    get
                    {
                        return getSliderValue("If Mana >").Value;
                    }
                }
            }

            internal class Misc
            {
                internal static void addItem(string DisplayName, object Value, bool ChampUniq = true)
                {
                    if (Value == null)
                    {
                        MainMenu_Manual.SubMenu("Champion").SubMenu("Misc").AddItem(new MenuItem("Misc." + DisplayName, DisplayName, ChampUniq));
                        return;
                    }

                    MainMenu_Manual.SubMenu("Champion").SubMenu("Misc").AddItem(new MenuItem("Misc." + DisplayName, DisplayName, ChampUniq)).SetValue(Value);
                }

                internal static bool getBoolValue(string DisplayName, bool ChampUniq = true)
                {
                    return MainMenu_Manual.Item("Misc." + DisplayName, ChampUniq).GetValue<bool>();
                }

                internal static Slider getSliderValue(string DisplayName, bool ChampUniq = true)
                {
                    return MainMenu_Manual.Item("Misc." + DisplayName, ChampUniq).GetValue<Slider>();
                }

                internal static StringList getStringListValue(string DisplayName, bool ChampUniq = true)
                {
                    return MainMenu_Manual.Item("Misc." + DisplayName, ChampUniq).GetValue<StringList>();
                }

                internal static KeyBind getKeyBIndValue(string DisplayName, bool ChampUniq = true)
                {
                    return MainMenu_Manual.Item("Misc." + DisplayName, ChampUniq).GetValue<KeyBind>();
                }

                internal static void addUseAntiGapcloser(bool Enabled = true)
                {
                    addItem("Use Anti-Gapcloser", Enabled);
                }

                internal static void addUseInterrupter(bool Enabled = true)
                {
                    addItem("Use Interrupter", Enabled);
                }

                internal static void addUseKillsteal(bool Enabled = true)
                {
                    addItem("Use Killsteal", Enabled);
                }

                internal static void addHitchanceSelector(HitChance defaultHitchance = HitChance.High)
                {
                    int defaultindex;

                    switch (defaultHitchance)
                    {
                        case HitChance.Low:
                            defaultindex = 0;
                            break;
                        case HitChance.Medium:
                            defaultindex = 1;
                            break;
                        case HitChance.High:
                            defaultindex = 2;
                            break;
                        case HitChance.VeryHigh:
                            defaultindex = 3;
                            break;
                        default:
                            defaultindex = 2;
                            break;
                    }

                    addItem("Hitchance", new StringList(new string[] { "Low", "Medium", "High", "Very High" }, defaultindex));
                }

                internal static bool UseAntiGapcloser
                {
                    get
                    {
                        return getBoolValue("Use Anti-Gapcloser");
                    }
                }

                internal static bool UseInterrupter
                {
                    get
                    {
                        return getBoolValue("Use Interrupter");
                    }
                }

                internal static bool UseKillsteal
                {
                    get
                    {
                        return getBoolValue("Use Killsteal");
                    }
                }

                internal static HitChance SelectedHitchance
                {
                    get
                    {
                        switch (getStringListValue("Hitchance").SelectedValue)
                        {
                            case "Low":
                                return HitChance.Low;
                            case "Medium":
                                return HitChance.Medium;
                            case "High":
                                return HitChance.High;
                            case "Very High":
                                return HitChance.VeryHigh;
                            default:
                                return HitChance.High;
                        }
                    }
                }
            }

            internal class Drawings
            {
                internal static void addItem(string DisplayName, object Value, bool ChampUniq = true)
                {
                    if (Value == null)
                    {
                        MainMenu_Manual.SubMenu("Champion").SubMenu("Drawings").AddItem(new MenuItem("Drawings." + DisplayName, DisplayName, ChampUniq));
                        return;
                    }

                    MainMenu_Manual.SubMenu("Champion").SubMenu("Drawings").AddItem(new MenuItem("Drawings." + DisplayName, DisplayName, ChampUniq)).SetValue(Value);
                }

                internal static bool getBoolValue(string DisplayName, bool ChampUniq = true)
                {
                    return MainMenu_Manual.Item("Drawings." + DisplayName, ChampUniq).GetValue<bool>();
                }

                internal static Slider getSliderValue(string DisplayName, bool ChampUniq = true)
                {
                    return MainMenu_Manual.Item("Drawings." + DisplayName, ChampUniq).GetValue<Slider>();
                }

                internal static Circle getCircleValue(string DisplayName, bool ChampUniq = true)
                {
                    return MainMenu_Manual.Item("Drawings." + DisplayName, ChampUniq).GetValue<Circle>();
                }

                internal static void addQrange(bool Enabled = true)
                {
                    addItem("Q Range", new Circle(Enabled, System.Drawing.Color.FromArgb(200, System.Drawing.Color.SpringGreen)));
                }

                internal static void addWrange(bool Enabled = true)
                {
                    addItem("W Range", new Circle(Enabled, System.Drawing.Color.FromArgb(200, System.Drawing.Color.SpringGreen)));
                }

                internal static void addErange(bool Enabled = true)
                {
                    addItem("E Range", new Circle(Enabled, System.Drawing.Color.FromArgb(200, System.Drawing.Color.SpringGreen)));
                }

                internal static void addRrange(bool Enabled = true)
                {
                    addItem("R Range", new Circle(Enabled, System.Drawing.Color.FromArgb(200, System.Drawing.Color.SpringGreen)));
                }

                internal static Circle Qrange
                {
                    get
                    {
                        return getCircleValue("Q Range");
                    }
                }

                internal static Circle Wrange
                {
                    get
                    {
                        return getCircleValue("W Range");
                    }
                }

                internal static Circle Erange
                {
                    get
                    {
                        return getCircleValue("E Range");
                    }
                }

                internal static Circle Rrange
                {
                    get
                    {
                        return getCircleValue("R Range");
                    }
                }

                internal static void addDamageIndicator(DamageIndicator.DamageToUnitDelegate damage)
                {
                    var drawDamageMenu = new MenuItem("Draw_Damage", "DamageIndicator", true).SetValue(true);
                    var drawDamageFill = new MenuItem("Draw_Fill", "DamageIndicator Fill", true).SetValue(new Circle(true, System.Drawing.Color.FromArgb(100, 255, 228, 0)));

                    MainMenu_Manual.SubMenu("Champion").SubMenu("Drawings").AddItem(drawDamageMenu);
                    MainMenu_Manual.SubMenu("Champion").SubMenu("Drawings").AddItem(drawDamageFill);

                    DamageIndicator.DamageToUnit = damage;
                    DamageIndicator.Enabled = drawDamageMenu.GetValue<bool>();
                    DamageIndicator.Fill = drawDamageFill.GetValue<Circle>().Active;
                    DamageIndicator.FillColor = drawDamageFill.GetValue<Circle>().Color;

                    drawDamageMenu.ValueChanged +=
                    delegate(object sender, OnValueChangeEventArgs eventArgs)
                    {
                        DamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
                    };

                    drawDamageFill.ValueChanged +=
                    delegate(object sender, OnValueChangeEventArgs eventArgs)
                    {
                        DamageIndicator.Fill = eventArgs.GetNewValue<Circle>().Active;
                        DamageIndicator.FillColor = eventArgs.GetNewValue<Circle>().Color;
                    };
                }
            }
        }
    }
}
