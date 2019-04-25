namespace Jock.NR.BL
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;

    /// <summary>
    /// Конвертер изображений
    /// </summary>
    public static class ImageConverter
    {
        /// <summary>
        /// Путь до сохранённого изображения с изменённой размерностью.
        /// </summary>
        public static string PathToResizedFile { get; private set; }

        /// <summary>
        /// Путь до файла с ARGB таблицей изображения.
        /// </summary>
        private static string PathToIntBitMap;

        /// <summary>
        /// Загрузка данных из файла.
        /// </summary>
        /// <returns>Возвращает массив значений.</returns>
        public static int[] LoadData()
        {
            if (string.IsNullOrEmpty(PathToIntBitMap))
                return null;

            var data = new List<int>();

            try
            {
                using (var fileStream = File.OpenRead(PathToIntBitMap))
                {
                    byte[] array = new byte[fileStream.Length];
                    fileStream.Read(array, 0, array.Length);

                    string dataString = System.Text.Encoding.Default.GetString(array);
                    var dataToParse = dataString.Split(' ');

                    for (int i = 0; i < dataToParse.Length; i++)
                        if (int.TryParse(dataToParse[i], out int result))
                            data.Add(result);

                    return data.ToArray();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки файла: \n{PathToIntBitMap}" +
                    $"\n Ошибка: \n{ex.ToString()}");

                return null;
            }
        }

        /// <summary>
        /// Конвертирует изображение в ARGB.
        /// </summary>
        /// <param name="imagePath"> Путь до изображения</param>
        /// <param name="isConsoleWrite"> Флаг необходимости выводить в консоль.</param>
        /// <param name="newFileName"> Имя файла для сохранения данных.</param>
        /// <returns> Возвращает строку результата вывода в консоль.</returns>
        public static string ConvertBitmapToInt(string imagePath, string newFileName, bool isConsoleWrite = false)
        {
            try
            {
                PathToIntBitMap = $@"{ResourcesConst.RESOURCES_PATH}{newFileName}.txt";

                using (var bitmap = new Bitmap(imagePath))
                using (var streamWriter = new StreamWriter(PathToIntBitMap, false))
                {
                    var bitToInt = new int[bitmap.Width, bitmap.Height];

                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        for (int x = 0; x < bitmap.Width; x++)
                        {
                            bitToInt[x, y] = bitmap.GetPixel(x, y).ToArgb();
                            streamWriter.Write(bitToInt[x, y] + " ");

                            if (isConsoleWrite)
                                Console.Write(bitToInt[x, y] + " ");
                        }

                        streamWriter.WriteLine();
                    }

                    streamWriter.Close();
                }

                return "Convertation status: succes";
            }
            catch (Exception ex)
            {
                return $"Convertation status: fail \n" +
                    $"{ex.ToString()}";
            }
        }

        /// <summary>
        /// Задаёт новый размер изображения и сохраняет в стандартной директории.
        /// </summary>
        /// <param name="image">Изображение.</param>
        /// <param name="width">Ширина.</param>
        /// <param name="height">Высота.</param>
        /// <param name="newName">Новое имя.</param>
        /// <returns>Возвращает изображение с новой размерностью.</returns>
        public static Bitmap ImageResize(Bitmap image, int width, int height, string newName)
        {
            var size = new Size(width, height);

            try
            {
                using (var bitmap = new Bitmap(image, size))
                {
                    bitmap.Save($"{ResourcesConst.RESOURCES_PATH}{newName}.bmp");
                    PathToResizedFile = $"{ResourcesConst.RESOURCES_PATH}{newName}.bmp";

                    return bitmap;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.ToString()}");
                return null;
            }
        }
    }
}
