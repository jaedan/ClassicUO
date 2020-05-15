#region license
// Copyright (C) 2020 ClassicUO Development Community on Github
// 
// This project is an alternative client for the game Ultima Online.
// The goal of this is to develop a lightweight client considering
// new technologies.
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
// 
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using ClassicUO.Configuration;
using ClassicUO.Game.Scenes;
using ClassicUO.Utility.Logging;
using Microsoft.Xna.Framework.Graphics;
using SDL2;

namespace ClassicUO.Game.Managers
{
    internal struct KeyCombination
    {
        public SDL.SDL_Keycode Key;
        public SDL.SDL_Keymod Mod;

        public override bool Equals(object obj)
        {
            if (obj is KeyCombination)
            {
                var kc = (KeyCombination)obj;

                SDL.SDL_Keymod mod = SDL.SDL_Keymod.KMOD_NONE;
                if ((kc.Mod & SDL.SDL_Keymod.KMOD_CTRL) != 0)
                    mod |= SDL.SDL_Keymod.KMOD_CTRL;
                if ((kc.Mod & SDL.SDL_Keymod.KMOD_ALT) != 0)
                    mod |= SDL.SDL_Keymod.KMOD_ALT;
                if ((kc.Mod & SDL.SDL_Keymod.KMOD_SHIFT) != 0)
                    mod |= SDL.SDL_Keymod.KMOD_SHIFT;
                if ((kc.Mod & SDL.SDL_Keymod.KMOD_NUM) != 0)
                    mod |= SDL.SDL_Keymod.KMOD_NUM;

                return kc.Key == Key && (Mod & mod) == Mod;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (int)Key;
        }
    }

    internal class KeyBind
    {
        public string Category;
        public string Action;
        public KeyCombination Key;
        public Action KeyDownHandler;
        public Action KeyUpHandler;
    }

    internal class KeyBindManager
    {
        private Dictionary<KeyCombination, KeyBind> _bindings = new Dictionary<KeyCombination, KeyBind>();

        private Dictionary<string, Dictionary<string, KeyBind>> _actions = new Dictionary<string, Dictionary<string, KeyBind>>();

        public KeyBindManager()
        {
            AddAction("Magery", "Clumsy", () => GameActions.CastSpell(1));
            AddAction("Magery", "Create Food", () => GameActions.CastSpell(2));
            AddAction("Magery", "Feeblemind", () => GameActions.CastSpell(3));
            AddAction("Magery", "Heal", () => GameActions.CastSpell(4));
            AddAction("Magery", "Magic Arrow", () => GameActions.CastSpell(5));
            AddAction("Magery", "Night Sight", () => GameActions.CastSpell(6));
            AddAction("Magery", "Reactive Armor", () => GameActions.CastSpell(7));
            AddAction("Magery", "Weaken", () => GameActions.CastSpell(8));
            AddAction("Magery", "Agility", () => GameActions.CastSpell(9));
            AddAction("Magery", "Cunning", () => GameActions.CastSpell(10));
            AddAction("Magery", "Cure", () => GameActions.CastSpell(11));
            AddAction("Magery", "Harm", () => GameActions.CastSpell(12));
            AddAction("Magery", "Magic Trap", () => GameActions.CastSpell(13));
            AddAction("Magery", "Magic Untrap", () => GameActions.CastSpell(14));
            AddAction("Magery", "Protection", () => GameActions.CastSpell(15));
            AddAction("Magery", "Strength", () => GameActions.CastSpell(16));
            AddAction("Magery", "Bless", () => GameActions.CastSpell(17));
            AddAction("Magery", "Fireball", () => GameActions.CastSpell(18));
            AddAction("Magery", "Magic Lock", () => GameActions.CastSpell(19));
            AddAction("Magery", "Posion", () => GameActions.CastSpell(20));
            AddAction("Magery", "Telekinesis", () => GameActions.CastSpell(21));
            AddAction("Magery", "Teleport", () => GameActions.CastSpell(22));
            AddAction("Magery", "Unlock", () => GameActions.CastSpell(23));
            AddAction("Magery", "Wall Of Stone", () => GameActions.CastSpell(24));
            AddAction("Magery", "Arch Cure", () => GameActions.CastSpell(25));
            AddAction("Magery", "Arch Protection", () => GameActions.CastSpell(26));
            AddAction("Magery", "Curse", () => GameActions.CastSpell(27));
            AddAction("Magery", "Fire Field", () => GameActions.CastSpell(28));
            AddAction("Magery", "Greater Heal", () => GameActions.CastSpell(29));
            AddAction("Magery", "Lightning", () => GameActions.CastSpell(30));
            AddAction("Magery", "Mana Drain", () => GameActions.CastSpell(31));
            AddAction("Magery", "Recall", () => GameActions.CastSpell(32));
            AddAction("Magery", "Blade Spirits", () => GameActions.CastSpell(33));
            AddAction("Magery", "Dispel Field", () => GameActions.CastSpell(34));
            AddAction("Magery", "Incognito", () => GameActions.CastSpell(35));
            AddAction("Magery", "Magic Reflection", () => GameActions.CastSpell(36));
            AddAction("Magery", "Mind Blast", () => GameActions.CastSpell(37));
            AddAction("Magery", "Paralyze", () => GameActions.CastSpell(38));
            AddAction("Magery", "Poison Field", () => GameActions.CastSpell(39));
            AddAction("Magery", "Summon Creature", () => GameActions.CastSpell(40));
            AddAction("Magery", "Dispel", () => GameActions.CastSpell(41));
            AddAction("Magery", "Energy Bolt", () => GameActions.CastSpell(42));
            AddAction("Magery", "Explosion", () => GameActions.CastSpell(43));
            AddAction("Magery", "Invisibility", () => GameActions.CastSpell(44));
            AddAction("Magery", "Mark", () => GameActions.CastSpell(45));
            AddAction("Magery", "Mass Curse", () => GameActions.CastSpell(46));
            AddAction("Magery", "Paralyze Field", () => GameActions.CastSpell(47));
            AddAction("Magery", "Reveal", () => GameActions.CastSpell(48));
            AddAction("Magery", "Chain Lightning", () => GameActions.CastSpell(49));
            AddAction("Magery", "Energy Field", () => GameActions.CastSpell(50));
            AddAction("Magery", "Flamestrike", () => GameActions.CastSpell(51));
            AddAction("Magery", "Gate Travel", () => GameActions.CastSpell(52));
            AddAction("Magery", "Mana Vampire", () => GameActions.CastSpell(53));
            AddAction("Magery", "Mass Dispel", () => GameActions.CastSpell(54));
            AddAction("Magery", "Meteor Swam", () => GameActions.CastSpell(55));
            AddAction("Magery", "Polymorph", () => GameActions.CastSpell(56));
            AddAction("Magery", "Earthquake", () => GameActions.CastSpell(57));
            AddAction("Magery", "Energy Vortex", () => GameActions.CastSpell(58));
            AddAction("Magery", "Resurrection", () => GameActions.CastSpell(59));
            AddAction("Magery", "Air Elemental", () => GameActions.CastSpell(60));
            AddAction("Magery", "Summon Daemon", () => GameActions.CastSpell(61));
            AddAction("Magery", "Earth Elemental", () => GameActions.CastSpell(62));
            AddAction("Magery", "Fire Elemental", () => GameActions.CastSpell(63));
            AddAction("Magery", "Water Elemental", () => GameActions.CastSpell(64));

            AddAction("Necromancy", "Animated Dead", () => GameActions.CastSpell(101));
            AddAction("Necromancy", "Blood Oath", () => GameActions.CastSpell(102));
            AddAction("Necromancy", "Corpse Skin", () => GameActions.CastSpell(103));
            AddAction("Necromancy", "Curse Weapon", () => GameActions.CastSpell(104));
            AddAction("Necromancy", "Evil Omen", () => GameActions.CastSpell(105));
            AddAction("Necromancy", "Horrific Beast", () => GameActions.CastSpell(106));
            AddAction("Necromancy", "Lich Form", () => GameActions.CastSpell(107));
            AddAction("Necromancy", "Mind Rot", () => GameActions.CastSpell(108));
            AddAction("Necromancy", "Pain Spike", () => GameActions.CastSpell(109));
            AddAction("Necromancy", "Poison Strike", () => GameActions.CastSpell(110));
            AddAction("Necromancy", "Strangle", () => GameActions.CastSpell(111));
            AddAction("Necromancy", "Summon Familiar", () => GameActions.CastSpell(112));
            AddAction("Necromancy", "Vampiric Embrace", () => GameActions.CastSpell(113));
            AddAction("Necromancy", "Vangeful Spririt", () => GameActions.CastSpell(114));
            AddAction("Necromancy", "Wither", () => GameActions.CastSpell(115));
            AddAction("Necromancy", "Wraith Form", () => GameActions.CastSpell(116));
            AddAction("Necromancy", "Exorcism", () => GameActions.CastSpell(117));

            AddAction("Chivalry", "Cleanse By Fire", () => GameActions.CastSpell(201));
            AddAction("Chivalry", "Close Wounds", () => GameActions.CastSpell(202));
            AddAction("Chivalry", "Consecrate Weapon", () => GameActions.CastSpell(203));
            AddAction("Chivalry", "Dispel Evil", () => GameActions.CastSpell(204));
            AddAction("Chivalry", "Divine Fury", () => GameActions.CastSpell(205));
            AddAction("Chivalry", "Enemy Of One", () => GameActions.CastSpell(206));
            AddAction("Chivalry", "Holy Light", () => GameActions.CastSpell(207));
            AddAction("Chivalry", "Noble Sacrifice", () => GameActions.CastSpell(208));
            AddAction("Chivalry", "Remove Curse", () => GameActions.CastSpell(209));
            AddAction("Chivalry", "Sacred Journey", () => GameActions.CastSpell(210));


            AddAction("Bushido", "Honorable Execution", () => GameActions.CastSpell(401));
            AddAction("Bushido", "Confidence", () => GameActions.CastSpell(402));
            AddAction("Bushido", "Evasion", () => GameActions.CastSpell(403));
            AddAction("Bushido", "Counter Attack", () => GameActions.CastSpell(404));
            AddAction("Bushido", "Lightning Strike", () => GameActions.CastSpell(405));
            AddAction("Bushido", "Momentum Strike", () => GameActions.CastSpell(406));


            AddAction("Ninjitsu", "Focus Attack", () => GameActions.CastSpell(501));
            AddAction("Ninjitsu", "Death Strike", () => GameActions.CastSpell(502));
            AddAction("Ninjitsu", "Animal Form", () => GameActions.CastSpell(503));
            AddAction("Ninjitsu", "Ki Attack", () => GameActions.CastSpell(504));
            AddAction("Ninjitsu", "Surprise Attack", () => GameActions.CastSpell(505));
            AddAction("Ninjitsu", "Backstab", () => GameActions.CastSpell(506));
            AddAction("Ninjitsu", "Shadowjump", () => GameActions.CastSpell(507));
            AddAction("Ninjitsu", "Mirror Image", () => GameActions.CastSpell(508));


            AddAction("Spellweaving", "Arcane Circle", () => GameActions.CastSpell(601));
            AddAction("Spellweaving", "Gift Of Renewal", () => GameActions.CastSpell(602));
            AddAction("Spellweaving", "Immolating Weapon", () => GameActions.CastSpell(603));
            AddAction("Spellweaving", "Attune Weapon", () => GameActions.CastSpell(604));
            AddAction("Spellweaving", "Thunderstorm", () => GameActions.CastSpell(605));
            AddAction("Spellweaving", "Natures Fury", () => GameActions.CastSpell(606));
            AddAction("Spellweaving", "Summon Fey", () => GameActions.CastSpell(607));
            AddAction("Spellweaving", "Summon Fiend", () => GameActions.CastSpell(608));
            AddAction("Spellweaving", "Reaper Form", () => GameActions.CastSpell(609));
            AddAction("Spellweaving", "Wild Fire", () => GameActions.CastSpell(610));
            AddAction("Spellweaving", "Essence Of Wind", () => GameActions.CastSpell(611));
            AddAction("Spellweaving", "Dryad Allure", () => GameActions.CastSpell(612));
            AddAction("Spellweaving", "Ethereal Voyage", () => GameActions.CastSpell(613));
            AddAction("Spellweaving", "Word Of Death", () => GameActions.CastSpell(614));
            AddAction("Spellweaving", "Gift Of Life", () => GameActions.CastSpell(615));


            AddAction("Mysticism", "Nether Bolt", () => GameActions.CastSpell(678));
            AddAction("Mysticism", "Healing Stone", () => GameActions.CastSpell(679));
            AddAction("Mysticism", "Purge Magic", () => GameActions.CastSpell(680));
            AddAction("Mysticism", "Enchant", () => GameActions.CastSpell(681));
            AddAction("Mysticism", "Sleep", () => GameActions.CastSpell(682));
            AddAction("Mysticism", "Eagle Strike", () => GameActions.CastSpell(683));
            AddAction("Mysticism", "Animated Weapon", () => GameActions.CastSpell(684));
            AddAction("Mysticism", "Stone Form", () => GameActions.CastSpell(685));
            AddAction("Mysticism", "Spell Trigger", () => GameActions.CastSpell(686));
            AddAction("Mysticism", "Mass Sleep", () => GameActions.CastSpell(687));
            AddAction("Mysticism", "Cleansing Winds", () => GameActions.CastSpell(688));
            AddAction("Mysticism", "Bombard", () => GameActions.CastSpell(689));
            AddAction("Mysticism", "Spell Plague", () => GameActions.CastSpell(690));
            AddAction("Mysticism", "Hail Storm", () => GameActions.CastSpell(691));
            AddAction("Mysticism", "Nether Cyclone", () => GameActions.CastSpell(692));
            AddAction("Mysticism", "Rising Colossus", () => GameActions.CastSpell(693));


            AddAction("Barding", "Inspire", () => GameActions.CastSpell(701));
            AddAction("Barding", "Invigorate", () => GameActions.CastSpell(702));
            AddAction("Barding", "Resilience", () => GameActions.CastSpell(703));
            AddAction("Barding", "Perseverance", () => GameActions.CastSpell(704));
            AddAction("Barding", "Tribulation", () => GameActions.CastSpell(705));
            AddAction("Barding", "Despair", () => GameActions.CastSpell(706));

            AddAction("Skills", "Anatomy", () => GameActions.UseSkill(1));
            AddAction("Skills", "Animal Lore", () => GameActions.UseSkill(2));
            AddAction("Skills", "Animal Taming", () => GameActions.UseSkill(35));
            AddAction("Skills", "Arms Lore", () => GameActions.UseSkill(4));
            AddAction("Skills", "Begging", () => GameActions.UseSkill(6));
            AddAction("Skills", "Cartography", () => GameActions.UseSkill(12));
            AddAction("Skills", "Detecting Hidden", () => GameActions.UseSkill(14));
            AddAction("Skills", "Enticement", () => GameActions.UseSkill(15));
            AddAction("Skills", "Evaluating Intelligence", () => GameActions.UseSkill(16));
            AddAction("Skills", "Forensic Evaluation", () => GameActions.UseSkill(19));
            AddAction("Skills", "Hiding", () => GameActions.UseSkill(21));
            AddAction("Skills", "Imbuing", () => GameActions.UseSkill(56));
            AddAction("Skills", "Inscription", () => GameActions.UseSkill(23));
            AddAction("Skills", "Item Identification", () => GameActions.UseSkill(3));
            AddAction("Skills", "Meditation", () => GameActions.UseSkill(46));
            AddAction("Skills", "Peacemaking", () => GameActions.UseSkill(9));
            AddAction("Skills", "Poisoning", () => GameActions.UseSkill(30));
            AddAction("Skills", "Provocation", () => GameActions.UseSkill(22));
            AddAction("Skills", "Remove Trap", () => GameActions.UseSkill(48));
            AddAction("Skills", "Spirit Speak", () => GameActions.UseSkill(32));
            AddAction("Skills", "Stealing", () => GameActions.UseSkill(33));
            AddAction("Skills", "Stealth", () => GameActions.UseSkill(47));
            AddAction("Skills", "Taste Identification", () => GameActions.UseSkill(36));
            AddAction("Skills", "Tracking", () => GameActions.UseSkill(38));

            AddAction("Controls", "Walk Northwest (Up)", 
                      () => Client.Game.GetScene<GameScene>().OnMovement(Data.Direction.Up, true),
                      () => Client.Game.GetScene<GameScene>().OnMovement(Data.Direction.Up, false));
            AddAction("Controls", "Walk Southeast (Down)",
                      () => Client.Game.GetScene<GameScene>().OnMovement(Data.Direction.Down, true),
                      () => Client.Game.GetScene<GameScene>().OnMovement(Data.Direction.Down, false));
            AddAction("Controls", "Walk Southwest (Left)",
                      () => Client.Game.GetScene<GameScene>().OnMovement(Data.Direction.Left, true),
                      () => Client.Game.GetScene<GameScene>().OnMovement(Data.Direction.Left, false));
            AddAction("Controls", "Walk Northeast (Right)",
                      () => Client.Game.GetScene<GameScene>().OnMovement(Data.Direction.Right, true),
                      () => Client.Game.GetScene<GameScene>().OnMovement(Data.Direction.Right, false));
            AddAction("Controls", "AlwaysRun", () => ProfileManager.Current.AlwaysRun = true);

            AddAction("UI", "All Names", () => GameActions.AllNames());
            AddAction("UI", "Open Settings", () => GameActions.OpenSettings());
            AddAction("UI", "Open Paperdoll", () => GameActions.OpenPaperdoll(World.Player));
            AddAction("UI", "Open Status", () => GameActions.OpenStatusBar());
            AddAction("UI", "Open Journal", () => GameActions.OpenJournal());
            AddAction("UI", "Open Skills", () => GameActions.OpenSkills());

            /*
            AddAction("UI", "OpenMageSpellbook,
            AddAction("UI", "OpenNecroSpellbook,
            AddAction("UI", "OpenChivaSpellbook,
            AddAction("UI", "OpenBushidoSpellbook,
            AddAction("UI", "OpenNinjaSpellbook,
            AddAction("UI", "OpenSpellweaverSpellbook,
            AddAction("UI", "OpenMysticSpellbook,
            AddAction("UI", "OpenRacialAbilitiesBook,
            AddAction("UI", "OpenChat,
            AddAction("UI", "OpenBackpack,
            AddAction("UI", "OpenMinimap,
            AddAction("UI", "OpenParty,
            AddAction("UI", "OpenPartyChat,
            AddAction("UI", "OpenGuild,
            AddAction("UI", "OpenQuestLog,
            AddAction("UI", "ToggleBuffGump,
            AddAction("UI", "QuitGame,
            AddAction("UI", "SaveGumps,

            AddAction("Abilities", "UsePrimaryAbility,
            AddAction("Abilities", "UseSecondaryAbility,
            AddAction("Abilities", "ClearCurrentAbility,
            AddAction("Abilities", "ToggleGargoyleFly,

            AddAction("Items", "UseSelectedItem,
            AddAction("Items", "UseCurrentTarget,
            AddAction("Items", "BandageSelf,
            AddAction("Items", "BandageTarget,

            AddAction("Speech", "Say,
            AddAction("Speech", "Emote,
            AddAction("Speech", "Whisper,
            AddAction("Speech", "Yell,

            AddAction("Targeting", "TargetNext,
            AddAction("Targeting", "TargetClosest,
            AddAction("Targeting", "TargetSelf,
            AddAction("Targeting", "SelectNext,
            AddAction("Targeting", "SelectPrevious,
            AddAction("Targeting", "SelectNearest,
            */

            AddAction("Combat", "Warmode (Toggle)", () => GameActions.ToggleWarMode());
            AddAction("Combat", "Warmode (Hold)", 
                () => { if (!World.Player.InWarMode) GameActions.RequestWarMode(true); },
                () => { if (World.Player.InWarMode) GameActions.RequestWarMode(false); }); // TODO: This is probably messed up.

            /*
            AddAction("Combat", "AttackLast,
            AddAction("Combat", "AttackSelected,
            AddAction("Combat", "ArmDisarm,
            AddAction("Combat", "EquipLastWeapon,
            */

            AddAction("Emotes", "Bow", () => GameActions.EmoteAction("bow"));
            AddAction("Emotes", "Salute", () => GameActions.EmoteAction("salute"));
        }


        public bool Bind(string category, string action, SDL.SDL_Keycode key, SDL.SDL_Keymod mod)
        {
            // TODO: If the user tries to bind a key to an action that doesn't exist, make one. This
            // resolves ordering issues.

            if (!_actions.TryGetValue(category, out var actions))
                return false;

            if (!actions.TryGetValue(action, out var keybind))
                return false;

            keybind.Key = new KeyCombination() { Key = key, Mod = mod };
            _bindings[keybind.Key] = keybind;

            return true;
        }

        public void Unbind(string category, string action)
        {
            if (!_actions.TryGetValue(category, out var actions))
                return;

            if (!actions.TryGetValue(action, out var keybind))
                return;

            if (keybind.Key.Key == SDL.SDL_Keycode.SDLK_UNKNOWN)
                return;

            _bindings.Remove(keybind.Key);
        }

        public KeyCombination GetKeyCombination(string category, string action)
        {
            if (!_actions.TryGetValue(category, out var actions))
                return new KeyCombination();

            if (!actions.TryGetValue(action, out var keybind))
                return new KeyCombination();

            return keybind.Key;
        }

        public bool IsBound(SDL.SDL_Keycode key, SDL.SDL_Keymod mod)
        {
            var keyCombo = new KeyCombination() { Key = key, Mod = mod };

            if (!_bindings.TryGetValue(keyCombo, out var keybind))
                return false;

            if (keybind.KeyDownHandler == null)
                return false;

            return true;
        }

        public bool OnKeyDown(SDL.SDL_Keycode key, SDL.SDL_Keymod mod)
        {
            var keyCombo = new KeyCombination() { Key = key, Mod = mod };

            if (!_bindings.TryGetValue(keyCombo, out var keybind))
                return false;

            if (keybind.KeyDownHandler == null)
                return false;

            keybind.KeyDownHandler();
            return true;
        }

        public bool OnKeyUp(SDL.SDL_Keycode key, SDL.SDL_Keymod mod)
        {
            var keyCombo = new KeyCombination() { Key = key, Mod = mod };

            if (!_bindings.TryGetValue(keyCombo, out var keybind))
                return false;

            if (keybind.KeyUpHandler == null)
                return false;

            keybind.KeyUpHandler();
            return true;
        }

        public IEnumerable<string> GetCategories()
        {
            return new HashSet<string>(_actions.Keys);
        }

        public IEnumerable<string> GetActionsInCategory(string category)
        {
            if (!_actions.TryGetValue(category, out var actions))
                return new List<string>();

            return new List<string>(actions.Keys);
        }

        public bool AddAction(string category, string action, Action keyDownHandler, Action keyUpHandler = null)
        {
            Dictionary<string, KeyBind> actions;
            if (!_actions.TryGetValue(category, out actions))
            {
                actions = new Dictionary<string, KeyBind>();
                _actions[category] = actions;
            }

            if (actions.ContainsKey(action))
                return false;

            actions[action] = new KeyBind() { Category = category, Action = action, KeyDownHandler = keyDownHandler, KeyUpHandler = keyUpHandler };
            return true;
        }

        public void RemoveAction(string category, string action)
        {
            _actions.Remove(action);

            if (!_actions.TryGetValue(category, out var actions))
                return;

            if (actions.Remove(action))
            {
                if (actions.Count == 0)
                    _actions.Remove(category);
            }
        }

        public void SetDefaultKeyBindings()
        {
            Bind("UI", "Open Paperdoll", SDL.SDL_Keycode.SDLK_p, SDL.SDL_Keymod.KMOD_ALT);
            Bind("UI", "Open Options", SDL.SDL_Keycode.SDLK_o, SDL.SDL_Keymod.KMOD_ALT);
            Bind("UI", "Open Journal", SDL.SDL_Keycode.SDLK_j, SDL.SDL_Keymod.KMOD_ALT);
            Bind("UI", "Open Backpack", SDL.SDL_Keycode.SDLK_i, SDL.SDL_Keymod.KMOD_ALT);
            Bind("UI", "Open Radar", SDL.SDL_Keycode.SDLK_r, SDL.SDL_Keymod.KMOD_ALT);

            Bind("Combat", "Warmode (Toggle)", SDL.SDL_Keycode.SDLK_TAB, SDL.SDL_Keymod.KMOD_NONE);

            Bind("Emotes", "Bow", SDL.SDL_Keycode.SDLK_b, SDL.SDL_Keymod.KMOD_CTRL);
            Bind("Emotes", "Salute", SDL.SDL_Keycode.SDLK_s, SDL.SDL_Keymod.KMOD_CTRL);
        }

        public void Save()
        {
            string path = Path.Combine(CUOEnviroment.ExecutablePath, "Data", "Profiles", ProfileManager.Current.Username, ProfileManager.Current.ServerName, ProfileManager.Current.CharacterName, "keybinds.xml");

            using (XmlTextWriter xml = new XmlTextWriter(path, Encoding.UTF8)
            {
                Formatting = Formatting.Indented,
                IndentChar = '\t',
                Indentation = 1
            })
            {
                xml.WriteStartDocument(true);
                xml.WriteStartElement("keybinds");

                foreach (var i in _actions)
                {
                    var category = i.Key;
                    foreach (var j in i.Value)
                    {
                        var action = j.Key;
                        var keybind = j.Value;

                        if (keybind.Key.Key == SDL.SDL_Keycode.SDLK_UNKNOWN)
                            continue;

                        var key = (uint)keybind.Key.Key;
                        var mod = (uint)keybind.Key.Mod;

                        xml.WriteStartElement("keybind");
                        xml.WriteAttributeString("action", action);
                        xml.WriteAttributeString("category", category);
                        xml.WriteAttributeString("key", key.ToString());
                        xml.WriteAttributeString("mod", mod.ToString());
                        xml.WriteEndElement();
                    }
                }

                xml.WriteEndElement();
                xml.WriteEndDocument();
            }
        }

        public void Load()
        {
            string path = Path.Combine(CUOEnviroment.ExecutablePath, "Data", "Profiles", ProfileManager.Current.Username, ProfileManager.Current.ServerName, ProfileManager.Current.CharacterName, "keybinds.xml");

            if (!File.Exists(path))
            {
                Log.Trace("No keybinds.xml file. Creating one.");
                SetDefaultKeyBindings();
                Save();
                return;
            }

            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(path);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return;
            }

            XmlElement root = doc["keybinds"];

            if (root != null)
            {
                foreach (XmlElement xml in root.GetElementsByTagName("keybind"))
                {
                    var action = xml.GetAttribute("action");
                    var category = xml.GetAttribute("category");
                    var key = (SDL.SDL_Keycode)uint.Parse(xml.GetAttribute("key"));
                    var mod = (SDL.SDL_Keymod)uint.Parse(xml.GetAttribute("mod"));

                    Bind(category, action, key, mod);
                }
            }
        }
    }
}