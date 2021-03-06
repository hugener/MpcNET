// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MpdMessage.cs" company="MpcNET">
// Copyright (c) MpcNET. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace MpcNET.Message
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;

    internal class MpdMessage<T> : IMpdMessage<T>
    {
        private readonly Regex linePattern = new Regex(@"^(?<key>[\w-]*):[ ]{0,1}(?<value>.*)$");
        private readonly IList<string> rawResponse;

        public MpdMessage(IMpcCommand<T> command, bool connected, IReadOnlyCollection<string> response)
        {
            this.Request = new MpdRequest<T>(command);

            var endLine = response.Skip(response.Count - 1).Single();
            this.rawResponse = response.Take(response.Count - 1).ToList();

            var values = this.Request.Command.Deserialize(this.GetValuesFromResponse());
            this.Response = new MpdResponse<T>(endLine, values, connected);
        }

        public IMpdRequest<T> Request { get; }

        public IMpdResponse<T> Response { get; }

        public bool IsResponseValid => this.Response != null && this.Response.Result.Status == "OK";

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        private IReadOnlyList<KeyValuePair<string, string>> GetValuesFromResponse()
        {
            var result = new List<KeyValuePair<string, string>>();

            foreach (var line in this.rawResponse)
            {
                var match = this.linePattern.Match(line);
                if (match.Success)
                {
                    var mpdKey = match.Result("${key}");
                    if (!string.IsNullOrEmpty(mpdKey))
                    {
                        var mpdValue = match.Result("${value}");
                        if (!string.IsNullOrEmpty(mpdValue))
                        {
                            result.Add(new KeyValuePair<string, string>(mpdKey, mpdValue));
                        }
                    }
                }
            }

            return result;
        }
    }
}