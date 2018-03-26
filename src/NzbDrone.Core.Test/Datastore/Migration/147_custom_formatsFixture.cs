﻿using System.Linq;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class custom_formatsFixture : MigrationTest<add_custom_formats>
    {
        public void WithDefaultQualityDefinitions(add_custom_formats m)
        {
            foreach (var definition in QualityDefinition.DefaultQualityDefinitions)
            {
                m.Insert.IntoTable("QualityDefinitions").Row(new
                {
                    Quality = (int) definition.Quality,
                    Title = definition.Title,
                    MinSize = definition.MinSize,
                    MaxSize = definition.MaxSize,
                });
            }
        }

        public void AddDefaultProfile(add_custom_formats m, string name, Quality cutoff, params Quality[] allowed)
        {
            var items = QualityDefinition.DefaultQualityDefinitions
                .OrderBy(v => v.Weight)
                .Select(v => new { Quality = (int)v.Quality, Allowed = allowed.Contains(v.Quality) })
                .ToList();

            var profile = new { Name = name, Cutoff = (int)cutoff, Items = items.ToJson(), Language = (int)Language.English };

            m.Insert.IntoTable("Profiles").Row(profile);
        }

        public void WithDefaultProfiles(add_custom_formats m)
        {
            AddDefaultProfile(m, "Any", Quality.Bluray480p,
                Quality.WORKPRINT,
                Quality.CAM,
                Quality.TELESYNC,
                Quality.TELECINE,
                Quality.DVDSCR,
                Quality.REGIONAL,
                Quality.SDTV,
                Quality.DVD,
                Quality.DVDR,
                Quality.HDTV720p,
                Quality.HDTV1080p,
                Quality.HDTV2160p,
                Quality.WEBDL480p,
                Quality.WEBDL720p,
                Quality.WEBDL1080p,
                Quality.WEBDL2160p,
                Quality.Bluray480p,
                Quality.Bluray576p,
                Quality.Bluray720p,
                Quality.Bluray1080p,
                Quality.Bluray2160p,
                Quality.Remux1080p,
                Quality.Remux2160p,
                Quality.BRDISK);

            AddDefaultProfile(m, "SD", Quality.Bluray480p,
                Quality.WORKPRINT,
                Quality.CAM,
                Quality.TELESYNC,
                Quality.TELECINE,
                Quality.DVDSCR,
                Quality.REGIONAL,
                Quality.SDTV,
                Quality.DVD,
                Quality.WEBDL480p,
                Quality.Bluray480p,
                Quality.Bluray576p);

            AddDefaultProfile(m, "HD-720p", Quality.Bluray720p,
                Quality.HDTV720p,
                Quality.WEBDL720p,
                Quality.Bluray720p);

            AddDefaultProfile(m, "HD-1080p", Quality.Bluray1080p,
                Quality.HDTV1080p,
                Quality.WEBDL1080p,
                Quality.Bluray1080p,
                Quality.Remux1080p);

            AddDefaultProfile(m, "Ultra-HD", Quality.Remux2160p,
                Quality.HDTV2160p,
                Quality.WEBDL2160p,
                Quality.Bluray2160p,
                Quality.Remux2160p);

            AddDefaultProfile(m, "HD - 720p/1080p", Quality.Bluray720p,
                Quality.HDTV720p,
                Quality.HDTV1080p,
                Quality.WEBDL720p,
                Quality.WEBDL1080p,
                Quality.Bluray720p,
                Quality.Bluray1080p,
                Quality.Remux1080p,
                Quality.Remux2160p
                );
        }

        public List<QualityDefinition147> GetDefinitions(IDirectDataMapper db)
        {
            return db.Query<QualityDefinition147>("SELECT * FROM QualityDefinitions");
        }

        public QualityDefinition147 GetDefinitionForQuality(IDirectDataMapper db, Quality quality)
        {
            return GetDefinitions(db).First(d => d.Quality == (int)quality);
        }

        [Test]
        public void should_correctly_update_items_of_default_profiles()
        {
            var db = WithMigrationTestDb(c =>
            {
                WithDefaultQualityDefinitions(c);
                WithDefaultProfiles(c);
            });

            var items = db.Query<Profile70>("SELECT * FROM Profiles");

            items.Should().HaveCount(6);
            items.Select(i => i.Cutoff).Should().BeEquivalentTo(12, 12, 16, 19, 24, 16);
            foreach (var profile in items)
            {
                //Since definitions get added in the order of DefaultQualityDefinitions hashset, we can just use count here for the id.
                var count = 1;
                foreach (var item in profile.Items)
                {
                    item.QualityDefinition.Should().Be(count);
                    item.Quality.Should().Be((int)QualityDefinition.DefaultQualityDefinitions.ToList()[count-1].Quality);
                    count++;
                }
            }

            var any = items.First();
            var allowed = Enumerable.Repeat(true, 24).ToList();
            allowed.Insert(0, false);
            allowed.Insert(25, false);
            any.Items.Select(i => i.Allowed).Should().ContainInOrder(allowed);
        }

        [Test]
        public void should_correctly_migrate_custom_profile()
        {
            var db = WithMigrationTestDb(c =>
            {
                WithDefaultQualityDefinitions(c);
                AddDefaultProfile(c, "My Custom Profile", Quality.WEBDL720p, Quality.WEBDL720p, Quality.WEBDL1080p);
            });

            var items = db.Query<Profile70>("SELECT * FROM Profiles");
            items.Should().HaveCount(1);
            var profile = items.First();

            profile.Cutoff.Should().Be(GetDefinitionForQuality(db, Quality.WEBDL720p).Id);

            foreach (var item in profile.Items)
            {
                var definition = GetDefinitionForQuality(db, (Quality) item.Quality);
                item.QualityDefinition.Should().Be(definition.Id);
                item.Allowed.Should()
                    .Be(item.Quality == (int) Quality.WEBDL720p || item.Quality == (int) Quality.WEBDL1080p);
            }
        }
    }
}
