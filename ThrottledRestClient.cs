using RestSharp;
using System;
using System.Threading.Tasks;
using Validation;

namespace ThrottledRestClient
{
    public sealed class ThrottledRestClient
    {
        private readonly IRestClient _client;
        private readonly ConstrainedQueue<long> _timestampQueue;
        private readonly int _throttlePeriodMilliseconds;
        private readonly int _throttleRequestLimit;

        public ThrottledRestClient(string baseUrl, int throttlePeriodMilliseconds, int throttleRequestLimit)
        {
            Requires.NotNullOrWhiteSpace(baseUrl, nameof(baseUrl));
            Requires.Range(throttlePeriodMilliseconds > 0, nameof(throttlePeriodMilliseconds));
            Requires.Range(throttleRequestLimit > 0, nameof(throttleRequestLimit));

            this._throttlePeriodMilliseconds = throttlePeriodMilliseconds;
            this._throttleRequestLimit = throttleRequestLimit;

            this._client = new RestClient(baseUrl);
            this._timestampQueue = new ConstrainedQueue<long>(this._throttleRequestLimit);
        }

        public async Task<IRestResponse<T>> ExecuteAsync<T>(IRestRequest request)
        {
            Requires.NotNull(request, nameof(request));

            if (this._timestampQueue.IsFull)
            {
                var now = DateTime.Now.Ticks / 10000;
                var peeked = this._timestampQueue.Peek() / 10000;

                if (peeked > (now - this._throttlePeriodMilliseconds))
                {
                    await Task.Delay((int)(peeked - now + this._throttlePeriodMilliseconds)).ConfigureAwait(false);
                }
            }

            this._timestampQueue.Enqueue(DateTime.Now.Ticks);
            return await this._client.ExecuteAsync<T>(request).ConfigureAwait(false);
        }

        public void AddDefaultHeader(string name, string value)
            => this._client.AddDefaultHeader(name, value);
    }
}