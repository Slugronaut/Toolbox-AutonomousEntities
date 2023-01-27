/**
 * Copyright 2014
 * James Clark
 */
using UnityEngine;
using System;

namespace Toolbox
{
    /// <summary>
    /// Provides a standardized access point for message dispatching that can
    /// guarantee order of operations. This version is meant to be attached
    /// to individual entities.
    /// </summary>
    /// <remarks>
    /// There is now a local PostMessage function on this script but in many cases it is not recommended.
    /// Instead, messages should be forwarded through the <see cref="GlobalMessagePump.Dispatcher"/>
    /// which will internally track which GameObject is the recipient of any given message.
    /// </remarks>
    [AddComponentMenu("Toolbox/Core/Local Message Dispatch (Light)")]
    [RequireComponent(typeof(EntityRoot))]
    [DisallowMultipleComponent]
    //[Sirenix.OdinInspector.TypeInfoBox("This component enables direct message forwarding to a entity using the GlobalMessagePump. At startup, all children of this entity will be linked to this message dispatcher to allow for arbitraty gameobject targetting within the complete entity hierarchy. This component should be attached to the root of such a hierarchy.")]
    public class LocalMessageDispatchBase : MonoBehaviour
    {
        protected IMessageDispatcher Dispatcher = new InstantMessageDispatcher();

        [Tooltip("If set, this dispatcher will recursively register every GameObject under this one with the global message pump to listen for local dispatches. You should always leave this on unless you understand the implications of disabling it... which, if you are reading this means that you don't. WARNING: Never change this value at runtime!")]
        [SerializeField]
        bool _FullHierarchy = true;
        public bool FullHierarchy
        {
            get { return _FullHierarchy; }
            set
            {
                if (value != _FullHierarchy)
                {
                    //TODO: fixme!!
                    //POTENTIAL BUG ALERT: This could break if we dyanmically add/remove gameObjects to this hierarchy at runtime!
                    //This is because we'll be trying to remove references from the dispatch registry that no longer exist

                    //we'll have to change all registered local dispatch targets now
                    UnregisterWithGlobalPump(gameObject, Dispatcher, this, _FullHierarchy);
                    _FullHierarchy = value;
                    RegisterWithGlobalPump(gameObject, Dispatcher, this, _FullHierarchy);
                }
            }
        }

        /// <summary>
        /// Helper method for registering this dispatch with the global dispatcher.
        /// </summary>
        /// <param name="whom"></param>
        /// <param name="dispatch"></param>
        /// <param name="hierarchy"></param>
        protected virtual void RegisterWithGlobalPump(GameObject whom, IMessageDispatcher dispatch, LocalMessageDispatchBase lmd, bool hierarchy)
        {
            if (Toolbox.TypeHelper.IsReferenceNull(whom)) return;
            //NOTE: if you get NULL REFERENCE EXCEPTION somewhere in here it's probably because you removed
            //a GameObject in the hierarchy at runtime. See above for details why.
            var skip = whom.GetComponent<SkipLocalDispatch>();
            if (skip == null)
            {
                //if for some dumb reason, this entity has one already, just re-use it
                var target = whom.GetComponent<LocalDispatchTargetBase>();
                if (target == null) target = whom.AddComponent<LocalDispatchTargetBase>() as LocalDispatchTargetBase;
                //target.Dispatch = lmd;
                GlobalMessagePump.Instance.RegisterLocalDispatch(whom, dispatch);
            }

            if (hierarchy)
            {
                for (int i = 0; i < whom.transform.childCount; i++)
                {
                    var child = whom.transform.GetChild(i).gameObject;
                    //notice that we must check for AutonomousEntity. This way we don't have
                    //things like 'Rooms' containing 'Mobs' that are accidentally dispatching to the mobs.
                    if (child.GetComponent<EntityRoot>() == null && (skip == null || !skip.SkipChildren))
                        RegisterWithGlobalPump(child, dispatch, lmd, hierarchy);
                }
            }
        }

        /// <summary>
        /// Helper method for registering this dispatch with the global dispatcher.
        /// </summary>
        /// <param name="whom"></param>
        /// <param name="dispatch"></param>
        /// <param name="hierarchy"></param>
        protected static void UnregisterWithGlobalPump(GameObject whom, IMessageDispatcher dispatch, LocalMessageDispatchBase lmd, bool hierarchy)
        {
            if (Toolbox.TypeHelper.IsReferenceNull(whom)) return;
            //NOTE: if you get NULL REFERENCE EXCEPTION somewhere in here it's probably because you removed
            //a GameObject in the hierarchy at runtime. See above for details why.
            var skip = whom.GetComponent<SkipLocalDispatch>();
            if (skip == null)
            {
                //remove target component
                LocalDispatchTarget target = whom.GetComponent<LocalDispatchTarget>();
                if (target != null) Destroy(target);
                GlobalMessagePump.Instance.UnregisterLocalDispatch(whom);
            }

            if (hierarchy)
            {
                for (int i = 0; i < whom.transform.childCount; i++)
                {
                    var child = whom.transform.GetChild(i).gameObject;
                    //notice that we must check for AutonomousEntity. This way we don't have
                    //things like 'Rooms' containing 'Mobs' that are accidentally dispatching to the mobs.
                    if (child.GetComponent<EntityRoot>() == null && (skip == null || !skip.SkipChildren))
                        UnregisterWithGlobalPump(child, dispatch, lmd, hierarchy);
                }
            }
        }

        protected virtual void Awake()
        {
            RegisterWithGlobalPump(gameObject, Dispatcher, this, _FullHierarchy);
        }

        protected virtual void OnDestroy()
        {
            UnregisterWithGlobalPump(gameObject, Dispatcher, this, _FullHierarchy);
        }

        /// <summary>
        /// Used to register a child GameObject with the GlobalMessagePump as a forward
        /// target for this instance's internal dispatcher. Mostly used internally
        /// as a way of making child objects of this GameObject usable in ForwardMessage
        /// as a target for their whole hierarchy's local message dispatch system.
        /// </summary>
        /// <param name="who"></param>
        public void AddLocalForwardTarget(GameObject who)
        {
            if (who == this.gameObject) return; //no need to add this if we are the target

            //walk up the hierarchy and ensure this object is actually a child
            Transform p = who.transform.parent;
            while (p != null)
            {
                if (p.gameObject == this.gameObject)
                {
                    RegisterWithGlobalPump(who, Dispatcher, this, _FullHierarchy);
                    return;
                }
            }


        }

        /// <summary>
        /// Used to unregister a child GameObject wth the GlobalMessagePump as a forward
        /// target for this instance's internal dispatcher.
        /// </summary>
        /// <param name="who"></param>
        public void RemoveFowardTarget(GameObject who)
        {
            if (who == this.gameObject) return; //no need for this

            //walk up the hierarchy and ensure this object is actually a child
            Transform p = who.transform.parent;
            while (p != null)
            {
                if (p.gameObject == this.gameObject)
                {
                    UnregisterWithGlobalPump(who, Dispatcher, this, _FullHierarchy);
                    return;
                }
            }
        }

        /// <summary>
        /// Adds a listener to this local dispatch.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler"></param>
        public void AddLocalListener<T>(MessageHandler<T> handler) where T : IMessage
        {
            Dispatcher.AddListener(handler);
        }

        /// <summary>
        /// Adds a listener to this local dispatch.
        /// </summary>
        public void AddLocalListener(Type msgType, MessageHandler handler)
        {
            Dispatcher.AddListener(msgType, handler);
        }

        /// <summary>
        /// Removes a listener from this local dispatch.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler"></param>
        public void RemoveLocalListener<T>(MessageHandler<T> handler) where T : IMessage
        {
            Dispatcher.RemoveListener(handler);
        }

        /// <summary>
        /// Removes a listener from this local dispatch.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler"></param>
        public void RemoveLocalListener(Type msgType, MessageHandler handler)
        {
            Dispatcher.RemoveListener(msgType, handler);
        }

        /// <summary>
        /// Removes all listeners from this local dispatch.
        /// </summary>
        public void RemoveAllLocalListeners()
        {
            Dispatcher.RemoveAllListeners();
        }

        /// <summary>
        /// Posts a message to this local dispatcher.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="msg"></param>
        public void PostMessage<T>(T msg) where T : IMessage
        {
            Dispatcher.PostMessage(msg);
        }
    }
}
