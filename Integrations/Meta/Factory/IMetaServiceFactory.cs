using Integrations.Meta.Graph;
using System;
using System.Collections.Generic;
using System.Text;

namespace Integrations.Meta.Factory
{
    public interface IMetaServiceFactory
    {
        IMetaInstagramService CreateInstagram();

        IMetaPageService CreatePages();
    }
}
