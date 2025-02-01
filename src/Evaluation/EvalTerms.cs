namespace Puffin.Evaluation
{
   internal static class EvalTerms
   {
      [Eval("piece values", 6)]
      public static readonly Score[] PieceValues =
      [
          new( 37,  56),
          new(132, 145),
          new(153, 154),
          new(163, 236),
          new(308, 438),
          new(  0,   0),
      ];

      [Eval("pst", 384)]
      public static readonly Score[] PST =
      [
          // Pawn
          new(  0,   0),     new(  0,   0),     new(  0,   0),     new(  0,   0),     new(  0,   0),     new(  0,   0),     new(  0,   0),     new(  0,   0),
          new( 21,  87),     new( 26,  83),     new( 20,  94),     new( 60,  51),     new( 31,  55),     new( 30,  53),     new(-64,  96),     new(-76, 104),
          new( 27,  70),     new( 18,  83),     new( 42,  51),     new( 50,  23),     new( 60,  20),     new( 90,  26),     new( 59,  65),     new( 40,  62),
          new(  7,  54),     new( 11,  52),     new( 13,  37),     new( 16,  24),     new( 33,  29),     new( 38,  26),     new( 21,  48),     new( 26,  40),
          new(  3,  42),     new(  3,  45),     new( 13,  29),     new( 21,  23),     new( 21,  27),     new( 31,  22),     new( 12,  39),     new( 16,  29),
          new( -2,  35),     new(  3,  35),     new(  7,  28),     new( 11,  31),     new( 21,  34),     new( -2,  33),     new(  5,  35),     new( -5,  31),
          new(  4,  40),     new( 11,  39),     new( 14,  35),     new( 19,  35),     new( 24,  45),     new( 25,  38),     new( 21,  35),     new( -3,  33),
          new(  0,   0),     new(  0,   0),     new(  0,   0),     new(  0,   0),     new(  0,   0),     new(  0,   0),     new(  0,   0),     new(  0,   0), 

          // Knight
          new(-51,  52),     new(-48,  80),     new(-10,  91),     new( 23,  80),     new( 46,  86),     new( -3,  65),     new(-26,  80),     new(-15,  33),
          new( 48,  80),     new( 58,  86),     new( 79,  82),     new( 93,  82),     new( 81,  76),     new(119,  63),     new( 66,  82),     new( 77,  65),
          new( 59,  81),     new( 81,  82),     new( 91,  94),     new( 96,  96),     new(122,  85),     new(136,  73),     new( 96,  74),     new( 81,  72),
          new( 68,  93),     new( 78,  91),     new( 91, 101),     new(111, 104),     new( 92, 105),     new(118,  98),     new( 87,  94),     new(100,  84),
          new( 63,  95),     new( 67,  88),     new( 74, 103),     new( 79, 105),     new( 84, 109),     new( 86,  96),     new( 94,  86),     new( 78,  92),
          new( 46,  81),     new( 54,  82),     new( 60,  85),     new( 59,  99),     new( 72,  97),     new( 66,  82),     new( 76,  80),     new( 61,  84),
          new( 41,  80),     new( 45,  84),     new( 48,  79),     new( 61,  82),     new( 61,  81),     new( 63,  78),     new( 64,  81),     new( 62,  95),
          new(  9,  82),     new( 47,  76),     new( 32,  76),     new( 49,  78),     new( 52,  83),     new( 56,  75),     new( 51,  84),     new( 38,  85), 

          // Bishop
          new( 34,  97),     new(  0, 103),     new(  4, 100),     new(-35, 109),     new(-26, 108),     new(-16,  94),     new( 16,  95),     new( -2,  91),
          new( 37,  86),     new( 56,  89),     new( 47,  91),     new( 36,  96),     new( 53,  85),     new( 47,  91),     new( 35,  93),     new( 31,  89),
          new( 52, 101),     new( 66,  92),     new( 68,  96),     new( 74,  87),     new( 66,  93),     new(102,  93),     new( 80,  93),     new( 74,  97),
          new( 48,  96),     new( 77,  95),     new( 72,  93),     new( 85, 108),     new( 83,  97),     new( 83,  97),     new( 84,  91),     new( 52,  97),
          new( 61,  92),     new( 53,  98),     new( 66, 102),     new( 82, 101),     new( 77, 102),     new( 69,  96),     new( 60,  97),     new( 84,  78),
          new( 61,  92),     new( 77,  95),     new( 68,  97),     new( 70, 100),     new( 74, 103),     new( 75,  97),     new( 77,  90),     new( 79,  88),
          new( 79,  96),     new( 67,  84),     new( 78,  80),     new( 58,  92),     new( 64,  95),     new( 76,  86),     new( 87,  90),     new( 77,  85),
          new( 67,  88),     new( 82,  93),     new( 65,  90),     new( 55,  89),     new( 63,  87),     new( 58, 102),     new( 74,  86),     new( 84,  76), 

          // Rook
          new( 88, 204),     new( 80, 209),     new( 73, 216),     new( 70, 213),     new( 83, 207),     new(103, 204),     new(102, 207),     new(118, 199),
          new( 77, 204),     new( 77, 213),     new( 89, 216),     new(101, 207),     new( 90, 207),     new(113, 197),     new(118, 193),     new(132, 185),
          new( 71, 203),     new( 95, 200),     new( 94, 201),     new( 96, 198),     new(123, 186),     new(128, 182),     new(165, 177),     new(134, 176),
          new( 75, 205),     new( 94, 198),     new( 95, 202),     new( 94, 199),     new(102, 188),     new(118, 180),     new(128, 184),     new(114, 181),
          new( 70, 197),     new( 71, 198),     new( 81, 194),     new( 85, 192),     new( 84, 191),     new( 86, 188),     new(110, 181),     new( 97, 180),
          new( 70, 188),     new( 77, 185),     new( 80, 181),     new( 81, 182),     new( 91, 177),     new( 99, 171),     new(129, 158),     new(106, 163),
          new( 71, 177),     new( 74, 182),     new( 86, 179),     new( 88, 177),     new( 94, 171),     new(104, 165),     new(119, 158),     new( 86, 167),
          new( 87, 182),     new( 87, 180),     new( 91, 185),     new( 99, 177),     new(105, 172),     new(102, 176),     new(106, 171),     new( 94, 171), 

          // Queen
          new(212, 374),     new(199, 394),     new(217, 412),     new(236, 406),     new(232, 407),     new(243, 401),     new(287, 351),     new(235, 384),
          new(235, 355),     new(212, 382),     new(210, 416),     new(197, 439),     new(202, 453),     new(239, 412),     new(233, 401),     new(286, 385),
          new(237, 363),     new(232, 369),     new(230, 402),     new(233, 411),     new(246, 422),     new(277, 408),     new(287, 380),     new(284, 382),
          new(232, 372),     new(247, 372),     new(239, 384),     new(230, 407),     new(236, 422),     new(253, 412),     new(266, 411),     new(260, 396),
          new(245, 358),     new(234, 382),     new(236, 384),     new(238, 399),     new(236, 401),     new(244, 395),     new(254, 391),     new(263, 386),
          new(244, 342),     new(248, 359),     new(242, 372),     new(240, 370),     new(244, 375),     new(254, 369),     new(266, 357),     new(262, 351),
          new(245, 337),     new(243, 340),     new(250, 340),     new(253, 345),     new(252, 348),     new(259, 324),     new(264, 302),     new(274, 287),
          new(238, 334),     new(241, 333),     new(247, 335),     new(252, 346),     new(251, 332),     new(239, 332),     new(243, 327),     new(254, 307), 

          // King
          new(-17, -64),     new(  2, -28),     new( -8, -11),     new(-114,  25),     new(-69,   9),     new(-24,  16),     new( 69,   6),     new(161, -80),
          new(-96,   6),     new(-37,  28),     new(-76,  38),     new( 12,  25),     new(-31,  41),     new(-26,  56),     new( 28,  46),     new( 53,  21),
          new(-110,  17),     new( -1,  30),     new(-65,  46),     new(-94,  59),     new(-48,  58),     new( 33,  48),     new( 21,  46),     new(-11,  20),
          new(-70,   5),     new(-73,  30),     new(-102,  50),     new(-148,  62),     new(-135,  63),     new(-88,  57),     new(-78,  47),     new(-122,  30),
          new(-74,  -6),     new(-74,  18),     new(-91,  36),     new(-134,  53),     new(-124,  53),     new(-80,  39),     new(-86,  31),     new(-134,  23),
          new(-24, -19),     new(  8,  -1),     new(-44,  17),     new(-59,  27),     new(-53,  28),     new(-51,  23),     new(-20,  10),     new(-49,   1),
          new( 49, -21),     new( 18,   0),     new( 11,   0),     new(-18,   6),     new(-17,  10),     new( -4,   6),     new( 27,   2),     new( 25,  -8),
          new( 11, -42),     new( 33, -24),     new( 12,  -7),     new(-39, -13),     new(  2, -15),     new( -5, -15),     new( 33, -21),     new( 31, -49),

      ];

      [Eval("knight mobility", 9)]
      public static readonly Score[] KnightMobility =
      [
          new( -2, -18),
          new( 47,  55),
          new( 69,  90),
          new( 77, 109),
          new( 88, 119),
          new( 93, 131),
          new(101, 132),
          new(108, 135),
          new(118, 130),
      ];

      [Eval("bishop mobility", 14)]
      public static readonly Score[] BishopMobility =
      [
          new( 25, -10),
          new( 43,  25),
          new( 56,  72),
          new( 65,  92),
          new( 76, 102),
          new( 84, 113),
          new( 90, 121),
          new( 94, 126),
          new( 97, 130),
          new(100, 131),
          new(104, 132),
          new(111, 128),
          new(112, 133),
          new(119, 122),
      ];

      [Eval("rook mobility", 15)]
      public static readonly Score[] RookMobility =
      [
          new( 64, -28),
          new(100, 103),
          new(101, 167),
          new(107, 184),
          new(113, 193),
          new(117, 200),
          new(118, 208),
          new(121, 213),
          new(124, 216),
          new(129, 220),
          new(133, 224),
          new(132, 230),
          new(136, 232),
          new(141, 232),
          new(145, 229),
      ];

      [Eval("queen mobility", 28)]
      public static readonly Score[] QueenMobility =
      [
          new(-34, -12),
          new( 34,   0),
          new(262,  75),
          new(228, 180),
          new(240, 270),
          new(239, 339),
          new(250, 332),
          new(252, 354),
          new(255, 370),
          new(258, 385),
          new(263, 386),
          new(265, 392),
          new(268, 402),
          new(271, 402),
          new(273, 408),
          new(275, 413),
          new(275, 419),
          new(276, 425),
          new(274, 435),
          new(279, 433),
          new(284, 437),
          new(288, 433),
          new(300, 435),
          new(323, 419),
          new(311, 442),
          new(433, 369),
          new(348, 414),
          new(265, 439),
      ];

      [Eval("rook half open file")]
      public static readonly Score RookHalfOpenFile = new(11, 4);

      [Eval("rook open file")]
      public static readonly Score RookOpenFile = new(28, 5);

      [Eval("king open file")]
      public static readonly Score KingOpenFile = new(68, -9);

      [Eval("king half open file")]
      public static readonly Score KingHalfOpenFile = new(24, -16);

      [Eval("king attack weights", 5)]
      public static readonly Score[] KingAttackWeights =
      [
          new(  0,   0),
          new(  9,  -5),
          new( 13,  -1),
          new( 25,  -9),
          new( 17,  12),
      ];

      [Eval("pawn shield", 4)]
      public static readonly Score[] PawnShield =
      [
          new(-36,   0),
          new( -5,  -6),
          new( 29, -15),
          new( 62, -31),
      ];

      [Eval("passed pawn", 7)]
      public static readonly Score[] PassedPawn =
      [
          new(  0,   0),
          new( -3,  12),
          new( -7,  16),
          new( -8,  37),
          new(-14,   6),
          new(-29,  81),
          new( 21, 112),
      ];

      [Eval("defended pawn", 8)]
      public static readonly Score[] DefendedPawn =
      [
          new(-25, -18),
          new( -8,  -4),
          new(  5,   9),
          new( 18,  28),
          new( 29,  49),
          new( 40,  58),
          new( 34,  54),
          new(  0,   0),
      ];

      [Eval("connected pawn", 9)]
      public static readonly Score[] ConnectedPawn =
      [
          new(-12, -10),
          new( -2,   3),
          new(  6,   8),
          new( 14,  22),
          new( 23,  25),
          new( 26,  77),
          new( 57, -22),
          new( -9,  -2),
          new(  0,   0),
      ];

      [Eval("isolated pawn", 8)]
      public static readonly Score[] IsolatedPawn =
      [
          new(  0,   4),
          new(  1,  13),
          new(  9,   8),
          new(  7,  10),
          new( 11,  13),
          new(  7,   5),
          new(  0,  12),
          new(  4,   4),
      ];

      [Eval("friendly king pawn distance")]
      public static readonly Score FriendlyKingPawnDistance = new(8, -11);

      [Eval("enemy king pawn distance")]
      public static readonly Score EnemyKingPawnDistance = new(-4, 18);

      [Eval("bishop pair")]
      public static readonly Score BishopPair = new(23, 62);

      [Eval("pawn push threats")]
      public static readonly Score PawnPushThreats = new(19, 0);

      [Eval("pawn attacks")]
      public static readonly Score PawnAttacks = new(45, 7);

      [Eval("free advance pawn")]
      public static readonly Score FreeAdvancePawn = new(-16, 52);
   }
}
