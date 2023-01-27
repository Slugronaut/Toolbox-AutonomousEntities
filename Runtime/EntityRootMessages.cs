using System;

namespace Toolbox
{
    /// <summary>
    /// 
    /// </summary>
    public class EntityTargetEvent : TargetMessage<EntityRoot, EntityTargetEvent> { }


    /// <summary>
    /// 
    /// </summary>
    public class EntityAgentTargetEvent : AgentTargetMessage<EntityRoot, EntityRoot, EntityAgentTargetEvent> { }


    /// <summary>
    /// base demand message. This version does not accept a callback as a parameter.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SimpleDemand<T> : IDemand<T> where T : class
    {
        public T Desired { get; protected set; }
        bool Responded;

        public SimpleDemand() { }

        public void Respond(T desired)
        {
            //only one responder allowed
            if (Responded) return;

            Desired = desired;
            Responded = true;
        }

        /// <summary>
        /// Resets the internal response values to null.
        /// If you intend to cache a Demand, you must call this
        /// before each new call.
        /// </summary>
        public SimpleDemand<T> Reset()
        {
            Desired = null;
            Responded = false;
            return this;
        }
    }


    /// <summary>
    /// Base demand message.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Demand<T> : IDemand<T> where T : class
    {
        Action<T> Response;
        public T Desired { get; protected set; }
        protected bool Responded { get; private set; }

        private Demand() { }

        public Demand(Action<T> callback)
        {
            Response = callback;
        }

        public Demand<T> ChangeCallback(Action<T> callback)
        {
            Response = callback;
            return this;
        }

        public void Respond(T desired)
        {
            //only one responder allowed
            if (Responded) return;

            Desired = desired;
            Responded = true;
            Response?.Invoke(Desired);
        }

        /// <summary>
        /// Resets the internal response values to null.
        /// If you intend to cache a Demand, you must call this
        /// before each new call.
        /// </summary>
        public Demand<T> Reset()
        {
            Desired = null;
            Responded = false;
            return this;
        }

    }
}