using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace NightreignRelicEditor.Models;

public class RelicManager
{
    private GameLink nightreign;

    private const string processName = "nightreign";
    private const string moduleName = "nightreign.exe";

    private IntPtr relicAddress = IntPtr.Zero;
    private IntPtr gameDataManAddress = IntPtr.Zero;

    private nint relicBaseOffset = 0x2F4;

    private List<RelicEffect>[] playerRelics = new List<RelicEffect>[] { new(), new(), new() };

    public List<RelicEffect> AllRelicEffects = [];


    private ConnectionStates connectionStatus;
    public ConnectionStates ConnectionStatus
    {
        set => connectionStatus = value;

        get
        {
            if (connectionStatus != ConnectionStates.Connected)
                return connectionStatus;

            if (!nightreign.Connected)
                connectionStatus = ConnectionStates.ConnectionLost;

            return connectionStatus;
        }
    }

    public List<RelicEffect>[] PlayerRelics
    {
        get
        {
            return playerRelics;
        }
    }


    public RelicManager()
    {
        nightreign = new GameLink(processName, moduleName);
        LoadRelicEffects();
    }

    public void ConnectToNightreign()
    {
        if (nightreign.CheckProcessRunning("easyanticheat_eos"))
        {
            MessageBox.Show("EAC running");
            ConnectionStatus = ConnectionStates.EACDetected;
            
            return;
        }
        
        nightreign.InitGameLink();

        if (nightreign.Connected)
        {
            SetRelicOffsetByVersion(nightreign.Version);

            ConnectionStatus = LocateRelicAddress()
                ? ConnectionStates.Connected : ConnectionStates.ConnectedOffsetsNotFound;
        }
        else
        {
            ConnectionStatus = ConnectionStates.NightreignNotFound;
        }
    }

        

    private bool LocateRelicAddress()
    {
        using (AOBScanner scanner = new AOBScanner(nightreign.ProcessHandle,
                   nightreign.BaseAddress,
                   ".text"))
        {
            IntPtr address;

            address = scanner.FindAddress("8B 03 89 44 24 40 48 8B 0D ?? ?? ?? ?? 48 85 C9");   // CSGaitem
            if (address != IntPtr.Zero)
                relicAddress = CalculateLocation(address, 9);

            address = scanner.FindAddress("48 8B 05 ?? ?? ?? ?? 48 8B 70 08");   // GameDataMan
            if (address != IntPtr.Zero)
                gameDataManAddress = CalculateLocation(address);
        }

        if (relicAddress != IntPtr.Zero && gameDataManAddress != IntPtr.Zero)
            return true;

        return false;
    }

    private IntPtr CalculateLocation(IntPtr address, int offset = 3)
    {
        return (IntPtr)nightreign.ReadUInt64(address + offset + 4 + (IntPtr)nightreign.ReadInt32(address + offset));
    }

    private void SetRelicOffsetByVersion(uint[] version)
    {
        if (version[1] == 1)
            relicBaseOffset = 0x2E8;
        relicBaseOffset = 0x2F4;
    }

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

                    if (split.Length == 5)
                    {
                        RelicEffect re = new RelicEffect();

                        re.EffectId = UInt32.Parse(split[0]);
                        re.Description = split[1];
                        re.Category = Int32.Parse(split[2]);
                        re.OrderGroup = Int32.Parse(split[3]);

                        if (UInt32.TryParse(split[4], out uint weight))
                            re.Slot1Weight = weight;

                        AllRelicEffects.Add(re);
                    }

                }
            }
        }
        catch (Exception f)
        {
            MessageBox.Show("Error loading relic effects - " + f.ToString());
        }
    }

    private uint GetRelicOffset(uint relic)
    {
        switch (relic)
        {
            case 0:
                return nightreign.ReadUInt16((IntPtr)nightreign.ReadUInt64(gameDataManAddress + 0x8) + relicBaseOffset) * 8 + 8;
            case 1:
                return nightreign.ReadUInt16((IntPtr)nightreign.ReadUInt64(gameDataManAddress + 0x8) + relicBaseOffset + 4) * 8 + 8;
            case 2:
                return nightreign.ReadUInt16((IntPtr)nightreign.ReadUInt64(gameDataManAddress + 0x8) + relicBaseOffset + 8) * 8 + 8;
            case 3:
                return nightreign.ReadUInt16((IntPtr)nightreign.ReadUInt64(gameDataManAddress + 0x8) + relicBaseOffset + 12) * 8 + 8;
        }
        return 0;
    }

    public void AddRelicEffect(uint relic, RelicEffect effect, bool sort = true)
    {
        if (playerRelics[relic].Count < 3)
            playerRelics[relic].Add(effect);

        if (sort)
            playerRelics[relic] = playerRelics[relic].OrderBy(x => x.OrderGroup).ThenBy(x => x.EffectId).ToList();
    }

    public void RemoveRelicEffect(uint relic, uint slot)
    {
        Debug.Print("relic " + relic + " slot " + slot);
        if (slot < playerRelics[relic].Count)
            playerRelics[relic].RemoveAt((int)slot);
    }

    public uint GetRelicEffectId(uint relic, uint slot)
    {
        if (slot >= playerRelics[relic].Count)
            return 0xFFFFFFFF;
        return playerRelics[relic][(int)slot].EffectId;
    }

    public void SetRelic(uint relic, uint[] effectId)
    {
        playerRelics[relic].Clear();

        for (uint x = 0; x < 3; x++)
        {
            RelicEffect? effect = null;

            if (effectId[x] != 0xFFFFFFFF)
                effect = AllRelicEffects.FirstOrDefault(i => i.EffectId == effectId[x]);

            if (effect != null)
                AddRelicEffect(relic, effect);
        }
    }

    public void SetRelicInGame(uint relic)
    {
        nint relicOffset = (nint)GetRelicOffset(relic);
        nint slotOffset = 0x18;

        for (uint slot = 0; slot < 3; slot++)
            nightreign.WriteUInt32((IntPtr)nightreign.ReadUInt64(relicAddress + relicOffset) + (IntPtr)(slot * 4) + slotOffset, GetRelicEffectId(relic, slot));
    }

    public void GetRelicFromGame(uint relic)
    {
        uint[] effectId = new uint[3];
        nint relicOffset = (nint)GetRelicOffset(relic);
        nint slotOffset = 0x18;

        for (uint slot = 0; slot < 3; slot++)
            effectId[slot] = nightreign.ReadUInt32((IntPtr)nightreign.ReadUInt64(relicAddress + (IntPtr)relicOffset) + (IntPtr)(slot * 4) + slotOffset);

        SetRelic(relic, effectId);
    }

    //
    // Verification
    //

    public bool VerifyEffectIsRelicEffect(RelicEffect effect, bool includeUnique = false)
    {

        if (effect.Slot1Weight != 0)
            return true;

        if (effect.EffectId is >= 6_000_000 and < 8_000_000 || (includeUnique && effect.EffectId < 100_000))
            return true;

        return false;

    }

    public RelicErrors[] VerifyRelic(uint relic)
    {
        Debug.Print("Verifying relic " + relic);

        RelicErrors[] validator = new RelicErrors[3];

        for (int x = 0; x < playerRelics[relic].Count; x++)
        {
            validator[x] = RelicErrors.Legitimate;

            Debug.Print(playerRelics[relic][x].Slot1Weight + "");

            if (!playerRelics[relic][x].IsDeepEffect && playerRelics[relic][x].Slot1Weight == 0)
            {
                if (VerifyEffectIsRelicEffect(playerRelics[relic][x], true))
                    validator[x] = RelicErrors.UniqueRelicEffect;
                else
                {
                    validator[x] = RelicErrors.NotRelicEffect;
                    continue;
                }
            }

                
            for (int y = 0; y < playerRelics[relic].Count; y++)
            {
                if (x != y)
                {
                    if (playerRelics[relic][x].Category == playerRelics[relic][y].Category)
                    {
                        validator[x] = RelicErrors.MultipleFromCategory;
                        break;
                    }
                }
            }
        }
        return validator;
    }

    public string GetEffectDescription(uint relic, uint slot)
    {
        if (slot < playerRelics[relic].Count)
            return playerRelics[relic][(int)slot].Description;
        return "-";
    }
}