using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NTW.Presentation;
using NTW.Presentation.Attribute;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows;

namespace ModelTest
{
    public class MyPresentation : Presentation
    {
        public Dictionary<int, PresentationItem> dictionary { get; set; }
    }

    public class APresentation : Presentation
    {
        //[PresentationInfo(CaptionName = "Наименование", PresentCaption = TextWrapping.Wrap)]
        //public string Name { get; set; }

        //[PresentationInfo(CaptionName = "Некое значение", PresentCaption = TextWrapping.NoWrap)]
        //public int Value { get; set; }

        //[PresentationInfo(CaptionName = "Свойство типа interface", PresentCaption = TextWrapping.Wrap)]
        //public IMy My { get; set; }
        //[PresentationInfo(CaptionName = "Просто перечисление", PresentCaption = TextWrapping.NoWrap)]
        //public Aenum Enum { get; set; }

        public Dictionary<int, PresentationItem> dictionary { get; set; }

        //[PresentationCollectionInfo(MaxHeight = 50)]
        //public PresentationItem[] ArrayPresentationItems { get; set; }

        //[PresentationCollectionInfo(MaxHeight = 50)]
        //public PresentationGeneric<int, int>[] ArrayGeneric { get; set; }

        //[PresentationInfo(CaptionName = "Допэлемент", PresentCaption = TextWrapping.NoWrap)]
        //public PresentationItem Item { get; set; }

        //public PresentationGeneric<int, string> GItem { get; set; }

        //[PresentationCollectionInfo(MaxHeight = 100)]
        //public List<PresentationGeneric<double, long>> ListGItems { get; set; }

        //public List<PresentationGeneric<int, int>> ListG2Items { get; set; }

        //[PresentationInfo(CaptionName = "Массив генерации двойного типа", PresentCaption = TextWrapping.NoWrap)]
        //public PresentationGeneric<string, double>[] ArrayGItems { get; set; }

        //[PresentationInfo(CaptionName = "Повторная копия объекта с пустыми свойствами", PresentCaption = TextWrapping.NoWrap)]
        //public APresentation CurrentItem { get; set; }

        //[PresentationCollectionInfo(MinHeight = 100, MaxHeight = 200, AddButtonContentTemplate = "AddTemplate", ClearButtonContentTemplate = "NewTemplate")]
        //[PresentationInfo(CaptionName = "Список чисел (int)", PresentCaption = TextWrapping.NoWrap)]
        //public List<int> List { get; set; }

        //[PresentationInfo(CaptionName = "Представление ObservableCollection типа int", PresentCaption = TextWrapping.NoWrap)]
        //public ObservableCollection<int> Collection { get; set; }

        //[PresentationInfo(CaptionName = "Собственный список наследованный от IList", PresentCaption = TextWrapping.NoWrap)]
        //public PresentationList<PresentationItem> PList { get; set; }

        //[PresentationCollectionInfo(MinHeight = 100, MaxHeight = 300)]
        //[PresentationInfo(CaptionName = "Список Собственных объектов", PresentCaption = TextWrapping.NoWrap)]
        //public List<PresentationItem> Items { get; set; }

        //[PresentationInfo(CaptionName = "Собственные объекты в виде массива", PresentCaption = TextWrapping.NoWrap)]
        //public PresentationItem[] Items1 { get; set; }

        //[PresentationInfo(CaptionName = "Простой массив целых чисел", PresentCaption = TextWrapping.Wrap)]
        //public int[] Items2 { get; set; }
    }

    public class PresentationItem : Presentation
    {
        [PresentationInfo(CaptionName = "Имя допэлемента")]
        public string ItemName { get; set; }

        [PresentationInfo(CaptionName = "Значение допэлемента")]
        public double ItemValue { get; set; }

        public override string ToString()
        {
            return "ItemName = " + ItemName + ", ItemValue = " + ItemValue;
        }
    }

    public class PresentationList<T> : IList
    {
        [PresentationInfo(CaptionName = "Значения")]
        private List<T> Values = new List<T>(1);

        #region IList Members

        public int Add(object value)
        {
            Values.Add((T)value);
            return Values.Count - 1;
        }

        public void Clear()
        {
            Values.Clear();
        }

        public bool Contains(object value)
        {
            return Values.Contains((T)value);
        }

        public int IndexOf(object value)
        {
            return Values.IndexOf((T)value);
        }

        public void Insert(int index, object value)
        {
            Values.Insert(index, (T)value);
        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void Remove(object value)
        {
            Values.Remove((T)value);
        }

        public void RemoveAt(int index)
        {
            Values.RemoveAt(index);
        }

        public object this[int index]
        {
            get
            {
                return Values[index];
            }
            set
            {
                Values[index] = (T)value;
            }
        }

        #endregion

        #region ICollection Members

        public void CopyTo(Array array, int index)
        {
            Values.CopyTo((T[])array, index);
        }

        public int Count
        {
            get { return Values.Count; }
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public object SyncRoot
        {
            get { return Values; }
        }

        #endregion

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        #endregion
    }

    public class PresentationGeneric<T, S>: Presentation
    {
        public T Value1 { get; set; }
        public S Value2 { get; set; }

        public override string ToString()
        {
            return Value1.ToString() + ", " + Value2.ToString();
        }
    }


    public interface IMy
    {
        string Name { get; set; }

        void Execute();
    }

    public class My1 : Presentation, IMy
    {
        #region Private
        private string name = "Name1";
        private double value = 0;
        #endregion

        #region Public
        public double Value
        {
            get { return this.value; }
            set { this.value = value; }
        }
        #endregion

        #region IMy Members

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        public void Execute()
        {
            name += "1";
        }

        #endregion
    }

    public class My2 : Presentation, IMy
    {
        #region Private
        private string name = "Name2";
        private bool value = false;
        #endregion

        #region Public
        public bool Value
        {
            get { return this.value; }
            set { this.value = value; }
        }
        #endregion

        #region IMy Members

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        public void Execute()
        {
            name += "2";
        }

        #endregion
    }

    public class My3 : Presentation, IMy
    {
        #region Private
        private string name = "Name1";
        private int value = 0;
        #endregion

        #region Public
        public int Value
        {
            get { return this.value; }
            set { this.value = value; }
        }
        #endregion

        #region IMy Members

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        public void Execute()
        {
            name += "3";
        }

        #endregion
    }
}
