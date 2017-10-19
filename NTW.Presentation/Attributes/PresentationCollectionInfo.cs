using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NTW.Presentation.Attributes
{
    public class PresentationCollectionInfo:System.Attribute
    {
        /// <summary>
        /// Минимальная высота для контейнера.
        /// </summary>
        public double MinHeight { get; set; }
        /// <summary>
        /// Максимальная высота для контейнера.
        /// </summary>
        public double MaxHeight { get; set; }
        /// <summary>
        /// Специальный шаблон для элементов списка.
        /// Стоит учесть, что для списков с простыми типами данных (int, string и т.д.). 
        /// Производится обертка в класс с двуми свойствами 
        ///  - Index - индекс жлемента в списке;
        ///  - Value - значение.
        /// </summary>
        public string ItemTemplate { get; set; }

        public string ItemStyle { get; set; }

        public string AddButtonContentTemplate { get; set; }
    }
}
