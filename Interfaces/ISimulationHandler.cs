using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeuralBotMasterFramework.Interfaces;

namespace GameOfMasterSnake.Interfaces
{
    public interface ISimulationHandler
    {
        IGeneticAlgorithm Algorithm { get; }
        Type SimulationType { get; }

        void SetAlgorithm(IGeneticAlgorithm algorithm);
        void SetSimulationType(Type type);
        void InitializeSimulation();
    }
}
