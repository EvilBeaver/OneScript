namespace ScriptEngine.Hosting
{
    /// <summary>
    /// Предназначен для хранения стабильного конфига
    /// который не изменяется и не вычитывает провайдеры при каждом обращении к опциям.
    /// </summary>
    public class EngineConfiguration
    {
        private KeyValueConfig _config;
        private readonly ConfigurationProviders _providers;
        
        private readonly object _refreshLock = new object();

        public EngineConfiguration(ConfigurationProviders providers)
        {
            _providers = providers;
            _config = _providers.CreateConfig();
        }

        public KeyValueConfig GetConfig()
        {
            lock (_refreshLock)
            {
                return _config;
            }
        }
        
        public void Reload()
        {
            lock (_refreshLock)
            {
                _config = _providers.CreateConfig();
            }
        }
    }
}