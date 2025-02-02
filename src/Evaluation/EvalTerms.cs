namespace Puffin.Evaluation
{
   internal static class EvalTerms
   {
      [Eval("piece values", 6)]
      public static readonly Score[] PieceValues =
      [
          new( 37,  54),
          new(124, 136),
          new(111, 109),
          new(155, 225),
          new(294, 420),
          new(  0,   0),
      ];

      [Eval("pst", 384)]
      public static readonly Score[] PST =
      [
          // Pawn
          new(  0,   0),     new(  0,   0),     new(  0,   0),     new(  0,   0),     new(  0,   0),     new(  0,   0),     new(  0,   0),     new(  0,   0),
          new( 21,  84),     new( 25,  80),     new( 20,  91),     new( 58,  50),     new( 30,  54),     new( 29,  52),     new(-62,  93),     new(-72, 100),
          new( 26,  66),     new( 18,  79),     new( 41,  48),     new( 48,  22),     new( 57,  19),     new( 87,  25),     new( 57,  62),     new( 39,  58),
          new(  8,  52),     new( 10,  50),     new( 15,  36),     new( 17,  24),     new( 33,  28),     new( 38,  25),     new( 21,  46),     new( 26,  38),
          new(  4,  41),     new(  4,  43),     new( 14,  28),     new( 21,  22),     new( 21,  27),     new( 31,  21),     new( 12,  37),     new( 17,  28),
          new( -1,  34),     new(  2,  34),     new(  8,  27),     new( 11,  29),     new( 21,  32),     new( -2,  32),     new(  5,  33),     new( -3,  29),
          new(  5,  38),     new( 11,  38),     new( 15,  33),     new( 20,  35),     new( 25,  43),     new( 24,  37),     new( 20,  34),     new( -1,  32),
          new(  0,   0),     new(  0,   0),     new(  0,   0),     new(  0,   0),     new(  0,   0),     new(  0,   0),     new(  0,   0),     new(  0,   0), 

          // Knight
          new(-47,  56),     new(-43,  82),     new( -6,  92),     new( 25,  82),     new( 47,  87),     new(  0,  68),     new(-21,  82),     new(-12,  38),
          new( 49,  83),     new( 58,  87),     new( 78,  82),     new( 92,  82),     new( 81,  76),     new(118,  64),     new( 65,  84),     new( 77,  68),
          new( 59,  82),     new( 80,  81),     new( 90,  90),     new( 94,  92),     new(119,  81),     new(132,  70),     new( 94,  74),     new( 80,  75),
          new( 68,  93),     new( 77,  90),     new( 89,  97),     new(109, 100),     new( 91, 101),     new(114,  94),     new( 85,  93),     new( 98,  86),
          new( 62,  95),     new( 66,  87),     new( 72,  99),     new( 78, 101),     new( 82, 104),     new( 84,  93),     new( 92,  86),     new( 77,  93),
          new( 46,  81),     new( 54,  81),     new( 59,  82),     new( 59,  95),     new( 71,  94),     new( 65,  78),     new( 75,  79),     new( 61,  85),
          new( 41,  83),     new( 46,  85),     new( 48,  79),     new( 60,  80),     new( 60,  80),     new( 62,  78),     new( 64,  82),     new( 62,  96),
          new( 11,  85),     new( 48,  77),     new( 33,  78),     new( 49,  79),     new( 53,  84),     new( 56,  77),     new( 52,  85),     new( 39,  87), 

          // Bishop
          new(  8,  58),     new(-22,  68),     new(-23,  63),     new(-57,  73),     new(-50,  70),     new(-41,  60),     new( -8,  57),     new(-26,  57),
          new( 11,  52),     new( 28,  54),     new( 23,  58),     new(  9,  60),     new( 28,  52),     new( 21,  55),     new( 10,  60),     new(  4,  52),
          new( 24,  63),     new( 41,  60),     new( 40,  62),     new( 49,  56),     new( 38,  60),     new( 75,  62),     new( 52,  58),     new( 48,  62),
          new( 24,  62),     new( 48,  61),     new( 46,  63),     new( 56,  75),     new( 57,  67),     new( 53,  65),     new( 57,  58),     new( 26,  60),
          new( 33,  55),     new( 29,  66),     new( 38,  69),     new( 56,  72),     new( 47,  69),     new( 44,  65),     new( 33,  62),     new( 58,  43),
          new( 35,  58),     new( 49,  60),     new( 43,  66),     new( 42,  66),     new( 47,  71),     new( 47,  63),     new( 51,  56),     new( 50,  49),
          new( 51,  57),     new( 41,  51),     new( 48,  45),     new( 33,  61),     new( 36,  60),     new( 49,  52),     new( 58,  53),     new( 51,  48),
          new( 41,  54),     new( 52,  56),     new( 40,  59),     new( 28,  54),     new( 39,  55),     new( 31,  66),     new( 48,  49),     new( 55,  40), 

          // Rook
          new( 86, 197),     new( 79, 201),     new( 72, 208),     new( 69, 205),     new( 81, 199),     new(101, 196),     new(100, 199),     new(114, 192),
          new( 75, 196),     new( 75, 205),     new( 86, 207),     new( 98, 199),     new( 88, 199),     new(110, 190),     new(114, 186),     new(128, 178),
          new( 70, 195),     new( 93, 193),     new( 91, 193),     new( 93, 190),     new(118, 179),     new(124, 175),     new(158, 171),     new(129, 169),
          new( 74, 197),     new( 91, 190),     new( 92, 195),     new( 91, 191),     new( 98, 181),     new(113, 174),     new(123, 178),     new(109, 174),
          new( 69, 190),     new( 69, 190),     new( 79, 187),     new( 82, 185),     new( 81, 184),     new( 85, 181),     new(106, 175),     new( 94, 173),
          new( 68, 181),     new( 74, 178),     new( 78, 174),     new( 79, 175),     new( 89, 170),     new( 96, 164),     new(125, 153),     new(103, 157),
          new( 69, 171),     new( 73, 175),     new( 84, 172),     new( 86, 170),     new( 92, 164),     new(101, 159),     new(115, 152),     new( 84, 161),
          new( 85, 175),     new( 85, 173),     new( 88, 178),     new( 97, 170),     new(102, 165),     new( 99, 169),     new(104, 164),     new( 92, 164), 

          // Queen
          new(203, 360),     new(191, 379),     new(207, 396),     new(226, 390),     new(222, 392),     new(232, 386),     new(275, 337),     new(225, 369),
          new(224, 341),     new(202, 367),     new(201, 400),     new(189, 422),     new(193, 435),     new(229, 396),     new(223, 385),     new(273, 369),
          new(227, 349),     new(222, 354),     new(219, 386),     new(223, 394),     new(234, 405),     new(264, 393),     new(275, 364),     new(272, 366),
          new(222, 358),     new(235, 358),     new(228, 368),     new(220, 391),     new(226, 405),     new(241, 395),     new(254, 394),     new(249, 380),
          new(234, 344),     new(223, 367),     new(226, 368),     new(227, 383),     new(226, 385),     new(234, 380),     new(242, 376),     new(251, 371),
          new(233, 330),     new(237, 346),     new(231, 358),     new(230, 355),     new(235, 360),     new(242, 356),     new(254, 344),     new(250, 338),
          new(233, 326),     new(232, 327),     new(238, 328),     new(242, 332),     new(241, 336),     new(248, 312),     new(253, 292),     new(262, 276),
          new(227, 322),     new(230, 321),     new(236, 323),     new(241, 333),     new(240, 320),     new(228, 320),     new(233, 314),     new(242, 295), 

          // King
          new(-15, -62),     new(  2, -27),     new( -7, -10),     new(-111,  24),     new(-65,   8),     new(-22,  15),     new( 68,   5),     new(158, -77),
          new(-92,   6),     new(-35,  26),     new(-72,  36),     new( 12,  24),     new(-28,  38),     new(-25,  53),     new( 27,  43),     new( 53,  19),
          new(-107,  16),     new(  0,  29),     new(-61,  44),     new(-89,  56),     new(-46,  55),     new( 31,  46),     new( 19,  44),     new(-11,  19),
          new(-66,   5),     new(-70,  29),     new(-98,  48),     new(-142,  59),     new(-130,  60),     new(-85,  54),     new(-75,  45),     new(-119,  30),
          new(-71,  -6),     new(-70,  17),     new(-88,  34),     new(-129,  51),     new(-120,  50),     new(-76,  37),     new(-83,  30),     new(-130,  22),
          new(-22, -18),     new(  7,   0),     new(-41,  16),     new(-57,  26),     new(-52,  27),     new(-49,  23),     new(-20,  10),     new(-48,   2),
          new( 46, -20),     new( 18,   0),     new(  9,   0),     new(-18,   6),     new(-17,  10),     new( -4,   6),     new( 26,   2),     new( 23,  -8),
          new( 11, -40),     new( 32, -23),     new( 11,  -6),     new(-38, -12),     new(  1, -14),     new( -5, -14),     new( 31, -20),     new( 30, -47),

      ];

      [Eval("knight mobility", 9)]
      public static readonly Score[] KnightMobility =
      [
          new( -2, -21),
          new( 45,  48),
          new( 66,  81),
          new( 74, 100),
          new( 85, 111),
          new( 89, 123),
          new( 98, 126),
          new(104, 131),
          new(113, 127),
      ];

      [Eval("bishop mobility", 14)]
      public static readonly Score[] BishopMobility =
      [
          new( -2, -45),
          new( 14,  -7),
          new( 27,  38),
          new( 36,  57),
          new( 46,  66),
          new( 54,  76),
          new( 59,  83),
          new( 64,  86),
          new( 66,  90),
          new( 69,  89),
          new( 72,  90),
          new( 78,  84),
          new( 79,  87),
          new( 84,  76),
      ];

      [Eval("rook mobility", 15)]
      public static readonly Score[] RookMobility =
      [
          new( 64, -32),
          new( 96,  96),
          new( 97, 158),
          new(103, 175),
          new(108, 185),
          new(112, 191),
          new(113, 198),
          new(116, 204),
          new(119, 206),
          new(124, 210),
          new(127, 215),
          new(127, 220),
          new(130, 222),
          new(135, 222),
          new(139, 220),
      ];

      [Eval("queen mobility", 28)]
      public static readonly Score[] QueenMobility =
      [
          new(-32, -11),
          new( 38,   0),
          new(250,  73),
          new(219, 174),
          new(229, 262),
          new(229, 326),
          new(239, 319),
          new(241, 341),
          new(244, 355),
          new(247, 370),
          new(251, 370),
          new(254, 376),
          new(256, 385),
          new(259, 385),
          new(261, 391),
          new(262, 396),
          new(263, 402),
          new(263, 408),
          new(262, 417),
          new(266, 416),
          new(271, 419),
          new(274, 416),
          new(285, 418),
          new(307, 403),
          new(296, 425),
          new(412, 354),
          new(332, 398),
          new(253, 421),
      ];

      [Eval("rook half open file")]
      public static Score RookHalfOpenFile = new(11, 4);

      [Eval("rook open file")]
      public static Score RookOpenFile = new(27, 5);

      [Eval("king open file")]
      public static Score KingOpenFile = new(65, -8);

      [Eval("king half open file")]
      public static Score KingHalfOpenFile = new(23, -15);

      [Eval("king attack weights", 5)]
      public static readonly Score[] KingAttackWeights =
      [
          new(  0,   0),
          new(  9,  -4),
          new( 12,  -2),
          new( 24,  -8),
          new( 17,  11),
      ];

      [Eval("pawn shield", 4)]
      public static readonly Score[] PawnShield =
      [
          new(-35,   0),
          new( -5,  -5),
          new( 27, -14),
          new( 59, -30),
      ];

      [Eval("passed pawn", 7)]
      public static readonly Score[] PassedPawn =
      [
          new(  0,   0),
          new( -3,  11),
          new( -6,  15),
          new( -8,  36),
          new(-13,   6),
          new(-27,  78),
          new( 21, 107),
      ];

      [Eval("defended pawn", 8)]
      public static readonly Score[] DefendedPawn =
      [
          new(-24, -17),
          new( -9,  -4),
          new(  5,   9),
          new( 17,  28),
          new( 28,  48),
          new( 39,  58),
          new( 36,  49),
          new(  0,   0),
      ];

      [Eval("connected pawn", 9)]
      public static readonly Score[] ConnectedPawn =
      [
          new(-11, -10),
          new( -2,   3),
          new(  5,   8),
          new( 13,  22),
          new( 22,  23),
          new( 25,  76),
          new( 56, -22),
          new( -6,  -2),
          new(  0,   0),
      ];

      [Eval("isolated pawn", 8)]
      public static readonly Score[] IsolatedPawn =
      [
          new(  0,   4),
          new(  1,  12),
          new(  9,   7),
          new(  7,   9),
          new( 11,  12),
          new(  7,   5),
          new(  0,  11),
          new(  4,   4),
      ];

      [Eval("same color bishop pawns", 9)]
      public static readonly Score[] SameColorBishopPawns =
      [
          new(-96, -111),
          new(-95, -110),
          new(-93, -102),
          new(-89, -95),
          new(-84, -86),
          new(-81, -74),
          new(-75, -62),
          new(-70, -45),
          new(-61, -36),
      ];

      [Eval("friendly king pawn distance")]
      public static Score FriendlyKingPawnDistance = new(8, -11);

      [Eval("enemy king pawn distance")]
      public static Score EnemyKingPawnDistance = new(-4, 17);

      [Eval("bishop pair")]
      public static Score BishopPair = new(22, 58);

      [Eval("pawn push threats")]
      public static Score PawnPushThreats = new(18, 0);

      [Eval("pawn attacks")]
      public static Score PawnAttacks = new(43, 7);

      [Eval("free advance pawn")]
      public static Score FreeAdvancePawn = new(-15, 49);
   }
}
