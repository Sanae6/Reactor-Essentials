﻿using Hazel;
using Reactor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Essentials.CustomOptions
{
    public partial class CustomOption
    {
        [RegisterCustomRpc]
        private protected class Rpc : CustomRpc<EssentialsPlugin, PlayerControl, Rpc.Data>
        {
            public Rpc(EssentialsPlugin plugin) : base(plugin)
            {
            }

            public struct Data
            {
                public readonly (string ID, CustomOptionType Type, object Value)[] Options;

                public Data(CustomOption option)
                {
                    Options = new[] { OptionToData(option) };
                }

                public Data(IEnumerable<CustomOption> options)
                {
                    Options = options.Select(option => OptionToData(option)).ToArray();
                }

                private static (string, CustomOptionType, object) OptionToData(CustomOption option)
                {
                    return (option.ID, option.Type, option.GetValue<object>());
                }

                public Data(IEnumerable<(string, CustomOptionType, object)> options)
                {
                    Options = options.ToArray();
                }
            }

            public override RpcLocalHandling LocalHandling { get { return RpcLocalHandling.None; } }

            public override void Write(MessageWriter writer, Data data)
            {
                foreach ((string ID, CustomOptionType Type, object Value) option in data.Options)
                {
                    writer.Write(option.ID);
                    writer.Write((int)option.Type);
                    if (option.Type == CustomOptionType.Toggle) writer.Write((bool)option.Value);
                    else if (option.Type == CustomOptionType.Number) writer.Write((float)option.Value);
                    else if (option.Type == CustomOptionType.String) writer.Write((int)option.Value);
                }
            }

            public override Data Read(MessageReader reader)
            {
                List<(string, CustomOptionType, object)> options = new List<(string, CustomOptionType, object)>();
                while (reader.BytesRemaining > 0)
                {
                    string id = reader.ReadString();
                    CustomOptionType type = (CustomOptionType)reader.ReadInt32();
                    object value = null;
                    if (type == CustomOptionType.Toggle) value = reader.ReadBoolean();
                    else if (type == CustomOptionType.Number) value = reader.ReadSingle();
                    else if (type == CustomOptionType.String) value = reader.ReadInt32();

                    options.Add((id, type, value));
                }

                return new Data(options);
            }

            public override void Handle(PlayerControl innerNetObject, Data data)
            {
                Plugin.Log.LogWarning($"{innerNetObject.Data.PlayerName} sent options:");
                foreach ((string ID, CustomOptionType Type, object Value) option in data.Options)
                {
                    Plugin.Log.LogWarning($"\"{option.ID}\" type: {option.Type}, value: {option}");

                    CustomOption customOption = CustomOption.Options.FirstOrDefault(o => o.ID.Equals(option.ID, StringComparison.Ordinal));
                    customOption?.SetValue(option.Value, false);
                }
            }
        }
    }
}
