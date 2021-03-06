using System;
using System.Collections.Generic;
using System.Linq;
using Marr.Data.QGen;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.History
{
    public interface IHistoryRepository : IBasicRepository<History>
    {
        List<QualityModel> GetBestQualityInHistory(int movieId);
        History MostRecentForDownloadId(string downloadId);
        List<History> FindByDownloadId(string downloadId);
        List<History> FindDownloadHistory(int idMovieId, QualityModel quality);
        List<History> FindByMovieId(int movieId);
        void DeleteForMovie(int movieId);
        History MostRecentForMovie(int movieId);
    }

    public class HistoryRepository : BasicRepository<History>, IHistoryRepository
    {

        public HistoryRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<QualityModel> GetBestQualityInHistory(int movieId)
        {
            var history = Query(q => q.Where(c => c.MovieId == movieId).ToList());

            return history.Select(h => h.Quality).ToList();
        }

        public History MostRecentForDownloadId(string downloadId)
        {
            return Query(q => q.Where(h => h.DownloadId == downloadId)
             .OrderByDescending(h => h.Date)
             .FirstOrDefault());
        }

        public List<History> FindByDownloadId(string downloadId)
        {
            return Query(q => q.Where(h => h.DownloadId == downloadId).ToList());
        }

        public List<History> FindDownloadHistory(int idMovieId, QualityModel quality)
        {
            return Query(q => q.Where(h =>
                 h.MovieId == idMovieId &&
                 h.Quality == quality &&
                 (h.EventType == HistoryEventType.Grabbed ||
                 h.EventType == HistoryEventType.DownloadFailed ||
                 h.EventType == HistoryEventType.DownloadFolderImported)
                 ).ToList());
        }

        public List<History> FindByMovieId(int movieId)
        {
            return Query(q => q.Where(h => h.MovieId == movieId).ToList());
        }

        public void DeleteForMovie(int movieId)
        {
            Delete(c => c.MovieId == movieId);
        }

        protected override SortBuilder<History> GetPagedQuery(QueryBuilder<History> query, PagingSpec<History> pagingSpec)
        {
            var baseQuery = query.Join<History, Movie>(JoinType.Inner, h => h.Movie, (h, e) => h.MovieId == e.Id);

            return base.GetPagedQuery(baseQuery, pagingSpec);
        }

        public History MostRecentForMovie(int movieId)
        {
            return Query(q => q.Where(h => h.MovieId == movieId)
                        .OrderByDescending(h => h.Date)
                        .FirstOrDefault());
        }
    }
}
