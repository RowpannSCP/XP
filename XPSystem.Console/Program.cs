namespace XPSystem.Console
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using XPSystem.API;
    using XPSystem.Config.Events;

    internal class Program
    {
        public static void Main(string[] args)
        {
            Assembly.Load("Assembly-CSharp"); // doesn't like -Publicized for automatic loading

            XPAPI.LogDebug = s => Console.WriteLine($"[DEBUG] {s}");
            XPAPI.LogInfo = s => Console.WriteLine($"[INFO] {s}");
            XPAPI.LogWarn = s => Console.WriteLine($"[WARN] {s}");
            XPAPI.LogError = s => Console.WriteLine($"[ERROR] {s}");

#region Serialization
            int count = 0;
            int total = XPECManager.NeededFiles.Files.Count;
            int characters = 0;
            int errors = 0;

            Stopwatch stopwatchSerialize = new();
            Stopwatch stopwatchDeserialize = new();

            foreach (var needed in XPECManager.NeededFiles)
            {
                string text1 = "uninitialized";
                string text2 = "uninitialized";
                bool print1 = false;
                bool print2 = false;
                count++;

                Console.ForegroundColor = ConsoleColor.White;

                Console.Write("Serializing: ");
                Console.Write(needed.Key);
                Console.Write(" => ");
                Console.Write(needed.Value.GetType().Name);

                Console.Write(" (");
                Console.Write(count);
                Console.Write('/');
                Console.Write(total);
                Console.Write(')');

                Console.Write("... ");

                Console.ResetColor();

                try
                {
                    stopwatchSerialize.Restart();
                    text1 = XPAPI.Serializer.Serialize(needed.Value);
                    stopwatchSerialize.Stop();

                    characters = text1.Length;

                    stopwatchDeserialize.Restart();
                    object deserialized = XPAPI.Deserializer.Deserialize(text1, needed.Value.GetType()) 
                                          ?? throw new Exception("Deserialized object is null.");
                    stopwatchDeserialize.Stop();

                    text2 = XPAPI.Serializer.Serialize(deserialized);
                    if (text1 != text2)
                    {
                        print1 = true;
                        print2 = true;
                        throw new Exception("Serialized text is not equal to original text.");
                    }
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;

                    Console.Write("Failed: ");
                    Console.WriteLine(e);

                    Console.ResetColor();

                    if (print1)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine("Original:");

                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(text1);

                        Console.ResetColor();
                    }

                    if (print2)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkBlue;
                        Console.WriteLine("Deserialized:");

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(text2);

                        Console.ResetColor();
                    }

                    errors++;
                    continue;
                }

                Console.ForegroundColor = ConsoleColor.Green;

                Console.Write("Done ");
                Console.Write(characters);
                Console.Write(" in ");
                Console.Write(stopwatchSerialize.ElapsedMilliseconds);
                Console.Write("ms + ");
                Console.Write(stopwatchDeserialize.ElapsedMilliseconds);
                Console.WriteLine("ms");

                Console.ResetColor();
            }
#endregion
#region Level calculation
            Random random = new();
            Stopwatch stopwatchLevel = new();
            Stopwatch stopwatchLevel2 = new();
            Stopwatch stopwatchXP = new();

            LevelCalculator.Init();
            // initial calls very slow
            LevelCalculator.GetLevel(0);
            LevelCalculator.GetXP(0);

            Console.ForegroundColor = ConsoleColor.White;

            int level = -1;
            int xp = -1;
            foreach ((int level, int xp) kvp in new[]
                     {
                         (0, 0),
                         (1, 76),
                         (446, 121783),
                         (random.Next(0, 100), -1),
                         (random.Next(100, 1000), -1),
                         (random.Next(1000, 1000), -1),
                         (-1, random.Next(1, 100) * 100),
                         (-1, random.Next(10, 1000) * 100),
                         (-1, random.Next(10000, 9999999) * 100),
                     })
            {
                Console.ForegroundColor = ConsoleColor.White;

                Console.Write("Validating level ");
                Console.Write(kvp.level);
                Console.Write(" <=> ");
                Console.Write(kvp.xp);

                Console.Write("... ");

                Console.ResetColor();

                try
                {
                    if (kvp.xp != -1)
                    {
                        stopwatchLevel.Restart();
                        level = LevelCalculator.GetLevel(kvp.xp);
                        stopwatchLevel.Stop();

                        if (kvp.level != -1 && level != kvp.level)
                            throw new Exception("Level mismatch.");
                    }
                    else
                    {
                        level = kvp.level;
                    }

                    stopwatchXP.Restart();
                    xp = LevelCalculator.GetXP(level);
                    stopwatchXP.Stop();

                    if (kvp.level != -1 && kvp.xp != -1 && xp != kvp.xp)
                        throw new Exception("XP mismatch.");

                    stopwatchLevel2.Restart();
                    int level2 = LevelCalculator.GetLevel(xp);
                    stopwatchLevel2.Stop();

                    if (level2 != level)
                        throw new Exception("Recalculated level mismatch.");
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;

                    Console.Write("Failed (");
                    Console.Write(level);
                    Console.Write(" <=> ");
                    Console.Write(xp);
                    Console.Write("): ");
                    Console.WriteLine(e);

                    Console.ResetColor();

                    errors++;
                    continue;
                }

                Console.ForegroundColor = ConsoleColor.Green;

                Console.Write("Done in ");
                Console.Write(stopwatchLevel.ElapsedMilliseconds);
                Console.Write("ms + ");
                Console.Write(stopwatchXP.ElapsedMilliseconds);
                Console.Write("ms + ");
                Console.Write(stopwatchLevel2.ElapsedMilliseconds);
                Console.WriteLine("ms");

                Console.ResetColor();
            }
#endregion
            if (errors > 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write("Failed ");
                Console.Write(errors);
                Console.WriteLine(" times");
                Console.ResetColor();

                Environment.Exit(1);
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("All tests completed successfully.");
            Console.ResetColor();

            Console.WriteLine("Version:");
            Console.Write(XPSystem.Main.VersionString);
        }
    }
}
