using System;
using System.Reflection;

#region Type
var myClass = new MyClass(1, "Test1", "Test2");
var type1 = myClass.GetType();

Console.WriteLine($"type using GetType() method:");
type1.PrintType();
Console.WriteLine($"----------------------------------");

var type2 = typeof(MyClass);
Console.WriteLine($"type using typeof() method:");
type2.PrintType();
Console.WriteLine($"----------------------------------");

Console.WriteLine($"properties of type [GetType()] [typeof() doesn't have value]:");
type1.PrintProperties(myClass);
Console.WriteLine($"----------------------------------");
#endregion

#region reflection
Console.WriteLine($"constructors of type:");
type1.PrintConstructors();
Console.WriteLine($"----------------------------------");

Console.WriteLine($"methods of type (all):");
type1.PrintMethods();
Console.WriteLine($"----------------------------------");

Console.WriteLine($"methods of type (remove specials):");
type1.PrintMethods(false);
Console.WriteLine($"----------------------------------");

Console.WriteLine($"properties of type include get and set:");
type1.PrintPropertiesWithGetSet();
Console.WriteLine($"----------------------------------");
#endregion

#region late bindings
Console.WriteLine($"create instance with activator:");
(Activator.CreateInstance(type1) as MyClass).PrintT();
Console.WriteLine($"----------------------------------");

Console.WriteLine($"create instance with activator and fields of cunstructor:");
(Activator.CreateInstance(type1, 2, "Test3", "Test4") as MyClass).PrintT();
Console.WriteLine($"----------------------------------");

Console.WriteLine($"create instance with activator and fields of cunstructor:");
AdvanceActivator.CreateInstance<MyClass>(3, "Test5", "Test6").PrintT();
Console.WriteLine($"----------------------------------");

Console.WriteLine($"create instance with activator and fields:");
type1.PrintFieldWithLateBindingValue("Myproperty1","Test7");
Console.WriteLine($"----------------------------------");

Console.WriteLine($"create instance with activator and methods:");
type1.PrintMethodResultWithLateBinding<MyClass>("GetMyProperty");
Console.WriteLine($"----------------------------------");

Console.WriteLine($"create instance with activator and methods with input:");
type1.PrintMethodResultWithLateBinding<MyClass>("GetMyPropertyWithWord", "hello");
Console.WriteLine($"----------------------------------");
#endregion

#region Attribute
Console.WriteLine($"attributes of type:");
type1.PrintAttributes();
Console.WriteLine($"----------------------------------");
#endregion


#region class
[My]
[MyWithField(name:"my custom attribute")]
class MyClass
{
    public MyClass()
    {
    }

    public MyClass(int id, string myProperty1, string myProperty2)
    {
        Id = id;
        MyProperty1 = myProperty1;
        MyProperty2 = myProperty2;
        MyProperty = $"{MyProperty1} with {MyProperty2}.";
    }

    public string GetMyProperty()
    {
        return $"{MyProperty}";
    }

    public string GetMyPropertyWithWord(string word)
    {
        return $"{word} {GetMyProperty()}";
    }

    public int Id { get; set; }
    public string MyProperty1 { get; set; } = string.Empty;
    public string MyProperty2 { get; set; } = string.Empty;
    public string MyProperty { get; } = string.Empty;
}
#endregion

#region print methods
static class ExtensionMethod
{
    public static void PrintType(this Type type)
    {
        Console.WriteLine($"Name: {type.Name}");
        Console.WriteLine($"NameSpace: {type.Namespace}");
        Console.WriteLine($"IsPublic: {type.IsPublic}");
        Console.WriteLine($"IsAbstract: {type.IsAbstract}");
        Console.WriteLine($"IsGeneric: {type.IsGenericType}");
        Console.WriteLine($"IsEnum: {type.IsEnum}");
        Console.WriteLine($"IsValueType: {type.IsValueType}");
    }

    public static void PrintProperties<T>(this Type type, T? t)
    {
        var properties = type.GetProperties();
        foreach (PropertyInfo propertyInfo in properties)
        {
            Console.WriteLine($"{propertyInfo.Name}: {propertyInfo.PropertyType.Name} - value: {propertyInfo.GetValue(t)}" );
        }
    }

    public static void PrintConstructors(this Type type)
    {
        var constructors = type.GetConstructors();
        foreach (ConstructorInfo constructorInfo in constructors)
        {
            var parameters = new List<string>();
            foreach (ParameterInfo parameterInfo in constructorInfo.GetParameters())
            {
                parameters.Add($"{parameterInfo.ParameterType.Name} {parameterInfo.Name}");
            }
            Console.Write($"{type.Name}({string.Join(", ", parameters)})");
            Console.WriteLine();
        }
    }

    public static void PrintMethods(this Type type, bool IncludeSpecials = true)
    {
        var methods = type.GetMethods();
        foreach (MethodInfo methodInfo in methods)
        {
            if (!IncludeSpecials && methodInfo.IsSpecialName)
                continue;

            var parameters = new List<string>();
            foreach (ParameterInfo parameterInfo in methodInfo.GetParameters())
            {
                parameters.Add($"{parameterInfo.ParameterType.Name} {parameterInfo.Name}");
            }
            Console.Write($"{methodInfo.ReturnType.Name} {methodInfo.Name}({string.Join(",", parameters)})");
            Console.WriteLine();
        }
    }

    public static void PrintPropertiesWithGetSet(this Type type)
    {
        var properties = type.GetProperties();
        foreach (PropertyInfo propertyInfo in properties)
        {
            if (propertyInfo.GetMethod != null && propertyInfo.SetMethod != null)
            {
                Console.WriteLine("{0} {{ get; set; }}", propertyInfo.Name);
            }
            else if (propertyInfo.GetMethod != null && propertyInfo.SetMethod == null)
            {
                Console.WriteLine("{0} {{ get; }}", propertyInfo.Name);
            }
            else
            {
                Console.WriteLine("{0} {{ set; }}", propertyInfo.Name);
            }
        }
    }

    public static void PrintFieldWithLateBindingValue(this Type type, string field, string value)
    {
        var instance = Activator.CreateInstance(type);
        var propertyInfo = type.GetProperty(field);
        propertyInfo?.SetValue(instance, value);
        Console.WriteLine($"field {field}: value {propertyInfo?.GetValue(instance)}");
    }

    public static void PrintMethodResultWithLateBinding<T>(this Type type, string methodName)
    {
        var instance = (T)(Activator.CreateInstance(type, 4, "Test8", "Test9") ?? new());
        var method = type.GetMethod(methodName);
        Console.WriteLine($"output of method: {method?.Invoke(instance, null)}");
    }

    public static void PrintMethodResultWithLateBinding<T>(this Type type, string methodName, string input)
    {
        var instance = (T)(Activator.CreateInstance(type, 5, "Test10", "Test11") ?? new());
        var method = type.GetMethod(methodName);
        Console.WriteLine($"output of method: {method?.Invoke(instance, new object[1] {input})}");
    }

    public static void PrintAttributes(this Type type)
    {
        var attributes = type.GetCustomAttributes();
        foreach (Attribute attribute in attributes)
        {
            Console.WriteLine($"{attribute}");
        }
    }

    public static void PrintT<T>(this T t)
    {
        var properties = t?.GetType().GetProperties();
        foreach (PropertyInfo propertyInfo in properties?? Array.Empty<PropertyInfo>())
        {
            Console.WriteLine($"{propertyInfo.Name}: {propertyInfo.PropertyType.Name} - value: {propertyInfo.GetValue(t)}");
        }
    }

}
#endregion

#region AdvanceActivator
public class AdvanceActivator
{
    public static T CreateInstance<T>(params object[] parameters)
    {
        return (T)(Activator.CreateInstance(typeof(T), parameters) ?? new());
    }
}
#endregion

#region MyAttribute
public class MyAttribute : Attribute
{

}

public class MyWithFieldAttribute : Attribute
{
    public MyWithFieldAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; set; }
}
#endregion