using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VSSpotify
{
    internal class TempDictionary : IDictionary<string, string>, IDisposable
    {
        private readonly string _cachePath = $"{Path.GetTempPath()}{Path.DirectorySeparatorChar}VS_SPOTIFY.json";

        private IDictionary<string, string> _cache;
        private IDictionary<string, string> Cache
        {
            get
            {
                if (_cache == null)
                {
                    if (!File.Exists(_cachePath))
                    {
                        _cache = new Dictionary<string, string>();
                    }
                    else
                    {
                        using (var reader = File.OpenText(_cachePath))
                        {
                            var serializer = new JsonSerializer();
                            _cache = (Dictionary<string, string>)serializer.Deserialize(reader, typeof(Dictionary<string, string>));
                        }
                    }
                }

                return _cache;
            }
        }

        private void WriteToDisk()
        {
            var json = JsonConvert.SerializeObject(Cache);
            File.WriteAllText(_cachePath, json);
        }

        public void Dispose() => WriteToDisk();

        #region IDictionary
        public string this[string key] { get => Cache[key]; set => Cache[key] = value; }

        public ICollection<string> Keys => Cache.Values;

        public ICollection<string> Values => Cache.Values;

        public int Count => Cache.Count;

        public bool IsReadOnly => false;

        public void Add(string key, string value) => Cache.Add(key, value);

        public void Add(KeyValuePair<string, string> item) => Cache.Add(item);

        public void Clear() => Cache.Clear();

        public bool Contains(KeyValuePair<string, string> item) => Cache.Contains(item);

        public bool ContainsKey(string key) => Cache.ContainsKey(key);

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex) => Cache.CopyTo(array, arrayIndex);

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => Cache.GetEnumerator();

        public bool Remove(string key) => Cache.Remove(key);

        public bool Remove(KeyValuePair<string, string> item) => Cache.Remove(item);

        public bool TryGetValue(string key, out string value) => Cache.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion
    }
}
