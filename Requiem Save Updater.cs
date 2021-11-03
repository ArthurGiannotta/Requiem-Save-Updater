// Thanks to pixeltris for some offsets and other constants for the save file

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Requiem_Save_Updater
{
    class Program
    {
        public enum DuelSeries : byte
        {
            YuGiOh = 0,
            YuGiOhGX = 1,
            YuGiOh5D = 2,
            YuGiOhZEXAL = 3,
            YuGiOhARCV = 4,
            YuGiOhVRAINS = 5
        }

        public enum CampaignDuelState : byte
        {
            /// <summary>
            /// Locked / unavailable (characters are blacked out, padlock on the duel name, no '!' mark)
            /// </summary>
            Locked = 0,

            /// <summary>
            /// Available (characters are blacked out, '!' mark)
            /// </summary>
            Available = 1,

            /// <summary>
            /// Available and an attempt has been made to complete this duel (character are visible, '!' mark)
            /// </summary>
            AvailableAttempted = 2,

            /// <summary>
            /// Complete (characters are visible, no '!' mark)
            /// </summary>
            Complete = 3,

            /// <summary>
            /// Available (characters are blacked out, no '!' mark)
            /// Note that anything 4+ seems to give the same result
            /// </summary>
            AvailableAlt = 4,
        }

        static void PatchExecutable()
        {
            byte[] executable;

            try
            {
                executable = File.ReadAllBytes("YuGiOh.exe");
            }
            catch
            {
                MessageBox.Show("Failed to open file YuGiOh.exe", "Link Evolution Signature Check Bypass - Error");

                return;
            }

            /*
                00000001407 | 48:895C24 08     | mov qword ptr ss:[rsp+8],rbx         |
                00000001407 | 57               | push rdi                             |
                00000001407 | 48:83EC 20       | sub rsp,20                           |
                00000001407 | 8B59 0C          | mov ebx,dword ptr ds:[rcx+C]         |
                00000001407 | 48:8BF9          | mov rdi,rcx                          |
                00000001407 | 8B51 08          | mov edx,dword ptr ds:[rcx+8]         |
                00000001407 | C741 0C 00000000 | mov dword ptr ds:[rcx+C],0           |
                00000001407 | E8 91C6F5FF      | call yugioh.1407556F0                | Calculate Signature
--------------> 00000001407 | 3BD8             | cmp ebx,eax                          |
                00000001407 | 895F 0C          | mov dword ptr ds:[rdi+C],ebx         |
                00000001407 | 48:8B5C24 30     | mov rbx,qword ptr ss:[rsp+30]        |
                00000001407 | 0F94C0           | sete al                              |
                00000001407 | 48:83C4 20       | add rsp,20                           |
                00000001407 | 5F               | pop rdi                              |
                00000001407 | C3               | ret                                  |
             */
            ByteSignature check = new("3B ?? 89 5F ?? 48 8B 5C 24 ?? 0F 94 C0 48 83 C4 ?? 5F C3");
            int offset;

            if ((offset = check.FindPattern(executable)) < 0)
            {
                MessageBox.Show("Failed to find pattern in YuGiOh.exe. Maybe it is not the latest Steam executable?", "Link Evolution Signature Check Bypass - Error");

                return;
            }

            executable[offset + 1] = 0xDB; // "cmp ebx, eax" -> "cmp ebx, ebx"

            // Console.WriteLine($"Pattern modified at {offset}.");

            try
            {
                File.WriteAllBytes("YuGiOh - Bypassed.exe", executable);
            }
            catch
            {
                MessageBox.Show("Failed to save file YuGiOh - Bypassed.exe", "Link Evolution Signature Check Bypass - Error");

                return;
            }

            MessageBox.Show("Executable updated to YuGiOh - Bypassed.exe", "Link Evolution Signature Check Bypass");
        }

        static void UpdateSave()
        {
            /* This part is not implemented
            // Values for the duels that have moved from one slot to another
            // 0 is the first duel slot, 1 is the second duel slot, ...
            Dictionary<DuelSeries, (int, int)[]> MovedDuels = new()
            {
                [DuelSeries.YuGiOh] = new (int, int)[]
                {
                }
            };*/

            // Values for the duels that need to be reset
            // 0 is the first duel slot, 1 is the second duel slot, ...
            Dictionary<DuelSeries, int[]> ResetDuels = new()
            {
                [DuelSeries.YuGiOh] = new int[]
                {
                    5,  // Bonus - Everything's Relative
                    10, // Bonus - Double Trouble Duel
                    18, // Bonus - Dungeon Dice Monsters
                    21, // Bonus - The ESP Duelist
                    25, // Bonus - Double Duel
                    26, // Bonus - Friends 'Til The End
                    29, // Bonus - Mind Game
                    31, // Bonus - Showdown In The Shadows
                    32, // Bonus - Noah's Final Threat
                    33, // Bonus - The Darkness Returns
                    36, // * A New Evil
                    41, // Bonus - One Step Ahead
                    42, // Bonus - Sinister Secrets
                    44, // Bonus - Dark Side of Dimensions Pt. 1
                    45, // Bonus - Dark Side of Dimensions Pt. 2
                    46, // Bonus - Shadi Vs. Grandpa
                    47, // Bonus - Rebecca Vs Tristan
                    48, // Bonus - Sera vs Anubis
                },
                [DuelSeries.YuGiOhGX] = new int[]
                {
                    14, // Bonus - Dormitory Demolition
                    15, // * Schooling the Master
                    18, // * Heart of Ice
                    19, // * Tough Love
                    27, // * A Dimensional Duel
                    29, // Bonus - What Lies Beneath
                    32, // Bonus - The Invasion of Darkness
                    33, // * Darkness Returns
                    35, // Bonus - Fonda vs Sarina
                    36, // Bonus - Kuriboh vs Banner
                    37, // Bonus - Obelisk Girl's Battle
                    38, // Bonus - Shadow Rider Duel
                },
                [DuelSeries.YuGiOh5D] = new int[]
                {
                    10, // Bonus - Good Cop, Bad Cop
                    12, // * Digging Deeper
                    13, // Bonus - Surely You Jest
                    22, // Bonus - Dawn of the Duel Board
                    31, // Bonus - Against All Odds
                    36, // Bonus - Hanging Out in New Domino City
                    37, // Bonus - No One Remember Us
                    38, // Bonus - Bonds Beyond Time
                    39, // Bonus - Death's Favorites, Pt. 1
                    40, // Bonus - Death's Favorites, Pt. 2
                },
                [DuelSeries.YuGiOhZEXAL] = new int[]
                {
                    14, // Bonus - Doom in Bloom
                    15, // Bonus - The Friendship Games
                    24, // Bonus - Battle With The Barians
                },
                [DuelSeries.YuGiOhARCV] = new int[]
                {
                    33, // Bonus - Who Gets The Kids?
                    34, // Bonus - Riley Vs Ray
                    35, // Bonus - Dueling Sisters
                },
                [DuelSeries.YuGiOhVRAINS] = new int[]
                {
                    30, // Bonus - Final Battle
                },
            };

            // Credits to pixeltris for the following save file offsets
            const int CampaignOffset = 7024;
            const int DecksOffset = 14280;
            const int DuelsPerSeries = 50;

            byte[] save;

            try
            {
                save = File.ReadAllBytes("savegame.dat");
            }
            catch
            {
                MessageBox.Show("Failed to open file savegame.dat", "Requiem Save Updater - Error");

                return;
            }

            int offset = CampaignOffset + 2 * sizeof(int);
            DuelSeries series = DuelSeries.YuGiOh;

            while (offset < DecksOffset)
            {
                // Skips first entry of each series, since it is not an actual duel
                offset += 8 * sizeof(int);

                foreach (int duel in ResetDuels[series])
                {
                    int pointer = offset + duel * 6 * sizeof(int);

                    // Reset Non-Reverse Duel
                    if (save[pointer] != (byte)CampaignDuelState.Locked)
                    {
                        save[pointer] = (byte)CampaignDuelState.Available;
                    }

                    // Reset Reverse Duel
                    save[pointer + 4] = (byte)CampaignDuelState.Locked;
                }

                offset += (DuelsPerSeries - 1) * 6 * sizeof(int);

                series += 1;
            }

            // Console.WriteLine($"Finished saving at offset {offset}.");

            try
            {
                File.WriteAllBytes("savegame - requiem.dat", save);
            }
            catch
            {
                MessageBox.Show("Failed to save file savegame - requiem.dat", "Requiem Save Updater - Error");

                return;
            }

            MessageBox.Show("Save file updated to savegame - requiem.dat", "Requiem Save Updater");
        }

        static void Main(string[] args)
        {
            UpdateSave();

            PatchExecutable();
        }
    }
}
