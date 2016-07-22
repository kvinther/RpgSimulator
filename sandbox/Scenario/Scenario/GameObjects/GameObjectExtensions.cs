using System;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Text;
using Newtonsoft.Json;
using NLua;

namespace RpgSim.GameObjects
{
    public static class GameObjectExtensions
    {
        public static string ToJson(this BaseObject gameObject)
        {
            return (JsonConvert.SerializeObject(gameObject, Formatting.Indented,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects,
                    TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple
                }));
        }

        public static T FromJsonToObject<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects
            });
        }

        public static Container WithItem(this Container container, Item item)
        {
            container.Items.Add(item);
            return container;
        }

        public static Location WithContainer(this Location location, Container container)
        {
            location.Containers.Add(container);
            return location;
        }

        public static Location WithItem(this Location location, Item item)
        {
            location.Items.Add(item);
            return location;
        }

        public static Scene WithLocation(this Scene scene, Location location)
        {
            scene.Locations.Add(location);
            return scene;
        }

        public static string LowerCaseFirst(this string str)
        {
            if (string.IsNullOrEmpty(str) || char.IsLower(str, 0))
                return str;

            return char.ToLowerInvariant(str[0]) 
                   + str.Length == 1 ? "" : str.Substring(1);
        }

        public static string EvaluateLuaSnippets(this string s, Lua lua)
        {
            const string startToken = "<lua>";
            const string endToken = "</lua>";
            var parts = s.Split(new[] {startToken}, StringSplitOptions.RemoveEmptyEntries);
            var b = new StringBuilder();

            foreach (var part in parts)
            {
                if (part.Contains(endToken))
                {
                    var leftRight = part.Split(new[] {endToken}, StringSplitOptions.None);
                    var luaCode = leftRight[0];
                    var plainText = leftRight[1];
                    LuaRunner.DoString(lua, luaCode);
                    var text = lua.DoString(luaCode).FirstOrDefault();
                    b.Append(text ?? "<null>");
                    b.Append(plainText);
                }
                else
                {
                    b.Append(part);
                }
            }
            return b.ToString();
        }
    }
}