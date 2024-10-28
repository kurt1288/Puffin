using System.Reflection;
using System.Text.Json;

namespace Puffin
{
   [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
   internal class TunableAttribute : Attribute
   {
      // Store all values as doubles internally
      private readonly double _min;
      private readonly double _max;
      private readonly double _step;
      private readonly bool _isInteger;

      // Constructor for int parameters
      public TunableAttribute(int min, int max, double step)
      {
         _min = min;
         _max = max;
         _step = step;
         _isInteger = true;
      }

      // Constructor for double parameters
      public TunableAttribute(double min, double max, double step)
      {
         _min = min;
         _max = max;
         _step = step;
         _isInteger = false;
      }

      // Properties that return the appropriate type and scale doubles (UCI only works with ints)
      public object Min => _isInteger ? (int)_min : _min * 100;
      public object Max => _isInteger ? (int)_max : _max * 100;
      public double Step => _isInteger ? _step : _step * 100;
      public bool IsInteger => _isInteger;
   }

   internal class TunableHelpers()
   {
      public record TuningParameterInfo(string Name, object Current, object Min, object Max, double Step);

      public static string ExportWeatherFactoryConfig()
      {
         Dictionary<string, object> parameters = [];

         foreach (var prop in typeof(Search).GetProperties(BindingFlags.Public | BindingFlags.Static))
         {
            TunableAttribute? attr = prop.GetCustomAttribute<TunableAttribute>();

            if (attr != null)
            {
               var value = prop.GetValue(null);
               var scaledValue = value is double doubleValue ? doubleValue * 100 : value;

               parameters[prop.Name] = new
               {
                  value = scaledValue,
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
                  var scaledValue = value is double doubleValue ? doubleValue * 100 : value;

                  yield return new TuningParameterInfo(
                      prop.Name,
                      scaledValue,
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
