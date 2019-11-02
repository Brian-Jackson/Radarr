using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(164)]
    public class movie_collections_crew : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Movies").AddColumn("Collection").AsString().Nullable();
            Alter.Table("Movies").AddColumn("Crew").AsString().Nullable();
            Alter.Table("Movies").AddColumn("Cast").AsString().Nullable();

            Delete.Column("Actors").FromTable("Movies");
        }
    }
}
