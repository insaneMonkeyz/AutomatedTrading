namespace Strategies
{
    public interface ISettingsProvider
    {
        string GetSettings();
          void LoadFromSettings(string settings);
    }
}
