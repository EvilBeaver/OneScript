using System;
using System.Collections.Generic;
using System.Linq;
using OneScript.Language.Sources;
using OneScript.Sources;

namespace ScriptEngine.Serialization
{
    public class CodeSourceImageSerializer
    {
        private readonly List<ICodeSourceImageProvider> _providers;

        public CodeSourceImageSerializer(IEnumerable<ICodeSourceImageProvider> providers)
        {
            _providers = providers?.ToList() ?? new List<ICodeSourceImageProvider>();
        }
        
        public bool TryCreate(SourceCode source, out CodeSourceImage image)
        {
            image = null;
            if (source == null || source.Source == null)
                return false;

            var provider = _providers.FirstOrDefault(p => p.CanHandle(source.Source));
            if (provider == null)
                return false;

            image = provider.CreateImage(source.Source);
            if (image == null)
                return false;

            image.ProviderKey = provider.ProviderKey;
            image.Name = source.Name;
            image.OwnerPackageId = source.OwnerPackageId;
            if (string.IsNullOrEmpty(image.Location))
                image.Location = source.Location;

            return true;
        }

        public bool TryRestore(
            CodeSourceImage image,
            out ICodeSource source,
            out string error)
        {
            source = null;
            error = null;

            if (image == null)
            {
                error = "Source image is missing";
                return false;
            }

            var provider = _providers.FirstOrDefault(p => string.Equals(p.ProviderKey, image.ProviderKey, StringComparison.OrdinalIgnoreCase));
            if (provider == null)
            {
                error = $"Provider '{image.ProviderKey}' is not registered";
                return false;
            }

            if (!provider.TryRestore(image, out source, out error))
            {
                return false;
            }

            return true;
        }
    }
}
