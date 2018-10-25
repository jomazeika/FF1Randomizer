using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RomUtilities;

namespace FF1Lib
{
	public partial class FF1Rom : NesRom
	{
		public const int MagicOffset = 0x301E0;
		public const int MagicSize = 8;
		public const int MagicCount = 64;
		public const int MagicNamesOffset = 0x2BE03;
		public const int MagicNameSize = 5;
		public const int MagicTextPointersOffset = 0x304C0;
		public const int MagicPermissionsOffset = 0x3AD18;
		public const int MagicPermissionsSize = 8;
		public const int MagicPermissionsCount = 12;
		public const int MagicOutOfBattleOffset = 0x3AEFA;
		public const int MagicOutOfBattleSize = 7;
		public const int MagicOutOfBattleCount = 13;

		public const int ConfusedSpellIndexOffset = 0x3321E;
		public const int FireSpellIndex = 4;

		public const int WeaponOffset = 0x30000;
		public const int WeaponSize = 8;
		public const int WeaponCount = 40;

		public const int ArmorOffset = 0x30140;
		public const int ArmorSize = 4;
		public const int ArmorCount = 40;

		private struct MagicSpell
		{
			public byte Index;
			public Blob Data;
			public Blob Name;
			public byte TextPointer;
		}

		private readonly List<byte> _outOfBattleSpells = new List<byte> { 0, 16, 32, 48, 19, 51, 35, 24, 33, 56, 38, 40, 41 };

		//Generate random names for the different spells!
		//Each spell is made of two syllables = two parts
		//We'll use Wote's list for now and refine as we go
		public void ShuffleMagicNames(MT19337 rng)
		{
			Dictionary<String, byte[]> syllableRef = new Dictionary<String, byte[]>();

			List<String[]> spellsReference = new List<String[]>();
			//Level 1 Spells
			spellsReference.Add(new string[] { "Single Ally", "Healing" });
			spellsReference.Add(new string[] { "Single Enemy", "Lightning" });
			spellsReference.Add(new string[] { "Stronger Ally", "Protection" });
			spellsReference.Add(new string[] { "Single Enemy", "Fire" });
			spellsReference.Add(new string[] { "Weak Enemy Group", "Anti-Undead" });
			spellsReference.Add(new string[] { "Single Enemy", "Time" });
			spellsReference.Add(new string[] { "Single Ally", "Protection" });
			spellsReference.Add(new string[] { "Weak Enemy Group", "Status" });
			//Level 2 Spells
			spellsReference.Add(new string[] { "Strong Ally", "Protection" });
			spellsReference.Add(new string[] { "Strong Enemy Group", "Status" });
			spellsReference.Add(new string[] { "Single Ally", "Counter" });
			spellsReference.Add(new string[] { "Single Enemy", "Ice" });
			spellsReference.Add(new string[] { "Protection", "Lightning" });
			spellsReference.Add(new string[] { "Weak Enemy Group", "Time" });
			spellsReference.Add(new string[] { "Counter", "Weak Enemy Group" });
			spellsReference.Add(new string[] { "Single Ally", "Damage" });
			//Level 3 Spells
			spellsReference.Add(new string[] { "Strong Ally", "Healing" });
			spellsReference.Add(new string[] { "Single Enemy", "Status" });
			spellsReference.Add(new string[] { "Strong Enemy Group", "Anti-Undead" });
			spellsReference.Add(new string[] { "Weak Enemy Group", "Lightning" });
			spellsReference.Add(new string[] { "Weak Ally Group", "Healing" });
			spellsReference.Add(new string[] { "Weak Enemy Group", "Fire" });
			spellsReference.Add(new string[] { "Protection", "Fire" });
			spellsReference.Add(new string[] { "Strong Enemy Group", "Time" });
			//Level 4 Spells
			spellsReference.Add(new string[] { "Healing", "Poison" });
			spellsReference.Add(new string[] { "Strongest Ally", "Time" });
			spellsReference.Add(new string[] { "Strong Enemy Group", "Leaving" });
			spellsReference.Add(new string[] { "Weak Enemy Group", "Fire" });
			spellsReference.Add(new string[] { "Protection", "Ice" });
			spellsReference.Add(new string[] { "Stronger Enemy Group", "Status" });
			spellsReference.Add(new string[] { "Protection", "Counter" });
			spellsReference.Add(new string[] { "None", "Status" });
			//Level 5 Spells
			spellsReference.Add(new string[] { "Stronger Ally", "Healing" });
			spellsReference.Add(new string[] { "Strong Enemy Group", "Fire" });
			spellsReference.Add(new string[] { "Stronger Enemy Group", "Anti-Undead" });
			spellsReference.Add(new string[] { "Strongest Enemy Group", "Poison" });
			spellsReference.Add(new string[] { "Strong Ally Group", "Healing" });
			spellsReference.Add(new string[] { "Counter", "Time" });
			spellsReference.Add(new string[] { "Healing", "Death" });
			spellsReference.Add(new string[] { "Strongest Ally", "Leaving" });
			//Level 6 Spells
			spellsReference.Add(new string[] { "Strongest Ally Group", "Protection" });
			spellsReference.Add(new string[] { "Strong Enemy Group", "Lightning" });
			spellsReference.Add(new string[] { "Stronger Ally Group", "Protection" });
			spellsReference.Add(new string[] { "Strongest Enemy Group", "Earth" });
			spellsReference.Add(new string[] { "Healing", "Earth" });
			spellsReference.Add(new string[] { "Single Enemy", "Death" });
			spellsReference.Add(new string[] { "Strongest Ally Group", "Leaving" });
			spellsReference.Add(new string[] { "HP Based", "Time" });
			//Level 7 Spells
			spellsReference.Add(new string[] { "Strongest Ally", "Healing" });
			spellsReference.Add(new string[] { "HP Based", "Status" });
			spellsReference.Add(new string[] { "Strongest Enemy Group", "Anti-Undead" });
			spellsReference.Add(new string[] { "Single Enemy", "Poison" });
			spellsReference.Add(new string[] { "Strongest Ally Group", "Healing" });
			spellsReference.Add(new string[] { "Strong Enemy Group", "Ice" });
			spellsReference.Add(new string[] { "Protection", "Death" });
			spellsReference.Add(new string[] { "Strong Ally", "Damage" });
			//Level 8 Spells
			spellsReference.Add(new string[] { "Counter", "All" });
			spellsReference.Add(new string[] { "Strongest Enemy Group", "Time" });
			spellsReference.Add(new string[] { "Stronger Enemy Group", "Damage" });
			spellsReference.Add(new string[] { "HP Based", "Death" });
			spellsReference.Add(new string[] { "Counter", "Death" });
			spellsReference.Add(new string[] { "Strongest Enemy Group", "Damage" });
			spellsReference.Add(new string[] { "Protection", "All" });
			spellsReference.Add(new string[] { "Strong Enemy Group", "Time" });


			List<String> spellComponents = new List<string>(){
				"Single Ally", "Strong Ally", "Stronger Ally", "Strongest Ally",
				"Weak Ally Group","Strong Ally Group","Stronger Ally Group","Strongest Ally Group",
				"Single Enemy","Weak Enemy Group","Strong Enemy Group","Stronger Enemy Group","Strongest Enemy Group",
				"Protection","Counter","Healing","Anti-Undead","Leaving","HP Based","Damage",
				"Lightning","Fire","Time","Status","Ice","Poison","Earth","Death","All","None"};
			List < byte[] > syllableList = new List<byte[]>();
			
			byte[] vowels = { 0xA4, 0xA8, 0xAC, 0xB2, 0xB8 }; //a,e,i,o,u

			byte[] consts = { 0x8B, 0x8C, 0x8D, 0x8F, 0x90, 0x91, //B,C,D,F,G,H,
				0x93, 0x94, 0x95, 0x96, 0x97, 0x99, 0x9B, 0x9C, //J,K,L,M,N,P,R,S
				0x9D, 0x9F, 0xA0, 0xA1, 0xA2, 0xA3}; //T,V,W,X,Y,Z

			//Word Filters. They're important.
			//Don't remove stuff from this unless you're sure.
			List<byte[]> toRemove = new List<byte[]>()
			{
				new byte[]{0x9F,0xA4}, //Va
				new byte[]{0x94,0xAC}, //Ki
				new byte[]{0x93,0xAC}, //Ji
				new byte[]{0x97,0xAC}, //Ni
				new byte[]{0x8B,0xA8}, //Be
				new byte[]{0x94,0xA8}, //Ke
			};

			for (var i = 0; i < consts.Length; i++)
			{
				for (var j = 0; j < vowels.Length; j++)
				{
					byte[] toAdd = new byte[] { consts[i], vowels[j] };
					if (!toRemove.Contains(toAdd))
					{
						syllableList.Add(toAdd);
					}
				}
			}

			syllableList.Shuffle(rng);

			for (var i = 0; i < spellComponents.Count; i++)
			{
				syllableRef.Add(spellComponents[i], syllableList[i]);
			}

			//Get all of the spell names
			var names = Get(MagicNamesOffset, MagicNameSize * MagicCount).Chunk(MagicNameSize);

			for (var i = 0; i < spellsReference.Count; i++)
			{
				byte[] syllable_0 = syllableRef[spellsReference[i][0]];
				byte[] syllable_1 = syllableRef[spellsReference[i][1]];
				names[i] = new byte[] { syllable_0[0], syllable_0[1], syllable_1[0], syllable_1[1], 0x00};
			}
			Put(MagicNamesOffset, names.Aggregate((seed, next) => seed + next));
		}

		public void ShuffleMagicLevels(MT19337 rng, bool keepPermissions)
		{
			var spells = Get(MagicOffset, MagicSize * MagicCount).Chunk(MagicSize);
			var names = Get(MagicNamesOffset, MagicNameSize * MagicCount).Chunk(MagicNameSize);
			var pointers = Get(MagicTextPointersOffset, MagicCount);

			var magicSpells = spells.Select((spell, i) => new MagicSpell
			{
				Index = (byte)i,
				Data = spell,
				Name = names[i],
				TextPointer = pointers[i]
			})
			.ToList();

			// First we have to un-interleave white and black spells.
			var whiteSpells = magicSpells.Where((spell, i) => (i / 4) % 2 == 0).ToList();
			var blackSpells = magicSpells.Where((spell, i) => (i / 4) % 2 == 1).ToList();

			whiteSpells.Shuffle(rng);
			blackSpells.Shuffle(rng);

			// Now we re-interleave the spells.
			var shuffledSpells = new List<MagicSpell>();
			for (int i = 0; i < MagicCount; i++)
			{
				var sourceIndex = 4 * (i / 8) + i % 4;
				if ((i / 4) % 2 == 0)
				{
					shuffledSpells.Add(whiteSpells[sourceIndex]);
				}
				else
				{
					shuffledSpells.Add(blackSpells[sourceIndex]);
				}
			}

			Put(MagicOffset, shuffledSpells.Select(spell => spell.Data).Aggregate((seed, next) => seed + next));
			Put(MagicNamesOffset, shuffledSpells.Select(spell => spell.Name).Aggregate((seed, next) => seed + next));
			Put(MagicTextPointersOffset, shuffledSpells.Select(spell => spell.TextPointer).ToArray());

			if (keepPermissions)
			{
				// Shuffle the permissions the same way the spells were shuffled.
				for (int c = 0; c < MagicPermissionsCount; c++)
				{
					var oldPermissions = Get(MagicPermissionsOffset + c * MagicPermissionsSize, MagicPermissionsSize);

					var newPermissions = new byte[MagicPermissionsSize];
					for (int i = 0; i < 8; i++)
					{
						for (int j = 0; j < 8; j++)
						{
							var oldIndex = shuffledSpells[8 * i + j].Index;
							var oldPermission = (oldPermissions[oldIndex / 8] & (0x80 >> oldIndex % 8)) >> (7 - oldIndex % 8);
							newPermissions[i] |= (byte)(oldPermission << (7 - j));
						}
					}

					Put(MagicPermissionsOffset + c * MagicPermissionsSize, newPermissions);
				}
			}

			// Map old indices to new indices.
			var newIndices = new byte[MagicCount];
			for (byte i = 0; i < MagicCount; i++)
			{
				newIndices[shuffledSpells[i].Index] = i;
			}

			// Fix enemy spell pointers to point to where the spells are now.
			var scripts = Get(ScriptOffset, ScriptSize * ScriptCount).Chunk(ScriptSize);
			foreach (var script in scripts)
			{
				// Bytes 2-9 are magic spells.
				for (int i = 2; i < 10; i++)
				{
					if (script[i] != 0xFF)
					{
						script[i] = newIndices[script[i]];
					}
				}
			}
			Put(ScriptOffset, scripts.SelectMany(script => script.ToBytes()).ToArray());

			// Fix weapon and armor spell pointers to point to where the spells are now.
			var weapons = Get(WeaponOffset, WeaponSize * WeaponCount).Chunk(WeaponSize);
			foreach (var weapon in weapons)
			{
				if (weapon[3] != 0x00)
				{
					weapon[3] = (byte)(newIndices[weapon[3] - 1] + 1);
				}
			}
			Put(WeaponOffset, weapons.SelectMany(weapon => weapon.ToBytes()).ToArray());

			var armors = Get(ArmorOffset, ArmorSize * ArmorCount).Chunk(ArmorSize);
			foreach (var armor in armors)
			{
				if (armor[3] != 0x00)
				{
					armor[3] = (byte)(newIndices[armor[3] - 1] + 1);
				}
			}
			Put(ArmorOffset, armors.SelectMany(armor => armor.ToBytes()).ToArray());

			// Fix the crazy out of battle spell system.
			var outOfBattleSpellOffset = MagicOutOfBattleOffset;
			for (int i = 0; i < MagicOutOfBattleCount; i++)
			{
				var oldSpellIndex = _outOfBattleSpells[i];
				var newSpellIndex = newIndices[oldSpellIndex];

				Put(outOfBattleSpellOffset, new[] { (byte)(newSpellIndex + 0xB0) });

				outOfBattleSpellOffset += MagicOutOfBattleSize;
			}

			// Confused enemies are supposed to cast FIRE, so figure out where FIRE ended up.
			var newFireSpellIndex = shuffledSpells.FindIndex(spell => spell.Data == spells[FireSpellIndex]);
			Put(ConfusedSpellIndexOffset, new[] { (byte)newFireSpellIndex });
		}
	}
}
