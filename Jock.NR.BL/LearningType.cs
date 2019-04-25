namespace Jock.NR.BL
{
    /// <summary>
    /// Типы обучения.
    /// </summary>
    public enum LearningType
    {
        /// <summary>
        /// Обучение обратным распространением.
        /// </summary>
        Backpropagation = 0,

        /// <summary>
        /// Обучение нейроэволюционным методом.
        /// </summary>
        Neuroevolution = 1,

        /// <summary>
        /// Не выбран.
        /// </summary>
        None = 2
    }
}
