using UnityEngine;
using System.Collections.Generic;
using System;

namespace Peg.AutonomousEntities
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
            if (DispatchRoot == null) DispatchRoot = GetComponent<LocalMessageDispatch>();
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
            if (comp != null) LookupTable[t] = comp;
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

}

