using System;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine.SocialPlatforms;
using System.ComponentModel.Design;
using Facepunch.Actor;
using Facepunch.Cursor;
using Facepunch.Geometry;
using Facepunch.ID;
using Facepunch.Utility;
using uLink;
using DizzyHacks;

// wannaxri on dc
// This is an external esp for the Rust Legacy game

namespace TESTESP
{
    internal class Cvars
    {
        public static void Initialize()
        {
            CVars.ESP.DrawPlayers = false;
            CVars.ESP.DrawSleepers = false;
        }
    }
    internal class ESP
    {
        // Token: 0x04000061 RID: 97
        public static bool DrawPlayers;

        // Token: 0x04000064 RID: 100
        public static bool DrawSleepers;
    }
}


namespace Hacks
{
    internal class Local : UnityEngine.MonoBehaviour
    {
        // Token: 0x06000043 RID: 67 RVA: 0x000048CC File Offset: 0x00002ACC
        public static string GetEquippedItemName(Transform parent)
        {
            string empty = string.Empty;
            foreach (Transform transform in parent.GetComponentsInChildren<Transform>())
            {
                ItemRepresentation component = transform.gameObject.GetComponent<ItemRepresentation>();
                if (component != null)
                {
                    return component.datablock.name;
                }
            }
            return empty;
        }
    }
}

namespace LegacyESP
{
    unsafe class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            int* lpBuffer,
            int dwSize,
            out IntPtr lpNumberOfBytesRead);


        [DllImport("kernel32.dll")]
        static extern bool WriteProcessMemory(
            IntPtr hProcess,
            int* lpBaseAddress,
            byte[] lpBuffer,
            Int32 nSize,
            out IntPtr lpNumberOfBytesWritten
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(
            uint processAccess,
            bool bInheritHandle,
            uint processId
        );

        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }

        static unsafe void Main(string[] args)
        {
            Process[] processess = Process.GetProcessesByName("rust");
            if (processess.Length < 1)
            {
                Console.WriteLine("Rust Is Not Open");
                Console.ReadKey();
                return;
            }

            Process rust_maybe = processess[0];
            var h_process = OpenProcess((uint)ProcessAccessFlags.All, false, (uint)rust_maybe.Id);
            if (h_process == IntPtr.Zero)
            {
                Console.WriteLine("Open with Administarator");
                Console.ReadKey();
                return;
            }

            ProcessModule monoModule = null;
            foreach (ProcessModule module in rust_maybe.Modules)
            {
                if (module.ModuleName == "mono.dll")
                {
                    monoModule = module;
                }
            }

            if (monoModule == null)
            {
                Console.WriteLine("Wait For Game to Load");
                Console.ReadKey();
                return;
            }

        }
    }
}


// Token: 0x02000016 RID: 22
public class UColor
{
    // Token: 0x06000072 RID: 114 RVA: 0x00006CFF File Offset: 0x00004EFF
    public UColor(float r, float g, float b, float a = 255f)
    {
        this.color = new UnityEngine.Color(r / 255f, g / 255f, b / 255f, a / 255f);
    }

    // Token: 0x06000073 RID: 115 RVA: 0x00006D34 File Offset: 0x00004F34
    public UnityEngine.Color Get()
    {
        return this.color;
    }

    // Token: 0x04000051 RID: 81
    private UnityEngine.Color color;
}

namespace ChangeLater
{
    internal class ESP_Player : UnityEngine.MonoBehaviour
    {
        private UColor playerColor = new UColor(255f, 0f, 0f, 255f);
        private UColor sleeperColor = new UColor(255f, 153f, 255f, 255f);

        private void DrawPlayers()
        {
            if (CVars.ESP.DrawPlayers)
            {
                foreach (Character character in ESP_UpdateOBJs.GetPlayerList())
                {
                    UnityEngine.Color color = this.playerColor.Get();
                    string equippedItemName = Local.GetEquippedItemName(character.transform);
                    BoundingBox2D boundingBox2D = new BoundingBox2D(character);

                    if (boundingBox2D.IsValid)
                    {
                        float x = boundingBox2D.X;
                        float y = boundingBox2D.Y;
                        float width = boundingBox2D.Width;
                        float height = boundingBox2D.Height;
                        float distance = Vector3.Distance(character.transform.position, ESP_UpdateOBJs.LocalCharacter.transform.position);

                        Canvas.DrawString(new Vector2(x + width / 2f, y - 22f), color, Canvas.TextFlags.TEXT_FLAG_DROPSHADOW, character.playerClient.userName);
                        Canvas.DrawString(new Vector2(x + width / 2f, y + height + 2f), color, Canvas.TextFlags.TEXT_FLAG_DROPSHADOW, ((int)distance).ToString());
                        Canvas.DrawBoxOutlines(new Vector2(x, y), new Vector2(width, height), 1f, color);

                        if (!string.IsNullOrEmpty(equippedItemName))
                        {
                            Vector2 textSize = Canvas.TextBounds(equippedItemName);
                            Canvas.DrawString(new Vector2(x - textSize.x - 8f, y + height / 2f - textSize.y / 2f), color, Canvas.TextFlags.TEXT_FLAG_OUTLINED, equippedItemName);
                        }
                    }
                }
            }
        }

        private void DrawSleepers()
        {
            if (CVars.ESP.DrawSleepers)
            {
                foreach (UnityEngine.Object obj in ESP_UpdateOBJs.SleeperOBJs)
                {
                    if (obj != null)
                    {
                        SleepingAvatar sleepingAvatar = (SleepingAvatar)obj;
                        Vector3 screenPosition = Camera.main.WorldToScreenPoint(sleepingAvatar.transform.position);

                        if (screenPosition.z > 0f)
                        {
                            screenPosition.y = Screen.height - (screenPosition.y + 1f);
                            Canvas.DrawString(new Vector2(screenPosition.x, screenPosition.y), this.sleeperColor.Get(), Canvas.TextFlags.TEXT_FLAG_DROPSHADOW, string.Format("[S] [{0}]", (int)screenPosition.z));
                        }
                    }
                }
            }
        }

        private void OnGUI()
        {
            if (Event.current.type == EventType.Repaint && ESP_UpdateOBJs.IsIngame)
            {
                try
                {
                    this.DrawPlayers();
                    this.DrawSleepers();
                }
                catch
                {
                    // Handle exceptions if necessary
                }
            }
        }
    }

    internal static class ESP_UpdateOBJs
    {
        public static List<Character> GetPlayerList()
        {
            List<Character> players = new List<Character>();

            foreach (UnityEngine.Object obj in PlayerOBJs)
            {
                if (obj != null)
                {
                    Player player = (Player)obj;

                    if (player != LocalCharacter.gameObject && player.PlayerClient != null)
                    {
                        players.Add(player.Character);
                    }
                }
            }

            return players;
        }

        public static List<UnityEngine.Object> SleeperOBJs = new List<UnityEngine.Object>();
        public static List<UnityEngine.Object> PlayerOBJs = new List<UnityEngine.Object>();
        public static Character LocalCharacter;
        public static bool IsIngame;
    }
}
