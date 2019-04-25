namespace Jock.NR.Core
{
    using Jock.NR.BL;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Класс для обучения.
    /// </summary>
    public class Learning
    {
        /// <summary>
        /// Количество эпох.
        /// </summary>
        private int _epochCount { get; set; }

        /// <summary>
        /// Список всех данных входного слоя.
        /// </summary>
        private List<List<double>> _inputDataList { get; set; }

        /// <summary>
        /// Сумма MSE для ошибки.
        /// </summary>
        private double sumMSE;

        /// <summary>
        /// Веса скрытого слоя.
        /// </summary>
        private Dictionary<int, List<double>> _hiddenLayerWeights { get; set; }

        /// <summary>
        /// Веса выходного нейрона.
        /// </summary>
        private List<double> _outputWeights { get; set; }

        /// <summary>
        /// Тип обучения.
        /// </summary>
        private LearningType _learningType;

        /// <summary>
        /// Класс обучения нейронной сети.
        /// </summary>
        /// <param name="epochCount">Количество эпох.</param>
        /// <param name="inputDataList">Список входных параметров.</param>
        /// <param name="isNeedInitialize">Необходимость инициализирования.</param>
        /// <param name="idealResult">Ожидаемый результат.</param>
        /// <param name="isWeightsLoaded">Флаг загрузки весов.</param>
        /// <param name="hiddenLayerWeights">Веса скрытого слоя.</param>
        /// <param name="outputWeights">Веса выходного нейрона.</param>
        /// <remarks>При положительном флаге загрузки весов в нейронную сеть - 
        /// продолжается обучения по существующим весам.</remarks>
        public Learning(int epochCount, List<List<double>> inputDataList, bool isNeedInitialize,
            double idealResult, LearningType learningType, bool isWeightsLoaded = false, Dictionary<int, List<double>> hiddenLayerWeights = null,
            List<double> outputWeights = null)
        {
            _inputDataList = inputDataList;
            _learningType = learningType;

            if (_learningType == LearningType.Neuroevolution)
                _epochCount = 10 * epochCount;
            else
                _epochCount = epochCount;

            if (!isWeightsLoaded)
            {
                Start(_epochCount, _inputDataList, isNeedInitialize, idealResult);
            }
            else
            {
                try
                {
                    _hiddenLayerWeights = hiddenLayerWeights;
                    _outputWeights = outputWeights;

                    if (_hiddenLayerWeights == null || _outputWeights == null)
                        throw new Exception();
                }
                catch
                {
                    Console.WriteLine($"\nYou must load weights, before continue learning process!" +
                        $"\nPress any key to reload...");

                    Console.ReadKey();
                    Environment.Exit(0);
                }

                LearningContinue(_epochCount, _inputDataList, idealResult);
            }
        }

        /// <summary>
        /// Метод вызывается при продолжении обучения 
        /// нейронной сети и загрузки всех весов.
        /// </summary>
        /// <param name="_epochCount">Количество эпох.</param>
        /// <param name="_inputDataList">Загруженные данные.</param>
        /// <param name="idealResult">Ожидаемый результат.</param>
        private void LearningContinue(int _epochCount, List<List<double>> _inputDataList, double idealResult)
        {
            var dictionaryOfDeltas = new Dictionary<int, List<double>>();

            var learningSet = new List<double>();
            var hiddenLayer = new List<Neuron>();

            var inputsToOutput = new List<double>();

            var outputNeuron = OutputNeuronInitilize(_inputDataList.First().Count / 2, new List<double>());

            var errorPercent = 0d;

            var previousDeltaToWeightsChange = new List<double>();
            var previousInputDeltaToWeightsChange = new List<double>();

            for (var epoch = 0; epoch < _epochCount; epoch++)
            {
                for (var dataCounter = 0; dataCounter < _inputDataList.Count; dataCounter++)
                {
                    learningSet = _inputDataList[dataCounter];

                    if (epoch == 0 && dataCounter == 0)
                    {
                        for (var index = 0; index < _hiddenLayerWeights.Count; index++)
                            hiddenLayer.Add(new Neuron(learningSet, _hiddenLayerWeights[index]));

                        hiddenLayer.ForEach(neuron => inputsToOutput.Add(neuron.Output));

                        outputNeuron = new Neuron(inputsToOutput, _outputWeights);

                        if (outputNeuron.Output != idealResult)
                        {
                            for (var index = 0; index < outputNeuron.Inputs.Count; index++)
                                previousDeltaToWeightsChange.Add(0d);

                            for (var indexOfDictionary = 0; indexOfDictionary < hiddenLayer.Count; indexOfDictionary++)
                            {
                                var emptyList = new List<double>();

                                for (var index = 0; index < learningSet.Count; index++)
                                    emptyList.Add(0d);

                                dictionaryOfDeltas.Add(indexOfDictionary, emptyList);
                            }

                            errorPercent = ErrorByRootMSE(ref sumMSE, idealResult, outputNeuron.Output, (epoch + 1));

                            if (_learningType == LearningType.Backpropagation)
                                    Backpropagation(outputNeuron.Output, idealResult, learningSet, ref outputNeuron,
                                    ref hiddenLayer, ref previousDeltaToWeightsChange, ref dictionaryOfDeltas);

                            if (_learningType == LearningType.Neuroevolution)
                                Neuroevolution(ref outputNeuron, ref hiddenLayer);

                            continue;
                        }
                    }

                    foreach (var neuron in hiddenLayer)
                    {
                        neuron.Inputs = learningSet;
                    }

                    foreach (var neuron in hiddenLayer)
                        outputNeuron.Inputs[hiddenLayer.IndexOf(neuron)] = neuron.Output;

                    if (outputNeuron.Output != idealResult)
                    {
                        errorPercent = ErrorByRootMSE(ref sumMSE, idealResult, outputNeuron.Output, (epoch + 1));

                        if (_learningType == LearningType.Backpropagation)
                            Backpropagation(outputNeuron.Output, idealResult, learningSet, ref outputNeuron,
                            ref hiddenLayer, ref previousDeltaToWeightsChange, ref dictionaryOfDeltas);

                        if (_learningType == LearningType.Neuroevolution)
                            Neuroevolution(ref outputNeuron, ref hiddenLayer);
                    }
                }

                InfoOutput(epoch, outputNeuron.Output, errorPercent);
            }

            EndOfLearning(hiddenLayer, outputNeuron);
        }

        /// <summary>
        /// Начало обучения нейронной сети.
        /// </summary>
        /// <param name="_epochCount">Количество эпох.</param>
        /// <param name="_inputDataList">Список входных данных.</param>
        /// <param name="isNeedInitialize">Необходимость инициализациию</param>
        /// <param name="idealResult">Идеальный результат.</param>
        private void Start(int _epochCount, List<List<double>> _inputDataList, bool isNeedInitialize, double idealResult)
        {
            var dictionaryOfDeltas = new Dictionary<int, List<double>>();

            var learningSet = new List<double>();
            var hiddenLayer = new List<Neuron>();

            var outputNeuron = OutputNeuronInitilize(_inputDataList.First().Count/2, new List<double>());

            var isFirstStart = false;
            var errorPercent = 0d;

            var previousDeltaToWeightsChange = new List<double>();
            var previousInputDeltaToWeightsChange = new List<double>();

            for (var epoch = 0; epoch < _epochCount; epoch++)
            {
                for (var dataCounter = 0; dataCounter < _inputDataList.Count; dataCounter++)
                {
                    learningSet = _inputDataList[dataCounter];

                    var hiddenLayerFinalCount = learningSet.Count / 2;

                    if (isNeedInitialize && dataCounter == 0 && epoch == 0)
                    {
                        HiddenLayerInitialize(hiddenLayerFinalCount, learningSet, ref hiddenLayer);

                        foreach (var neuron in hiddenLayer)
                            outputNeuron.Inputs.Add(neuron.Output);

                        var currentOutput = outputNeuron.Output;

                        if(currentOutput != idealResult)
                        {
                            for (var index = 0; index < outputNeuron.Inputs.Count; index++)
                                previousDeltaToWeightsChange.Add(0d);

                            for (var indexOfDictionary = 0; indexOfDictionary < hiddenLayer.Count; indexOfDictionary++)
                            {
                                var emptyList = new List<double>();

                                for (var index = 0; index < learningSet.Count; index++)
                                    emptyList.Add(0d);

                                dictionaryOfDeltas.Add(indexOfDictionary, emptyList);
                            }

                            errorPercent = ErrorByRootMSE(ref sumMSE, idealResult, currentOutput, (epoch + 1));

                            if (_learningType == LearningType.Backpropagation)
                                Backpropagation(currentOutput, idealResult, learningSet, ref outputNeuron, 
                                ref hiddenLayer, ref previousDeltaToWeightsChange, ref dictionaryOfDeltas);

                            if (_learningType == LearningType.Neuroevolution)
                                Neuroevolution(ref outputNeuron, ref hiddenLayer);
                        }

                        isNeedInitialize = false;
                        isFirstStart = true;
                    }

                    if(!isFirstStart)
                    {
                        foreach (var neuron in hiddenLayer)
                        {
                            neuron.Inputs = learningSet;
                        }

                        foreach (var neuron in hiddenLayer)
                            outputNeuron.Inputs[hiddenLayer.IndexOf(neuron)] = neuron.Output;

                        var currentOutput = outputNeuron.Output;

                        if (currentOutput != idealResult)
                        {
                            errorPercent = ErrorByRootMSE(ref sumMSE, idealResult, currentOutput, (epoch + 1));

                            if (_learningType == LearningType.Backpropagation)
                                Backpropagation(currentOutput, idealResult, learningSet, ref outputNeuron, 
                                ref hiddenLayer, ref previousDeltaToWeightsChange, ref dictionaryOfDeltas);

                            if (_learningType == LearningType.Neuroevolution)
                                Neuroevolution(ref outputNeuron, ref hiddenLayer);
                        }
                    }

                    isFirstStart = false;
                }

                InfoOutput(epoch, outputNeuron.Output, errorPercent);
            }

            EndOfLearning(hiddenLayer, outputNeuron);
        }

        /// <summary>
        /// Обёртка для вывода информации.
        /// </summary>
        /// <param name="epoch">Текущая эпоха.</param>
        /// <param name="output">Вывод.</param>
        /// <param name="erroPercent">Процент ошибки.</param>
        private void InfoOutput(int epoch, double output, double erroPercent)
        {
            if (epoch == 0)
                Console.WriteLine();

            Console.WriteLine($"Epoch: {epoch + 1}");
            Console.WriteLine($"Output: {output}");
            Console.WriteLine($"Error: {erroPercent}\n");
        }

        /// <summary>
        /// Действия при окончании обучения.
        /// </summary>
        /// <param name="hiddenLayer">Скрытый слой.</param>
        /// <param name="outputNeuron">Выходной нейрон.</param>
        private void EndOfLearning(List<Neuron> hiddenLayer, Neuron outputNeuron)
        {
            var answer = string.Empty;
            var breakFlag = false;

            do
            {
                Console.WriteLine($"Are you want to save weights? (Y/N)");
                answer = Console.ReadLine();

                if (DialogConstants.YES_RESULT.Any(result => result.Contains(answer)) ||
                    DialogConstants.NO_RESULT.Any(result => result.Contains(answer)))
                    breakFlag = true;
            } while (!breakFlag);

            if (DialogConstants.NO_RESULT.Any(result => result == answer))
                Environment.Exit(0);
            else
            {
                try
                {
                    SaveSettings(hiddenLayer, outputNeuron);
                    Console.WriteLine($"\nPress any key...");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка сохранения весов:" +
                        $"\n{ex.ToString()}");
                }
            }
        }

        /// <summary>
        /// Применение нейроэволюционного метода.
        /// </summary>
        /// <param name="outputNeuron">Нейрон выходного слоя.</param>
        /// <param name="hiddenLayer">Нейроны скртого слоя.</param>
        private void Neuroevolution(ref Neuron outputNeuron, ref List<Neuron> hiddenLayer)
        {
            var countOfOutputWeights = outputNeuron.Weights.Count;
            var countOfHiddenWeights = hiddenLayer.First().Weights.Count;

            var copyOfOutputWeights = outputNeuron.Weights;
            Swipe(countOfOutputWeights, ref copyOfOutputWeights);

            outputNeuron.Weights = copyOfOutputWeights;

            var dictionaryOfNewWeights = new Dictionary<int, List<double>>();

            foreach(var neuron in hiddenLayer)
            {
                var copyOfNeuronWeights = neuron.Weights;

                Swipe(countOfOutputWeights, ref copyOfOutputWeights);
                dictionaryOfNewWeights.Add(hiddenLayer.IndexOf(neuron), copyOfNeuronWeights);
            }

            for (var index = 0; index < hiddenLayer.Count; index++)
            {
                hiddenLayer[index].Weights = dictionaryOfNewWeights[index];
            }
        }

        /// <summary>
        /// Функция кроссинговера.
        /// </summary>
        /// <param name="count">Количество весов.</param>
        /// <param name="weights">Список весов.</param>
        private void Swipe(int count, ref List<double> weights)
        {
            var random = new Random();
            var breakFlag = false;

            var dictionaryOfPopulation = new Dictionary<int, double>();
            var indexes = new List<int>();
            int countOfWeights;

            do
            {
                countOfWeights = random.Next(0, count + 1);

                if (countOfWeights % 2 == 0)
                    breakFlag = true;
            } while (!breakFlag);

            for (var index = 0; index < countOfWeights; index++)
            {
                breakFlag = false;
                int randomIndex;

                do
                {
                    randomIndex = random.Next(0, weights.Count);

                    if (indexes.FindAll(x => x == randomIndex).Count == 0)
                    {
                        breakFlag = true;
                        indexes.Add(randomIndex);
                    }
                } while (!breakFlag);

                dictionaryOfPopulation.Add(randomIndex, weights[randomIndex]);
            }

            if (random.Next(0, 2) == 1)
                Mutation(ref dictionaryOfPopulation, indexes);

            var pairIndexes = new List<int>();

            for (var index = 0; index < dictionaryOfPopulation.Count / 2; index++)
            {
                int randomFirstIndex;
                int randomSecondIndex;

                breakFlag = false;

                do
                {
                    randomFirstIndex = random.Next(0, indexes.Max() + 1);

                    if (indexes.Any(x => x == randomFirstIndex) &&
                        pairIndexes.FindAll(x => x == randomFirstIndex).Count == 0)
                    {
                        pairIndexes.Add(randomFirstIndex);
                        breakFlag = true;
                    }
                } while (!breakFlag);

                breakFlag = false;

                do
                {
                    randomSecondIndex= random.Next(0, indexes.Max() + 1);

                    if (indexes.Any(x => x == randomSecondIndex) &&
                        pairIndexes.FindAll(x => x == randomSecondIndex).Count == 0)
                    {
                        pairIndexes.Add(randomSecondIndex);
                        breakFlag = true;
                    }
                } while (!breakFlag);

                weights[randomFirstIndex] = dictionaryOfPopulation[randomSecondIndex];
                weights[randomSecondIndex] = dictionaryOfPopulation[randomFirstIndex];
            }
        }

        /// <summary>
        /// Применение мутации для популяций.
        /// </summary>
        /// <param name="population">Популяция.</param>
        /// <param name="tempIndexes">Индекс весов.</param>
        private void Mutation(ref Dictionary<int, double> population, List<int> tempIndexes)
        {
            var random = new Random();
            var countOfMutated = random.Next(0, population.Count + 1);

            var indexes = new List<int>();

            for (var index = 0; index < countOfMutated; index++)
            {
                var breakFlag = false;

                do
                {
                    var randomIndex = random.Next(0, tempIndexes.Max() + 1);

                    if (tempIndexes.Any(x => x == randomIndex) &&
                        indexes.FindAll(x => x == randomIndex).Count == 0)
                    {
                        breakFlag = true;
                        indexes.Add(randomIndex);
                    }
                } while (!breakFlag);
            }

            foreach(var index in indexes)
            {
                var mutationDelta = random.NextDouble();
                var minus = 1;

                if (random.Next(0, 2) == 1)
                    minus = -1;

                population[index] += minus * mutationDelta;
            }
        }

        /// <summary>
        /// Метод обратного распространения.
        /// </summary>
        /// <param name="currentOutput">Текущий вывод.</param>
        /// <param name="idealResult">Идеальный результат.</param>
        /// <param name="learningSet">Обучающая выборка.</param>
        /// <param name="outputNeuron">Выходной нейрон.</param>
        /// <param name="hiddenLayer">Скрытый слой.</param>
        /// <param name="previousDeltaToWeightsChange">Ссылка на предыдущие дельты синапсов до выходного нейрона.</param>
        /// <param name="dictionaryOfDeltas">Ссылка на предыдущие дельты синапсов до скрытого слоя.</param>
        private void Backpropagation(double currentOutput, double idealResult, List<double> learningSet, ref Neuron outputNeuron, 
            ref List<Neuron> hiddenLayer, ref List<double> previousDeltaToWeightsChange, ref Dictionary<int, List<double>> dictionaryOfDeltas)
        {
            var outputDeltas = new List<double>();
            var deltaToWeightsChange = new List<double>();

            /*Блок расчёта по обратному распространению*/

            //Вычисления для выходного слоя
            var deltaOutput = DeltaOfOutputNeuron(currentOutput, idealResult);
            outputDeltas.Add(deltaOutput);

            var outputLayer = new List<Neuron>
            {
                outputNeuron
            };

            //Вычисления для скрытого слоя
            var hiddenDeltas = new List<double>();

            foreach (var neuron in hiddenLayer)
            {
                var deltaHidden = DeltaOfNeuron(neuron, deltaOutput,
                    SummOfWeights(hiddenLayer.IndexOf(neuron), outputLayer, outputDeltas));

                hiddenDeltas.Add(deltaHidden);
                var gradient = GetGradient(deltaOutput, neuron.Output);

                deltaToWeightsChange.Add(GetWeightsDeltaToChange(gradient,
                    previousDeltaToWeightsChange[hiddenLayer.IndexOf(neuron)]));
            }

            previousDeltaToWeightsChange = deltaToWeightsChange;

            //Замена весов выходного слоя
            for (var index = 0; index < outputNeuron.Weights.Count; index++)
                outputNeuron.Weights[index] += deltaToWeightsChange[index];

            //Вычисления для входного слоя
            foreach (var delta in hiddenDeltas)
            {
                var hiddenDeltaToWeightChange = new List<double>();

                var listOfCurrentDeltas = dictionaryOfDeltas[hiddenDeltas.IndexOf(delta)];

                for (var index = 0; index < learningSet.Count; index++)
                {
                    var currentGradient = GetGradient(delta, learningSet[index]);

                    hiddenDeltaToWeightChange.Add(GetWeightsDeltaToChange(currentGradient, listOfCurrentDeltas[index]));
                }

                for (var index = 0; index < listOfCurrentDeltas.Count; index++)
                    hiddenLayer[hiddenDeltas.IndexOf(delta)].Weights[index] += hiddenDeltaToWeightChange[index];

                dictionaryOfDeltas[hiddenDeltas.IndexOf(delta)] = hiddenDeltaToWeightChange;
            }
        }

        /// <summary>
        /// Подсчёт ошибки по root MSE.
        /// </summary>
        /// <param name="sumMSE">сумма верхней части дроби.</param>
        /// <param name="ideal">Идеальный вывод.</param>
        /// <param name="current">Текущий вывод.</param>
        /// <param name="currentEpoch">Текущая эпоха.</param>
        /// <returns>Возвращает ошибку.</returns>
        private double ErrorByRootMSE(ref double sumMSE, double ideal, double current, double currentEpoch)
        {
            sumMSE += Math.Pow(ideal - current, 2);

            return Math.Pow(sumMSE / currentEpoch, 0.5);
        }

        /// <summary>
        /// Вычисляет сумму произведений исходящих синапсов с дельтами.
        /// </summary>
        /// <param name="currentNeuronIndex">Индекс текущего нейрона в списке.</param>
        /// <param name="nextLayer">Следующий слой.</param>
        /// <param name="nextLayerNeuronsDelta">Дельты следующего слоя.</param>
        /// <returns>Возвращает значение суммы произведений исходящих синапсов с дельтами.</returns>
        private double SummOfWeights(int currentNeuronIndex, List<Neuron> nextLayer, List<double> nextLayerNeuronsDelta)
        {
            var sum = 0d;

            foreach (var neuron in nextLayer)
                sum += neuron.Weights[currentNeuronIndex] * nextLayerNeuronsDelta[nextLayer.IndexOf(neuron)];

            return sum;
        }

        /// <summary>
        /// Вычисляет значение на которое нужно изменить вес.
        /// </summary>
        /// <param name="gradient">Градиент веса.</param>
        /// <returns>Возвращает значение на которое нужно изменить вес.</returns>
        private double GetWeightsDeltaToChange(double gradient, double previousDelta)
        {
            return BL.NetworkConstants.EPSILON * gradient + BL.NetworkConstants.ALPHA * previousDelta;
        }

        /// <summary>
        /// Расчёт градиента.
        /// </summary>
        /// <param name="delta">Дельта связанного нейрона.</param>
        /// <param name="output">Вывод текущего нейрона.</param>
        /// <returns>Возвращает градиент.</returns>
        private double GetGradient(double delta, double output)
        {
            return delta * output;
        }

        /// <summary>
        /// Подсчёт дельты выходного нейрона.
        /// </summary>
        /// <param name="ideal">Идеальный результат.</param>
        /// <param name="current">Текущий результат.</param>
        /// <returns>Возвращает дельту выходного нейрона.</returns>
        private double DeltaOfOutputNeuron(double ideal, double current)
        {
            return (ideal - current) * (Math.Pow(1 + Math.Exp(-current), -1));
        }

        /// <summary>
        /// Подсчёт дельты нейрона.
        /// </summary>
        /// <param name="neuron">Нейрон, для которого подсчитываем дельту.</param>
        /// <param name="deltaOfNextLayerNeuron">Дельта нейрона, с которым он связан в следующей слое.</param>
        /// <param name="summOfWeights">Сумма исходящих весов.</param>
        /// <returns>Возвращает дельту нейрона.</returns>
        private double DeltaOfNeuron(Neuron neuron, double deltaOfNextLayerNeuron, double summOfWeights)
        {
            return (Math.Pow(1 + Math.Exp(-neuron.Output), -1)) * (summOfWeights * deltaOfNextLayerNeuron);
        }

        /// <summary>
        /// Инициализация скрытого слоя.
        /// </summary>
        /// <param name="hiddenLayerFinalCount">Количество нейронов скрытого слоя.</param>
        /// <param name="learningSet">Обучающая выборка.</param>
        /// <param name="hiddenLayer">Скрытый слой.</param>
        private void HiddenLayerInitialize(int hiddenLayerFinalCount, List<double> learningSet, ref List<Neuron> hiddenLayer)
        {
            for (var layerCounter = 0; layerCounter < hiddenLayerFinalCount; layerCounter++)
            {
                var randomWeights = WeightsInitialize(learningSet.Count);
                hiddenLayer.Add(new Neuron(learningSet, randomWeights));
            }
        }

        /// <summary>
        /// Инициализация выходного нейрона.
        /// </summary>
        /// <param name="hiddenLayerFinalCount">Число входных параметров и весов</param>
        /// <param name="inputs">Входные параметры.</param>
        private Neuron OutputNeuronInitilize(int hiddenLayerFinalCount, List<double> inputs)
        {
            var randomWeights = WeightsInitialize(hiddenLayerFinalCount);
            return new Neuron(inputs, randomWeights);
        }

        /// <summary>
        /// Инициализирует веса нейронов.
        /// </summary>
        /// <param name="count">Количество необходимых весов.</param>
        /// <returns>Возвращает список рандомных весов.</returns>
        private List<double> WeightsInitialize(int count)
        {
            var weightsList = new List<double>();

            var random = new Random();
            var notNull = 0d;

            for (var index = 0; index < count; index++)
            {
                do
                    notNull = random.NextDouble();
                while (notNull == 0d);

                weightsList.Add(notNull);
            }

            return weightsList;
        }

        /// <summary>
        /// Сохранение настроек.
        /// </summary>
        /// <param name="neurons">Список нейронов.</param>
        /// <param name="outputNeuron">Выходной нейрон.</param>
        private void SaveSettings(List<Neuron> neurons, Neuron outputNeuron)
        {
            var hiddenLayerPath = string.Empty;
            var outputNeuronPath = string.Empty;

            if (_learningType == LearningType.Backpropagation)
            {
                hiddenLayerPath = $@"{ResourcesConst.RESOURCES_PATH}{ResourcesConst.HIDDEN_LAYER_WEIGHTS_NAME}.txt";
                outputNeuronPath = $@"{ResourcesConst.RESOURCES_PATH}{ResourcesConst.OUTPUT_LAYER_WEIGHTS_NAME}.txt";
            }
            else
            {
                hiddenLayerPath = $@"{ResourcesConst.RESOURCES_PATH}{ResourcesConst.EVOLUTION_WEIGHTS_NAME}" + 
                                    $@"{ResourcesConst.HIDDEN_LAYER_WEIGHTS_NAME}.txt";

                outputNeuronPath = $@"{ResourcesConst.RESOURCES_PATH}{ResourcesConst.EVOLUTION_WEIGHTS_NAME}"+
                                    $@"{ResourcesConst.OUTPUT_LAYER_WEIGHTS_NAME}.txt";
            }

            using (var streamWriter = new StreamWriter(hiddenLayerPath, false))
            {
                foreach (var neuron in neurons)
                {
                    var index = neurons.IndexOf(neuron);
                    if(index == 0)
                        streamWriter.Write($"{NetworkConstants.DEFAULT_NEURON_NAME} {index}\n");
                    else
                        streamWriter.Write($"\n{NetworkConstants.DEFAULT_NEURON_NAME} {index}\n");

                    for (var indexOfWeight = 0; indexOfWeight < neuron.Weights.Count; indexOfWeight++)
                        streamWriter.Write(neuron.Weights[indexOfWeight] + " ");
                }
            }
            using (var streamWriter = new StreamWriter(outputNeuronPath, false))
            {
                foreach (var weight in outputNeuron.Weights)
                    streamWriter.Write(weight + " ");
            }
        }

        /// <summary>
        /// Метод сохранения выходов для проверки.
        /// </summary>
        /// <param name="neurons">Список нейронов.</param>
        private void SaveOutputs(List<Neuron> neurons)
        {
            using (var streamWriter = new StreamWriter($@"{ResourcesConst.RESOURCES_PATH}saveOutputs.txt", false))
            {
                foreach (var neuron in neurons)
                    streamWriter.Write(neuron.Output + " ");
            }
        }
    }
}
