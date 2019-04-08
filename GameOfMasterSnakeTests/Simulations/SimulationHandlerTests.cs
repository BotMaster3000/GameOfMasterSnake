using Microsoft.VisualStudio.TestTools.UnitTesting;
using GameOfMasterSnake.Simulations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameOfMasterSnake.Interfaces;
using NeuralBotMasterFramework.Interfaces;
using NeuralBotMasterFramework.Logic.Algorithms;
using GameOfMasterSnake.Snake;

namespace GameOfMasterSnake.Simulations.Tests
{
    [TestClass]
    public class SimulationHandlerTests
    {
        private const int TOTAL_NETWORKS = 100;
        private const int INPUT_NODES = 10;
        private const int HIDDEN_LAYERS = 10;
        private const int HIDDEN_NODES_PER_LAYER = 10;
        private const int OUTPUT_NODES = 5;
        readonly IGeneticAlgorithm Algorithm = new GeneticAlgorithm(TOTAL_NETWORKS, INPUT_NODES, HIDDEN_NODES_PER_LAYER, HIDDEN_LAYERS, OUTPUT_NODES);
        private readonly Type SimulationType = typeof(SnakeGame);

        [TestMethod]
        public void ConstructorTest_NoParameter()
        {
            ISimulationHandler handler = new SimulationHandler();
            Assert.IsNull(handler.Algorithm);
            Assert.IsNull(handler.SimulationType);
        }

        [TestMethod]
        public void ConstructorTest_IAlgorithmAndSimulationType_AsParameter()
        {
            ISimulationHandler handler = new SimulationHandler(Algorithm, SimulationType);
            Assert.AreEqual(Algorithm, handler.Algorithm);
            Assert.AreEqual(SimulationType, handler.SimulationType);
        }

        [TestMethod]
        public void SetAlgorithmTest()
        {
            ISimulationHandler handler = new SimulationHandler();
            handler.SetAlgorithm(Algorithm);
            Assert.AreEqual(Algorithm, handler.Algorithm);
        }

        [TestMethod]
        public void SetSimulationTypeTest()
        {
            ISimulationHandler handler = new SimulationHandler();
            handler.SetSimulationType(SimulationType);
            Assert.AreEqual(SimulationType, handler.SimulationType);
        }

        [TestMethod]
        public void InitializeSimulationTest()
        {
            ISimulationHandler handler = new SimulationHandler(Algorithm, SimulationType);
            handler.InitializeSimulation();
        }
    }
}