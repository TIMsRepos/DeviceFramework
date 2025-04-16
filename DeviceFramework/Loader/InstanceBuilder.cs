using System;
using System.Reflection;

namespace TIM.Devices.Framework.Loader
{
    /// <summary>
    /// Class to instantiate objects by there given Type object
    /// </summary>
    internal class InstanceBuilder
    {
        #region Methods

        /// <summary>
        /// Creates a new instance of the given type, defined in the provided assembly using the default constructor
        /// </summary>
        /// <typeparam name="T">The wanted return format, may be a more generic base class oder interface</typeparam>
        /// <param name="MyAssembly">The assembly where the type is defined</param>
        /// <param name="MyType">The type to create an instance</param>
        /// <returns>The created object of the class, represented by the given type</returns>
        public static T New<T>(Assembly MyAssembly, Type MyType)
        {
            T MyObject = default(T);

            ConstructorInfo MyConstructor = MyType.GetConstructor(new Type[0]);
            if (MyConstructor == null)
                throw new FrameworkException(string.Format("Type '{0}' doesn't have a parameterless default constructor", MyType.FullName));
            MyObject = New<T>(delegate ()
            {
                return (T)MyAssembly.CreateInstance(MyType.FullName);
            });

            return MyObject;
        }

        /// <summary>
        /// Creates a new instance of the given type, defined in the provided assembly using a fitting constructor
        /// </summary>
        /// <typeparam name="T">The wanted return format, may be a more generic base class oder interface</typeparam>
        /// <param name="MyAssembly">The assembly where the type is defined</param>
        /// <param name="MyType">The type to create an instance</param>
        /// <param name="objParams">The objects to pass to the constructor, need to be ordered correctly</param>
        /// <returns>The created object of the class, represented by the given type</returns>
        public static T New<T>(Assembly MyAssembly, Type MyType, params object[] objParams)
        {
            T MyObject = default(T);

            ConstructorInfo MyConstructor = MyType.GetConstructor(GetTypes(objParams));
            if (MyConstructor == null)
                throw new FrameworkException(string.Format("Type '{0}' doesn't have a fitting paramterized constructor", MyType.FullName));
            MyObject = (T)MyAssembly.CreateInstance(MyType.FullName,
                false, BindingFlags.Default, null, objParams, null, null);

            return MyObject;
        }

        /// <summary>
        /// Helper method to get a Type object list for the given parameter object, used for lookup of the constructor
        /// </summary>
        /// <param name="objParams">Parameter objects</param>
        /// <returns>The Type object list, accordingly to the parameter objects</returns>
        private static Type[] GetTypes(params object[] objParams)
        {
            Type[] MyTypes = new Type[objParams.Length];

            for (int i = 0; i < objParams.Length; ++i)
                MyTypes[i] = objParams[i].GetType();

            return MyTypes;
        }

        #endregion

        private static T New<T>(Func<T> MyFunc)
        {
            T MyObj = default(T);

            MyObj = MyFunc();

            return MyObj;
        }
    }
}