using System.Configuration;

namespace Escc.Umbraco.UnpublishOverrides
{
    public class UnpublishOverridesContentTypesCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new UnpublishOverridesContentTypeElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((UnpublishOverridesContentTypeElement)element).Name;
        }
    }
}