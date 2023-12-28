using System;
using UnityEngine.Events;

namespace Peg.AutonomousEntities
{
    [Serializable]
    public class TargetEntityEvent : UnityEvent<EntityRoot> { }

    [Serializable]
    public class AgentTargetEntityEvent : UnityEvent<EntityRoot, EntityRoot> { }
}
