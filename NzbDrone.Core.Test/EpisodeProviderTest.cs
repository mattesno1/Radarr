﻿// ReSharper disable RedundantUsingDirective
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using AutoMoq;
using FizzWare.NBuilder;
using MbUnit.Framework;
using Moq;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;
using SubSonic.Repository;
using TvdbLib.Data;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class EpisodeProviderTest : TestBase
    {
        [Test]
        public void RefreshEpisodeInfo()
        {
            //Arrange
            const int seriesId = 71663;
            const int episodeCount = 10;

            var fakeEpisodes = Builder<TvdbSeries>.CreateNew().With(
                c => c.Episodes =
                     new List<TvdbEpisode>(Builder<TvdbEpisode>.CreateListOfSize(episodeCount).
                                               WhereAll()
                                               .Have(l => l.Language = new TvdbLanguage(0, "eng", "a"))
                                               .Build())
                ).With(c => c.Id = seriesId).Build();

            var mocker = new AutoMoqer();

            mocker.SetConstant(MockLib.GetEmptyRepository());

            mocker.GetMock<TvDbProvider>()
                .Setup(c => c.GetSeries(seriesId, true))
                .Returns(fakeEpisodes).Verifiable();

            //mocker.GetMock<IRepository>().SetReturnsDefault();


            //Act
            var sw = Stopwatch.StartNew();
            mocker.Resolve<EpisodeProvider>().RefreshEpisodeInfo(seriesId);
            var actualCount = mocker.Resolve<EpisodeProvider>().GetEpisodeBySeries(seriesId);
            //Assert
            mocker.GetMock<TvDbProvider>().VerifyAll();
            Assert.Count(episodeCount, actualCount);
            Console.WriteLine("Duration: " + sw.Elapsed);
        }
        

        [Test]
        [Explicit]
        public void Add_daily_show_episodes()
        {
            var mocker = new AutoMoqer();
            mocker.SetConstant(MockLib.GetEmptyRepository());
            mocker.Resolve<TvDbProvider>();
            const int tvDbSeriesId = 71256;
            //act
            var seriesProvider = mocker.Resolve<SeriesProvider>();

            seriesProvider.AddSeries("c:\\test\\", tvDbSeriesId, 0);
            var episodeProvider = mocker.Resolve<EpisodeProvider>();
            episodeProvider.RefreshEpisodeInfo(tvDbSeriesId);

            //assert
            var episodes = episodeProvider.GetEpisodeBySeries(tvDbSeriesId);
            Assert.IsNotEmpty(episodes);
        }



    }
}                                                                                                                                                                                                                                                                                                                                                                             