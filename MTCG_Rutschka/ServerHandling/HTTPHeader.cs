using System;

namespace MTCG_Rutschka {
    /// <summary> Class for HTTP Headers </summary>
    public class HttpHeader {
        /// <summary> Cunstroctor to set Key-Value Pairs </summary>
        /// <param name="name">The Key</param>
        /// <param name="value">The Value</param>
        public HttpHeader(string name, string value) {
            Name = name;
            Value = value;
        }

        /// <summary> Construct new key value pair from a string </summary>
        /// <param name="headerString"></param>
        public HttpHeader(string headerString) {
            Name = Value = "";
            try {
                int ind = headerString.IndexOf(":");
                Name = headerString.Substring(0, ind).Trim();
                Value = headerString.Substring(ind + 1).Trim();
            } catch(Exception){}
        }

        /// <summary> Key </summary>
        public string Name { get; private set; }
        /// <summary> Value </summary>
        public string Value { get; private set; }
    }
}
