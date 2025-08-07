using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NightreignRelicEditor
{
    class RelicManager
    {
        private GameLink nightreign;

        private const string processName = "nightreign";
        private const string moduleName = "nightreign.exe";

        private uint[,] playerRelics = new uint[3, 3];

        public ConnectionStates ConnectionStatus { get; set; }

        public RelicManager()
        {
            nightreign = new GameLink(processName, moduleName);

            LoadRelicEffects();

            if (nightreign.CheckProcessRunning("easyanticheat_eos"))
            {
                MessageBox.Show("EAC running");
                ConnectionStatus = ConnectionStates.EACDetected;
            }
            else
            {
                nightreign.InitGameLink();

                if (nightreign.Connected())
                {
                    if (LocateRelicAddress())
                        ConnectionStatus = ConnectionStates.Connected;
                    else
                        ConnectionStatus = ConnectionStates.ConnectedOffsetsNotFound;
                }
                else
                {
                    ConnectionStatus = ConnectionStates.NightreignNotFound;
                }
            }
        }

        IntPtr relicAddress = IntPtr.Zero;
        IntPtr gameDataManAddress = IntPtr.Zero;

        uint relic1Offset;
        uint relic2Offset;
        uint relic3Offset;

        public bool Connected()
        {
            return nightreign.Connected();
        }


        public enum ConnectionStates
        {
            NotConnected,
            NightreignNotFound,
            EACDetected,
            ConnectedOffsetsNotFound,
            Connected,
        }

        private bool LocateRelicAddress()
        {
            using (AOBScanner scanner = new AOBScanner(nightreign.GetProcessHandle(),
                                                 nightreign.GetBaseAddress(),
                                                 ".text"))
            {
                IntPtr temp;

                temp = scanner.FindAddress("8B 03 89 44 24 40 48 8B 0D ?? ?? ?? ?? 48 85 C9");   // CSGaitem
                if (temp != IntPtr.Zero)
                    relicAddress = (IntPtr)nightreign.ReadUInt64(temp + 13 + (IntPtr)nightreign.ReadUInt32(temp + 9));

                Debug.Print("CSGaitem " + temp.ToString("X") + " | " + relicAddress.ToString("X"));

                temp = scanner.FindAddress("48 8B 05 ?? ?? ?? ?? 48 8B 70 08");   // GameDataMan
                if (temp != IntPtr.Zero)
                    gameDataManAddress = (IntPtr)nightreign.ReadUInt64(temp + 7 + (IntPtr)nightreign.ReadUInt32(temp + 3));

                Debug.Print("GameDataMan " + temp.ToString("X") + " | " + gameDataManAddress.ToString("X"));

                // Here for d ebug reasons, remove for release
                relic1Offset = nightreign.ReadUInt16((IntPtr)nightreign.ReadUInt64(gameDataManAddress + 0x8) + 0x2F4) * 8 + 8;
                relic2Offset = nightreign.ReadUInt16((IntPtr)nightreign.ReadUInt64(gameDataManAddress + 0x8) + 0x2F8) * 8 + 8;
                relic3Offset = nightreign.ReadUInt16((IntPtr)nightreign.ReadUInt64(gameDataManAddress + 0x8) + 0x2FC) * 8 + 8;

                Debug.Print("Relic offsets " + relic1Offset.ToString("X") + " " + relic2Offset.ToString("X") + " " + relic3Offset.ToString("X"));
            }

            if (relicAddress != IntPtr.Zero && gameDataManAddress != IntPtr.Zero)
                return true;

            return false;
        }

        public List<RelicEffect> relicEffects = new List<RelicEffect>();

        private void LoadRelicEffects()
        {
            try
            {
                string line;
                var assembly = Assembly.GetExecutingAssembly();
                string resource = Assembly.GetExecutingAssembly().GetManifestResourceNames().Single(str => str.EndsWith("reliceffects.tsv"));
                using (StreamReader sr = new StreamReader(assembly.GetManifestResourceStream(resource)))
                {

                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] split = line.Split("\t");
                        if (split.Length == 2 || split.Length == 6)
                        {
                            RelicEffect re = new RelicEffect();

                            re.EffectID = Convert.ToUInt32(split[0]);
                            re.Description = split[1];

                            if (split.Length == 6)
                            {
                                re.Category = Convert.ToUInt32(split[2]);
                                re.Slot1Weight = Convert.ToUInt32(split[3]);
                                re.Slot2Weight = Convert.ToUInt32(split[4]);
                                re.Slot3Weight = Convert.ToUInt32(split[5]);
                            }
                            else
                            {
                                re.Category = 0;
                            }

                            relicEffects.Add(re);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show("fail?");
            }
        }

        private uint GetRelicOffset(uint relic)
        {
            switch (relic)
            {
                case 0:
                    return nightreign.ReadUInt16((IntPtr)nightreign.ReadUInt64(gameDataManAddress + 0x8) + 0x2F4) * 8 + 8; // +0x2E8 pre 1.02
                case 1:
                    return nightreign.ReadUInt16((IntPtr)nightreign.ReadUInt64(gameDataManAddress + 0x8) + 0x2F8) * 8 + 8;
                case 2:
                    return nightreign.ReadUInt16((IntPtr)nightreign.ReadUInt64(gameDataManAddress + 0x8) + 0x2FC) * 8 + 8;
            }
            return 0;
        }

        public void SetRelicEffect(uint relic, uint slot, uint effect)
        {
            playerRelics[relic, slot] = effect;
        }

        public uint GetRelicEffect(uint relic, uint slot)
        {
            return playerRelics[relic, slot];
        }

        public void SetRelicSlotInGame(uint relic, uint slot, uint effect)
        {
            uint relicOffset = GetRelicOffset(relic);
            uint slotOffset = 0x18 + (slot * 4);

            if (effect == 0)
                effect = 0xFFFFFFFF;

            nightreign.WriteUInt32((IntPtr)nightreign.ReadUInt64(relicAddress + (IntPtr)relicOffset) + (IntPtr)slotOffset, effect);
        }

        public uint GetRelicSlotInGame(uint relic, uint slot)
        {
            uint relicOffset = GetRelicOffset(relic);
            uint slotOffset = 0x18 + (slot * 4);

            return nightreign.ReadUInt32((IntPtr)nightreign.ReadUInt64(relicAddress + (IntPtr)relicOffset) + (IntPtr)slotOffset);
        }

        enum RelicErrors
        {
            CharacterEffectInWrongSlot,
            MultipleFromCategory,
        }

        public bool VerifyRelic(uint relic)
        {
            //uint[] slotEffect = new uint[3];
            RelicEffect[] relicEffect = new RelicEffect[3];

            //slotEffect[0] = GetRelicSlotEffectID(relic, 0);
            //slotEffect[1] = GetRelicSlotEffectID(relic, 1);
            //slotEffect[2] = GetRelicSlotEffectID(relic, 2);

            

            for (uint x = 0; x < 3; x++)
            {
                uint effectID = GetRelicSlotEffectID(relic, x);

                foreach (RelicEffect re in relicEffects)
                {
                    if (effectID == re.EffectID)
                    {
                        relicEffect[x] = re;
                        break;
                    }
                }
            }

            return true;
        }

        public uint GetRelicSlotEffectID(uint relic, uint slot)
        {
            uint relicOffset = GetRelicOffset(relic);
            uint slotOffset = 0x18 + (slot * 4);

            return nightreign.ReadUInt32((IntPtr)nightreign.ReadUInt64(relicAddress + (IntPtr)relicOffset) + (IntPtr)slotOffset);
        }

        //public string GetEffectDescription(uint effectID)
        public (string description, bool valid) GetEffectDescription(uint effectID)
        {
            if (effectID == 0xFFFFFFFF)
                return ("-", true);

            foreach (RelicEffect re in relicEffects)
            {
                if (effectID == re.EffectID)
                {
                    bool valid = true;
                    if (re.Category == 0)
                        valid = false;
                    return (re.Description, valid);
                }
            }
            return (effectID.ToString(), false);
        }
    }

    class RelicEffect
    {
        public uint EffectID { get; set; }
        public string Description { get; set; }
        public uint Category { get; set; }
        public uint Slot1Weight { get; set; }
        public uint Slot2Weight { get; set; }
        public uint Slot3Weight { get; set; }
    }
}
