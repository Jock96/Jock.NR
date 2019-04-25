namespace Jock.NR.Core
{
    using System.Collections.Generic;

    /// <summary>
    /// Класс входного слоя.
    /// </summary>
    internal class InputLayer
    {
        /// <summary>
        /// Класс входного слоя.
        /// </summary>
        /// <param name="count">Количество нейронов входного слоя.</param>
        public InputLayer(int count)
        {
            _count = count;
        }

        /// <summary>
        /// Колиество нейронов входного слоя.
        /// </summary>
        private readonly int _count;

        /// <summary>
        /// Массив нейронов.
        /// </summary>
        private List<double> _inputLayerNeurons;

        /// <summary>
        /// Заполняет входной слой неронов значениями.
        /// </summary>
        /// <param name="data">Значения для заполнения входного слоя.</param>
        /// <returns>Список значений входного слоя.</returns>
        public List<double> FillInputLayer(int[] data)
        {
            _inputLayerNeurons = new List<double>();

            for (int index = 0; index < _count; index++)
                _inputLayerNeurons.Add(data[index]);

            return _inputLayerNeurons;
        }
    }
}
