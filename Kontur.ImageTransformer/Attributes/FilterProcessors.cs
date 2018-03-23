using System;

namespace Kontur.ImageTransformer.Attributes
{
    public class ContainsFilterProcessors : Attribute
    {
        public ContainsFilterProcessors(bool isContainsProcessors = true)
        {
            _containsFilterProcessors = isContainsProcessors;
        }

        private bool _containsFilterProcessors;
    }
}