using System;
using UnityEngine.Events;

namespace Peg
{
    [Serializable]
    public class TargetEntityEvent : UnityEvent<EntityRoot> { }

    [Serializable]
    public class AgentTargetEntityEvent : UnityEvent<EntityRoot, EntityRoot> { }
}
