using System.Collections.Generic;

namespace Jock.NR.BL
{
    /// <summary>
    /// Диалоговые константы.
    /// </summary>
    public static class DialogConstants
    {
        /// <summary>
        /// Положительные результаты.
        /// </summary>
        public static readonly List<string> YES_RESULT = new List<string>() { "Yes", "Y", "y", "yes" };

        /// <summary>
        /// Положительные результаты.
        /// </summary>
        public static readonly List<string> NO_RESULT = new List<string>() { "No", "N", "n", "no" };

        /// <summary>
        /// Результат диалога: обучение.
        /// </summary>
        public static readonly List<string> LEARNING_RESULT = new List<string>()
        { "Learning", "learning", "LEARNING", "L", "l" };

        /// <summary>
        /// Результат диалога: распознавание.
        /// </summary>
        public static readonly List<string> RECOGNIZE_RESULT = new List<string>()
        { "Recognize", "recognize", "RECOGNIZE", "R", "r" };

        /// <summary>
        /// Результат диалога: обратное распространение.
        /// </summary>
        public static readonly List<string> BACKPROPAGATION_RESULT = new List<string>()
        { "Backpropagation", "backpropagation", "BACKPROPAGATION", "b", "B"};

        /// <summary>
        /// Результат диалога: нейроэволюционный метод.
        /// </summary>
        public static readonly List<string> NEUROEVOLUTION_RESULT = new List<string>()
        { "Neuroevolution", "neuroevolution", "NEUROEVOLUTION", "N", "n"};

        /// <summary>
        /// Неудачный результат.
        /// </summary>
        public const string RESULT_FAIL = "Load result: fail.";

        /// <summary>
        /// Удачный результат.
        /// </summary>
        public const string RESULT_SUCCES = "Load result: succes.";
    }
}
