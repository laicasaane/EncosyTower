#if UNITY_WEBREQUEST

using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace EncosyTower.Tasks
{
    /// <summary>
    /// Thrown when a <see cref="UnityWebRequest"/> awaited through <c>UnityTask</c> ends with a connection, protocol or
    /// data-processing error.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The name matches <c>Cysharp.Threading.Tasks.UnityWebRequestException</c>. A file that imports both namespaces
    /// refers to this type by its full name.
    /// </para>
    /// <para>
    /// <b>Counterparts:</b> UniTask: <c>Cysharp.Threading.Tasks.UnityWebRequestException</c>; Unity: none.
    /// </para>
    /// </remarks>
    public sealed class UnityWebRequestException : Exception
    {
        private string _message;

        /// <summary>
        /// Creates an exception for <paramref name="unityWebRequest"/> and copies its result, error and response.
        /// </summary>
        /// <param name="unityWebRequest">The failed request.</param>
        public UnityWebRequestException(UnityWebRequest unityWebRequest)
        {
            UnityWebRequest = unityWebRequest;
            Result = unityWebRequest.result;
            Error = unityWebRequest.error;
            ResponseCode = unityWebRequest.responseCode;
            ResponseHeaders = unityWebRequest.GetResponseHeaders();

            if (unityWebRequest.downloadHandler is DownloadHandlerBuffer buffer)
            {
                Text = buffer.text;
            }
        }

        /// <summary>Gets the failed request.</summary>
        public UnityWebRequest UnityWebRequest { get; }

        /// <summary>Gets the result of the request.</summary>
        public UnityWebRequest.Result Result { get; }

        /// <summary>Gets the error text of the request.</summary>
        public string Error { get; }

        /// <summary>Gets the downloaded text, or <c>null</c> when the download handler is not a buffer.</summary>
        public string Text { get; }

        /// <summary>Gets the HTTP response code.</summary>
        public long ResponseCode { get; }

        /// <summary>Gets the response headers.</summary>
        public Dictionary<string, string> ResponseHeaders { get; }

        /// <summary>Gets the error text followed by the downloaded text when there is any.</summary>
        public override string Message
            => _message ??= string.IsNullOrWhiteSpace(Text) ? Error : $"{Error}{Environment.NewLine}{Text}";

        internal static bool IsError(UnityWebRequest unityWebRequest)
            => unityWebRequest.result is UnityWebRequest.Result.ConnectionError
                or UnityWebRequest.Result.ProtocolError
                or UnityWebRequest.Result.DataProcessingError;
    }
}

#endif
