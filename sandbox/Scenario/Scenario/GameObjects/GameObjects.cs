using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Text;
using Newtonsoft.Json;
using NLua;

namespace Scenario.GameObjects
{
    public class BaseObject {}

    public class GameObject : BaseObject
    {
        public GameObject(string id)
        {
            Id = id;
        }

        public string Id { get; set; }
        public override string ToString()
        {
            return Id;
        }
    }

    public class Item : GameObject
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public Item(string id) : base(id)
        {
        }
    }

    public class Lock : GameObject
    {
        public bool Locked { get; set; }
        public string Key { get; set; }
        public int Strength { get; set; }

        public Lock(string id) : base(id)
        {
        }
    }

    public class Container : GameObject
    {
        public Container(string id) : base(id)
        {
            Items = new List<Item>();
        }

        public string Name { get; set; }
        public int Size { get; set; }
        public string Description { get; set; }
        public Lock Lock { get; set; }
        public bool Open { get; set; }

        public ICollection<Item> Items;
    }

    public class Exit : GameObject
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string DestinationId { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public Exit(string id) : base(id)
        {
        }
    }

    public class Location : GameObject
    {
        public Location(string id) : base(id)
        {
            Items = new List<Item>();
            Containers = new List<Container>();
            Exits = new List<Exit>();
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<Item> Items { get; set; }
        public ICollection<Container> Containers { get; set; }
        public ICollection<Exit> Exits { get; set; }
    }

    public class Scene : GameObject
    {
        public Scene(string id) : base(id)
        {
            Locations = new List<Location>();
        }

        public string Comments { get; set; }
        public ICollection<Location> Locations { get; set; }
    }

    public class Scenario : GameObject
    {
        public string Name { get; set; }
        public ICollection<Scene> Scenes { get; set; }

        public Scenario(string id) : base(id)
        {
            Scenes = new List<Scene>();
        }
    }

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

                    var text = lua.DoString(luaCode).First();
                    b.Append(text);
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
