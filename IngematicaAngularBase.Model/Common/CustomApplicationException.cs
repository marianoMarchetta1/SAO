using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngematicaAngularBase.Model.Common
{
    public class CustomApplicationException : System.Exception
    {
        public CustomApplicationException(string message)
            : base(message)
        {

        }

        public CustomApplicationException(string message,
            Exception innerException)
            : base(message, innerException)
        {
        }

        public Dictionary<string, object> Errors;

        public CustomApplicationException(string message, Dictionary<string, object> errors)
            : base(message)
        {
            Errors = errors;
        }

        public CustomApplicationException(string message, ApplicationError applicationError)
            : base(message)
        {
            Errors = applicationError.Errors;
        }

    }
}
