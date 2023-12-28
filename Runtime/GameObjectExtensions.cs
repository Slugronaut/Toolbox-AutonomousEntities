using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Assertions;
using System.Collections;
using Peg.Collections;
using Object = UnityEngine.Object;

namespace Peg.AutonomousEntities
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
                FindComponentsHelper(child, list);
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
                T comp = t.GetChild(i).gameObject.FindComponentOnGameObject<T>(name);
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
                GameObject go = t.GetChild(i).gameObject.FindGameObject(name);
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
            FindComponentsHelper(root, list);
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
            if (useLookup)
            {
                comps = entity.LookupComponents<T>();
                if (comps != null) return comps;
            }

            var list = SharedArrayFactory.RequestTempList<T>();
            FindComponentsHelper(entity.transform, list);
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
        public static void DestroyInvarient(this MonoBehaviour behaviour, Object obj, bool delayInEditor = false)
        {
#if UNITY_EDITOR
            if (Application.isPlaying) Object.Destroy(obj);
            else if (!delayInEditor) Object.DestroyImmediate(obj);
            else
            {
                DestroyCallback = () =>
                {
                    Object.DestroyImmediate(obj);
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
        public static Coroutine InvarientDelay(this MonoBehaviour mb, Action action)
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
        public static Coroutine InvarientDelay(this MonoBehaviour mb, Action action, Coroutine chained)
        {
            if (Application.isPlaying) return mb.StartCoroutine(DelayProcessChained(action, chained));
            else if (action != null) action();
            return null;
        }


        static IEnumerator DelayProcess(Action action)
        {
            yield return new WaitForEndOfFrame();
            if (action != null) action();
            yield break;
        }

        static IEnumerator DelayProcessChained(Action action, Coroutine chained)
        {
            yield return chained;
            yield return new WaitForEndOfFrame();
            if (action != null) action();
            yield break;
        }
    }
}
