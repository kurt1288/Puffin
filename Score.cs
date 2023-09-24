namespace Skookum
{
   internal struct Score
   {
      readonly int _Score;
      public readonly short Mg { get => (short)_Score; }
      public readonly short Eg { get => (short)((_Score + 0x8000) >> 16); }

      public Score() { }

      public Score(short mg, short eg)
      {
         _Score = (eg << 16) + mg;
      }

      private Score(int score)
      {
         _Score = score;
      }

      public static Score operator +(Score a, Score b) => new(a._Score + b._Score);

      public static Score operator -(Score a, Score b) => new(a._Score - b._Score);

      public static Score operator *(Score a, int b) => new(a._Score * b);
   }
}
