using System;

namespace Peg.AutonomousEntities
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
