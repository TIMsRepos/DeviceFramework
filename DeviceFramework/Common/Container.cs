namespace TIM.Devices.Framework.Common
{
    public class Container<T1, T2>
    {
        public T1 Value1 { get; set; }
        public T2 Value2 { get; set; }

        public Container()
        {
        }

        public Container(T1 MyValue1, T2 MyValue2)
        {
            this.Value1 = MyValue1;
            this.Value2 = MyValue2;
        }
    }

    public class Container<T1, T2, T3>
    {
        public T1 Value1 { get; set; }
        public T2 Value2 { get; set; }
        public T3 Value3 { get; set; }

        public Container()
        {
        }

        public Container(T1 MyValue1, T2 MyValue2, T3 MyValue3)
        {
            this.Value1 = MyValue1;
            this.Value2 = MyValue2;
            this.Value3 = MyValue3;
        }
    }

    public class Container<T1, T2, T3, T4>
    {
        public T1 Value1 { get; set; }
        public T2 Value2 { get; set; }
        public T3 Value3 { get; set; }
        public T4 Value4 { get; set; }

        public Container()
        {
        }

        public Container(T1 MyValue1, T2 MyValue2, T3 MyValue3, T4 MyValue4)
        {
            this.Value1 = MyValue1;
            this.Value2 = MyValue2;
            this.Value3 = MyValue3;
            this.Value4 = MyValue4;
        }
    }

    public class Container<T1, T2, T3, T4, T5>
    {
        public T1 Value1 { get; set; }
        public T2 Value2 { get; set; }
        public T3 Value3 { get; set; }
        public T4 Value4 { get; set; }
        public T5 Value5 { get; set; }

        public Container()
        {
        }

        public Container(T1 MyValue1, T2 MyValue2, T3 MyValue3, T4 MyValue4, T5 MyValue5)
        {
            this.Value1 = MyValue1;
            this.Value2 = MyValue2;
            this.Value3 = MyValue3;
            this.Value4 = MyValue4;
            this.Value5 = MyValue5;
        }
    }
}