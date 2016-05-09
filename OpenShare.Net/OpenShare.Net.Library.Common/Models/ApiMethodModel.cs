using System;
using System.Collections.Generic;
using System.Text;

namespace OpenShare.Net.Library.Common.Models
{
    /// <summary>
    /// This class describes the model of an API Method.
    /// </summary>
    public class ApiMethodModel
    {
        private const string MethodSignatureError = "Can not generate a method signature due to missing information.";

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public ApiMethodModel()
        {
            Parameters = new List<ApiMethodParameterModel>();
        }

        /// <summary>
        /// The name of the API Method.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The Parameters of the API Method.
        /// </summary>
        public List<ApiMethodParameterModel> Parameters { get; set; }

        /// <summary>
        /// The Return type of the API Method.
        /// </summary>
        public string ReturnType { get; set; }

        /// <summary>
        /// The API Method Signature.
        /// </summary>
        public string MethodSignature
        {
            get
            {
                if (string.IsNullOrEmpty(Name) || Parameters == null || string.IsNullOrEmpty(ReturnType))
                    throw new Exception(MethodSignatureError);

                var builder = new StringBuilder(255);
                builder.Append(ReturnType).
                    Append(" ").
                    Append(Name).
                    Append("(");

                if (Parameters.Count > 0)
                {
                    foreach (var parameter in Parameters)
                    {
                        if (string.IsNullOrEmpty(parameter.Type) || string.IsNullOrEmpty(parameter.Name))
                            throw new Exception(MethodSignatureError);

                        builder.Append(parameter.Type)
                            .Append(" ")
                            .Append(parameter.Name)
                            .Append(", ");
                    }

                    builder.Remove(builder.Length - 2, 2);
                }

                builder.Append(")");
                return builder.ToString();
            }
        }

        /// <summary>
        /// The API Method Signature to use for sorting.
        /// </summary>
        public string MethodSignatureSort
        {
            get
            {
                if (string.IsNullOrEmpty(Name) || Parameters == null || string.IsNullOrEmpty(ReturnType))
                    throw new Exception(MethodSignatureError);

                var builder = new StringBuilder(255);
                builder.Append(Name).
                    Append("(");

                if (Parameters.Count > 0)
                {
                    foreach (var parameter in Parameters)
                    {
                        if (string.IsNullOrEmpty(parameter.Type) || string.IsNullOrEmpty(parameter.Name))
                            throw new Exception(MethodSignatureError);

                        builder.Append(parameter.Name).
                            Append(",");
                    }

                    builder.Remove(builder.Length - 1, 1);
                }

                builder.Append(")(");

                if (Parameters.Count > 0)
                {
                    foreach (var parameter in Parameters)
                        builder.Append(parameter.Type).
                            Append(",");

                    builder.Remove(builder.Length - 1, 1);
                }

                builder.Append(")").
                    Append(ReturnType);

                return builder.ToString();
            }
        }
    }
}
