using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NTW.Commands;
using NTW.Presentation.Models;
using NTW.Presentation.Construction;

namespace NTW.Presentation
{
    internal class DictionaryViewModel<TKey, TValue>:AbstractDictionaryView
    {
        #region Private
        private TKey selectedKey;
        private TValue defaultValue;

        private Command addCommand;
        private Command removeCommand;
        private Command clearCommand;
        #endregion

        public DictionaryViewModel() { }

        #region Public
        public List<TKey> MKeys
        {
            get 
            {
                List<TKey> temp = new List<TKey>();
                if (Items != null)
                {
                    foreach (TKey item in (Items as IDictionary<TKey, TValue>).Keys)
                        temp.Add(item);
                }
                return temp;
            }
        }

        public TKey SelectedKey
        {
            get { 
                if(Items != null && selectedKey == null)
                    foreach (TKey key in Items.Keys)
                    {
                        selectedKey = key;
                        break;
                    }
                return selectedKey; 
            }
            set { selectedKey = value; Change("SelectedKey"); Change("Value"); }
        }

        public TValue Value
        {
            get { if(selectedKey != null && Items != null && Items.Contains(selectedKey)) return (Items as IDictionary<TKey, TValue>)[selectedKey]; else return defaultValue; }
            set { (Items as IDictionary<TKey, TValue>)[selectedKey] = value; }
        }
        #endregion

        #region Commands
        public Command AddCommand
        {
            get 
            {
                return addCommand ?? (addCommand = new Command(obj => {

                    Result<object> res = GenerateKey();
                    if (res.ActiveResult)
                    {
                        object key, value;
                        key = res.ResultChanage;

                        value = Activator.CreateInstance(typeof(TValue));

                        Items.Add(key, value);
                        Change("MKeys");
                        SelectedKey = (TKey)key;
                    }
                }, obj => Items != null));
            }
        }

        public Command RemoveCommand
        {
            get { return removeCommand ?? (removeCommand = new Command(obj => {
                if (Items != null)
                {
                    Items.Remove(selectedKey);
                    Change("MKeys");
                    if (Items.Count > 0)
                    {
                        foreach (TKey item in Items.Keys)
                        {
                            SelectedKey = item;
                            break;
                        }
                    }
                    else
                        SelectedKey = (TKey)GenerateKey().ResultChanage;
                }
            }, obj => selectedKey != null && (object)selectedKey != GenerateKey().ResultChanage));
            }
        }
        #endregion

        #region Helps
        private Result<object> GenerateKey()
        {
            object value = null;
            if (TypeBuilder.SimpleTypes.Contains(typeof(TKey)))//значит простой тип
            {
                #region string
                if (typeof(TKey) == typeof(string))
                {
                    //делаем генерацию ключа по шаблону "key#"
                    bool tr = true;
                    int index = 1;//отсчет с еденицы (идентификатор)
                    while (tr)
                    {
                        value = "key" + index.ToString();
                        index++;
                        tr = Items.Contains(value);
                    }

                    return new Result<object>(value);
                } 
                #endregion
                #region int
                else if (typeof(TKey) == typeof(int))
                {
                    //простой числовой сдвиг
                    bool tr = true;
                    int index = 1;//отсчет с еденицы (идентификатор)
                    while (tr)
                    {
                        value = index;
                        index++;
                        if (Items != null)
                            tr = Items.Contains(value);
                        else
                            tr = false;
                    }

                    return new Result<object>(value);
                } 
                #endregion
                #region Long
                else if (typeof(TKey) == typeof(long))
                {
                    //простой числовой сдвиг
                    bool tr = true;
                    long index = 1;//отсчет с еденицы (идентификатор)
                    while (tr)
                    {
                        value = index;
                        index++;
                        tr = Items.Contains(value);
                    }

                    return new Result<object>(value);
                } 
                #endregion
                #region bool
                else if (typeof(TKey) == typeof(bool))//глупо но все же
                {
                    value = true;
                    if (Items.Contains(value))
                        value = false;
                    if (Items.Contains(value))
                        return new Result<object>(false);
                    else
                        return new Result<object>(value);
                } 
                #endregion
                #region byte
                else if (typeof(TKey) == typeof(byte))
                {
                    //byte вроде как ограничен только 265 значениями начиная с 0
                    for (byte i = 0; i < 256; i++)
                    {
                        value = i;
                        if (!Items.Contains(value))
                            return new Result<object>(value);
                    }
                    return new Result<object>(false);
                } 
                #endregion
                #region sbyte
                else if (typeof(TKey) == typeof(sbyte))
                {
                    //byte вроде как ограничен от -128 до 127
                    for (sbyte i = -128; i < 128; i++)
                    {
                        value = i;
                        if (!Items.Contains(value))
                            return new Result<object>(value);
                    }
                    return new Result<object>(false);
                } 
                #endregion
                #region char
                else if (typeof(TKey) == typeof(char))
                {
                    return new Result<object>(false);
                } 
                #endregion
                else
                    return new Result<object>(false);
            }
            else
            {
                value = Activator.CreateInstance(typeof(TKey));
                return new Result<object>(value);
            }
        }
        #endregion
    }
}
