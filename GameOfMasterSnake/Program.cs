using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GameOfMasterSnake.Enums;
using GameOfMasterSnake.Interfaces;
using GameOfMasterSnake.Snake;
using NeuralBotMasterFramework.Interfaces;
using NeuralBotMasterFramework.Logic.Algorithms;
using NeuralBotMasterFramework.Logic.PoolGenerators;

namespace GameOfMasterSnake
{
    internal static class Program
    {
        private const int TOTAL_NETWORKS = 10000;
        private const int NETWORKS_TO_KEEP = 100;
        private const int RANDOM_NETWORKS_PER_BREEDING = 10000;
        private const int INPUT_NODES = 9;
        private const int HIDDEN_NODS = 10;
        private const int HIDDEN_LAYERS = 2;
        private const int OUTPUT_NODES = 4;
        private const double MUTATION_CHANCE = 0.5;
        private const double MUTATION_RATE = 0.5;

        private const int FORCE_BREAK_AFTER_ITERATIONS = 50;

        private static readonly GeneticAlgorithm algorithm = new GeneticAlgorithm(TOTAL_NETWORKS, INPUT_NODES, HIDDEN_NODS, HIDDEN_LAYERS, OUTPUT_NODES);

        private const int HEIGHTS_GAME = 10;
        private const int WIDTH_GAME = 10;
        private const int INITIAL_SNAKE_LENGTH = 5;

        private static bool isPrintingMap;
        private static int nextRoundDelay = 0;

        private static readonly object setFitnessLock = new object();

        private static void Main()
        {
            algorithm.NetworksToKeep = NETWORKS_TO_KEEP;
            algorithm.MutationChance = MUTATION_CHANCE;
            algorithm.MutationRate = MUTATION_RATE;
            algorithm.RandomNetworkAmount = RANDOM_NETWORKS_PER_BREEDING;
            algorithm.PoolGenerator = new FitnessBasedPoolGenerator();

            int generation = 0;
            while (true)
            {
                Dictionary<IWeightedNetwork, SnakeGame> networkAndSnakeGame = SetupAlgorithmDictionary();
                Task[] tasks = new Task[networkAndSnakeGame.Count];
                int counter = 0;

                foreach (KeyValuePair<IWeightedNetwork, SnakeGame> networkAndGame in networkAndSnakeGame)
                {
                    KeyValuePair<IWeightedNetwork, SnakeGame> currentNetworkAndGame = networkAndGame;
                    tasks[counter++] = Task.Factory.StartNew(() => RunSnakeGame(currentNetworkAndGame));
                }
                Task.WaitAll(tasks);

                Console.SetCursorPosition(15, 5);
                Console.Write($"Generation: {generation}");

                algorithm.SortByFitness();

                double[] fitnesses = algorithm.GetFitnesses();
                for (int i = 0; i < 10; ++i)
                {
                    Console.SetCursorPosition(15, 7 + i);
                    Console.Write($"{i + 1}: {fitnesses[i].ToString().PadLeft(15)} ID: {algorithm.NetworksAndFitness.Where(x => x.Value == fitnesses[i]).Select(x => x.Key.ID).First()}");

                    Console.SetCursorPosition(15, 3);
                    Console.Write($"Average: {fitnesses.Average()}");
                }

                Console.SetCursorPosition(15, 17);
                Console.Write($"{fitnesses.Length}: {fitnesses.Min().ToString().PadLeft(15)} ID: {algorithm.NetworksAndFitness.Where(x => x.Value == fitnesses.LastOrDefault()).Select(x => x.Key.ID).First()}");

                ReplayWithBestNetwork();

                algorithm.BreedBestNetworks();
                ++generation;
            }
        }

        private static void RunSnakeGame(KeyValuePair<IWeightedNetwork, SnakeGame> networkAndGame, bool setfitness = true)
        {
            SnakeGame currentSnakeGame = networkAndGame.Value;
            IWeightedNetwork currentNetwork = networkAndGame.Key;

            double totalFitness = 0.0;
            const int TOTAL_PLAYS_PER_NETWORK = 10;
            for (int i = 0; i < TOTAL_PLAYS_PER_NETWORK; ++i)
            {
                int iterationsSinceLastFood = 0;
                int previousSnakeLength = currentSnakeGame.SnakeLength;
                while (!currentSnakeGame.IsPlayerGameOver())
                {
                    double[] input = GetNetworkInput(currentSnakeGame);
                    currentNetwork.SetInput(input);
                    currentNetwork.Propagate();
                    double[] output = currentNetwork.GetOutput();

                    double maxNumber = output.Max();
                    int index = output.ToList().IndexOf(maxNumber);

                    if (currentSnakeGame.Printer.IsPrintingMap)
                    {
                        ConsoleColor previousColor = Console.ForegroundColor;
                        for(int j = 0; j < output.Length; ++j)
                        {
                            if(j == index)
                            {
                                Console.ForegroundColor = ConsoleColor.DarkRed;
                            }
                            Console.SetCursorPosition(0, currentSnakeGame.Map.Height + j + 1);
                            Console.Write($"{Enum.GetValues(typeof(Direction)).GetValue(j)}");
                            Console.ForegroundColor = previousColor;
                        }
                    }

                    Direction newDirection = Enum.TryParse(index.ToString(), out Direction result)
                        ? result
                        : Direction.None;

                    if(newDirection == Direction.Up && currentSnakeGame.Printer.IsPrintingMap)
                    {

                    }

                    //string stringNumberToParse = (Math.Round(output[0], 0) % 4).ToString();
                    //string stringNumberToParse = Math.Round(4 / (1 + output[0]), 0).ToString();
                    //Direction newDirection = Enum.TryParse(stringNumberToParse, out Direction result)
                    //    ? result
                    //    : Direction.None;
                    currentSnakeGame.SetSnakeDirection(newDirection);

                    currentSnakeGame.NextRound();
                    if (nextRoundDelay > 0)
                    {
                        Thread.Sleep(nextRoundDelay);
                    }

                    ++iterationsSinceLastFood;
                    if (currentSnakeGame.SnakeLength > previousSnakeLength)
                    {
                        iterationsSinceLastFood = 0;
                        previousSnakeLength = currentSnakeGame.SnakeLength;
                    }
                    if (iterationsSinceLastFood >= FORCE_BREAK_AFTER_ITERATIONS)
                    {
                        break;
                    }
                }
                //totalFitness += (currentSnakeGame.SnakeLength - INITIAL_SNAKE_LENGTH) * currentSnakeGame.TotalMoves;
                totalFitness += ((currentSnakeGame.SnakeLength - INITIAL_SNAKE_LENGTH) * currentSnakeGame.TotalMoves) + currentSnakeGame.TotalMoves;
                //totalFitness += (currentSnakeGame.SnakeLength - INITIAL_SNAKE_LENGTH);
                networkAndGame.Value.InitializeGame();
                networkAndGame.Value.PlaceSnakeOnMap();
            }
            if (setfitness)
            {
                lock (setFitnessLock)
                {
                    algorithm.SetFitness(currentNetwork, totalFitness / TOTAL_PLAYS_PER_NETWORK);
                }
            }
        }

        private static Dictionary<IWeightedNetwork, SnakeGame> SetupAlgorithmDictionary()
        {
            Dictionary<IWeightedNetwork, SnakeGame> networkAndSnakeGame = new Dictionary<IWeightedNetwork, SnakeGame>();
            for (int i = 0; i < algorithm.NetworksAndFitness.Count; ++i)
            {
                SnakeGame game = new SnakeGame(HEIGHTS_GAME, WIDTH_GAME, INITIAL_SNAKE_LENGTH);
                game.Printer.IsPrintingMap = isPrintingMap;
                game.PlaceSnakeOnMap();
                networkAndSnakeGame.Add(algorithm.NetworksAndFitness.Select(x => x.Key).ElementAt(i), game);
            }
            return networkAndSnakeGame;
        }

        private static double[] GetNetworkInput(SnakeGame game)
        {
            double nextObstacleLeftDistance = GetObstacleDistance(game, Direction.Left);
            double nextObstacleUpDistance = GetObstacleDistance(game, Direction.Up);
            double nextObstacleDownDistance = GetObstacleDistance(game, Direction.Down);
            double nextObstacleRightDistance = GetObstacleDistance(game, Direction.Right);
            double foodToRight = ContainsFood(game, Direction.Right);
            double foodToLeft = ContainsFood(game, Direction.Left);
            double foodToDown = ContainsFood(game, Direction.Down);
            double foodToUp = ContainsFood(game, Direction.Up);
            double obstacleToRight = ObstacleNearby(game, Direction.Right);
            double obstacleToLeft = ObstacleNearby(game, Direction.Left);
            double obstacleToDown = ObstacleNearby(game, Direction.Down);
            double obstacleToUp = ObstacleNearby(game, Direction.Up);

            return new double[INPUT_NODES]
            {
                //game.SnakeXPos,
                //game.SnakeYPos,
                (double)game.Direction,
                nextObstacleLeftDistance,
                nextObstacleUpDistance,
                nextObstacleDownDistance,
                nextObstacleRightDistance,
                //obstacleToRight,
                //obstacleToLeft,
                //obstacleToUp,
                //obstacleToDown,
                foodToRight,
                foodToLeft,
                foodToDown,
                foodToUp,
                //game.SnakeXPos - game.FoodXPos,
                //game.SnakeYPos - game.FoodYPos,
            };
        }

        private static int GetObstacleDistance(SnakeGame game, Direction direction)
        {
            ITile tile;
            switch (direction)
            {
                case Direction.Left:
                    tile = game.Map.Tiles.Where(x
                       => x.Value == TileValues.Snake
                       && x.XPos < game.SnakeXPos
                       && x.YPos == game.SnakeYPos)
                            .OrderByDescending(x => x.XPos)
                            .FirstOrDefault();
                    return tile == null
                        ? game.SnakeXPos + 1
                        : game.SnakeXPos - tile.XPos;
                case Direction.Right:
                    tile = game.Map.Tiles.Where(x
                        => x.Value == TileValues.Snake
                        && x.XPos > game.SnakeXPos
                        && x.YPos == game.SnakeYPos)
                            .OrderBy(x => x.XPos)
                            .FirstOrDefault();
                    return tile == null
                        ? game.Map.Width - game.SnakeXPos
                        : tile.XPos - game.SnakeXPos;
                case Direction.Up:
                    tile = game.Map.Tiles.Where(x
                        => x.Value == TileValues.Snake
                        && x.XPos == game.SnakeXPos
                        && x.YPos < game.SnakeYPos)
                            .OrderByDescending(x => x.YPos)
                            .FirstOrDefault();
                    return tile == null
                        ? game.SnakeYPos + 1
                        : game.SnakeYPos - tile.YPos;
                case Direction.Down:
                    tile = game.Map.Tiles.Where(x
                        => x.Value == TileValues.Snake
                        && x.XPos == game.SnakeXPos
                        && x.YPos > game.SnakeYPos)
                            .OrderBy(x => x.YPos)
                            .FirstOrDefault();
                    return tile == null
                        ? game.Map.Height - game.SnakeYPos
                        : tile.YPos - game.SnakeYPos;
                default:
                    return -1;
            }
        }

        private static double ContainsFood(SnakeGame game, Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return (game.SnakeXPos == game.FoodXPos
                            && game.SnakeYPos > game.FoodYPos)
                        ? 5
                        : 0;
                case Direction.Down:
                    return (game.SnakeXPos == game.FoodXPos
                            && game.SnakeYPos < game.FoodYPos)
                        ? 5
                        : 0;
                case Direction.Left:
                    return (game.SnakeXPos > game.FoodXPos
                            && game.SnakeYPos == game.FoodYPos)
                        ? 5
                        : 0;
                case Direction.Right:
                    return (game.SnakeXPos < game.FoodXPos
                            && game.SnakeYPos == game.FoodYPos)
                        ? 5
                        : 0;
                default:
                    return -1;
            }
        }

        private static double ObstacleNearby(SnakeGame game, Direction direction)
        {
            int snakeXPos = game.SnakeXPos;
            int snakeYPos = game.SnakeYPos;
            ITile tile;
            switch (direction)
            {
                case Direction.Up:
                    tile = game.Map.GetTile(snakeXPos, snakeYPos - 1);
                    return tile?.Value == TileValues.Snake
                        || game.SnakeYPos == 0
                            ? 1
                            : 0;
                case Direction.Down:
                    tile = game.Map.GetTile(snakeXPos, snakeYPos + 1);
                    return tile?.Value == TileValues.Snake
                        || game.SnakeYPos == game.Map.Height - 1
                            ? 1
                            : 0;
                case Direction.Left:
                    tile = game.Map.GetTile(snakeXPos - 1, snakeYPos);
                    return tile?.Value == TileValues.Snake
                        || game.SnakeXPos == 0
                            ? 1
                            : 0;
                case Direction.Right:
                    tile = game.Map.GetTile(snakeXPos + 1, snakeYPos);
                    return tile?.Value == TileValues.Snake
                        || game.SnakeXPos == game.Map.Width + 1
                            ? 1
                            : 0;
                default:
                    return -1;
            }
        }

        private static void ReplayWithBestNetwork()
        {
            SnakeGame game = new SnakeGame(HEIGHTS_GAME, WIDTH_GAME, INITIAL_SNAKE_LENGTH);
            game.Printer.IsPrintingMap = true;
            game.InitializeGame();
            game.PlaceSnakeOnMap();
            KeyValuePair<IWeightedNetwork, SnakeGame> bestnetworkAndGame = new KeyValuePair<IWeightedNetwork, SnakeGame>(
                algorithm.NetworksAndFitness[0].Key,
                game);

            int previousNextRoundDelay = nextRoundDelay;
            nextRoundDelay = 100;
            RunSnakeGame(bestnetworkAndGame, false);
            nextRoundDelay = previousNextRoundDelay;
        }
    }
}
