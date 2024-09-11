using Il2CppInterop.Runtime.InteropTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Jevil.Internal.Utilities;

internal static class ObjectDumper
{
    // Modified version of https://github.com/lassevk/ObjectDumper/blob/main/src/ObjectDumper/Dumper.cs
    internal static void InternalDump(int indentationLevel, string name, object value, StringBuilder builder, ObjectIDGenerator idGenerator, int maxRecurse)
    {
        bool recursiveDump = maxRecurse > 0;
        var indentation = new string(' ', indentationLevel * 3);

        if (value == null)
        {
            builder.AppendFormat("{0}{1} = <null>", indentation, name);
            return;
        }

        Type type = value.GetType();

        // figure out if this is an object that has already been dumped, or is currently being dumped
        string keyRef = string.Empty;
        string keyPrefix = string.Empty;
        if (!type.IsValueType)
        {
            bool firstTime;
            long key = idGenerator.GetId(value, out firstTime);
            if (!firstTime)
                keyRef = string.Format(" (see #{0})", key);
            else
            {
                keyPrefix = string.Format("#{0}: ", key);
            }
        }

        // work out how a simple dump of the value should be done
        bool isString = value is string;
        string typeName = value.GetType().FullName;
        string formattedValue = value.ToString();

        var exception = value as Exception;
        if (exception != null)
        {
            formattedValue = exception.GetType().Name + ": " + exception.Message;
        }

        if (formattedValue == typeName)
            formattedValue = string.Empty;
        else
        {
            // escape tabs and line feeds
            formattedValue = formattedValue.Replace("\t", "\\t").Replace("\n", "\\n").Replace("\r", "\\r");

            // chop at 80 characters
            //int length = formattedValue.Length;
            //if (length > 80)
            //    formattedValue = formattedValue.Substring(0, 80);
            if (isString)
                formattedValue = string.Format("\"{0}\"", formattedValue);
            //if (length > 80)
            //    formattedValue += " (+" + (length - 80) + " chars)";
            formattedValue = " = " + formattedValue;
        }

        //writer.WriteLine("{0}{1}{2}{3} [{4}]{5}", indentation, keyPrefix, name, formattedValue, value.GetType(), keyRef);
        builder.AppendFormat("{0}{1}{2}{3} [{4}]", indentation, keyPrefix, name, formattedValue, keyRef);

        // Avoid dumping objects we've already dumped, or is already in the process of dumping
        if (keyRef.Length > 0)
            return;

        // don't dump strings, we already got at around 80 characters of those dumped
        if (isString)
            return;

        // don't dump value-types in the System namespace
        if (type.IsValueType && type.Namespace.StartsWith("System"))
            return;

        // Avoid certain types that will result in endless recursion
        if (type.Namespace.StartsWith("System.Reflection"))
            return;

        if (value is System.Security.Principal.SecurityIdentifier)
            return;

        if (!recursiveDump)
            return;

        PropertyInfo[] properties =
            (from property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
             where property.GetIndexParameters().Length == 0
                   && property.CanRead
             select property).ToArray();
        IEnumerable<FieldInfo> fields = value is Il2CppObjectBase ? Enumerable.Empty<FieldInfo>() : type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (!properties.Any() && !fields.Any())
            return;

        builder.AppendFormat("{0}{{", indentation);
        if (properties.Any())
        {
            builder.AppendFormat("{0}   properties {{", indentation);
            foreach (PropertyInfo pi in properties)
            {
                try
                {
                    object propertyValue = pi.GetValue(value, null);
                    InternalDump(indentationLevel + 1, pi.Name, propertyValue, builder, idGenerator, maxRecurse);
                }
                catch (TargetInvocationException ex)
                {
                    InternalDump(indentationLevel + 1, pi.Name, ex, builder, idGenerator, 0);
                }
                catch (ArgumentException ex)
                {
                    InternalDump(indentationLevel + 1, pi.Name, ex, builder, idGenerator, 0);
                }
            }
            builder.AppendFormat("{0}   }}", indentation);
        }

        if (fields.Any())
        {
            builder.AppendFormat("{0}   fields {{", indentation);
            foreach (FieldInfo field in fields)
            {
                try
                {
                    object fieldValue = field.GetValue(value);
                    InternalDump(indentationLevel + 1, field.Name, fieldValue, builder, idGenerator, maxRecurse);
                }
                catch (TargetInvocationException ex)
                {
                    InternalDump(indentationLevel + 1, field.Name, ex, builder, idGenerator, maxRecurse);
                }
            }
            builder.AppendFormat("{0}   }}", indentation);
        }

        if (value is IEnumerable enumerable)
        {
            builder.AppendFormat("{0}   collections {{", indentation);

            foreach (var collectionItem in enumerable)
            {
                try
                {
                    object fieldValue = collectionItem;
                    InternalDump(indentationLevel + 1, "collectionItem", fieldValue, builder, idGenerator, maxRecurse);
                }
                catch (TargetInvocationException ex)
                {
                    InternalDump(indentationLevel + 1, "collectionItem", ex, builder, idGenerator, 0);
                }
            }
        }


        builder.AppendFormat("{0}}}", indentation);
    }

    internal static void TestCallerAttr(Action parameter, [CallerArgumentExpression("parameter")] string exp = "<none>")
    {
        TestCallerAttr(() => Console.WriteLine("who else up late wonkin they willy"));
    }
}
