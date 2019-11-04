using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.Cmd.FillElasticSearch
{
    class ElasticSearchContext : IDisposable
    {
        private ElasticClient _client;
        private string _defalutIndex = String.Empty;

        public ElasticSearchContext(string url = "http://localhost:9200", string defaultIndex = "")
        {
            var node = new Uri(url);

            var settings = new ConnectionSettings(node);
            settings.DefaultIndex(_defalutIndex = defaultIndex);

            _client = new ElasticClient(settings);
        }

        #region IDisposable

        public void Dispose()
        {
        }

        #endregion

        #region Create/Delete Index

        public void CreateIndex(string indexName = "")
        {
            indexName = CurrentIndexName(indexName);
            if (String.IsNullOrWhiteSpace(indexName))
                throw new ArgumentException("No Index name");

            var createIndexResult = _client.CreateIndex(indexName);
            //var mapResult = _client.Map<T>(c => c
            //    .AutoMap());
        }

        public void Map<T>(string indexName="")
            where T : class
        {
            indexName = CurrentIndexName(indexName);
            if (String.IsNullOrWhiteSpace(indexName))
                throw new ArgumentException("No Index name");

            var mapResult = _client.Map<T>(c => c
                .AutoMap()
                .Index(indexName));
        }

        public bool DeleteIndex(string indexName="")
        {
            indexName = CurrentIndexName(indexName);
            if (String.IsNullOrWhiteSpace(indexName))
                throw new ArgumentException("No Index name");

            var result = _client.DeleteIndex(indexName);
            return result.IsValid;
        }

        #endregion

        #region Index

        public bool Index<T>(T document, string indexName = "")
            where T : class
        {
            indexName = CurrentIndexName(indexName);
            if (String.IsNullOrWhiteSpace(indexName))
                throw new ArgumentException("No Index name");

            var response = _client.Index<T>(document, idx => idx.Index(indexName));
            return response.Created;
        }

        public bool IndexMany<T>(T[] documents, string indexName = "")
            where T : class
        {
            indexName = CurrentIndexName(indexName);
            if (String.IsNullOrWhiteSpace(indexName))
                throw new ArgumentException("No Index name");

            string[] type = typeof(T).ToString().Split('.');

            var response = _client.IndexMany<T>(documents, indexName, type[type.Length - 1].ToLower());

            return response.Errors == false;
        }

        public bool Remove<T>(string id, string indexName = "")
            where T : class, new()
        {
            indexName = CurrentIndexName(indexName);
            if (String.IsNullOrWhiteSpace(indexName))
                throw new ArgumentException("No Index name");

            var response = _client.Delete<T>(id, idx => idx.Index(indexName));
            
            return response.Found;
        }

        public bool RemoveMany<T>(IEnumerable<T> objects, string indexName="")
            where T : class, new()
        {
            indexName = CurrentIndexName(indexName);
            if (String.IsNullOrWhiteSpace(indexName))
                throw new ArgumentException("No Index name");

            var response = _client.DeleteMany<T>(objects, indexName);

            return response.Errors == false;
        }

        #endregion

        #region Queries

        public IEnumerable<T> QueryAll<T>(string indexName = "")
            where T : class
        {
            indexName = CurrentIndexName(indexName);

            List<T> ret = new List<T>();
            int pos = 0, size = 100;
            while (true)
            {
                var result = _client.Search<T>(s => s.From(pos).Size(size));

                var count = result.Documents != null ? result.Documents.Count() : 0;

                pos += result.Documents.Count();
                if (count > 0)
                    ret.AddRange(result.Documents);
                if (count < size)
                    break;
            }
            return ret;
        }

        public IEnumerable<T> Query<T>(SearchFilter filter, int max = int.MaxValue, string indexName = "")
            where T : class
        {
            return Query<T>(new SearchFilter[] { filter }, max, indexName);
        }

        public IEnumerable<T> Query<T>(SearchFilter[] filters, int max = int.MaxValue, string indexName = "")
            where T : class
        {
            indexName = CurrentIndexName(indexName);

            List<T> ret = new List<T>();
            int pos = 0, size = 10000;
            while (true)
            {
                var request = new SearchRequest<T>
                {
                    From = pos,
                    Size = Math.Min(max, size)
                };
                AppendFilters<T>(request, filters);
                var result = _client.Search<T>(request);

                var count = result.Documents != null ? result.Documents.Count() : 0;

                pos += result.Documents.Count();
                if (count > 0)
                    ret.AddRange(result.Documents);
                if (count < size || ret.Count() >= max)
                    break;
            }
            return ret;
        }

        #endregion

        #region Aggregate 

        public Aggragtion[] Aggregate<T>(string field, SearchFilter[] filters = null, string function = "count", string indexName = "")
            where T : class
        {
            switch (function.ToLower())
            {
                case "count":
                    return CountAggregation<T>(field, filters, indexName);
                case "sum":
                    return SumAggregation<T>(field, filters, indexName);
                case "avg":
                    return AverageAggregation<T>(field, filters, indexName);
                case "min":
                    return MinAggregation<T>(field, filters, indexName);
                case "max":
                    return MaxAggregation<T>(field, filters, indexName);
            }

            throw new ArgumentException("Unsupported function: " + function);
        }

        private Aggragtion[] CountAggregation<T>(string field, SearchFilter[] filters = null, string indexName = "")
            where T : class
        {
            indexName = CurrentIndexName(indexName);

            var request = new SearchRequest<T>
            {
                Size = 0,
                Aggregations = new TermsAggregation("agg_" + field) { Field = field, Size = 1000 }
            };
            AppendFilters<T>(request, filters);
            var result = _client.Search<T>(request);
            var aggHelper = result.Aggs.Terms("agg_" + field);

            List<Aggragtion> ret = new List<Aggragtion>();
            foreach (var bucket in aggHelper.Buckets)
            {
                var aggregation = new Aggragtion()
                {
                    Field = field,
                    Result = bucket.DocCount != null ? (long)bucket.DocCount : 0,
                    Element = new Dictionary<string, object>()
                };
                aggregation.Element.Add(field, bucket.Key);
                ret.Add(aggregation);
            }
            return ret.ToArray();
        }

        private Aggragtion[] SumAggregation<T>(string field, SearchFilter[] filters = null, string indexName = "")
            where T : class
        {
            indexName = CurrentIndexName(indexName);

            var request = new SearchRequest<T>
            {
                Size = 0,
                Aggregations = new SumAggregation("agg_" + field, field)
            };
            AppendFilters<T>(request, filters);
            var result = _client.Search<T>(request);
            var aggHelper = result.Aggs.Sum("agg_" + field);

            List<Aggragtion> ret = new List<Aggragtion>();

            var aggregation = new Aggragtion()
            {
                Field = field,
                Result = aggHelper.Value != null ? (double)aggHelper.Value : 0,
                Element = new Dictionary<string, object>()
            };
            aggregation.Element.Add(field, aggHelper.Value);
            ret.Add(aggregation);

            return ret.ToArray();
        }

        private Aggragtion[] AverageAggregation<T>(string field, SearchFilter[] filters = null, string indexName = "")
            where T : class
        {
            indexName = CurrentIndexName(indexName);

            var request = new SearchRequest<T>
            {
                Size = 0,
                Aggregations = new AverageAggregation("agg_" + field, field)
            };
            AppendFilters<T>(request, filters);
            var result = _client.Search<T>(request);
            var aggHelper = result.Aggs.Average("agg_" + field);

            List<Aggragtion> ret = new List<Aggragtion>();

            var aggregation = new Aggragtion()
            {
                Field = field,
                Result = aggHelper.Value != null ? (double)aggHelper.Value : 0,
                Element = new Dictionary<string, object>()
            };
            aggregation.Element.Add(field, aggHelper.Value);
            ret.Add(aggregation);

            return ret.ToArray();
        }

        private Aggragtion[] MinAggregation<T>(string field, SearchFilter[] filters = null, string indexName = "")
            where T : class
        {
            indexName = CurrentIndexName(indexName);

            var request = new SearchRequest<T>
            {
                Size = 0,
                Aggregations = new MinAggregation("agg_" + field, field)
            };
            AppendFilters<T>(request, filters);
            var result = _client.Search<T>(request);
            var aggHelper = result.Aggs.Average("agg_" + field);

            List<Aggragtion> ret = new List<Aggragtion>();

            var aggregation = new Aggragtion()
            {
                Field = field,
                Result = aggHelper.Value != null ? (double)aggHelper.Value : 0,
                Element = new Dictionary<string, object>()
            };
            aggregation.Element.Add(field, aggHelper.Value);
            ret.Add(aggregation);

            return ret.ToArray();
        }

        private Aggragtion[] MaxAggregation<T>(string field, SearchFilter[] filters = null, string indexName = "")
           where T : class
        {
            indexName = CurrentIndexName(indexName);

            var request = new SearchRequest<T>
            {
                Size = 0,
                Aggregations = new MaxAggregation("agg_" + field, field)
            };
            AppendFilters<T>(request, filters);
            var result = _client.Search<T>(request);
            var aggHelper = result.Aggs.Average("agg_" + field);

            List<Aggragtion> ret = new List<Aggragtion>();

            var aggregation = new Aggragtion()
            {
                Field = field,
                Result = aggHelper.Value != null ? (double)aggHelper.Value : 0,
                Element = new Dictionary<string, object>()
            };
            aggregation.Element.Add(field, aggHelper.Value);
            ret.Add(aggregation);

            return ret.ToArray();
        }

        #endregion

        #region Helper

        private void AppendFilters<T>(SearchRequest<T> request, SearchFilter[] filters)
        {
            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    if (filter.Value == null)
                        continue;

                    //var qsq = new TermQuery { Field = new Field(filter.Field), Value = filter.Value };
                    //MatchQuery qsq=new MatchQuery { Field = filter.Field,  Query = new  };
                    QueryStringQuery qsq = new QueryStringQuery { DefaultField = filter.Field, Query = filter.Value.ToString() };  // Fuzzy
                    if (request.Query == null)
                        request.Query = qsq;
                    else
                        request.Query &= qsq;
                }
            }
        }

        private string CurrentIndexName(string indexName)
        {
            if (String.IsNullOrWhiteSpace(indexName))
                indexName = _defalutIndex;

            return indexName;
        }

        #endregion

        #region Classes

        public class Aggragtion
        {
            public string Field { get; set; }
            public double Result { get; set; }
            public Dictionary<string, object> Element { get; set; }
        }

        public class SearchFilter
        {
            public string Field { get; set; }
            public object Value { get; set; }
            public string Operator { get; set; }
        }

        #endregion
    }
}
