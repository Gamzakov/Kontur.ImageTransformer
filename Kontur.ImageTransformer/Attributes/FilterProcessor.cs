using System;

namespace Kontur.ImageTransformer.Attributes
{
    public class FilterProcessor : Attribute
    {
        public string Filter { get; }

        public FilterProcessor(string filter)
        {
            Filter = filter;
        }
    }
}