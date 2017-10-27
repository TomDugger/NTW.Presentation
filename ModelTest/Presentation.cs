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
using System.ComponentModel;
using System.Windows.Data;
using System.Threading;

namespace ModelTest.Test
{
    [PresentationMarginInfo(LeftRight = 5)]
    public class Presentation:INotifyPropertyChanged
    {
        private PresentationItem p;

        public Presentation()
        {
            Enum = Aenum.a2;
            ValueClass = new PresentationItem();
            presGeneric = new PresentationGeneric<int>();
            presGenericString = new PresentationGeneric<string>();
            presGenericPresentationItem = new PresentationGeneric<PresentationItem>() { Value = new PresentationItem() { ItemName = "name", ItemValue = 10.25 }, Value2 = "dasdasd" };
            presGeneric1 = new PresentationGeneric<int, PresentationGeneric<bool>>() { Value2 = new PresentationGeneric<bool>() };
            ValueArrayInt = new int[] { 0, 1, 2, 3, 4, 5 };
            ValueArrayUInt = new uint[] { 0, 1, 2, 3, 4, 5 };
            ValueArrayByte = new byte[] { 0, 1, 2, 3, 4, 5 };
            ValueArraySByte = new sbyte[] { 0, 1, 2, 3, 4, 5 };
            ValueArrayLong = new long[] { 0, 1, 2, 3, 4, 5 };
            ValueArrayULong = new ulong[] { 0, 1, 2, 3, 4, 5 };
            ValueArrayShort = new short[] { 0, 1, 2, 3, 4, 5 };
            ValueArrayUShort = new ushort[] { 0, 1, 2, 3, 4, 5 };
            ValueArrayFloat = new float[] { 0, 1, 2, 3, 4, 5 };
            ValueArrayDouble = new double[] { 0, 1, 2, 3, 4, 5 };
            ValueArrayDecimal = new decimal[] { 0, 1, 2, 3, 4, 5 };
            ValueArrayChar = new char[] { '0', '1', '2', '3', '4', '5' };
            ValueArrayString = new string[] { "0", "1", "2", "3", "4", "5" };
            ValueArrayBool = new bool[] { false, true, false, true, false };
            ValueArrayEnum = new Aenum[] { Aenum.a1, Aenum.a2, Aenum.a1, Aenum.a4 };


            ValueArrayPresentationItem = new PresentationItem[Application.Current.Resources.Count];
            ValueArrayPresentationGenericInt = new PresentationGeneric<int>[Application.Current.Resources.Count];

            ValueListInt = new List<int>() { 1, 2, 3, 4, 5, 6 };

            ValueListBool = new List<bool>() { false, true, false, false };

            ValueListEnum = new List<Aenum>() { Aenum.a1, Aenum.a4, Aenum.a3 };

            ValueListPresentationItem = new List<PresentationItem>() { new PresentationItem() { ItemName = "Item1", ItemValue = 12.5 } };

            ValueListPresentationGenericString = new List<PresentationGeneric<string>>() { 
                new PresentationGeneric<string>() { Value = "имя" }
            };

            ValueGenericListString = new PresentationList<string>() { "newname", "newname1", "uhjkhkjh" };

            ValueGenericListPresentationItem = new PresentationList<PresentationItem>() { 
                new PresentationItem() { ItemName = "Item 1", ItemValue = 12.5 },
                new PresentationItem() { ItemName = "Item 2", ItemValue = 156.3 }
            };

            ValueGenericLisPresentationGenericInt = new PresentationList<PresentationGeneric<int>> {
                new PresentationGeneric<int>() { Value = 0 },
                new PresentationGeneric<int>() { Value = 1 }
            };

            ValueDictionaryStringString = new Dictionary<int, int>();

            ValueDictionaryDoublePresentationItem = new Dictionary<double, PresentationItem>();

            ValueDictionaryDoublePresentationItem2 = new Dictionary<double, PresentationItem>();

            ValueDictionaryPresentationItemPresentationListString = new Dictionary<PresentationItem, PresentationList<string>>();


            int i = 0;
            foreach (var key in Application.Current.Resources.Keys)
            {
                ValueArrayPresentationItem[i] = new PresentationItem() { ItemName = key.ToString(), ItemValue = i * 0.158 };
                ValueArrayPresentationGenericInt[i] = new PresentationGeneric<int>() { Value = i * 5 };
                ValueListInt.Add(i);
                ValueListPresentationItem.Add(new PresentationItem() { ItemName = key.ToString(), ItemValue = i });

                ValueGenericListString.Add(key.ToString());
                ValueGenericListPresentationItem.Add(new PresentationItem() { ItemName = key.ToString(), ItemValue = i });
                ValueGenericLisPresentationGenericInt.Add(new PresentationGeneric<int>() { Value = i, Value2 = key.ToString() });

                ValueDictionaryStringString.Add(i, Application.Current.Resources.Keys.Count - i);
                ValueDictionaryDoublePresentationItem.Add(i, new PresentationItem() { ItemName = key.ToString(), ItemValue = i });
                ValueDictionaryDoublePresentationItem2.Add(i, new PresentationItem() { ItemName = key.ToString(), ItemValue = i });
                ValueDictionaryPresentationItemPresentationListString.Add(new PresentationItem() { ItemName = key.ToString(), ItemValue = i }, new PresentationList<string>() { });
                i++;
            }
            ValueInterface = new ChildrenMyInterface();

            Commanda = new Command(x =>
            {
                if (ValueInterface is ChildrenMyInterface)
                    ValueInterface = new Children2MyInterface();
                else
                    ValueInterface = new ChildrenMyInterface();

                Change("ValueInterface");
            }, null);

            Commands = new Command[3];
            Commands[0] = new Command(x =>
            {
                if (ValueInterface is ChildrenMyInterface)
                    ValueInterface = new Children2MyInterface();
                else
                    ValueInterface = new ChildrenMyInterface();

                Change("ValueInterface");
            }, null);

            Commands[1] = new Command(x =>
            {
                if (ValueInterface is ChildrenMyInterface)
                    ValueInterface = new Children2MyInterface();
                else
                    ValueInterface = new ChildrenMyInterface();

                Change("ValueInterface");
            }, null);

            Commands[2] = new Command(x =>
            {
                if (ValueInterface is ChildrenMyInterface)
                    ValueInterface = new Children2MyInterface();
                else
                    ValueInterface = new ChildrenMyInterface();

                Change("ValueInterface");
            }, null);

            ValueListCommand = new List<Command>();

            for (i = 0; i < 10; i++)
                ValueListCommand.Add(new Command(x =>
                {
                    if (ValueInterface is ChildrenMyInterface)
                        ValueInterface = new Children2MyInterface();
                    else
                        ValueInterface = new ChildrenMyInterface();

                    Change("ValueInterface");
                }, null));

            ValueDictionaryStringCommand = new Dictionary<string, Command>();

            for (i = 0; i < 3; i++)
                ValueDictionaryStringCommand.Add("Key -" + i, new Command(x =>
                                {
                                    if (ValueInterface is ChildrenMyInterface)
                                        ValueInterface = new Children2MyInterface();
                                    else
                                        ValueInterface = new ChildrenMyInterface();

                                    Change("ValueInterface");
                                }, null));

            ValueStructure = new PresentatinoStructure() { ValuePresentationItem = new PresentationItem() };
        }

        #region structure
        [PresentationGroupInfo(GroupName = "Structure")]
        public PresentatinoStructure ValueStructure { get; set; }
        #endregion

        #region Simple
        [PresentationGroupInfo(GroupName = "Simple values")]
        public byte ValueByte { get; set; }
        [PresentationGroupInfo(GroupName = "Simple values")]
        public sbyte ValueSByte { get; set; }

        [PresentationGroupInfo(GroupName = "Simple values")]
        public short ValueShort { get; set; }
        [PresentationGroupInfo(GroupName = "Simple values")]
        public ushort ValueUshort { get; set; }

        [PresentationGroupInfo(GroupName = "Simple values")]
        public Int64 ValueInt32 { get; set; }
        [PresentationGroupInfo(GroupName = "Simple values")]
        public int ValueInt { get; set; }
        [PresentationGroupInfo(GroupName = "Simple values")]
        public uint ValueUint { get; set; }

        [PresentationGroupInfo(GroupName = "Simple values")]
        public long ValueLong { get; set; }
        [PresentationGroupInfo(GroupName = "Simple values")]
        public ulong ValueUlong { get; set; }

        [PresentationGroupInfo(GroupName = "Simple values")]
        public float ValueFloat { get; set; }
        [PresentationGroupInfo(GroupName = "Simple values")]
        public double ValueDouble { get; set; }
        [PresentationGroupInfo(GroupName = "Simple values")]
        public decimal ValueDecimal { get; set; }

        [PresentationGroupInfo(GroupName = "Simple values")]
        public char ValueChar { get; set; }
        [PresentationGroupInfo(GroupName = "Simple values")]
        public string ValueString { get; set; }

        [PresentationGroupInfo(GroupName = "Simple values")]
        public bool ValueBool { get; set; }
        #endregion

        #region Enum
        [PresentationGroupInfo(GroupName = "Enum")]
        public Aenum Enum { get; set; }
        #endregion

        #region Class
        [PresentationGroupInfo(GroupName = "Class")]
        [PresentationBinding]
        public PresentationItem ValueClass
        {
            get { Thread.Sleep(15000); return p; }
            set { p = value; }
        }
        #endregion

        #region Generic
        [PresentationGroupInfo(GroupName = "Generic")]
        public PresentationGeneric<int> presGeneric { get; set; }
        [PresentationGroupInfo(GroupName = "Generic")]
        public PresentationGeneric<string> presGenericString { get; set; }
        [PresentationGroupInfo(GroupName = "Generic")]
        public PresentationGeneric<PresentationItem> presGenericPresentationItem { get; set; }

        [PresentationGroupInfo(GroupName = "Generic")]
        public PresentationGeneric<int, PresentationGeneric<bool>> presGeneric1 { get; set; }
        #endregion

        #region Array
        [PresentationGroupInfo(GroupName = "Array")]
        [PresentationInfo(CaptionName = "Массив целых чисел", MaxHeight = 100, MinHeight = 20, PresentCaption = TextWrapping.Wrap)]
        public int[] ValueArrayInt { get; set; }

        [PresentationGroupInfo(GroupName = "Array")]
        public uint[] ValueArrayUInt { get; set; }

        [PresentationGroupInfo(GroupName = "Array")]
        public byte[] ValueArrayByte { get; set; }

        [PresentationGroupInfo(GroupName = "Array")]
        public sbyte[] ValueArraySByte { get; set; }

        [PresentationGroupInfo(GroupName = "Array")]
        public long[] ValueArrayLong { get; set; }

        [PresentationGroupInfo(GroupName = "Array")]
        public ulong[] ValueArrayULong { get; set; }

        [PresentationGroupInfo(GroupName = "Array")]
        public short[] ValueArrayShort { get; set; }

        [PresentationGroupInfo(GroupName = "Array")]
        public ushort[] ValueArrayUShort { get; set; }

        [PresentationGroupInfo(GroupName = "Array")]
        public float[] ValueArrayFloat { get; set; }

        [PresentationGroupInfo(GroupName = "Array")]
        public double[] ValueArrayDouble { get; set; }

        [PresentationGroupInfo(GroupName = "Array")]
        public decimal[] ValueArrayDecimal { get; set; }

        [PresentationGroupInfo(GroupName = "Array")]
        public char[] ValueArrayChar { get; set; }

        [PresentationGroupInfo(GroupName = "Array")]
        public string[] ValueArrayString { get; set; }

        [PresentationGroupInfo(GroupName = "Array")]
        public bool[] ValueArrayBool { get; set; }

        [PresentationGroupInfo(GroupName = "Array")]
        public Aenum[] ValueArrayEnum { get; set; }

        [PresentationGroupInfo(GroupName = "Array")]
        public PresentationItem[] ValueArrayPresentationItem { get; set; }

        [PresentationGroupInfo(GroupName = "Array")]
        public PresentationGeneric<int>[] ValueArrayPresentationGenericInt { get; set; }

        [PresentationGroupInfo(GroupName = "Array")]
        public Command[] Commands { get; set; }
        #endregion

        #region List
        [PresentationGroupInfo(GroupName = "List")]
        [PresentationInfo(CaptionName = "Список целых чисел", MaxHeight = 100, MinHeight = 20, PresentCaption = TextWrapping.Wrap)]
        public List<int> ValueListInt { get; set; }

        [PresentationGroupInfo(GroupName = "List")]
        public List<bool> ValueListBool { get; set; }

        [PresentationGroupInfo(GroupName = "List")]
        public List<Aenum> ValueListEnum { get; set; }

        [PresentationGroupInfo(GroupName = "List")]
        public List<PresentationItem> ValueListPresentationItem { get; set; }

        [PresentationGroupInfo(GroupName = "List")]
        public List<PresentationGeneric<string>> ValueListPresentationGenericString { get; set; }

        [PresentationGroupInfo(GroupName = "List")]
        public List<Command> ValueListCommand { get; set; }
        #endregion

        #region MyList
        [PresentationGroupInfo(GroupName = "MyList")]
        public PresentationList<string> ValueGenericListString { get; set; }

        [PresentationGroupInfo(GroupName = "MyList")]
        public PresentationList<PresentationItem> ValueGenericListPresentationItem { get; set; }

        [PresentationGroupInfo(GroupName = "MyList")]
        public PresentationList<PresentationGeneric<int>> ValueGenericLisPresentationGenericInt { get; set; }
        #endregion

        #region Dictionary
        [PresentationGroupInfo(GroupName = "Dictionary")]
        [PresentationInfo(CaptionName = "словарь целых чисел", MaxHeight = 200, MinHeight = 20, PresentCaption = TextWrapping.Wrap)]
        [PresentationMarginInfo()]
        public Dictionary<int, int> ValueDictionaryStringString { get; set; }

        [PresentationGroupInfo(GroupName = "Dictionary")]
        [PresentationMarginInfo()]
        public Dictionary<double, PresentationItem> ValueDictionaryDoublePresentationItem { get; set; }

        [PresentationGroupInfo(GroupName = "Dictionary")]
        [PresentationMarginInfo()]
        public Dictionary<double, PresentationItem> ValueDictionaryDoublePresentationItem2 { get; set; }

        [PresentationGroupInfo(GroupName = "Dictionary")]
        [PresentationMarginInfo()]
        public Dictionary<PresentationItem, PresentationList<string>> ValueDictionaryPresentationItemPresentationListString { get; set; }

        [PresentationGroupInfo(GroupName = "Dictionary")]
        public Dictionary<string, Command> ValueDictionaryStringCommand { get; set; }
        #endregion

        #region Commands
        [PresentationGroupInfo(GroupName = "Command")]
        public Command Commanda { get; set; }
        #endregion

        #region Interface
        [PresentationGroupInfo(GroupName = "Interface")]
        public MyInterface ValueInterface { get; set; }
        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void Change(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        #endregion
    }

    public struct PresentatinoStructure
    {
        public int ValueInt { get; set; }

        public string ValueString { get; set; }

        public PresentationItem ValuePresentationItem { get; set;}
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
        private string _ItemName = "result";

        public string ItemName 
        {
            get { return _ItemName; }
            set { _ItemName = value; }
        }

        public double ItemValue { get; set; }
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

    public interface MyInterface
    {
        int ValueInt { get; set; }

        string ValueString { get; set; }

        PresentationItem ValuePresentationItem { get; set; }
    }

    public class ChildrenMyInterface : MyInterface
    {

        public ChildrenMyInterface()
        {
            ValueIntChildrenMyInterface = 112;
            ValueInt = 23;
            ValueString = "25qwerty";
            ValuePresentationItem = new PresentationItem() { ItemName = "Item children", ItemValue = 85.2 };
        }

        public int ValueIntChildrenMyInterface { get; set; }

        #region MyInterface Members

        public int ValueInt
        {
            get;
            set;
        }

        public string ValueString
        {
            get;
            set;
        }

        public PresentationItem ValuePresentationItem
        {
            get;
            set;
        }

        #endregion
    }

    public class Children2MyInterface : MyInterface
    {
        public Children2MyInterface()
        {
            ValueChildrenMyListString = new PresentationList<string>();
            ValueInt = 55;
            ValueString = "qazxsw";
            ValuePresentationItem = new PresentationItem() { ItemName = "Children2MyInterface", ItemValue = 2 };
        }

        public PresentationList<string> ValueChildrenMyListString { get; set; }

        #region MyInterface Members

        public int ValueInt
        {
            get;
            set;
        }

        public string ValueString
        {
            get;
            set;
        }

        public PresentationItem ValuePresentationItem
        {
            get;
            set;
        }

        #endregion
    }
}
