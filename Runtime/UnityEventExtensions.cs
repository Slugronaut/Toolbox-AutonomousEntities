using System;
using UnityEngine.Events;

namespace Toolbox
{
    [Serializable]
    public class TargetEntityEvent : UnityEvent<EntityRoot> { }

    [Serializable]
    public class AgentTargetEntityEvent : UnityEvent<EntityRoot, EntityRoot> { }
}
