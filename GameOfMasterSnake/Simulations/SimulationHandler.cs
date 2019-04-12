using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameOfMasterSnake.Interfaces;
using NeuralBotMasterFramework.Interfaces;

namespace GameOfMasterSnake.Simulations
{
    public class SimulationHandler : ISimulationHandler
    {
        public IGeneticAlgorithm Algorithm { get; private set; }
        public Type SimulationType { get; private set; }

        private Dictionary<IGeneticAlgorithm, object> AlgorithmAndSimulationType { get; } = new Dictionary<IGeneticAlgorithm, object>();

        public SimulationHandler() { }

        public SimulationHandler(IGeneticAlgorithm algorithm, Type simulationType)
        {
            Algorithm = algorithm;
            SimulationType = simulationType;
        }

        public void SetAlgorithm(IGeneticAlgorithm algorithm)
        {
            Algorithm = algorithm;
        }

        public void SetSimulationType(Type type)
        {
            SimulationType = type;
        }

        public void InitializeSimulation()
        {
            AlgorithmAndSimulationType.Clear();
            for (int i = 0; i < Algorithm.TotalNetworks; ++i)
            {
                IWeightedNetwork network = Algorithm.NetworksAndFitness.Select(x => x.Key).ElementAt(i);
                //AlgorithmAndSimulationType.Add(network, SimulationType.());
            }
            throw new NotImplementedException();
        }
    }
}
