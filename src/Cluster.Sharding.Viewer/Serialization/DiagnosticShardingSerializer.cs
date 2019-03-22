using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Akka.Actor;
using Akka.Cluster.Sharding.Serialization.Proto.Msg;
using Akka.Serialization;
using Google.Protobuf;

namespace Cluster.Sharding.Viewer.Serialization
{
    public class ShardRegionBaseMsg
    {
        public ShardRegionBaseMsg(string shardOperation, IActorRef region)
        {
            Region = region;
            ShardOperation = shardOperation;
        }

        public string ShardOperation { get; }

        public IActorRef Region { get; }

        public override string ToString()
        {
            return $"{ShardOperation}({Region})";
        }
    }

    public class DiagnosticClusterShardingMessageSerializer : SerializerWithStringManifest
    {
        #region manifests

        private const string CoordinatorStateManifest = "AA";
        private const string ShardRegionRegisteredManifest = "AB";
        private const string ShardRegionProxyRegisteredManifest = "AC";
        private const string ShardRegionTerminatedManifest = "AD";
        private const string ShardRegionProxyTerminatedManifest = "AE";
        private const string ShardHomeAllocatedManifest = "AF";
        private const string ShardHomeDeallocatedManifest = "AG";

        private const string RegisterManifest = "BA";
        private const string RegisterProxyManifest = "BB";
        private const string RegisterAckManifest = "BC";
        private const string GetShardHomeManifest = "BD";
        private const string ShardHomeManifest = "BE";
        private const string HostShardManifest = "BF";
        private const string ShardStartedManifest = "BG";
        private const string BeginHandOffManifest = "BH";
        private const string BeginHandOffAckManifest = "BI";
        private const string HandOffManifest = "BJ";
        private const string ShardStoppedManifest = "BK";
        private const string GracefulShutdownReqManifest = "BL";

        private const string EntityStateManifest = "CA";
        private const string EntityStartedManifest = "CB";
        private const string EntityStoppedManifest = "CD";

        private const string StartEntityManifest = "EA";
        private const string StartEntityAckManifest = "EB";

        private const string GetShardStatsManifest = "DA";
        private const string ShardStatsManifest = "DB";

        #endregion

        private readonly Dictionary<string, Func<byte[], object>> _fromBinaryMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticClusterShardingMessageSerializer"/> class.
        /// </summary>
        /// <param name="system">The actor system to associate with this serializer.</param>
        public DiagnosticClusterShardingMessageSerializer(ExtendedActorSystem system) : base(system)
        {
            _fromBinaryMap = new Dictionary<string, Func<byte[], object>>
            {
                {EntityStateManifest, EntityStateFromBinary},
                {EntityStartedManifest, EntityStartedFromBinary},
                {EntityStoppedManifest, EntityStoppedFromBinary},

                {CoordinatorStateManifest, bytes => CoordinatorStateFromBinary(bytes)},
                {ShardRegionRegisteredManifest, bytes => new ShardRegionBaseMsg("ShardRegionRegistered",ActorRefMessageFromBinary(bytes)) },
                {ShardRegionProxyRegisteredManifest, bytes => new ShardRegionBaseMsg("ShardRegionProxyRegistered",ActorRefMessageFromBinary(bytes)) },
                {ShardRegionTerminatedManifest, bytes => new ShardRegionBaseMsg("ShardRegionTerminated",ActorRefMessageFromBinary(bytes)) },
                {ShardRegionProxyTerminatedManifest, bytes => new ShardRegionBaseMsg("ShardRegionProxyTerminated",ActorRefMessageFromBinary(bytes)) },
                {ShardHomeAllocatedManifest, ShardHomeAllocatedFromBinary},
                {ShardHomeDeallocatedManifest, bytes => ShardIdMessageFromBinary(bytes) },

                {RegisterManifest, bytes => new ShardRegionBaseMsg("RegionRegistered",ActorRefMessageFromBinary(bytes)) },
                {RegisterProxyManifest, bytes => new ShardRegionBaseMsg("ProxyRegistered",ActorRefMessageFromBinary(bytes)) },
                {RegisterAckManifest, bytes => new ShardRegionBaseMsg("RegisterAck",ActorRefMessageFromBinary(bytes))},
                {GetShardHomeManifest, bytes => ShardIdMessageFromBinary(bytes) },
                {ShardHomeManifest, ShardHomeFromBinary},
                //{HostShardManifest, bytes => new PersistentShardCoordinator.HostShard(ShardIdMessageFromBinary(bytes)) },
                //{ShardStartedManifest, bytes => new PersistentShardCoordinator.ShardStarted(ShardIdMessageFromBinary(bytes)) },
                //{BeginHandOffManifest, bytes => new PersistentShardCoordinator.BeginHandOff(ShardIdMessageFromBinary(bytes)) },
                //{BeginHandOffAckManifest, bytes => new PersistentShardCoordinator.BeginHandOffAck(ShardIdMessageFromBinary(bytes)) },
                //{HandOffManifest, bytes => new PersistentShardCoordinator.HandOff(ShardIdMessageFromBinary(bytes)) },
                //{ShardStoppedManifest, bytes => new PersistentShardCoordinator.ShardStopped(ShardIdMessageFromBinary(bytes)) },
                //{GracefulShutdownReqManifest, bytes => new PersistentShardCoordinator.GracefulShutdownRequest(ActorRefMessageFromBinary(bytes)) },

               // {GetShardStatsManifest, bytes => Shard.GetShardStats.Instance },
                {ShardStatsManifest, ShardStatsFromBinary},

                {StartEntityManifest, StartEntityFromBinary },
                {StartEntityAckManifest, StartEntityAckFromBinary}
            };
        }

        /// <summary>
        /// Serializes the given object into a byte array
        /// </summary>
        /// <param name="obj">The object to serialize</param>
        /// <exception cref="ArgumentException">
        /// This exception is thrown when the specified <paramref name="obj"/> is of an unknown type.
        /// </exception>
        /// <returns>A byte array containing the serialized object</returns>
        public override byte[] ToBinary(object obj)
        {
            throw new ArgumentException($"Can't serialize object of type [{obj.GetType()}] in [{this.GetType()}]");
        }

        /// <summary>
        /// Deserializes a byte array into an object using an optional <paramref name="manifest" /> (type hint).
        /// </summary>
        /// <param name="bytes">The array containing the serialized object</param>
        /// <param name="manifest">The type hint used to deserialize the object contained in the array.</param>
        /// <exception cref="ArgumentException">
        /// This exception is thrown when the specified <paramref name="bytes"/>cannot be deserialized using the specified <paramref name="manifest"/>.
        /// </exception>
        /// <returns>The object contained in the array</returns>
        public override object FromBinary(byte[] bytes, string manifest)
        {
            if (_fromBinaryMap.TryGetValue(manifest, out var factory))
                return factory(bytes);

            throw new ArgumentException($"Unimplemented deserialization of message with manifest [{manifest}] in [{this.GetType()}]");
        }

        /// <summary>
        /// Returns the manifest (type hint) that will be provided in the <see cref="FromBinary(System.Byte[],System.String)" /> method.
        /// <note>
        /// This method returns <see cref="String.Empty" /> if a manifest is not needed.
        /// </note>
        /// </summary>
        /// <param name="o">The object for which the manifest is needed.</param>
        /// <exception cref="ArgumentException">
        /// This exception is thrown when the specified <paramref name="o"/> does not have an associated manifest.
        /// </exception>
        /// <returns>The manifest needed for the deserialization of the specified <paramref name="o" />.</returns>
        public override string Manifest(object o)
        {
            throw new ArgumentException($"Can't serialize object of type [{o.GetType()}] in [{this.GetType()}]");
        }

        private static object ShardStatsFromBinary(byte[] bytes)
        {
            var message = Akka.Cluster.Sharding.Serialization.Proto.Msg.ShardStats.Parser.ParseFrom(bytes);
            return message;
        }

        private static object StartEntityFromBinary(byte[] bytes)
        {
            var message = Akka.Cluster.Sharding.Serialization.Proto.Msg.StartEntity.Parser.ParseFrom(bytes);
            return message;
        }


        private static object StartEntityAckFromBinary(byte[] bytes)
        {
            var message = Akka.Cluster.Sharding.Serialization.Proto.Msg.StartEntityAck.Parser.ParseFrom(bytes);
            return message;
        }

        private static object EntityStartedFromBinary(byte[] bytes)
        {
            var message = Akka.Cluster.Sharding.Serialization.Proto.Msg.EntityStarted.Parser.ParseFrom(bytes);
            return message;
        }

        private static object EntityStoppedFromBinary(byte[] bytes)
        {
            var message = Akka.Cluster.Sharding.Serialization.Proto.Msg.EntityStopped.Parser.ParseFrom(bytes);
            return message;
        }

        private (ImmutableDictionary<string, IActorRef> shards, 
            ImmutableDictionary<IActorRef, IImmutableList<string>> regions, 
            ImmutableHashSet<IActorRef> proxies,
            ImmutableHashSet<string> unallocatedShards) CoordinatorStateFromBinary(byte[] bytes)
        {
            var state = Akka.Cluster.Sharding.Serialization.Proto.Msg.CoordinatorState.Parser.ParseFrom(bytes);
            var shards = ImmutableDictionary.CreateRange(state.Shards.Select(entry => new KeyValuePair<string, IActorRef>(entry.ShardId, ResolveActorRef(entry.RegionRef))));
            var regionsZero = ImmutableDictionary.CreateRange(state.Regions.Select(region => new KeyValuePair<IActorRef, IImmutableList<string>>(ResolveActorRef(region), ImmutableList<string>.Empty)));
            var regions = shards.Aggregate(regionsZero, (acc, entry) => acc.SetItem(entry.Value, acc[entry.Value].Add(entry.Key)));
            var proxies = state.RegionProxies.Select(ResolveActorRef).ToImmutableHashSet();
            var unallocatedShards = state.UnallocatedShards.ToImmutableHashSet();

            return (
                shards: shards,
                regions: regions,
                proxies,
                unallocatedShards: unallocatedShards);
        }

        private object ShardHomeAllocatedFromBinary(byte[] bytes)
        {
            var msg = Akka.Cluster.Sharding.Serialization.Proto.Msg.ShardHomeAllocated.Parser.ParseFrom(bytes);
            return msg;
        }

        private object ShardHomeFromBinary(byte[] bytes)
        {
            var msg = Akka.Cluster.Sharding.Serialization.Proto.Msg.ShardHome.Parser.ParseFrom(bytes);
            return msg;
        }

        private IActorRef ActorRefMessageFromBinary(byte[] binary)
        {
            return new PrintableActorRef(ActorRefMessage.Parser.ParseFrom(binary).Ref);
        }
        
        private object EntityStateFromBinary(byte[] bytes)
        {
            var msg = Akka.Cluster.Sharding.Serialization.Proto.Msg.EntityState.Parser.ParseFrom(bytes);
            return msg;
        }

        private string ShardIdMessageFromBinary(byte[] bytes)
        {
            return Akka.Cluster.Sharding.Serialization.Proto.Msg.ShardIdMessage.Parser.ParseFrom(bytes).Shard;
        }

        private IActorRef ResolveActorRef(string path)
        {
            return system.Provider.ResolveActorRef(path);
        }
    }
}
