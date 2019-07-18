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
        private const int TOTAL_NETWORKS = 1000;
        private const int NETWORKS_TO_KEEP = 10;
        private const int RANDOM_NETWORKS_PER_BREEDING = 100;
        private const int INPUT_NODES = 9;
        private const int HIDDEN_NODS = 10;
        private const int HIDDEN_LAYERS = 5;
        private const int OUTPUT_NODES = 4;
        private const double MUTATION_CHANCE = 0.1;
        private const double MUTATION_RATE = 0.1;

        private static readonly GeneticAlgorithm algorithm = new GeneticAlgorithm(TOTAL_NETWORKS, INPUT_NODES, HIDDEN_NODS, HIDDEN_LAYERS, OUTPUT_NODES);

        private const int HEIGHTS_GAME = 10;
        private const int WIDTH_GAME = 10;
        private const int INITIAL_SNAKE_LENGTH = 5;

        private static bool isPrintingMap = false;
        private static int nextRoundDelay = 0;

        private static object setFitnessLock = new object();
        private static void Main()
        {
            algorithm.NetworksToKeep = NETWORKS_TO_KEEP;
            algorithm.MutationChance = MUTATION_CHANCE;
            algorithm.MutationRate = MUTATION_RATE;
            algorithm.RandomNetworkAmount = RANDOM_NETWORKS_PER_BREEDING;
            algorithm.PoolGenerator = new FitnessBasedPoolGenerator();

            const int FORCE_BREAK_AFTER_ITERATIONS = 50;

            int generation = 0;
            while (true)
            {
                //if (generation % 100 == 0)
                //{
                //    isPrintingMap = true;
                //    nextRoundDelay = 100;
                //}
                Console.SetCursorPosition(15, 5);
                Console.Write($"Generation: {generation}");
                Dictionary<IWeightedNetwork, SnakeGame> networkAndSnakeGame = SetupAlgorithmDictionary();
                Task[] tasks = new Task[networkAndSnakeGame.Count];
                int counter = 0;

                foreach (KeyValuePair<IWeightedNetwork, SnakeGame> networkAndGame in networkAndSnakeGame)
                {
                    KeyValuePair<IWeightedNetwork, SnakeGame> currentNetworkAndGame = networkAndGame;
                    tasks[counter++] = Task.Factory.StartNew(() =>
                    {

                        //if (counter > 10)
                        //{
                        //    isPrintingMap = false;
                        //    nextRoundDelay = 0;
                        //}
                        //currentNetworkAndGame.Value.Printer.IsPrintingMap = isPrintingMap;
                        //Console.SetCursorPosition(15, 6);
                        //Console.Write($"Current Network-Index: {counter.ToString().PadLeft(5)}");
                        //++counter;
                        SnakeGame currentSnakeGame = currentNetworkAndGame.Value;
                        IWeightedNetwork currentNetwork = currentNetworkAndGame.Key;

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
                                Direction newDirection = Enum.TryParse(index.ToString(), out Direction result)
                                    ? result
                                    : Direction.None;

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
                            //isPrintingMap = false;
                            nextRoundDelay = 0;
                            totalFitness += (currentSnakeGame.SnakeLength - INITIAL_SNAKE_LENGTH) * currentSnakeGame.TotalMoves;
                            currentNetworkAndGame.Value.InitializeGame();
                            currentNetworkAndGame.Value.PlaceSnakeOnMap();
                        }
                        lock (setFitnessLock)
                        {
                            algorithm.SetFitness(currentNetwork, totalFitness / TOTAL_PLAYS_PER_NETWORK);
                        }
                    });
                }
                Task.WaitAll(tasks);
                algorithm.SortByFitness();
                double[] fitnesses = algorithm.GetFitnesses();
                for (int i = 0; i < 10; ++i)
                {
                    Console.SetCursorPosition(15, 7 + i);
                    Console.Write($"{i + 1}: {fitnesses[i].ToString().PadLeft(15)} ID: {algorithm.NetworksAndFitness.Where(x => x.Value == fitnesses[i]).Select(x => x.Key.ID).First()}");
                }
                algorithm.BreedBestNetworks();
                ++generation;
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
            bool tileFound = false;
            int value = 0;
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
                        ? 1
                        : game.SnakeXPos - tile.XPos;
                case Direction.Right:
                    tile = game.Map.Tiles.Where(x
                        => x.Value == TileValues.Snake
                        && x.XPos > game.SnakeXPos
                        && x.YPos == game.SnakeYPos)
                            .OrderBy(x => x.XPos)
                            .FirstOrDefault();
                    return tile == null
                        ? 1
                        : tile.XPos - game.SnakeXPos;
                case Direction.Up:
                    tile = game.Map.Tiles.Where(x
                        => x.Value == TileValues.Snake
                        && x.XPos == game.SnakeXPos
                        && x.YPos < game.SnakeYPos)
                            .OrderByDescending(x => x.YPos)
                            .FirstOrDefault();
                    return tile == null
                        ? 1
                        : game.SnakeYPos - tile.YPos;
                case Direction.Down:
                    tile = game.Map.Tiles.Where(x
                        => x.Value == TileValues.Snake
                        && x.XPos == game.SnakeXPos
                        && x.YPos > game.SnakeYPos)
                            .OrderBy(x => x.YPos)
                            .FirstOrDefault();
                    return tile == null
                        ? 1
                        : tile.YPos - game.SnakeYPos;

                    //for (int xPos = game.SnakeXPos - 1; xPos >= 0; --xPos)
                    //{
                    //    if (game.Map.GetTile(xPos, game.SnakeYPos).Value == TileValues.Snake)
                    //    {
                    //        value = game.SnakeXPos - xPos - 1;
                    //        value = value >= 0 ? value : -value;
                    //        tileFound = true;
                    //        break;
                    //    }
                    //}
                    //if (!tileFound)
                    //{
                    //    value = game.SnakeXPos;
                    //}
                    //break;
                    //case Direction.Up:
                    //    for (int yPos = game.SnakeYPos - 1; yPos >= 0; --yPos)
                    //    {
                    //        if (game.Map.GetTile(game.SnakeXPos, yPos).Value == TileValues.Snake)
                    //        {
                    //            value = game.SnakeYPos - yPos - 1;
                    //            value = value >= 0 ? value : -value;
                    //            tileFound = true;
                    //            break;
                    //        }
                    //    }
                    //    if (!tileFound)
                    //    {
                    //        value = game.SnakeYPos;
                    //    }
                    //    break;
                    //case Direction.Right:
                    //    for (int xPos = game.SnakeXPos + 1; xPos < game.Map.Width; ++xPos)
                    //    {
                    //        if (game.Map.GetTile(xPos, game.SnakeYPos).Value == TileValues.Snake)
                    //        {
                    //            value = game.SnakeXPos - xPos - 1;
                    //            value = value >= 0 ? value : -value;
                    //            tileFound = true;
                    //            break;
                    //        }
                    //    }
                    //    if (!tileFound)
                    //    {
                    //        value = game.Map.Width - 1 - game.SnakeXPos;
                    //    }
                    //    break;
                    //case Direction.Down:
                    //    for (int yPos = game.SnakeYPos + 1; yPos < game.Map.Height; ++yPos)
                    //    {
                    //        if (game.Map.GetTile(game.SnakeXPos, yPos).Value == TileValues.Snake)
                    //        {
                    //            value = game.SnakeYPos - yPos - 1;
                    //            value = value >= 0 ? value : -value;
                    //            tileFound = true;
                    //            break;
                    //        }
                    //    }
                    //    if (!tileFound)
                    //    {
                    //        value = game.Map.Height - 1 - game.SnakeYPos;
                    //    }
                    //    break;
            }
            return value;
        }

        private static double ContainsFood(SnakeGame game, Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return (game.SnakeXPos == game.FoodXPos
                            && game.SnakeYPos > game.FoodYPos)
                        ? 1
                        : 0;
                case Direction.Down:
                    return (game.SnakeXPos == game.FoodXPos
                            && game.SnakeYPos < game.FoodYPos)
                        ? 1
                        : 0;
                case Direction.Left:
                    return (game.SnakeXPos > game.FoodXPos
                            && game.SnakeYPos == game.FoodYPos)
                        ? 1
                        : 0;
                case Direction.Right:
                    return (game.SnakeXPos < game.FoodXPos
                            && game.SnakeYPos == game.FoodYPos)
                        ? 1
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
    }
}
