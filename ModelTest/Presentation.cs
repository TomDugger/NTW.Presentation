using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NTW.Presentation;
using NTW.Presentation.Attributes;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;

namespace ModelTest.Test
{
    [PresentationMarginInfo(LeftRight = 50)]
    public class Presentation
    {
        public Presentation()
        {
            //ValueClass = new PresentationItem();
            //presGeneric = new PresentationGeneric<int>();
            //presGenericString = new PresentationGeneric<string>();
            //presGenericPresentationItem = new PresentationGeneric<PresentationItem>() { Value = new PresentationItem() { ItemName = "name", ItemValue = 10.25}, Value2 = "dasdasd" };
            //presGeneric1 = new PresentationGeneric<int, PresentationGeneric<bool>>() { Value2 = new PresentationGeneric<bool>() };
            //ValueArrayInt = new int[] { 0, 1, 2, 3, 4, 5 };
            //ValueArrayUInt = new uint[] { 0, 1, 2, 3, 4, 5 };
            //ValueArrayByte = new byte[] { 0, 1, 2, 3, 4, 5 };
            //ValueArraySByte = new sbyte[] { 0, 1, 2, 3, 4, 5 };
            //ValueArrayLong = new long[] { 0, 1, 2, 3, 4, 5 };
            //ValueArrayULong = new ulong[] { 0, 1, 2, 3, 4, 5 };
            //ValueArrayShort = new short[] { 0, 1, 2, 3, 4, 5 };
            //ValueArrayUShort = new ushort[] { 0, 1, 2, 3, 4, 5 };
            //ValueArrayFloat = new float[] { 0, 1, 2, 3, 4, 5 };
            //ValueArrayDouble = new double[] { 0, 1, 2, 3, 4, 5 };
            //ValueArrayDecimal = new decimal[] { 0, 1, 2, 3, 4, 5 };
            //ValueArrayChar = new char[] { '0', '1', '2', '3', '4', '5' };
            //ValueArrayString = new string[] { "0", "1", "2", "3", "4", "5" };
            //ValueArrayBool = new bool[] { false, true, false, true, false };
            //ValueArrayEnum = new Aenum[] { Aenum.a1, Aenum.a2, Aenum.a1, Aenum.a4 };


            //ValueArrayPresentationItem = new PresentationItem[Application.Current.Resources.Count];
            //ValueArrayPresentationGenericInt = new PresentationGeneric<int>[Application.Current.Resources.Count];

            //ValueListInt = new List<int>() { 1, 2, 3, 4, 5, 6 };

            //ValueListBool = new List<bool>() { false, true, false, false };

            //ValueListEnum = new List<Aenum>() { Aenum.a1, Aenum.a4, Aenum.a3 };

            //ValueListPresentationItem = new List<PresentationItem>() { new PresentationItem() { ItemName = "Item1", ItemValue = 12.5 } };

            //ValueListPresentationGenericString = new List<PresentationGeneric<string>>() { 
            //    new PresentationGeneric<string>() { Value = "имя" }
            //};

            //ValueGenericListString = new PresentationList<string>() { "newname", "newname1" };

            //ValueGenericListPresentationItem = new PresentationList<PresentationItem>() { 
            //    new PresentationItem() { ItemName = "Item 1", ItemValue = 12.5 },
            //    new PresentationItem() { ItemName = "Item 2", ItemValue = 156.3 }
            //};

            //ValueGenericLisPresentationGenericInt = new PresentationList<PresentationGeneric<int>> {
            //    new PresentationGeneric<int>() { Value = 0 },
            //    new PresentationGeneric<int>() { Value = 1 }
            //};

            //ValueDictionaryStringString = new Dictionary<int, int>();
            //ValueDictionaryStringString.Add(0, 10);
            //ValueDictionaryStringString.Add(1, 152);
            //ValueDictionaryStringString.Add(2, 854);

            //ValueDictionaryDoublePresentationItem = new Dictionary<double, PresentationItem>();
            //ValueDictionaryDoublePresentationItem.Add(10.1, new PresentationItem() { ItemName = "Item 1", ItemValue = 10.1 });
            //ValueDictionaryDoublePresentationItem.Add(20.2, new PresentationItem() { ItemName = "Item 2", ItemValue = 20.2 });

            //ValueDictionaryDoublePresentationItem2 = new Dictionary<double, PresentationItem>();
            //ValueDictionaryDoublePresentationItem2.Add(10.1, new PresentationItem() { ItemName = "Item 11", ItemValue = 10.1 });
            //ValueDictionaryDoublePresentationItem2.Add(20.2, new PresentationItem() { ItemName = "Item 12", ItemValue = 20.2 });

            //ValueDictionaryPresentationItemPresentationListString = new Dictionary<PresentationItem, PresentationList<string>>();
            //ValueDictionaryPresentationItemPresentationListString.Add(new PresentationItem() { ItemName = "Item 1", ItemValue = 10.1 }, new PresentationList<string>());


            int i = 0;
            foreach (var key in Application.Current.Resources.Keys)
            {
                //ValueArrayPresentationItem[i] = new PresentationItem() { ItemName = key.ToString(), ItemValue = i * 0.158 };
                //ValueArrayPresentationGenericInt[i] = new PresentationGeneric<int>() { Value = i * 5 };
                //ValueListInt.Add(i);
                //ValueListPresentationItem.Add(new PresentationItem() { ItemName = key.ToString(), ItemValue = i });

                //ValueGenericListString.Add(key.ToString());
                //ValueGenericListPresentationItem.Add(new PresentationItem() { ItemName = key.ToString(), ItemValue = i });
                //ValueGenericLisPresentationGenericInt.Add(new PresentationGeneric<int>() { Value = i, Value2 = key.ToString() });

                //ValueDictionaryStringString.Add(i, Application.Current.Resources.Keys.Count - i);
                //ValueDictionaryDoublePresentationItem.Add(i, new PresentationItem() { ItemName = key.ToString(), ItemValue = i });
                //ValueDictionaryDoublePresentationItem2.Add(i, new PresentationItem() { ItemName = key.ToString(), ItemValue = i });
                //ValueDictionaryPresentationItemPresentationListString.Add(new PresentationItem() { ItemName = key.ToString(), ItemValue = i}, new PresentationList<string>() { });
                i++;
            }
        }

        #region Simple
        //public byte ValueByte { get; set; }
        //public sbyte ValueSByte { get; set; }

        //public short ValueShort { get; set; }
        //public ushort ValueUshort { get; set; }

        //public Int64 ValueInt32 { get; set; }
        //public int ValueInt { get; set; }
        //public uint ValueUint { get; set; }

        //public long ValueLong { get; set; }
        //public ulong ValueUlong { get; set; }

        //public float ValueFloat { get; set; }
        //public double ValueDouble { get; set; }
        //public decimal ValueDecimal { get; set; }

        //public char ValueChar { get; set; }
        //public string ValueString { get; set; }

        //public bool ValueBool { get; set; } 
        #endregion

        #region Enum
        //public Aenum Enum { get; set; } 
        #endregion

        #region Class
        //public PresentationItem ValueClass { get; set; } 
        #endregion

        #region Generic
        //public PresentationGeneric<int> presGeneric { get; set; }
        //public PresentationGeneric<string> presGenericString { get; set; }
        //public PresentationGeneric<PresentationItem> presGenericPresentationItem { get; set; }

        //public PresentationGeneric<int, PresentationGeneric<bool>> presGeneric1 { get; set; } 
        #endregion

        #region Array
        //public int[] ValueArrayInt { get; set; }

        //public uint[] ValueArrayUInt { get; set; }

        //public byte[] ValueArrayByte { get; set; }

        //public sbyte[] ValueArraySByte { get; set; }

        //public long[] ValueArrayLong { get; set; }

        //public ulong[] ValueArrayULong { get; set; }

        //public short[] ValueArrayShort { get; set; }

        //public ushort[] ValueArrayUShort { get; set; }

        //public float[] ValueArrayFloat { get; set; }

        //public double[] ValueArrayDouble { get; set; }

        //public decimal[] ValueArrayDecimal { get; set; }

        //public char[] ValueArrayChar { get; set; }

        //public string[] ValueArrayString { get; set; }

        //public bool[] ValueArrayBool { get; set; }

        //public Aenum[] ValueArrayEnum { get; set; }

        //public PresentationItem[] ValueArrayPresentationItem { get; set; }

        //public PresentationGeneric<int>[] ValueArrayPresentationGenericInt { get; set; } 
        #endregion

        #region List
        //public List<int> ValueListInt { get; set; }

        //public List<bool> ValueListBool { get; set; }

        //public List<Aenum> ValueListEnum { get; set; }

        //public List<PresentationItem> ValueListPresentationItem { get; set; }

        //public List<PresentationGeneric<string>> ValueListPresentationGenericString { get; set; }
        #endregion

        #region MyList
        //public PresentationList<string> ValueGenericListString { get; set; }

        //public PresentationList<PresentationItem> ValueGenericListPresentationItem { get; set; }

        //public PresentationList<PresentationGeneric<int>> ValueGenericLisPresentationGenericInt { get; set; }
        #endregion

        #region Dictionary
        //public Dictionary<int, int> ValueDictionaryStringString { get; set; }

        //public Dictionary<double, PresentationItem> ValueDictionaryDoublePresentationItem { get; set; }

        //public Dictionary<double, PresentationItem> ValueDictionaryDoublePresentationItem2 { get; set; }

        //public Dictionary<PresentationItem, PresentationList<string>> ValueDictionaryPresentationItemPresentationListString { get; set; }
        #endregion

        #region Commands
        //public Command Commanda { get; set; }
        #endregion

        //[PresentationMarginInfo(Top = 50)]
        //[PresentationInfo(CaptionName = "Просто перечисление", PresentCaption = TextWrapping.NoWrap)]
        //[NonPresentation]
        //public Aenum Enum { get; set; }

        //public Command Commanda { get; set; }

        //public Dictionary<int, PresentationItem> dictionary { get; set; }

        //public Dictionary<string, PresentationItem> dictionary2 { get; set; }

        //public PresentationItem[] ArrayPresentationItems { get; set; }

        //public PresentationGeneric<int, int>[] ArrayGeneric { get; set; }

        //[PresentationInfo(CaptionName = "Допэлемент", PresentCaption = TextWrapping.NoWrap)]
        //public PresentationItem Item { get; set; }

        //public PresentationGeneric<int, string> GItem { get; set; }

        //public List<PresentationGeneric<double, long>> ListGItems { get; set; }

        //public List<PresentationGeneric<int, int>> ListG2Items { get; set; }

        //[PresentationInfo(CaptionName = "Массив генерации двойного типа", PresentCaption = TextWrapping.NoWrap)]
        //public PresentationGeneric<string, double>[] ArrayGItems { get; set; }

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

    public class Command : ICommand
    {
        private Action<object> execute;
        private Func<object, bool> canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public Command(Action<object> execute, Func<object, bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return this.canExecute == null || this.canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            this.execute(parameter);
        }
    }

    public class PresentationItem
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

    public class PresentationGeneric<T>
    {
        public T Value { get; set; }

        public string Value2 { get; set; }
    }

    public class PresentationGeneric<T, S> 
    {
        public T Value1 { get; set; }
        public S Value2 { get; set; }

        public override string ToString()
        {
            return Value1.ToString() + ", " + Value2.ToString();
        }
    }

    public class PresentationList<T> : IList<T>
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

        #region IList<T> Members
        public int IndexOf(T item) {
            return Values.IndexOf(item);
        }

        public void Insert(int index, T item) {
            Values.Insert(index, item);
        }

        T IList<T>.this[int index] {
            get {
                return Values[index];
            }
            set {
                Values[index] = value;
            }
        }

        public void Add(T item) {
            Values.Add(item);
        }

        public bool Contains(T item) {
            return Values.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            Values.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item) {
            return Values.Remove(item);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() {
            return Values.GetEnumerator();
        } 
        #endregion
    }
}
