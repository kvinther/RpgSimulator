using System;

namespace RpgSim
{
    [AttributeUsage(AttributeTargets.Method)]
    public class LuaRegisterAttribute : Attribute
    {
        public string FunctionName { get; set; }

        public LuaRegisterAttribute(string functionName)
        {
            FunctionName = functionName;
        }
    }
}