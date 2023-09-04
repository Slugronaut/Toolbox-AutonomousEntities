#define LOCALMESSAGEDISPATCH_SUPPRESSTRANSFORMEVENTS
/**
 * Copyright 2014
 * James Clark
 */
using UnityEngine;

namespace Peg
{
    /// <summary>
    /// Provides a standardized access point for message dispatching that can
    /// guarantee order of operations. This version is meant to be attached
    /// to individual entities.
    /// </summary>
    /// <remarks>
    /// There is now a local PostMessage function on this script but in many cases it is not recommended.
    /// Instead, messages should be forwarded through the <see cref="GlobalMessagePump.Instance.Dispatcher"/>
    /// which will internally track which GameObject is the recipient of any given message.
    /// </remarks>
    [AddComponentMenu("Toolbox/Core/Local Message Dispatch")]
    [RequireComponent(typeof(EntityRoot))]
    [DisallowMultipleComponent]
    public sealed class LocalMessageDispatch : LocalMessageDispatchBase
    {

#if !LOCALMESSAGEDISPATCH_SUPPRESSTRANSFORMEVENTS
        /// <summary>
        /// Usually triggered by Unity but can also be called manually. It internally refreshes the
        /// list of all child GameObjects that they can be valid forwarding targets for this hierarchy's
        /// message dispatch system.
        /// </summary>
        public void OnTransformChildrenChanged()
        {
            //we need to check for null on this very component since this may get called while shutting down
            //which causes errors due to gameobjects being destroyed
            if (TypeHelper.IsReferenceNull(this)) return;

            //Ok, so I'm lazy and it's late. Just gonna do this the easy way and simply remove all and then re-add all

            //POSSIBLE BUG?
            //TODO: The line below was commented out but I can't recall why. Does it need testing?
            UnregisterWithGlobalPump(gameObject, Dispatcher, this, FullHierarchy); //<--- THIS WAS COMMENTED OUT. NOT SURE WHY!
            RegisterWithGlobalPump(gameObject, Dispatcher, this, FullHierarchy);
        }
#endif

        /// <summary>
        /// Helper method for registering this dispatch with the global dispatcher.
        /// </summary>
        /// <param name="whom"></param>
        /// <param name="dispatch"></param>
        /// <param name="hierarchy"></param>
        protected override void RegisterWithGlobalPump(GameObject whom, IMessageDispatcher dispatch, LocalMessageDispatchBase lmd, bool hierarchy)
        {
            if (Peg.TypeHelper.IsReferenceNull(whom)) return;
            //NOTE: if you get NULL REFERENCE EXCEPTION somewhere in here it's probably because you removed
            //a GameObject in the hierarchy at runtime. See above for details why.
            var skip = whom.GetComponent<SkipLocalDispatch>();
            if (skip == null)
            {
                //if for some dumb reason, this entity has one already, just re-use it
                var target = whom.GetComponent<LocalDispatchTarget>();
                if (target == null) target = whom.AddComponent<LocalDispatchTarget>() as LocalDispatchTarget;
                target.Dispatch = lmd as LocalMessageDispatch;
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
    }
}
