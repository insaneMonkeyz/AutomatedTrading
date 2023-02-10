using ClassGetter = System.Func<System.Collections.Generic.IEnumerable<string>>;
using SecuritiesByClassGetter = System.Func<string, System.Collections.Generic.IEnumerable<string>>;

namespace Quik.EntityProviders
{
    internal class SecuritiesToClasscodesMap
    {
        private ClassGetter _getClasses;
        private SecuritiesByClassGetter _getSecuritiesOfClass;
        private Dictionary<string, IEnumerable<string>>? _securitiesByClasscode;
        private Dictionary<string, string>? _classcodeBySecurity;

        private static readonly IEnumerable<string> _emptyResult = Enumerable.Empty<string>();

        public int TotalSecuritiesCount { get; private set; }

        public SecuritiesToClasscodesMap(ClassGetter getClasses, SecuritiesByClassGetter getSecuritiesOfClass)
        {
            _getClasses = getClasses;
            _getSecuritiesOfClass = getSecuritiesOfClass;
        }

        public void Initialize()
        {
            _securitiesByClasscode = _getClasses()
                .Where(c => MoexSpecifics.AllowedClassCodes.Contains(c))
                .ToDictionary(classcode => classcode,
                              classcode => _getSecuritiesOfClass(classcode));

            TotalSecuritiesCount = _securitiesByClasscode.Values.Count;

            _classcodeBySecurity = new(TotalSecuritiesCount);

            GC.Collect(3, GCCollectionMode.Forced, blocking: true, compacting: true);
        }

        public string? GetClassCode(string ticker)
        {
            try
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
            catch (NullReferenceException)
            {
                "Forgot to initialize classcode maps".DebugPrintWarning();
                return null;
            }
        }
        public IEnumerable<string> GetSecurities(string classcode)
        {
            try
            {
                return _securitiesByClasscode.TryGetValue(classcode, out IEnumerable<string>? securities)
                        ? securities
                        : _emptyResult;
            }
            catch (NullReferenceException)
            {
                "Forgot to initialize securities by classcode map".DebugPrintWarning();
                return null;
            }
        }
    }
}
