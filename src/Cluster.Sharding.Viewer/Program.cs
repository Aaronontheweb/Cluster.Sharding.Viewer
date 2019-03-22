using System;
using System.IO;
using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.Configuration;

namespace Cluster.Sharding.Viewer
{
    class Program
    {
        static void Main(string[] args)
        {
            var hocon = ConfigurationFactory.ParseString(File.ReadAllText("reference.conf"));
            var actorSystem = ActorSystem.Create("ClusterShardingViewer",
                hocon.WithFallback(ClusterSharding.DefaultConfig()));
            actorSystem.Log.Info("Starting up...");



            Console.ReadLine();
        }
    }
}
