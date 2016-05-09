using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using OpenShare.Net.Library.Common.Models;

namespace OpenShare.Net.Library.Common
{
    /// <summary>
    /// Useful WCF Service extension methods.
    /// </summary>
    public static class WcfServiceExtensions
    {
        /// <summary>
        /// This extension method creates a safe WCF Service call and
        /// aborts connections appropriately under certain exceptions
        /// and handles proper disposal of any resources. The method
        /// to pass in is a Function delegate. The safe service call
        /// extension method handles the following workarounds:
        /// https://msdn.microsoft.com/en-us/library/aa355056.aspx
        /// ,
        /// http://stackoverflow.com/questions/573872/what-is-the-best-workaround-for-the-wcf-client-using-block-issue
        /// </summary>
        /// <typeparam name="TResult">The result type of the Service call.</typeparam>
        /// <typeparam name="TService">The type of Service.</typeparam>
        /// <param name="client">The WCF Service being extended.</param>
        /// <param name="method">The WCF Service call as a Function delegate.</param>
        /// <returns>Returns type of TResult, which cannot be void.</returns>
        public static TResult SafeServiceCall<TResult, TService>(
            this TService client, Func<TService, TResult> method)
            where TService : ICommunicationObject
        {
            TResult result;

            try
            {
                result = method(client);
            }
            finally
            {
                try
                {
                    client.Close();
                }
                catch (CommunicationException)
                {
                    client.Abort(); // Don't care about these exceptions. The call has completed anyway.
                }
                catch (TimeoutException)
                {
                    client.Abort(); // Don't care about these exceptions. The call has completed anyway.
                }
                catch (Exception)
                {
                    client.Abort();
                    throw;
                }
            }

            return result;
        }

        /// <summary>
        /// This extension method creates a safe WCF Service call and
        /// aborts connections appropriately under certain exceptions
        /// and handles proper disposal of any resources. The method
        /// to pass in is an Action delegate, which has a void return type.
        /// http://stackoverflow.com/questions/917551/func-delegate-with-no-return-type
        /// The safe service call extension method handles the following workarounds:
        /// https://msdn.microsoft.com/en-us/library/aa355056.aspx
        /// ,
        /// http://stackoverflow.com/questions/573872/what-is-the-best-workaround-for-the-wcf-client-using-block-issue
        /// </summary>
        /// <typeparam name="TService">The type of Service.</typeparam>
        /// <param name="client">The WCF Service being extended.</param>
        /// <param name="method">The WCF Service call as a Function delegate.</param>
        public static void SafeServiceCall<TService>(
            this TService client, Action<TService> method)
            where TService : ICommunicationObject
        {
            try
            {
                method(client);
            }
            finally
            {
                try
                {
                    client.Close();
                }
                catch (CommunicationException)
                {
                    client.Abort(); // Don't care about these exceptions. The call has completed anyway.
                }
                catch (TimeoutException)
                {
                    client.Abort(); // Don't care about these exceptions. The call has completed anyway.
                }
                catch (Exception)
                {
                    client.Abort();
                    throw;
                }
            }
        }

        /// <summary>
        /// The following extension methods lists out definitions
        /// of methods contained within the WCF Service Reference.
        /// </summary>
        /// <typeparam name="TService">The type of Service.</typeparam>
        /// <param name="client">The WCF Service being extended.</param>
        /// <returns>A list of ApiMethodModel definitions.</returns>
        public static List<ApiMethodModel> GetApiMethods<TService>(this TService client)
            where TService : ICommunicationObject
        {
            return typeof(TService).
                GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).
                Where(p => !p.Name.EndsWith("Async")).
                Select(p => new ApiMethodModel
                {
                    Name = p.Name,
                    ReturnType = p.ReturnType.Name,
                    Parameters = p.GetParameters().
                    Select(q => new ApiMethodParameterModel
                    {
                        Name = q.Name,
                        Type = q.ParameterType.Name
                    }).ToList()
                }).
                OrderBy(p => p.MethodSignatureSort).
                ToList();
        }

        /// <summary>
        /// The following extension methods lists out definitions
        /// all of the types contained within the WCF Service Reference.
        /// </summary>
        /// <typeparam name="TService">The type of Service.</typeparam>
        /// <param name="client">The WCF Service being extended.</param>
        /// <returns>A list of types specific to the WCF Service Reference.</returns>
        public static List<string> GetApiTypes<TService>(this TService client)
            where TService : ICommunicationObject
        {
            var ns = typeof(TService).Namespace;
            if (string.IsNullOrEmpty(ns))
                ns = string.Empty;

            return new HashSet<string>(
                typeof(TService).
                Assembly.GetTypes().
                Where(p => !string.IsNullOrEmpty(p.Namespace) && p.Namespace.Contains(ns)).
                Select(p => p.Name).
                OrderBy(p => p)).Except(new HashSet<string>(client.GetApiMethods().Select(p => p.Name))).
                Where(p => !p.EndsWith("Response")).
                ToList();
        }
    }
}
