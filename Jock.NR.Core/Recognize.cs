namespace Jock.NR.Core
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Класс для распознования изображения.
    /// </summary>
    public class Recognize
    {
        /// <summary>
        /// Веса скрытого слоя.
        /// </summary>
        private Dictionary<int, List<double>> _hiddenLayerWeights { get; set; }

        /// <summary>
        /// Веса скрытого слоя, полученные нейроэволюционным методом.
        /// </summary>
        private Dictionary<int, List<double>> _evolutionHiddenLayerWeights { get; set; }

        /// <summary>
        /// Веса выходного нейрона.
        /// </summary>
        private List<double> _outputWeights { get; set; }

        /// <summary>
        /// Веса выходного нейрона, полученные нейроэволюционным методом.
        /// </summary>
        private List<double> _evolutionOutputWeights { get; set; }

        /// <summary>
        /// Данные для распознования.
        /// </summary>
        private List<double> _recognizedData { get; set; }

        /// <summary>
        /// Значение коэфициента распознования.
        /// </summary>
        private const double RECOGNIZE_VALUE = 0.7;

        /// <summary>
        /// Класс для распознования изображения.
        /// </summary>
        public Recognize(List<double> recognizedData, 
            Dictionary<int, List<double>> hiddenLayerWeights, List<double> outputWeights,
            Dictionary<int, List<double>> evolutionHiddenLayerWeights, List<double> evolutionOutputWeights)
        {
            _recognizedData = recognizedData;

            _hiddenLayerWeights = hiddenLayerWeights;
            _outputWeights = outputWeights;

            _evolutionHiddenLayerWeights = evolutionHiddenLayerWeights;
            _evolutionOutputWeights = evolutionOutputWeights;
        }

        /// <summary>
        /// Инициализация распознования.
        /// </summary>
        public void Start()
        {
            if ((_hiddenLayerWeights == null || _outputWeights == null) &&
                (_evolutionHiddenLayerWeights == null || _evolutionOutputWeights == null))
            {
                Console.WriteLine("Can not recognize the data:" +
                    "\nWeights not be loaded correctly.");
                return;
            }

            if (_hiddenLayerWeights == null || _outputWeights == null)
            {
                RecognizeByOneType(_evolutionHiddenLayerWeights, _evolutionOutputWeights);
                return;
            }

            if (_evolutionHiddenLayerWeights == null || _evolutionOutputWeights == null)
            {
                RecognizeByOneType(_hiddenLayerWeights, _outputWeights);
                return;
            }

            RecognizeByTwoType();
        }

        /// <summary>
        /// Распознование с одним типом весов.
        /// </summary>
        /// <param name="hiddenLayerWeights">Веса скрытого слоя.</param>
        /// <param name="outputWeights">Веса выходного слоя.</param>
        private void RecognizeByOneType(Dictionary<int, List<double>> hiddenLayerWeights,
            List<double> outputWeights)
        {
            Console.WriteLine("Start recognizing by one type.");

            var hiddenLayer = new List<Neuron>();
            var inputsToOutput = new List<double>();

            for (var index = 0; index < hiddenLayerWeights.Count; index++)
                hiddenLayer.Add(new Neuron(_recognizedData, hiddenLayerWeights[index]));

            hiddenLayer.ForEach(neuron => inputsToOutput.Add(neuron.Output));

            var outputNeuron = new Neuron(inputsToOutput, outputWeights);

            if (outputNeuron.Output > RECOGNIZE_VALUE)
                Console.WriteLine($"\nI think it's 1.");
            else
                Console.WriteLine($"\nCan't recognize this number.");
        }

        /// <summary>
        /// Распознование по двум типам.
        /// </summary>
        private void RecognizeByTwoType()
        {
            Console.WriteLine("Start recognizing by two type.");

            var hiddenLayer = new List<Neuron>();
            var inputsToOutput = new List<double>();

            for (var index = 0; index < _hiddenLayerWeights.Count; index++)
                hiddenLayer.Add(new Neuron(_recognizedData, _hiddenLayerWeights[index]));

            hiddenLayer.ForEach(neuron => inputsToOutput.Add(neuron.Output));

            var outputNeuron = new Neuron(inputsToOutput, _outputWeights);
            var firstOutput = outputNeuron.Output;

            hiddenLayer.Clear();
            inputsToOutput.Clear();

            for (var index = 0; index < _evolutionHiddenLayerWeights.Count; index++)
                hiddenLayer.Add(new Neuron(_recognizedData, _evolutionHiddenLayerWeights[index]));

            hiddenLayer.ForEach(neuron => inputsToOutput.Add(neuron.Output));

            var outpuEvolutiontNeuron = new Neuron(inputsToOutput, _evolutionOutputWeights);
            var secondOutput = outpuEvolutiontNeuron.Output;

            var bothOpinion = (firstOutput + secondOutput) / 2;

            Console.WriteLine($"\n*DEBUG INFO:*" +
                $"\nFirst output: {firstOutput}" +
                $"\nSecond output: {secondOutput}" +
                $"\nBoth opinion: {bothOpinion}");

            if (bothOpinion > RECOGNIZE_VALUE)
                Console.WriteLine($"\nI think it's 1.");
            else
                Console.WriteLine($"\nCan't recognize this number.");
        }
    }
}
