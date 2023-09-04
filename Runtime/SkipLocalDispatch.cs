using UnityEngine;
using System.Collections;

namespace Peg
{
    /// <summary>
    /// Attach to a child object of an entity that should be excluded from
    /// local dispatches when using <see cref="LocalMessageDispatch.FullHierarchy"/>.
    /// </summary>
    [AddComponentMenu("Toolbox/Core/Skip Local Dispatch")]
    [DisallowMultipleComponent]
    public class SkipLocalDispatch : MonoBehaviour
    {
        /// <summary>
        /// If set, all of this component's children will also be excluded.
        /// </summary>
        [Tooltip("If set, all of this component's children will also be excluded.")]
        public bool SkipChildren = true;
    }
}