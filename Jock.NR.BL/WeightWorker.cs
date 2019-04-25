namespace Jock.NR.BL
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Класс для работы с файлами весов.
    /// </summary>
    public static class WeightWorker
    {
        /// <summary>
        /// Считывает веса из указанного файла.
        /// </summary>
        /// <remarks>Лучше использовать для файла весов одиночного нейрона,
        /// в данном случае выходного.</remarks>
        /// <param name="path">Путь до файла весов.</param>
        /// <returns>Возвращает список весов.</returns>
        public static List<double> LoadNeuronWeights(string path)
        {
            var data = new List<double>();
            
            try
            {
                using (var fileStream = File.OpenRead(path))
                {
                    var array = new byte[fileStream.Length];
                    fileStream.Read(array, 0, array.Length);

                    var dataString = System.Text.Encoding.Default.GetString(array);
                    var dataToParse = dataString.Split(' ');

                    for (var index = 0; index < dataToParse.Length; index++)
                        if (double.TryParse(dataToParse[index], out double result))
                            data.Add(result);

                    return data;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nОшибка чтения файла весов:" +
                    $"\n{ex.ToString()}");

                return null;
            }
        }

        /// <summary>
        /// Считывает веса из указанного файла.
        /// </summary>
        /// <param name="path">Путь до файла.</param>
        /// <returns>Словарь весов, где ключ - номер нейрона.</returns>
        public static Dictionary<int, List<double>> LoadLayerWeights(string path)
        {
            var data = new Dictionary<int, List<double>>();

            try
            {
                using (var fileStream = File.OpenRead(path))
                {
                    var array = new byte[fileStream.Length];
                    fileStream.Read(array, 0, array.Length);

                    var dataString = System.Text.Encoding.Default.GetString(array);

                    for(var index = 0; index < dataString.Length; index++)
                    {
                        var indexOfFirstChar = dataString.IndexOf(NetworkConstants.DEFAULT_NEURON_NAME + $" {index}");
                        var newString = dataString.Remove(indexOfFirstChar, 9);

                        var indexOfLastChar = dataString.IndexOf(NetworkConstants.DEFAULT_NEURON_NAME + $" {index + 1}");

                        var newDataString = string.Empty;

                        if (indexOfLastChar > 0)
                        {
                            dataString = newString.Substring(indexOfLastChar - 9);
                            newDataString = newString.Remove(indexOfLastChar);
                        }
                        else
                        {
                            dataString = string.Empty;
                            newDataString = newString;
                        }

                        var newDataToParse = newDataString.Split(' ');
                        data.Add(index, new List<double>());

                        for (var indexOfData = 0; indexOfData < newDataString.Length; indexOfData++)
                            if (double.TryParse(newDataToParse[index], out double result))
                                data[index].Add(result);
                    }

                    return data;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nОшибка чтения файла весов:" +
                    $"\n{ex.ToString()}");

                return null;
            }
        }
    }
}
