namespace Jock.NR.Core
{
    using System;

    using Jock.NR.BL;
    using System.Drawing;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Входной класс.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Входная точка.
        /// </summary>
        /// <param name="args">Аргументы командной строки.</param>
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;

            var imageStubPath = $@"{ResourcesConst.RESOURCES_PATH}1.bmp";
            var resizedImage = string.Empty;

            var fileName = "test";

            // Размерность изображения.
            var width = 10;
            var height = 10;

            // Сжатие изображение и извлечение битов данных.
            using (var image = new Bitmap(imageStubPath))
                BL.ImageConverter.ImageResize(image, width, height, fileName);

            if (BL.ImageConverter.PathToResizedFile != string.Empty && BL.ImageConverter.PathToResizedFile != null)
                resizedImage = BL.ImageConverter.PathToResizedFile;

            Console.WriteLine(BL.ImageConverter.ConvertBitmapToInt(resizedImage, fileName));

            var answer = string.Empty;
            var breakFlag = false;

            var data = BL.ImageConverter.LoadData();

            var inputLayer = new InputLayer(data.Length);
            var inputData = inputLayer.FillInputLayer(data);

            var choosenLearningType = LearningType.None;

            Console.WriteLine($"\nHello! What do you want? ((L)earning/(R)ecognize)");

            do
            {
                answer = Console.ReadLine();

                if (DialogConstants.LEARNING_RESULT.Any(result => result.Contains(answer)) ||
                    DialogConstants.RECOGNIZE_RESULT.Any(result => result.Contains(answer)))
                    breakFlag = true;
                else
                    Console.WriteLine($"\nI'm don't understand..." +
                        $"\nRepeat please!\n");
            } while (!breakFlag);

            if (DialogConstants.LEARNING_RESULT.Any(result => result == answer))
            {
                var learningType = string.Empty;
                var typeBreakFlag = false;

                Console.WriteLine($"\nPlease choose type of learning. ((B)ackpropagation/(N)euroevolution)");

                do
                {
                    learningType = Console.ReadLine();

                    if (DialogConstants.BACKPROPAGATION_RESULT.Any(result => result.Contains(learningType)) ||
                        DialogConstants.NEUROEVOLUTION_RESULT.Any(result => result.Contains(learningType)))
                    {
                        typeBreakFlag = true;
                    }
                    else
                        Console.WriteLine($"\nI'm don't understand..." +
                            $"\nRepeat please!\n");

                } while (!typeBreakFlag);

                if (DialogConstants.BACKPROPAGATION_RESULT.Any(result => result == learningType))
                    choosenLearningType = LearningType.Backpropagation;
                else
                    choosenLearningType = LearningType.Neuroevolution;

                Console.WriteLine($"\nStart of learning process...");

                // Часть с обучением.

                var loadWeightsAnswer = string.Empty;
                var loadBreakFlag = false;

                do
                {
                    Console.WriteLine("Do you want to load saved weights? (Y/N)");
                    loadWeightsAnswer = Console.ReadLine();

                    if (DialogConstants.YES_RESULT.Any(result => result.Contains(loadWeightsAnswer)) ||
                        DialogConstants.NO_RESULT.Any(result => result.Contains(loadWeightsAnswer)))
                        loadBreakFlag = true;
                } while (!loadBreakFlag);

                if (DialogConstants.NO_RESULT.Any(result => result == loadWeightsAnswer))
                {
                    const int epochStubCount = 3;
                    var allInputData = new List<List<double>>
                    {
                        inputData
                    };

                    var isNeedInitialize = true;
                    var learning = new Learning(epochStubCount, allInputData, isNeedInitialize, 1, choosenLearningType);
                }

                if (DialogConstants.YES_RESULT.Any(result => result == loadWeightsAnswer))
                {
                    const int epochStubCount = 3;
                    var allInputData = new List<List<double>>
                    {
                        inputData
                    };

                    var pathOfOutputNeuron = $@"{ResourcesConst.RESOURCES_PATH}outputLayerWeights.txt";
                    var weightsOfOutputNeuron = WeightWorker.LoadNeuronWeights(pathOfOutputNeuron);

                    if (!weightsOfOutputNeuron.Any())
                        Console.WriteLine($"{DialogConstants.RESULT_FAIL}");
                    else
                        Console.WriteLine($"{DialogConstants.RESULT_SUCCES}");

                    var pathOfHiddenLayer = $@"{ResourcesConst.RESOURCES_PATH}hiddenLayerWeights.txt";
                    var weightsOfHiddenLayer = WeightWorker.LoadLayerWeights(pathOfHiddenLayer);

                    var isNeedInitialize = true;
                    var learning = new Learning(epochStubCount, allInputData, isNeedInitialize, 1, choosenLearningType, 
                        true, weightsOfHiddenLayer, weightsOfOutputNeuron);
                }
            }
            else
            {
                Console.WriteLine($"\nStart of recognizing process...\n");

                // Часть с распозованием.

                Console.WriteLine($"Load: {ResourcesConst.OUTPUT_LAYER_WEIGHTS_NAME}");

                var pathOfOutputNeuron = $@"{ResourcesConst.RESOURCES_PATH}{ResourcesConst.OUTPUT_LAYER_WEIGHTS_NAME}.txt";
                var weightsOfOutputNeuron = WeightWorker.LoadNeuronWeights(pathOfOutputNeuron);

                if (!weightsOfOutputNeuron.Any())
                    Console.WriteLine($"{DialogConstants.RESULT_FAIL}\n");
                else
                    Console.WriteLine($"{DialogConstants.RESULT_SUCCES}\n");

                Console.WriteLine($"Load: {ResourcesConst.HIDDEN_LAYER_WEIGHTS_NAME}");

                var pathOfHiddenLayer = $@"{ResourcesConst.RESOURCES_PATH}{ResourcesConst.HIDDEN_LAYER_WEIGHTS_NAME}.txt";
                var weightsOfHiddenLayer = WeightWorker.LoadLayerWeights(pathOfHiddenLayer);

                if (!weightsOfHiddenLayer.Any())
                    Console.WriteLine($"{DialogConstants.RESULT_FAIL}\n");
                else
                    Console.WriteLine($"{DialogConstants.RESULT_SUCCES}\n");

                Console.WriteLine($"Load: {ResourcesConst.EVOLUTION_WEIGHTS_NAME}{ResourcesConst.OUTPUT_LAYER_WEIGHTS_NAME}");

                var pathOfEvolutionOutputNeuron = $@"{ResourcesConst.RESOURCES_PATH}{ResourcesConst.EVOLUTION_WEIGHTS_NAME}"+
                    $"{ResourcesConst.OUTPUT_LAYER_WEIGHTS_NAME}.txt";

                var weightsOfEvolutionOutputNeuron = WeightWorker.LoadNeuronWeights(pathOfEvolutionOutputNeuron);

                if (!weightsOfEvolutionOutputNeuron.Any())
                    Console.WriteLine($"{DialogConstants.RESULT_FAIL}\n");
                else
                    Console.WriteLine($"{DialogConstants.RESULT_SUCCES}\n");

                Console.WriteLine($"Load: {ResourcesConst.EVOLUTION_WEIGHTS_NAME}{ResourcesConst.HIDDEN_LAYER_WEIGHTS_NAME}");

                var pathOfEvolutionHiddenLayer = $@"{ResourcesConst.RESOURCES_PATH}{ResourcesConst.EVOLUTION_WEIGHTS_NAME}"+
                    $"{ResourcesConst.HIDDEN_LAYER_WEIGHTS_NAME}.txt";

                var weightsOfEvolutionHiddenLayer = WeightWorker.LoadLayerWeights(pathOfEvolutionHiddenLayer);

                if (!weightsOfEvolutionHiddenLayer.Any())
                    Console.WriteLine($"{DialogConstants.RESULT_FAIL}\n");
                else
                    Console.WriteLine($"{DialogConstants.RESULT_SUCCES}\n");

                var recognize = new Recognize(inputData, weightsOfHiddenLayer, weightsOfOutputNeuron,
                    weightsOfEvolutionHiddenLayer, weightsOfEvolutionOutputNeuron);

                recognize.Start();
            }

            Console.ReadKey();
        }
    }
}
