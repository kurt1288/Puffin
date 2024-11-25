namespace Puffin
{
   internal readonly struct Score
   {
      private readonly int Value;

      public readonly short Mg => (short)Value;
      public readonly short Eg => (short)(Value + 0x8000 >> 16);

      public Score(short mg, short eg)
      {
         Value = (eg << 16) + mg;
      }

      private Score(int score)
      {
         Value = score;
      }

      public static Score operator +(Score a, Score b) => new(a.Value + b.Value);

      public static Score operator -(Score a, Score b) => new(a.Value - b.Value);
      public static Score operator *(Score a, Score b) => new(a.Value * b.Value);
      public static Score operator *(Score a, int b) => new(a.Value * b);
      public static Score operator *(Score a, double b) => new((int)(a.Value * b));
      public static Score operator *(double a, Score b) => new((int)(a * b.Value));
      public static Score operator +(Score a, int b) => new(a.Value + b);
      public static Score operator /(Score a, int b) => new(a.Value / b);

      public static bool operator ==(Score a, Score b) => a.Value == b.Value;
      public static bool operator !=(Score a, Score b) => a.Value != b.Value;
   }
}
