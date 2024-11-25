namespace Puffin
{
   enum Square : byte
   {
      A8, B8, C8, D8, E8, F8, G8, H8,
      A7, B7, C7, D7, E7, F7, G7, H7,
      A6, B6, C6, D6, E6, F6, G6, H6,
      A5, B5, C5, D5, E5, F5, G5, H5,
      A4, B4, C4, D4, E4, F4, G4, H4,
      A3, B3, C3, D3, E3, F3, G3, H3,
      A2, B2, C2, D2, E2, F2, G2, H2,
      A1, B1, C1, D1, E1, F1, G1, H1, Null,
   }

   enum Color : byte
   {
      White,
      Black,
      Both,
      Null,
   }

   enum PieceType : byte
   {
      Pawn,
      Knight,
      Bishop,
      Rook,
      Queen,
      King,
      Null,
   }

   enum File
   {
      A, B, C, D, E, F, G, H
   }

   enum Rank
   {
      Rank_1, Rank_2, Rank_3, Rank_4, Rank_5, Rank_6, Rank_7, Rank_8,
   }

   // https://www.chessprogramming.org/Encoding_Moves
   enum MoveType : byte
   {
      Promotion = 8,
      Capture = 4,
      Special_1 = 2,
      Special_0 = 1,
   }

   enum MoveFlag
   {
      Quiet,
      DoublePawnPush,
      KingCastle,
      QueenCastle,
      Capture,
      EPCapture,
      KnightPromotion = 8,
      BishopPromotion,
      RookPromotion,
      QueenPromotion,
      KnightPromotionCapture,
      BishopPromotionCapture,
      RookPromotionCapture,
      QueenPromotionCapture,
   }

   enum Direction
   {
      Up = 8,
      Down = -Up,
      Right = 1,
      Left = -Right,
   }
}
