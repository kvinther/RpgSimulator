using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RpgSim
{
    public static class Extensions
    {
        public static IEnumerable<MethodInfo> GetMethodsBySig(this Type type, Type returnType,
            params Type[] parameterTypes)
        {
            return type.GetMethods().Where(m =>
            {
                if (m.ReturnType != returnType) return false;
                var parameters = m.GetParameters();
                if ((parameterTypes == null || parameterTypes.Length == 0))
                    return parameters.Length == 0;
                if (parameters.Length != parameterTypes.Length)
                    return false;
                for (int i = 0; i < parameterTypes.Length; i++)
                {
                    if (parameters[i].ParameterType != parameterTypes[i])
                        return false;
                }
                return true;
            });
        }
    }

    public static class ExceptionExtensions
    {
        public static string AllMessages(this Exception exception)
        {
            var builder = new StringBuilder();
            var innerException = exception;
            do
            {
                builder.AppendLine(innerException.Message);
                innerException = innerException.InnerException;
            }
            while (innerException != null);
            return builder.ToString();
        }

        private static IEnumerable<Exception> EnumerateInnerExceptions(this Exception exception)
        {
            var list = new List<Exception>();
            var item = exception;
            do
            {
                list.Add(item);
                item = item.InnerException;
            }
            while (item != null);
            return list;
        }

        public static string PrettyPrint(this Exception exception)
        {
            return string.Join(Environment.NewLine, exception.EnumerateInnerExceptions().Select(PrettyPrintException));
        }

        public static string PrettyPrintData(this Exception exception)
        {
            var builder = new StringBuilder();
            if (exception.Data.Count > 0)
            {
                builder.AppendLine("*** Exception Data ***");
                foreach (var obj2 in exception.Data.Keys)
                {
                    if (!obj2.Equals("___ERROR_REASON___"))
                    {
                        builder.AppendLine(string.Format("Key: {0}, Value: {1}", obj2, exception.Data[obj2]));
                    }
                }
            }
            return builder.ToString();
        }

        private static string PrettyPrintException(Exception exception)
        {
            var builder = new StringBuilder();
            builder.AppendLine(string.Format("Exception type: {0}", exception.GetType().Name));
            builder.AppendLine(string.Format("Message: {0}", exception.Message));
            if (!exception.Reason().Is(BasicErrorReason.NoReasonFound))
            {
                builder.AppendLine(string.Format("Reason: {0}", exception.Reason()));
            }
            builder.AppendLine(exception.PrettyPrintData());
            return builder.ToString();
        }

        public static Enum Reason(this Exception exception)
        {
            if (exception.Data.Contains("___ERROR_REASON___") && (exception.Data["___ERROR_REASON___"] is Enum))
            {
                return (Enum)exception.Data["___ERROR_REASON___"];
            }
            return BasicErrorReason.NoReasonFound;
        }

        public static void ThrowInnerMost(this Exception exception)
        {
            throw exception.EnumerateInnerExceptions().Last();
        }

        public static Exception WithData(this Exception exception, params KeyValuePair<object, object>[] keyValuePairs)
        {
            foreach (var pair in keyValuePairs)
            {
                exception.Data.Add(pair.Key, pair.Value);
            }
            return exception;
        }

        public static Exception WithData(this Exception exception, params object[] keyValuePairs)
        {
            var objArray = keyValuePairs.Take((keyValuePairs.Length - (((keyValuePairs.Length % 2) == 0) ? 0 : 1))).ToArray();
            for (var i = 0; i < objArray.Length; i += 2)
            {
                exception.Data.Add(objArray[i], objArray[i + 1]);
            }
            return exception;
        }

        public static Exception WithData(this Exception exception, object key, object value)
        {
            exception.Data.Add(key, value);
            return exception;
        }

        public static Exception WithReason(this Exception exception, Enum reason)
        {
            return exception.WithData("___ERROR_REASON___", reason);
        }
    }

    [Serializable]
    public enum BasicErrorReason
    {
        NoReasonFound = -1000
    }

    public static class EnumExtensions
    {
        public static bool Is(this Enum enum1, Enum enum2)
        {
            return Equals(enum1, enum2);
        }

        public static string Description(this Enum value)
        {
            Type enumType = value.GetType();
            string name = Enum.GetName(enumType, value);
            if (name == null)
            {
                return null;
            }
            FieldInfo field = enumType.GetField(name);
            if (field == null)
            {
                return null;
            }
            DescriptionAttribute customAttribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            if (customAttribute == null)
            {
                return null;
            }
            return customAttribute.Description;
        }
    }
}
