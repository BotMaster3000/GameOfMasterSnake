using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GameOfMasterSnake.Enums;
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
        private const int RANDOM_NETWORKS_PER_BREEDING = 100;
        private const int INPUT_NODES = 9;
        private const int HIDDEN_NODS = 10;
        private const int HIDDEN_LAYERS = 2;
        private const int OUTPUT_NODES = 1;
        private const double MUTATION_CHANCE = 0.5;
        private const double MUTATION_RATE = 0.001;

        private static readonly GeneticAlgorithm algorithm = new GeneticAlgorithm(TOTAL_NETWORKS, INPUT_NODES, HIDDEN_NODS, HIDDEN_LAYERS, OUTPUT_NODES);

        private const int HEIGHTS_GAME = 10;
        private const int WIDTH_GAME = 10;
        private const int INITIAL_SNAKE_LENGTH = 5;

        private const bool IS_PRINTING_MAP = false;
        private const int NEXT_ROUND_DELAY = 0;

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
                Console.SetCursorPosition(15, 5);
                Console.Write($"Generation: {generation}");
                Dictionary<IWeightedNetwork, SnakeGame> networkAndSnakeGame = SetupAlgorithmDictionary();
                int counter = 0;
                foreach (KeyValuePair<IWeightedNetwork, SnakeGame> networkAndGame in networkAndSnakeGame)
                {
                    Console.SetCursorPosition(15, 6);
                    Console.Write($"Current Network-Index: {counter.ToString().PadLeft(5)}");
                    ++counter;
                    SnakeGame currentSnakeGame = networkAndGame.Value;
                    IWeightedNetwork currentNetwork = networkAndGame.Key;

                    currentSnakeGame.PlaceSnakeOnMap();

                    int iterationsSinceLastFood = 0;
                    int previousSnakeLength = currentSnakeGame.SnakeLength;

                    while (!currentSnakeGame.IsPlayerGameOver())
                    {
                        double[] input = GetNetworkInput(currentSnakeGame);
                        currentNetwork.SetInput(input);
                        currentNetwork.Propagate();
                        double[] output = currentNetwork.GetOutput();

                        currentSnakeGame.SetSnakeDirection(
                            Enum.TryParse(
                                (Math.Round(output[0], 0) % 4).ToString(), out Direction result) ? result : Direction.None);
                        currentSnakeGame.NextRound();
                        if (NEXT_ROUND_DELAY > 0)
                        {
                            Thread.Sleep(NEXT_ROUND_DELAY);
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
                    algorithm.SetFitness(currentNetwork, currentSnakeGame.SnakeLength * currentSnakeGame.TotalMoves);
                }
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
                game.Printer.IsPrintingMap = IS_PRINTING_MAP;
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

            return new double[INPUT_NODES]
            {
                game.SnakeXPos,
                game.SnakeYPos,
                (double)game.Direction,
                nextObstacleLeftDistance,
                nextObstacleUpDistance,
                nextObstacleDownDistance,
                nextObstacleRightDistance,
                game.SnakeXPos - game.FoodXPos,
                game.SnakeYPos - game.FoodYPos,
            };
        }

        private static int GetObstacleDistance(SnakeGame game, Direction direction)
        {
            bool tileFound = false;
            int value = 0;
            switch (direction)
            {
                case Direction.Left:
                    for (int xPos = game.SnakeXPos - 1; xPos >= 0; --xPos)
                    {
                        if (game.Map.GetTile(xPos, game.SnakeYPos).Value == TileValues.Snake)
                        {
                            value = game.SnakeXPos - xPos;
                            value = value >= 0 ? value : -value;
                            tileFound = true;
                            break;
                        }
                    }
                    if (!tileFound)
                    {
                        value = game.SnakeXPos;
                    }
                    break;
                case Direction.Up:
                    for (int yPos = game.SnakeYPos - 1; yPos >= 0; --yPos)
                    {
                        if (game.Map.GetTile(game.SnakeXPos, yPos).Value == TileValues.Snake)
                        {
                            value = game.SnakeYPos - yPos;
                            value = value >= 0 ? value : -value;
                            tileFound = true;
                            break;
                        }
                    }
                    if (!tileFound)
                    {
                        value = game.SnakeYPos;
                    }
                    break;
                case Direction.Right:
                    for (int xPos = game.SnakeXPos + 1; xPos < game.Map.Width; ++xPos)
                    {
                        if (game.Map.GetTile(xPos, game.SnakeYPos).Value == TileValues.Snake)
                        {
                            value = game.SnakeXPos - xPos;
                            value = value >= 0 ? value : -value;
                            tileFound = true;
                            break;
                        }
                    }
                    if (!tileFound)
                    {
                        value = game.Map.Width - 1 - game.SnakeXPos;
                    }
                    break;
                case Direction.Down:
                    for (int yPos = game.SnakeYPos + 1; yPos < game.Map.Height; ++yPos)
                    {
                        if (game.Map.GetTile(game.SnakeXPos, yPos).Value == TileValues.Snake)
                        {
                            value = game.SnakeYPos - yPos;
                            value = value >= 0 ? value : -value;
                            tileFound = true;
                            break;
                        }
                    }
                    if (!tileFound)
                    {
                        value = game.Map.Height - 1 - game.SnakeYPos;
                    }
                    break;
            }
            return value;
        }
    }
}
