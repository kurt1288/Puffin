namespace Puffin.Evaluation
{
   [AttributeUsage(AttributeTargets.Field)]
   internal class EvalAttribute : Attribute
   {
      public int Length { get; }
      public string DisplayName { get; }
      public bool IsPst { get; }
      public bool IsArray { get; }

      // Default length of 2 for tuning trace values
      public EvalAttribute(string displayName, int length = 1, bool isPst = false)
      {
         DisplayName = displayName;
         Length = length;
         IsPst = isPst;
         IsArray = Length > 2;
      }
   }
}
