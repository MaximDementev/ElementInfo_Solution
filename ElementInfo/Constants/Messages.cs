namespace MagicEntry.Plugins.ElementInfo.Constants
{
    // Статический класс для хранения всех сообщений и постоянных данных
    public static class Messages
    {
        #region Dialog Messages
        public const string INFORMATION_TITLE = "Информация";
        public const string ERROR_TITLE = "Ошибка";
        public const string SUCCESS_TITLE = "Успешно";
        public const string COPY_TO_CLIPBOARD_QUESTION = "Скопировать текст в буфер обмена?";
        public const string TEXT_COPIED = "Текст скопирован в буфер обмена";
        public const string NO_ELEMENTS_SELECTED = "Не выбрано ни одного элемента";
        public const string NO_TEXT_IN_CLIPBOARD = "В буфере обмена нет текста";
        public const string ENTER_TEXT_TITLE = "Ввод текста";
        public const string ENTER_TEXT_INSTRUCTION = "Введите текст для анализа:";
        public const string INVALID_TEXT_FORMAT = "Неверный формат текста";
        public const string ELEMENTS_NOT_FOUND = "Элементы с указанными ID не найдены";
        public const string VIEW_NOT_FOUND = "Вид с указанным именем не найден";
        public const string ELEMENTS_SELECTED = "Выбрано элементов: {0}";
        public const string VIEW_OPENED = "Открыт вид: {0}";
        public const string INVALID_TEXT_TITLE = "Неверный формат текста";
        public const string INVALID_VIEW_FORMAT = "В тексте не найдено имя вида в формате 'Активный вид: [название]'";
        public const string INVALID_ID_FORMAT = "В тексте не найдены ID элементов в формате 'ID: [число]'";
        #endregion

        #region Instructions
        public const string SELECT_ELEMENTS_INSTRUCTION = "Выберите элементы (Esc - отмена)";
        #endregion

        #region Footer Links
        public const string FOOTER_LINK = "<a href=\"https://bz.krgp.ru/kb/rabota-s-oim/\">Инструкция -Работа с ОИМ-</a>";
        #endregion

        #region Format Templates
        public const string LOCATION_TEMPLATE = "Место расположения: {0}";
        public const string CURRENT_FILE_TEMPLATE = "Текущий файл: {0}";
        public const string USER_TEMPLATE = "Пользователь: {0}";
        public const string ACTIVE_VIEW_TEMPLATE = "Активный вид: {0}";
        public const string ELEMENT_SEPARATOR = "---\n";
        public const string WORKSET_TEMPLATE = "Рабочий набор: {0}";
        public const string NAME_TEMPLATE = "Имя: {0}";
        public const string ID_TEMPLATE = "ID: {0}";
        #endregion
    }
}
