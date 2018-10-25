﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassicUO.Game.Data
{
    static class SpellsNecromancy
    {
        static readonly Dictionary<int, SpellDefinition> _spellsDict;
        static readonly List<SpellDefinition> _spells;

        static SpellsNecromancy()
        {
            _spellsDict = new Dictionary<int, SpellDefinition>()
            {
                // Spell List
                { 1, new SpellDefinition("Animate Dead", 101, 0x5000, "Uus Corp",23, 40, Reagents.DaemonBlood, Reagents.GraveDust) },
                { 2, new SpellDefinition("Blood Oath", 102, 0x5001, "In Jux Mani Xen",13,20, Reagents.DaemonBlood) },
                { 3, new SpellDefinition("Corpse Skin", 103, 0x5002, "In Agle Corp Ylem",11,20, Reagents.BatWing, Reagents.GraveDust) },
                { 4, new SpellDefinition("Curse Weapon", 104, 0x5003, "An Sanct Gra Char",7,0, Reagents.PigIron) },
                { 5, new SpellDefinition("Evil Omen", 105, 0x5004, "Pas Tym An Sanct",11,20, Reagents.BatWing, Reagents.NoxCrystal) },
                { 6, new SpellDefinition("Horrific Beast", 106, 0x5005, "Rel Xen Vas Bal",11,40, Reagents.BatWing, Reagents.DaemonBlood) },
                { 7, new SpellDefinition("Lich Form", 107, 0x5006, "Rel Xen Corp Ort",25,70, Reagents.DaemonBlood, Reagents.GraveDust, Reagents.NoxCrystal) },
                { 8, new SpellDefinition("Mind Rot", 108, 0x5007, "Wis An Ben",17,30, Reagents.BatWing, Reagents.DaemonBlood, Reagents.PigIron) },
                { 9, new SpellDefinition("Pain Spike", 109, 0x5008, "In Sar",5,20, Reagents.GraveDust, Reagents.PigIron) },
                { 10, new SpellDefinition("Poison Strike", 110, 0x5009, "In Vas Nox",17,50, Reagents.NoxCrystal) },
                { 11, new SpellDefinition("Strangle", 111, 0x500A, "In Bal Nox",29,65, Reagents.DaemonBlood, Reagents.NoxCrystal) },
                { 12, new SpellDefinition("Summon Familiar", 112, 0x500B, "Kal Xen Bal",17,30, Reagents.BatWing, Reagents.DaemonBlood, Reagents.GraveDust) },
                { 13, new SpellDefinition("Vampiric Embrace", 113, 0x500C, "Rel Xen An Sanct",25,99, Reagents.BatWing, Reagents.NoxCrystal, Reagents.PigIron) },
                { 14, new SpellDefinition("Vengeful Spirit", 114, 0x500D, "Kal Xen Bal Beh",41,80, Reagents.BatWing, Reagents.GraveDust, Reagents.PigIron) },
                { 15, new SpellDefinition("Wither", 115, 0x500E, "Kal Vas An Flam",23,60, Reagents.GraveDust, Reagents.NoxCrystal, Reagents.PigIron) },
                { 16, new SpellDefinition("Wraith Form", 116, 0x500F, "Rel Xen Um",17,20, Reagents.NoxCrystal, Reagents.PigIron) },
                { 17, new SpellDefinition("Exorcism", 117, 0x5010, "Ort Corp Grav",40,80, Reagents.NoxCrystal, Reagents.GraveDust) },
            };

            _spells = _spellsDict.Values.ToList();
        }

        public static IReadOnlyList<SpellDefinition> Spells => _spells;

        public static SpellDefinition GetSpell(int spellIndex) => _spellsDict.TryGetValue(spellIndex, out SpellDefinition spell) ? spell : SpellDefinition.EmptySpell;

        public static string[][] SpellsSpecialsName => new string[17][]
        {
            new [] { "Animate Dead", "Uus Corp" },
            new [] { "Blood Oath", "In Jux Mani Xen" },
            new [] { "Corpse Skin", "In Aglo Corp Ylem" },
            new [] { "Curse Weapon", "An Sanct Gra Char" },
            new [] { "Evil Omen", "Pas Tym An Sanct" },
            new [] { "Horrific Beast", "Rel Xen Vas Bal" },
            new [] { "Lich Form", "Rel Xen Corp Ort" },
            new [] { "Mind Rot", "Wis An Ben" },
            new [] { "Pain Spike", "In Sar" },
            new [] { "Poison Strike", "In Vas Nox" },
            new [] { "Strangle", "In Bal Nox" },
            new [] { "Summon Familiar", "Kal Xen Bal" },
            new [] { "Vampiric Embrace", "Rel Xen An Sanct" },
            new [] { "Vengeful Spirit", "Kal Xen Bal Beh" },
            new [] { "Wither", "Kal Vas An Flam" },
            new [] { "Wraith Form", "Rel Xen Um" },
            new [] { "Exorcism", "Ort Corp Grav" }
        };

        public static int[][] SpellsRequires => new int[17][]
        {
            new [] { 23, 40 },  new [] { 13, 20 },  new [] { 11, 20 },  new [] { 7, 0 }, new [] { 11, 20 }, new [] { 11, 40 },
            new []{ 23, 70 },  new []{ 17, 30 },  new []{ 5, 20 },   new []{ 17, 50 }, new [] { 29, 65 }, new [] { 17, 30 },
            new []{ 23, 99 },  new []{ 41, 80 }, new [] { 23, 60 }, new [] { 17, 20 },  new []{ 40, 80 }
        };
}
}
