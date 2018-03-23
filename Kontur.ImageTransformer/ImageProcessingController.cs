using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Kontur.ImageTransformer.Attributes;
using Kontur.ImageTransformer.Enums;

namespace Kontur.ImageTransformer
{
    internal static class ImageProcessingController
    {
        static ImageProcessingController()
        {
            ImageProcessors = new Dictionary<string, Func<Bitmap, int[], Bitmap>>();
        }

        #region PublicMethods

        public static bool TryInitializeProcessors()
        {
            try
            {
                var types = GetTypesWithProcessors();
                InitializeProcessors(types);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public static Bitmap ApplyFilter(Bitmap bitmap, string filter, int[] selection)
        {
            if (ImageProcessors.TryGetValue(filter, out var processor))
            {
                return processor(bitmap, selection);
            }

            return null;
        }
        
        #endregion

        private static void InitializeProcessors(IEnumerable<Type> types)
        {
            var filters = Enum.GetNames(typeof(Filters)).Select(x => x.ToLowerInvariant()).ToArray();
            var methods = new List<MethodInfo>();
            
            foreach (var type in types)
            {
                methods.AddRange(type.GetMethods());
            }
            
            foreach (var method in methods)
            {
                var filterProcessor = method.GetCustomAttributes(typeof(FilterProcessor), true);
                
                if (filterProcessor.Length == 1)
                {
                    var filter = (filterProcessor[0] as FilterProcessor)?.Filter.ToLowerInvariant();
                    
                    if (!string.IsNullOrEmpty(filter) && filters.Contains(filter))
                    {
                        var instance = new ImageProcessors();
                        var processor = (Func<Bitmap, int[], Bitmap>)method.CreateDelegate(typeof(Func<Bitmap, int[], Bitmap>), instance);
                        ImageProcessors.Add(filter, processor);
                    }
                }
            }
        }

        private static IEnumerable<Type> GetTypesWithProcessors()
        {
            var types = from type in Assembly.GetAssembly(typeof(AsyncHttpServer)).ExportedTypes
                where type.GetCustomAttributes(typeof(ContainsFilterProcessors), true).Length == 1 select type;
            
            return types;
        }

        #region Fields

        private static readonly Dictionary<string, Func<Bitmap, int[], Bitmap>> ImageProcessors;

        #endregion
    }
}