using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Puffin.Tests
{
   public class TestPosition(string fen)
   {
      public string Fen { get; set; } = fen;
      public Dictionary<int, ulong> ExpectedMoves { get; set; } = [];
   }

   [TestClass]
   public class PerftTests
   {
      private List<TestPosition> TestPositions;
      private Engine Engine;

      [TestInitialize]
      public void Setup()
      {
         Engine = new();
         TestPositions = LoadTestPositions("perftsuite.epd");
      }

      private static List<TestPosition> LoadTestPositions(string filePath)
      {
         List<TestPosition> positions = [];

         foreach (string line in System.IO.File.ReadLines(filePath))
         {
            string[] fields = line.Split(';', StringSplitOptions.RemoveEmptyEntries);
            
            if (fields.Length < 2)
            {
               continue;
            }

            TestPosition position = new(fields[0].Trim());

            // Process each depth field (D1, D2, etc.)
            for (int i = 1; i < fields.Length; i++)
            {
               string depthField = fields[i].Trim();

               if (depthField.StartsWith('D'))
               {
                  string[] parts = depthField.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                  if (parts.Length == 2)
                  {
                     int depth = int.Parse(parts[0][1..]); // Remove 'D' and parse number
                     position.ExpectedMoves[depth] = ulong.Parse(parts[1]);
                  }
               }
            }

            positions.Add(position);
         }

         return positions;
      }

      [TestMethod]
      public void TestAllPositions()
      {
         int i = 1;

         foreach (TestPosition position in TestPositions)
         {
            Engine.SetPosition(position.Fen);
            Console.WriteLine($"Test {i} of {TestPositions.Count}");

            foreach (KeyValuePair<int, ulong> depthTest in position.ExpectedMoves.OrderBy(x => x.Key))
            {
               ulong actualMoves = Engine.Perft(depthTest.Key);
               Assert.AreEqual(depthTest.Value, actualMoves);
            }

            i++;
         }
      }

      [TestMethod]
      [DataRow("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")]
      [DataRow("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1")]
      [DataRow("4k3/8/8/8/8/8/8/4K2R w K - 0 1")]
      public void TestSpecificPosition(string fen)
      {
         TestPosition? position = TestPositions.FirstOrDefault(p => p.Fen == fen);
         Assert.IsNotNull(position, $"Test position not found: {fen}");

         Engine.SetPosition(position.Fen);

         foreach (KeyValuePair<int, ulong> depthTest in position.ExpectedMoves.OrderBy(x => x.Key))
         {
            ulong actualMoves = Engine.Perft(depthTest.Key);
            Assert.AreEqual(depthTest.Value, actualMoves);
         }
      }
   }
}
