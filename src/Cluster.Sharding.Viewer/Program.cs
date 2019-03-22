using System;
using System.IO;
using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.Configuration;
using Akka.Persistence.Query;
using Akka.Persistence.Query.Sql;
using Akka.Streams;

namespace Cluster.Sharding.Viewer
{
    class Program
    {
        static void Main(string[] args)
        {
            var hocon = ConfigurationFactory.ParseString(File.ReadAllText("reference.conf"));
            var actorSystem = ActorSystem.Create("ClusterShardingViewer",
                hocon.WithFallback(SqlReadJournal.DefaultConfiguration()));
            actorSystem.Log.Info("Starting up...");

            var readJournal = actorSystem.ReadJournalFor<SqlReadJournal>(SqlReadJournal.Identifier);
            var query =  readJournal.CurrentEventsByPersistenceId("/system/sharding/fubersCoordinator/singleton/coordinator", 0,
                330);

            query.RunForeach(e =>
            {
                Console.WriteLine("{0}: {1}", e.SequenceNr, e.Event);
            }, actorSystem.Materializer());

            Console.ReadLine();
        }
    }
}
