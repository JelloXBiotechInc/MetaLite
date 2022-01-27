using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace MetaLite_Viewer.Helper
{
    public class CellMask
    {
        public double Percentage { get; set; }
        public List<List<byte>> CellArea { get; set; }
    }
    public class FunctionData
    {
        public string FunctionName { get; set; }
        public string PostUri { get; set; }
        public bool IsSendImage { get; set; }
        public string ImagePath { get; set; }
        public bool IsSendMask { get; set; }
        public List<Column> Content { get; set; }
    }

    public class ReturnData
    {
        public string varName { get; set; }
        public string varType { get; set; }
    }

    public class Column
    {
        public string parameterType { get; set; }
        public string title { get; set; }
        public string parameterName { get; set; }
        public bool auto { get; set; }
        public Range range { get; set; }
        public List<Data> datas { get; set; }
        public List<string> items { get; set; }
        public bool boolean { get; set; }
    }

    public class Data
    {        
        public string varName { get; set; }
        public object value { get; set; }
    }

    public class Image
    {
        public string imageName { get; set; }
        public List<List<byte>> value { get; set; }
        public string colorString { get; set; }
    }

    public class JsonData
    {
        public List<Data> Datas { get; set; }
        public List<Image> Images { get; set; }

    }

    public class Range: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private double min { get; set; }
        private double max { get; set; }
        private double from { get; set; }
        private double to { get; set; }

        public double From
        {
            get { return from; }
            set
            {
                if (From != value)
                {
                    from = value;
                    OnPropertyChanged(nameof(From));
                }
            }
        }
        public double To
        {
            get { return to; }
            set
            {
                if (To != value)
                {
                    to = value;
                    OnPropertyChanged(nameof(To));
                }
            }
        }
        public double Min
        {
            get { return min; }
            set
            {
                if (Min != value)
                {
                    min = value;
                    OnPropertyChanged(nameof(Min));
                }
            }
        }
        public double Max
        {
            get { return max; }
            set
            {
                if (Max != value)
                {
                    max = value;
                    OnPropertyChanged(nameof(Max));
                }
            }
        }
    }

    public class DropOutStack<T> : LinkedList<T>
    {
        private readonly int _capacity;

        public DropOutStack(int capacity)
        {
            _capacity = capacity;
        }

        public void Push(T item)
        {
            lock (this)
            {
                if (this.Count >= _capacity)
                {
                    this.RemoveLast();
                }

                this.AddFirst(item);
            }            
        }

        public T Pop()
        {
            if (!this.Any())
            {
                throw new InvalidOperationException("Stack empty.");
            }

            var item = First;
            RemoveFirst();

            return item.Value;
        }

        public bool notEmpty
        {
            get
            {
                if (this.Count > 0)
                    return true;
                else
                    return false;
            }
        }
    }
    
    public class SvsTile
    {
        public readonly int ByteWidth;
        public readonly byte[] Data;
        public SvsTile(byte[] data, int byteWidth)
        {
            ByteWidth = byteWidth;
            Data = data;
        }
    }

    public class DropOutDict<Tkey, T> : Dictionary<Tkey,T>
    {
        private readonly int _capacity;

        private Queue<Tkey> TimeOutQueue = new Queue<Tkey>();

        public DropOutDict(int capacity)
        {
            _capacity = capacity;
        }

        public void AddItem(Tkey key, T item)
        {
            
            lock (this)
            {
                if (this.Count >= _capacity)
                {
                    var throwKey = TimeOutQueue.Dequeue();
                    if (ContainsKey(throwKey))
                        this.Remove(throwKey);
                }
                if (!this.ContainsKey(key))
                {
                    this.Add(key, item);
                    TimeOutQueue.Enqueue(key);
                }
            }
        }

        public bool notEmpty
        {
            get
            {
                if (this.Count > 0)
                    return true;
                else
                    return false;
            }
        }
    }
}
