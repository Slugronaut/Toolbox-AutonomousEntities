#define LOCALMESSAGEDISPATCH_SUPPRESSTRANSFORMEVENTS
using UnityEngine;

namespace Peg
{
    /// <summary>
    /// This component is attached automatically by a <see cref="LocalMessageDispatch"/>
    /// as it registers child objects. It can be used to determine an child GameObject's 
    /// root Dispatcher as well as identify that GameObject as being an valid forward target.
    /// You should never add or remove this component yourself! This allows the LocalMessageDispatcher
    /// to handle such tasks in order to properly maintain message dispatch structure.
    /// </summary>
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    public class LocalDispatchTarget : MonoBehaviour, IDispatchTarget
    {
        [Tooltip("Displays the Local Message Dispatch that forwards to this GameObject. Read-only.")]
        public LocalMessageDispatch Dispatch;

        void Awake()
        {
            this.hideFlags = HideFlags.NotEditable;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Run in editor only: We don't want this to exist when in
        /// edit mode so it destroys itself during OnEnable if not in playmode
        /// </summary>
        void OnEnable()
        {
            if (!Application.isPlaying)
            {
                DestroyImmediate(this);
                return;
            }
        }
#endif


#if !LOCALMESSAGEDISPATCH_SUPPRESSTRANSFORMEVENTS
        void OnTransformChildrenChanged()
        {
            Dispatch.OnTransformChildrenChanged();
        }
#endif
    }
}
