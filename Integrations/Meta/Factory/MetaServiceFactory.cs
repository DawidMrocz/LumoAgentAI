using Integrations.Meta.Graph;
using System;
using System.Collections.Generic;
using System.Text;

namespace Integrations.Meta.Factory
{
    internal sealed class MetaServiceFactory
    : IMetaServiceFactory
    {
        private readonly IMetaInstagramService _instagram;
        private readonly IMetaPageService _pages;

        public MetaServiceFactory(
            IMetaInstagramService instagram,
            IMetaPageService pages)
        {
            _instagram = instagram;
            _pages = pages;
        }

        public IMetaInstagramService CreateInstagram()
            => _instagram;

        public IMetaPageService CreatePages()
            => _pages;
    }
}
