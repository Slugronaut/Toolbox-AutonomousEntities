using UnityEngine;
using System.Collections.Generic;
using System;
using Toolbox;
using UnityEngine.Assertions;
using System.Collections;
using Toolbox.Collections;

namespace Toolbox
{
    /// <summary>
    /// Used extensively by many aspects of Toolbox. It is used to mark the root 
    /// GameObject of a standalone entity that should be considered a separate 
    /// and complete object even when nested within other GameObject hierarchies.
    /// </summary>
    /// <remarks>
    /// If a LocalMessageDispatch is attached to the same GameObject as this, you can
    /// forward a <see cref="DemandEntityRoot"/> message to obtain a reference
    /// rather than having to use <see cref="GameObject.GetEntityRoot"/>. Using the demand
    /// will be significantly faster in many cases.
    /// 
    /// UPDATE: Changed the name from AutonomousEntity to EntityRoot. It was getting to be
    /// a real pain-in-the-ass to use the old name. If you see anything about AutonomousEntities,
    /// Autonomous Entity Hierarchies, or AEHs, they are referring to this class.
    /// </remarks>
    [AddComponentMenu("Toolbox/Core/Entity Root")]
    [DisallowMultipleComponent]
    //[Sirenix.OdinInspector.TypeInfoBox("This component is used to signify the root of a cohesive and singular entity. Any GameObjects within this hierarchy that are also marked with this component will be treated as separate entities and this GameObject will be treated as such if contained within another such hierarchy marked with this component.")]
    public sealed class EntityRoot : MonoBehaviour, IAutonomousEntity
    {
        public string Name 
        {
            get { return gameObject.name; }
            set { gameObject.name = value; }
        }

        /// <summary>
        /// Provides a reference to this entity's local message dispatcher if one is present.
        /// </summary>
        public LocalMessageDispatch DispatchRoot { get; private set; }
        Dictionary<Type, object> LookupTable = new Dictionary<Type, object>();
        Dictionary<Type, object[]> LookupMultiTable = new Dictionary<Type, object[]>();
        Dictionary<int, object> HashTable = new Dictionary<int, object>();
        
        void Awake()
        {
            if (LookupTable == null) LookupTable = new Dictionary<Type, object>(5);
            if (LookupMultiTable == null) LookupMultiTable = new Dictionary<Type, object[]>(5);
        }

        void Start()
        {
            if(DispatchRoot == null) DispatchRoot = GetComponent<LocalMessageDispatch>();
            if (DispatchRoot != null) DispatchRoot.AddLocalListener<DemandEntityRoot>(OnDemandMe);
            #if TOOLBOX_DEBUG
            else Debug.LogWarning("No Local Message Dispatch found on AEH root of " +Name+". Cannot forward messages to this entity.");
            #endif
        }

        void OnDestroy()
        {
            if (DispatchRoot != null) DispatchRoot.RemoveLocalListener<DemandEntityRoot>(OnDemandMe);
            DispatchRoot = null;
            LookupTable.Clear();
            LookupTable = null;
            HashTable.Clear();
            HashTable = null;
        }

        void OnDemandMe(DemandEntityRoot msg)
        {
            msg.Respond(this);
        }

        /// <summary>
        /// Adds a component to a lookup table for quick reference.
        /// Only a single component of each type may be stored in this table.
        /// </summary>
        /// <param name="comp"></param>
        public void AddComponentToLookup(object comp, Type t)
        {
            if(comp != null) LookupTable[t] = comp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="comps"></param>
        public void AddComponentsToLookup(object[] comps, Type t)
        {
            if (comps != null && comps.Length > 0)
                LookupMultiTable[t] = comps;
        }

        /// <summary>
        /// Retreives a previously stored component based on its type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T LookupComponent<T>() where T : class
        {
            LookupTable.TryGetValue(typeof(T), out object v);
            return v as T;
        }

        /// <summary>
        /// Retreives a previously stored component based on its type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public object LookupComponent(Type t)
        {
            object v = null;
            LookupTable.TryGetValue(t, out v);
            return v;
        }

        /// <summary>
        /// Retreives a previously stored list of components based on its type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T[] LookupComponents<T>() where T : class
        {
            LookupMultiTable.TryGetValue(typeof(T), out object[] v);
            return v as T[];
        }

        /// <summary>
        /// Retreives a previously stored list of components based on its type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public object[] LookupComponents(Type t)
        {
            LookupMultiTable.TryGetValue(t, out object[] v);
            return v;
        }

        /// <summary>
        /// Adds a component that is identified by a hash-value to an internal lookup table.
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="comp"></param>
        public void AddHashedComponentToLookup(int hash, object comp)
        {
            if (comp != null) HashTable[hash] = comp;
        }

        /// <summary>
        /// Retrives the previously stored component associated with the hash value
        /// or null if there is no previously stored data for that hash.
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public object LookupHashedIdComponent(int hash)
        {
            HashTable.TryGetValue(hash, out object comp);
            return comp;
        }
    }


    /// <summary>
    /// Universal interface by which to access game entities.
    /// </summary>
    public interface IAutonomousEntity
    {
        string Name { get; set; }
    }
    
}


namespace Toolbox
{
    /// <summary>
    /// Demand message for obtaining an AEH (Autonomouse Entity Hierarchy) root.
    /// </summary>
    public class DemandEntityRoot : Demand<EntityRoot>
    {
        public static DemandEntityRoot Shared = new DemandEntityRoot(null);
        public DemandEntityRoot(Action<EntityRoot> callback) : base(callback) { }
    }
}


namespace UnityEngine
{
    /// <summary>
    /// Extension method for GameObject that allows us to search for the
    /// 'root' of a specific autonomous GameObject hierarchy.
    /// </summary>
    /// <remarks>
    /// If a GameObject has an <see cref="EntityRoot"/> component attached to it,
    /// it is considered to be the root of any child objects and can be retrieved
    /// using this method.
    /// 
    /// 
    /// TODO: The method 'public static T FindComponentInEntity<T>(this AutonomousEntity entity, bool useLookup = false) where T : class'
    ///     has been updated to use the AEH lookup table, all other methods still need to be updated to do this.
    /// </remarks>
    public static partial class GameObjectExtension
    {
        #region Helpers
        /// <summary>
        /// Helper method for searching through GameObject hierarchies for components.
        /// </summary>
        private static T FindComponentHelper<T>(Transform root) where T : class
        {
            T comp = root.GetComponent<T>();
            if (!TypeHelper.IsReferenceNull(comp)) return comp;

            Transform child;
            for (int i = 0; i < root.childCount; i++)
            {
                child = root.GetChild(i);

                //avoid entering into new child entities
                var subEnt = child.GetComponent<EntityRoot>();
                if (subEnt != null) continue;

                comp = FindComponentHelper<T>(child);
                if (comp != null) return comp;
            }

            return null;
        }

        /// <summary>
        /// Helper method for searching through GameObject hierarchies for components.
        /// </summary>
        private static Component FindComponentHelper(Transform root, Type type)
        {
            var comp = root.GetComponent(type);
            if (comp != null) return comp;

            Transform child;
            for (int i = 0; i < root.childCount; i++)
            {
                child = root.GetChild(i);

                //avoid entering into new child entities
                var subEnt = child.GetComponent<EntityRoot>();
                if (subEnt != null) continue;

                comp = FindComponentHelper(child, type);
                if (comp != null) return comp;
            }

            return null;
        }

        /// <summary>
        /// Helper method for searching through GameObject hierarchies for components.
        /// </summary>
        private static void FindComponentsHelper<T>(Transform root, List<T> list)
        {
            list.AddRange(root.GetComponents<T>());

            Transform child;
            for (int i = 0; i < root.childCount; i++)
            {
                child = root.GetChild(i);

                //avoid entering into new child entities
                var subEnt = child.GetComponent<EntityRoot>();
                if (subEnt != null) continue;
                FindComponentsHelper<T>(child, list);
            }
        }

        /// <summary>
        /// Helper method for searching through GameObject hierarchies for components.
        /// </summary>
        private static void FindComponentsHelper(Transform root, Type type, List<Component> list)
        {
            list.AddRange(root.GetComponents(type));

            Transform child;
            for (int i = 0; i < root.childCount; i++)
            {
                child = root.GetChild(i);

                //avoid entering into new child entities
                var subEnt = child.GetComponent<EntityRoot>();
                if (subEnt != null) continue;
                FindComponentsHelper(child, type, list);
            }

        }
        #endregion


        #region Hierarchy Methods
        /// <summary>
        /// Walks up the transform hierarchy, starting at this
        /// GameObject, searching for the root of this hierarchy
        /// as marked with an attached <see cref="EntityRoot"/> component.
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        public static EntityRoot GetEntityRoot(this GameObject go)
        {
            if (go == null) return null;
            Transform root = go.transform;
            while (root != null)
            {
                var ent = root.GetComponent<EntityRoot>();
                if (ent != null) return ent;
                root = root.transform.parent;
            }

            return null;
        }

        /// <summary>
        /// Searches within an autonomous GameObject entity for a component. It will check starting at the root of the AE.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="go"></param>
        /// <returns></returns>
        public static T FindComponentInHierarchy<T>(this GameObject go, bool includeInactive = true)
        {
            Transform root = go.transform;
            while (true)
            {
                var ent = root.GetComponent<EntityRoot>();
                if (ent != null || root.transform.parent == null) break;
                root = root.transform.parent;
            }
            return root.GetComponentInChildren<T>(includeInactive);
        }

        /// <summary>
        /// Searches within an autonomous GameObject entity for a component. It will check starting at the root of the AE.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static T FindComponentInHierarchy<T>(this EntityRoot entity, bool includeInactive = true)
        {
            return entity.GetComponentInChildren<T>(includeInactive);
        }

        /// <summary>
        /// Searches within an autonomous GameObject entity for a component. It will check starting at the root of the AE.
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        public static Component FindComponentInHierarchy(this GameObject go, Type type)
        {
            Transform root = go.transform;
            while (true)
            {
                var ent = root.GetComponent<EntityRoot>();
                if (ent != null || root.transform.parent == null) break;
                root = root.transform.parent;
            }

            return root.GetComponentInChildren(type);
        }

        /// <summary>
        /// Searches within an autonomous GameObject entity for a component. It will check starting at the root of the AE.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static Component FindComponentInHierarchy(this EntityRoot entity, Type type)
        {
            return entity.GetComponentInChildren(type);
        }

        /// <summary>
        /// Searches within an autonomous GameObject entity for a component. It will check starting at the root of the AE.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="go"></param>
        /// <returns></returns>
        /// <remarks>
        /// This method is obsurdly slow and should not be used with high-frequency on a regular basis.
        /// </remarks>
        public static T[] FindComponentsInHierarchy<T>(this GameObject go)
        {
            Transform root = go.transform;
            while (true)
            {
                var ent = root.GetComponent<EntityRoot>();
                if (ent != null || root.transform.parent == null) break;
                root = root.transform.parent;
            }
            return root.GetComponentsInChildren<T>();
        }

        /// <summary>
        /// Searches within an autonomous GameObject entity for a component. It will check starting at the root of the AE.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <remarks>
        /// This method is obsurdly slow and should not be used with high-frequency on a regular basis.
        /// </remarks>
        public static T[] FindComponentsInHierarchy<T>(this EntityRoot entity)
        {
            return entity.GetComponentsInChildren<T>();
        }

        /// <summary>
        /// Searches within an autonomous GameObject entity for a component. It will check starting at the root of the AE.
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        /// <remarks>
        /// This method is obsurdly slow and should not be used with high-frequency on a regular basis.
        /// </remarks>
        public static Component[] FindComponentsInHierarchy(this GameObject go, Type type)
        {
            Transform root = go.transform;
            while (true)
            {
                var ent = root.GetComponent<EntityRoot>();
                if (ent != null || root.transform.parent == null) break;
                root = root.transform.parent;
            }
            return root.GetComponentsInChildren(type);
        }

        /// <summary>
        /// Searches within an autonomous GameObject entity for a component. It will check starting at the root of the AE.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <remarks>
        /// This method is obsurdly slow and should not be used with high-frequency on a regular basis.
        /// </remarks>
        public static Component[] FindComponentsInHierarchy(this EntityRoot entity, Type type)
        {
            return entity.GetComponentsInChildren(type);
        }
        #endregion


        #region Entity Methods
        /// <summary>
        /// Finds the first child GameObject of root that has the given name.
        /// This method can also return the root itself.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="root"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T FindComponentOnGameObject<T>(this GameObject root, string name) where T : Component
        {
            if (root.name == name) return root.GetComponent<T>();

            Transform t = root.transform;
            for (int i = 0; i < t.childCount; i++)
            {
                T comp = FindComponentOnGameObject<T>(t.GetChild(i).gameObject, name);
                if (comp != null) return comp;
            }

            return null;
        }

        /// <summary>
        /// Finds the first child GameObject of root that has the given name.
        /// This method can also return the root itself.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="root"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static GameObject FindGameObject(this GameObject root, string name)
        {
            if (root.name == name) return root;

            Transform t = root.transform;
            for (int i = 0; i < t.childCount; i++)
            {
                GameObject go = FindGameObject(t.GetChild(i).gameObject, name);
                if (go != null) return go;
            }

            return null;
        }

        /// <summary>
        /// Searches within an autonomous GameObject entity for a component.
        /// It will check starting at the root of the AE and will not search child entities.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="go"></param>
        /// <returns></returns>
        public static T FindComponentInEntity<T>(this GameObject go, bool useLookup = false) where T : class
        {
            Assert.IsNotNull(go);

            //yeah, this happens now... Unity 5.5 is fucked
            if (go == null || go.transform == null) return null;

            Transform root = go.transform;
            EntityRoot ent = null;
            while (true)
            {
                ent = root.GetComponent<EntityRoot>();
                if (ent != null || root.transform.parent == null) break;
                root = root.transform.parent;
            }

            if (ent == null) return null;

            T comp = null;
            if (useLookup)
            {
                comp = ent.LookupComponent<T>();
                if (comp != null) return comp;
            }
            comp = FindComponentHelper<T>(root);
            if (useLookup) ent.AddComponentToLookup(comp as Component, typeof(T));
            return comp;
        }

        /// <summary>
        /// Searches within an autonomous GameObject entity for a component.
        /// It will check starting at the root of the AE and will not search child entities.
        /// </summary>
        /// <remarks>
        /// This will be considerably faster than the GameObject version in some cases.
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static T FindComponentInEntity<T>(this EntityRoot entity, bool useLookup = false) where T : class
        {
            Assert.IsNotNull(entity);

            T comp = null;
            if (useLookup)
            {
                comp = entity.LookupComponent<T>();
                if (comp != null) return comp;
            }
            comp = FindComponentHelper<T>(entity.transform);
            if (useLookup) entity.AddComponentToLookup(comp as Component, typeof(T));
            return comp;
        }

        /// <summary>
        /// Searches within an autonomous GameObject entity for a component.
        /// It will check starting at the root of the AE and will not search child entities.
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        public static Component FindComponentInEntity(this GameObject go, Type type, bool useLookup = false)
        {
            Assert.IsNotNull(go);

            //TODO: lookups don't seem to work so well with sub-classes?
            Transform root = go.transform;
            EntityRoot ent = null;
            while (true)
            {
                ent = root.GetComponent<EntityRoot>();
                if (ent != null || root.transform.parent == null) break;
                root = root.transform.parent;
            }

            if (ent == null) return null;

            Component comp = null;
            if (useLookup)
            {
                comp = ent.LookupComponent(type) as Component;
                if (comp != null) return comp;
            }
            comp = FindComponentHelper(root, type);
            if (useLookup) ent.AddComponentToLookup(comp as Component, type);
            return comp;
        }

        /// <summary>
        /// Searches within an autonomous GameObject entity for a component.
        /// It will check starting at the root of the AE and will not search child entities.
        /// </summary>
        /// <remarks>
        /// This will be considerably faster than the GameObject version in some cases.
        /// </remarks>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static Component FindComponentInEntity(this EntityRoot entity, Type type, bool useLookup = false)
        {
            Assert.IsNotNull(entity);
            Component comp = null;
            if (useLookup)
            {
                comp = entity.LookupComponent(type) as Component;
                if (comp != null) return comp;
            }
            comp = FindComponentHelper(entity.transform, type);
            if (useLookup) entity.AddComponentToLookup(comp as Component, type);
            return comp;
        }

        /// <summary>
        /// Searches within an autonomous GameObject entity for a component.
        /// It will check starting at the root of the AE and will not search child entities.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="go"></param>
        /// <returns></returns>
        /// <remarks>
        /// This method is obsurdly slow and should not be used with high-frequency on a regular basis.
        /// </remarks>
        public static T[] FindComponentsInEntity<T>(this GameObject go)
        {
            Assert.IsNotNull(go);
            Transform root = go.transform;
            while (true)
            {
                var ent = root.GetComponent<EntityRoot>();
                if (ent != null || root.transform.parent == null) break;
                root = root.transform.parent;
            }
            
            var list = new List<T>(5);
            FindComponentsHelper<T>(root, list);
            return list.ToArray();
        }
        
        /// <summary>
        /// Searches within an autonomous GameObject entity for a component.
        /// It will check starting at the root of the AE and will not search child entities.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <remarks>
        /// Using this method and setting useLookup to true can be significantly faster.
        /// </remarks>
        public static T[] FindComponentsInEntity<T>(this EntityRoot entity, bool useLookup) where T : class
        {
            Assert.IsNotNull(entity);
            T[] comps = null;
            if(useLookup)
            {
                comps = entity.LookupComponents<T>();
                if (comps != null) return comps;
            }

            var list = SharedArrayFactory.RequestTempList<T>();
            FindComponentsHelper<T>(entity.transform, list);
            var arr = list.ToArray();
            if (useLookup) entity.AddComponentsToLookup(arr, typeof(T));
            return arr;
        }

        /// <summary>
        /// Searches within an autonomous GameObject entity for a component.
        /// It will check starting at the root of the AE and will not search child entities.
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        /// <remarks>
        /// This method is obsurdly slow and should not be used with high-frequency on a regular basis.
        /// </remarks>
        public static Component[] FindComponentsInEntity(this GameObject go, Type type)
        {
            Assert.IsNotNull(go);

            Transform root = go.transform;
            while (true)
            {
                var ent = root.GetComponent<EntityRoot>();
                if (ent != null || root.transform.parent == null) break;
                root = root.transform.parent;
            }

            var list = new List<Component>(5);
            FindComponentsHelper(root, type, list);
            return list.ToArray();
        }

        /// <summary>
        /// Searches within an autonomous GameObject entity for a component.
        /// It will check starting at the root of the AE and will not search child entities.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <remarks>
        /// This method is obsurdly slow and should not be used with high-frequency on a regular basis.
        /// </remarks>
        public static Component[] FindComponentsInEntity(this EntityRoot entity, Type type)
        {
            Assert.IsNotNull(entity);

            var list = new List<Component>(5);
            FindComponentsHelper(entity.transform, type, list);
            return list.ToArray();
        }
        #endregion
    }


    /// <summary>
    /// Adds additional functionality to GameObjects.
    /// </summary>
    public static partial class GameObjectExtensions
    {
#if UNITY_EDITOR
        static UnityEditor.EditorApplication.CallbackFunction DestroyCallback;
#endif
        /// <summary>
        /// Helper for destroying GameObjects both when in editmode and playmode.
        /// </summary>
        /// <param name="go"></param>
        public static void DestroyInvarient(this MonoBehaviour behaviour, UnityEngine.Object obj, bool delayInEditor = false)
        {
            #if UNITY_EDITOR
            if (Application.isPlaying) GameObject.Destroy(obj);
            else if (!delayInEditor) GameObject.DestroyImmediate(obj);
            else
            {
                DestroyCallback = () =>
                {
                    GameObject.DestroyImmediate(obj);
                    UnityEditor.EditorApplication.delayCall -= DestroyCallback;
                };
                UnityEditor.EditorApplication.delayCall += DestroyCallback;
            }
            #else
            GameObject.Destroy(obj);
            #endif
        }

        /// <summary>
        /// Used as a means of splitting some processes up over multiple frames during runtime.
        /// This is necessary due to the fact that some stuff requires destroyed gameobjects to
        /// have been processed, which won't occur until the end of the frame during playmode.
        /// </summary>
        /// <param name="act"></param>
        public static Coroutine InvarientDelay(this MonoBehaviour mb, System.Action action)
        {
            if (Application.isPlaying) return mb.StartCoroutine(DelayProcess(action));
            else if (action != null) action();
            return null;
        }

        /// <summary>
        /// Used as a means of splitting some processes up over multiple frames during runtime.
        /// This is necessary due to the fact that some stuff requires destroyed gameobjects to
        /// have been processed, which won't occur until the end of the frame during playmode.
        /// </summary>
        /// <param name="act"></param>
        public static Coroutine InvarientDelay(this MonoBehaviour mb, System.Action action, Coroutine chained)
        {
            if (Application.isPlaying) return mb.StartCoroutine(DelayProcessChained(action, chained));
            else if (action != null) action();
            return null;
        }


        static IEnumerator DelayProcess(System.Action action)
        {
            yield return new WaitForEndOfFrame();
            if (action != null) action();
            yield break;
        }

        static IEnumerator DelayProcessChained(System.Action action, Coroutine chained)
        {
            yield return chained;
            yield return new WaitForEndOfFrame();
            if (action != null) action();
            yield break;
        }
    }
}

