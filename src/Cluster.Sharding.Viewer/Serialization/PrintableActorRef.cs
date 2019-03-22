using System;
using System.Collections.Generic;
using System.Text;
using Akka.Actor;

namespace Cluster.Sharding.Viewer.Serialization
{
    public sealed class PrintableActorRef : MinimalActorRef
    {
        public PrintableActorRef(string path)
        {
            _ref = path;
        }

        private readonly string _ref;

        public override ActorPath Path { get; }
        public override IActorRefProvider Provider => null;

        public override string ToString()
        {
            return _ref;
        }
    }
}
