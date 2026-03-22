using MinetaleConverter.Base.Logic.Serialization.NBT.Interfaces;
using MinetaleConverter.Minecraft.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharpNBT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinetaleConverter.Minecraft.Logic
{
    public class mc_BiomePaletterReader : INbtConvertReader
    {
        public object? Convert(JObject reader)
        {
            JProperty? top = (JProperty?)reader.First;
            if (top == null)
                throw new Exception("what the fuck?");

            var props = top?.First;
            if (props == null)
                throw new Exception("what the fuck?");

            JArray? jPalette = props.Value<JArray>("palette");
            if (jPalette == null)
                throw new Exception("What the fuck?");

            mc_Resource[] palette = new mc_Resource[jPalette.Count];
            for (int i = 0; i < jPalette.Count; i++)
            {
                var item = jPalette.Children().ElementAt(i);

                palette[i] = new mc_Resource()
                {
                    Name = item?.ToString() ?? "FUCK"
                };
            }

            JArray? jData = props.Value<JArray>("data");
            long[] data = new long[0];
            if (jData != null)
            {
                data = new long[jData.Count];
                for (int i = 0; i < jData.Count; i++)
                {
                    string longStr = jData.Children().ElementAt(i).ToString();
                    data[i] = long.Parse(longStr);
                }
            }
            return new mc_ResourcePalette(palette.ToList(), data, 1);
        }
    }
}
