using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NLua;

namespace RpgSim
{
    public static class LuaRunner
    {
        public static object[] DoString(Lua lua, string luaSource)
        {
            try
            {
                return lua.DoString(luaSource);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.PrettyPrint());
                Debug.WriteLine(ex.PrettyPrintData());
            }
            return new object[] {};
        }

        public static object[] DoFile(Lua lua, string luaFilePath)
        {
            if (File.Exists(luaFilePath))
            {
                Debug.WriteLine(string.Format("LUA: DoFile: {0}", Path.GetFileNameWithoutExtension(luaFilePath)));
                var luaSource = File.ReadAllText(luaFilePath);
                return DoString(lua, luaSource);
            }
            return new object[] {};
        }

        public static string GetScenarioLuaBaseFolder(string scenarioName)
        {
            // TODO: Add better logic for locating scenario files.
            return Path.Combine(@"..\..\..\LuaStuff", scenarioName);
        }

        public static string GetScenarioLuaScriptsFolder(string scenarioName)
        {
            // TODO: Add better logic for locating scenario files.
            return Path.Combine(@"..\..\..\LuaStuff", scenarioName, "scripts");
        }

        public static string GetScenarioSaveFolder(string scenarioName)
        {
            // TODO: Add better logic for locating scenario files.
            return Path.Combine(@"..\..\..\Scenario", scenarioName);
        }

        public static string GetEngineLuaScriptsFolder()
        {
            // TODO: Add better logic for locating scenario files.
            return Path.Combine(@"..\..\..\LuaStuff", "scripts");
        }

        public static void RunLuaScriptsInFolder(Lua lua, string scriptsFolder)
        {
            if (!Directory.Exists(scriptsFolder))
                throw new ApplicationException(string.Format("Scripts folder not found: {0}", scriptsFolder))
                    .WithData("scriptsFolder", scriptsFolder);
            var files = Directory.GetFiles(scriptsFolder, "*.lua")
                .OrderBy(x => x)
                .ToList();

            foreach (var file in files)
                DoFile(lua, file);
        }
    }
}