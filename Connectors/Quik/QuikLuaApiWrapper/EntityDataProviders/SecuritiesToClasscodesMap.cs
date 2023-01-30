using ClassGetter = System.Func<System.Collections.Generic.IEnumerable<string>>;
using SecuritiesByClassGetter = System.Func<string, System.Collections.Generic.IEnumerable<string>>;

namespace Quik.EntityProviders
{
    internal class SecuritiesToClasscodesMap
    {
        private readonly Dictionary<string, IEnumerable<string>> _securitiesByClasscode;
        private readonly Dictionary<string, string> _classcodeBySecurity;
        private readonly IEnumerable<string> _emptyResult;

        public int TotalSecuritiesCount { get; private set; }

        public SecuritiesToClasscodesMap(ClassGetter getClasses, SecuritiesByClassGetter getSecuritiesOfClass)
        {
            _securitiesByClasscode = getClasses()
                .ToDictionary(classcode => classcode, 
                              classcode => getSecuritiesOfClass(classcode));

            TotalSecuritiesCount = _securitiesByClasscode.Values.Count;

            _classcodeBySecurity = new(TotalSecuritiesCount);

            _emptyResult = Enumerable.Empty<string>();
        }

        public string? GetClassCode(string ticker)
        {
            if (_classcodeBySecurity.TryGetValue(ticker, out string? classcode))
            {
                return classcode;
            }

            classcode = _securitiesByClasscode.FirstOrDefault(map => map.Value.Contains(ticker)).Key;

            if (classcode != null)
            {
                _classcodeBySecurity[ticker] = classcode;
            }

            return classcode;
        }
        public IEnumerable<string> GetSecurities(string classcode)
        {
            return _securitiesByClasscode.TryGetValue(classcode, out IEnumerable<string>? securities)
                ? securities
                : _emptyResult;
        }
    }
}
