using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IngematicaAngularBase.Model.Common
{
    public class QueryObject
    {
        public QueryObject()
        {
            Order = new List<Order>();
            Skip = 0;
            Take = 20;
        }

        public int Skip { get; set; }
        public int Take { get; set; }
        public List<Order> Order { get; set; }

    }

    public class Order
    {
        public string Property { get; set; }
        public bool Descending { get; set; }
    }

    public class QueryResult<T> : IQueryResult
    {
        public IEnumerable<T> Data { get; set; }
        public int TotalCount { get; set; }
    }

    public interface IQueryResult
    {
        int TotalCount { get; set; }
    }


    public class ApplicationError
    {
        public ApplicationError()
        {
            Errors = new Dictionary<string, object>();
        }

        public Dictionary<string, object> Errors;
        public void AddError(string field, string error)
        {
            if (!Errors.Keys.Contains(field))
                Errors.Add(field, new List<String>());

            (Errors[field] as List<String>).Add(error);
        }


    }
}
