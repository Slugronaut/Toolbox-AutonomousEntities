using UnityEngine;
using System;
using System.Reflection;
using UnityEngine.Assertions;
using System.Linq.Expressions;

namespace Peg.AutonomousEntities
{

    /// <summary>
    /// Utility for pushing data to a container using reflection.
    /// </summary>
    public static class BindingHelper
    {
        public const BindingFlags DefaultFlags = BindingFlags.FlattenHierarchy | BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public;

        /*
         ************************************ FAILED EXPERIMENT - NONE OF THESE WORK *********************************
        /// <summary>
        /// Returns a delegate to an object's property getter.
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static Func<T> CreateGetterDelegate<T>(object context, string propName, BindingFlags flags = DefaultFlags)
        {
            return CreateGetterDelegate<T>(context, context.GetType().GetProperty(propName, flags));
        }

        /// <summary>
        /// Returns a delegate to an object's property getter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static Func<T> CreateGetterDelegate<T>(object context, PropertyInfo prop)
        {
            return (Func<T>)Delegate.CreateDelegate(typeof(Func<T>), prop.GetGetMethod());
        }

        /// <summary>
        /// Returns a delegate to an object's property getter.
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static Delegate CreateGetterDelegate(object context, string propName, BindingFlags flags = DefaultFlags)
        {
            return CreateGetterDelegate(context, context.GetType().GetProperty(propName, flags));
        }
        
        /// <summary>
        /// Returns a delegate to an object's property getter.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static Delegate CreateGetterDelegate(object context, PropertyInfo prop)
        {
            var delType = typeof(Func<,>).MakeGenericType(typeof(object), prop.PropertyType);
            return Delegate.CreateDelegate(delType, prop.GetGetMethod());
            //var meth = prop.GetGetMethod();
            //return Delegate.CreateDelegate(typeof(Func<object, float>), null, meth);
        }
  
        /// <summary>
        /// Returns a delegate to an object's property setter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static Action<T> CreateSetterDelegate<T>(object context, string propName, BindingFlags flags = DefaultFlags)
        {
            return CreateSetterDelegate<T>(context, context.GetType().GetProperty(propName, flags));
        }

        /// <summary>
        /// Returns a delegate to an object's property setter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static Action<T> CreateSetterDelegate<T>(object context, PropertyInfo prop)
        {
            return (Action<T>)Delegate.CreateDelegate(typeof(Action<T>), prop.GetSetMethod());
        }

        /// <summary>
        /// Returns a delegate to an object's property setter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static Action<object, object> CreateSetterDelegate(object context, string propName, BindingFlags flags = DefaultFlags)
        {
            return CreateSetterDelegate(context, context.GetType().GetProperty(propName, flags));
        }

        /// <summary>
        /// Returns a delegate to an object's property setter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static Action<object, object> CreateSetterDelegate(object context, PropertyInfo prop)
        {
            return (Action<object, object>)Delegate.CreateDelegate(typeof(Action<object, object>), prop.GetSetMethod());
        }

            ************************************ END FAILED EXPERIMENT *********************************
        */


        /// <summary>
        /// Dynamically constructs a delegate to an arbitrary property getter.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="propName"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static Func<object, object> BuildGetAccessor(object context, string propName, BindingFlags flags = DefaultFlags)
        {
            return BuildGetAccessor(context.GetType().GetProperty(propName, flags));
        }

        /// <summary>
        /// Dynamically constructs a delegate to an arbitrary property setter.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="propName"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static Action<object, object> BuildSetAccessor(object context, string propName, BindingFlags flags = DefaultFlags)
        {
            return BuildSetAccessor(context.GetType().GetProperty(propName, flags));
        }

        /// <summary>
        /// Dynamic way of building a delegate to an arbitrary property getter/setter.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static Func<object, object> BuildGetAccessor(PropertyInfo prop)
        {
            ParameterExpression context = Expression.Parameter(typeof(object), "context");
            var method = prop.GetGetMethod();

            var call = Expression.Call(Expression.Convert(context, prop.DeclaringType), method);
            var expr = Expression.Lambda<Func<object, object>>(Expression.Convert(call, typeof(object)), context);
            return expr.Compile();
        }

        /// <summary>
        /// Dynamic way of building a delegate to an arbitrary property getter/setter.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static Action<object, object> BuildSetAccessor(PropertyInfo prop)
        {
            ParameterExpression context = Expression.Parameter(typeof(object), "context");
            ParameterExpression value = Expression.Parameter(typeof(object), "value");
            var method = prop.GetSetMethod();

            var call = Expression.Call(
                Expression.Convert(context, prop.DeclaringType),
                method,
                Expression.Convert(value, prop.PropertyType));

            var expr = Expression.Lambda<Action<object, object>>(call, context, value);
            return expr.Compile();
        }

        /// <summary>
        /// Uses reflection to push a value to the field or property of a component.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data to push.</param>
        /// <param name="boundObject">The component whose field or property is being pushed to.</param>
        /// <param name="boundMember">The field or property to push to.</param>
        /// <param name="converter">An optional delegate that handles any special conversion rules. It must output the same datatpe it takes as an input.</param>
        public static void PushBind<T>(T data, Component boundObject, MemberInfo boundMember, Func<T, T> converter)
        {
            Assert.IsNotNull(boundObject);
            Assert.IsNotNull(boundMember);

            if (converter != null) data = converter(data);
            PropertyInfo prop = boundMember as PropertyInfo;
            if (prop != null && prop.CanWrite)
                prop.SetValue(boundObject, data, null);
            else
            {
                FieldInfo field = boundMember as FieldInfo;
                if (field != null)
                    field.SetValue(boundObject, data);
            }
        }

        /// <summary>
        /// Uses reflection to push a value to the field or property of a component on a GameObject. The bindPath
        /// specifies which component and what field or property on the target GameObject and takes the form of
        /// 'NameSpace.ComponentTypeName:FieldOrPropertyName'. It does not support accessing of nested classes,
        /// private fields, or properties that do not have a public setter.
        /// </summary>
        /// <param name="data">The data to push.</param>
        /// <param name="target">The GameObject who holds a reference to the component identified in the bindPath.</param>
        /// <param name="bindPath">The path to the field or property in the component that will be set. Takes the form of [Namespace.ComponentName:FieldOrPropName].</param>
        /// <param name="converter">A optional delegate that handles any special convertion rules. It must output the same dataype it takes as an input.</param>
        public static void PushBind<T>(T data, EntityRoot target, string bindPath, Func<T, T> converter)
        {
            if (string.IsNullOrEmpty(bindPath)) return;
            Component comp = null;
            MemberInfo info = BoundMember(bindPath, target, out comp);
            PushBind(data, comp, info, converter);
        }

        /// <summary>
        /// Given the full typename and field/property in an AEH, returns the member and component it belongs to.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="target"></param>
        /// <param name="comp"></param>
        /// <returns></returns>
        public static MemberInfo BoundMember(string bindPath, EntityRoot target, out Component comp)
        {
            string[] path = bindPath.Split(':');
            string name = path[1];//.Trim();
            string typeName = path[0];//.Trim();
            comp = target.FindComponentInEntity(TypeHelper.GetType(typeName), true);

            var mems = comp.GetType().GetMember(name, BindingFlags.FlattenHierarchy |
                                                      BindingFlags.Instance |
                                                      BindingFlags.Public |
                                                      BindingFlags.GetProperty |
                                                      BindingFlags.GetField);
            return mems.Length >= 0 ? mems[0] : null;
        }

        /// <summary>
        /// Returns the first found member of a object.
        /// </summary>
        /// <param name="memberName"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static MemberInfo BoundMember(string memberName, object context)
        {
            var mems = context.GetType().GetMember(memberName, BindingFlags.FlattenHierarchy |
                                                               BindingFlags.Instance |
                                                               BindingFlags.Public |
                                                               BindingFlags.GetProperty |
                                                               BindingFlags.GetField);
            return mems.Length >= 0 ? mems[0] : null;
        }

        /// <summary>
        /// Returns the value of a property or field with a given name on a given UnityEngine.Object.
        /// The desired member must either be a public field or a public, readable property of the object.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static object GetDataSource(this UnityEngine.Object context, string path)
        {
            var mems = context.GetType().GetMember(path, DefaultFlags);
            var mem = mems.Length > 0 ? mems[0] : null;
            if (mem == null) return null;
            return mem.GetValue(context);
        }

        /// <summary>
        /// Sets the value of a property or field with a given name on a given UnityEnigne.Object.
        /// The desired member must either be a public field or a public, writable property of the object.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="value"></param>
        /// <param name="path"></param>
        public static void SetDataSource(this UnityEngine.Object context, object value, string path)
        {
            var mems = context.GetType().GetMember(path, DefaultFlags);
            var mem = mems.Length > 0 ? mems[0] : null;
            if (mem != null) mem.SetValue(context, value);
        }
    }


}
