using System.Reflection;
using System.Text.Json;

namespace Puffin
{
   [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
   internal class TunableAttribute(int min, int max, double step) : Attribute
   {
      public int Min { get; } = min;
      public int Max { get; } = max;
      public double Step { get; } = step;
   }

   internal class TunableHelpers()
   {
      public record TuningParameterInfo(string Name, int Current, int Min, int Max, double Step);

      public static string ExportWeatherFactoryConfig()
      {
         Dictionary<string, object> parameters = [];

         foreach (var prop in typeof(Search).GetProperties(BindingFlags.Public | BindingFlags.Static))
         {
            TunableAttribute? attr = prop.GetCustomAttribute<TunableAttribute>();

            if (attr != null)
            {
               parameters[prop.Name] = new
               {
                  value = prop.GetValue(null),
                  min_value = attr.Min,
                  max_value = attr.Max,
                  step = attr.Step
               };
            }
         }

         return JsonSerializer.Serialize(parameters, s_writeOptions);
      }

      public static IEnumerable<TuningParameterInfo> GetTuningParametersAsOptions()
      {
         foreach (var prop in typeof(Search).GetProperties(BindingFlags.Public | BindingFlags.Static))
         {
            TunableAttribute? attr = prop.GetCustomAttribute<TunableAttribute>();

            if (attr != null)
            {
               var value = prop.GetValue(null);

               if (value != null)
               {
                  yield return new TuningParameterInfo(
                      prop.Name,
                      (int)value,
                      attr.Min,
                      attr.Max,
                      attr.Step
                  );
               }
            }
         }
      }

      private static readonly JsonSerializerOptions s_writeOptions = new()
      {
         WriteIndented = true,
      };
   }
}
