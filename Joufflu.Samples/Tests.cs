using System.Collections;
using System.ComponentModel;
using System.Reflection.Emit;
using Usuel.Shared;

namespace Joufflu.Samples
{
    public enum EnumTest
    {
        None,
        One,
        Two,
    }

    public class TestClass : INotifyDataErrorInfo
    {
        private string _name = "default";
        public string Name
        {
            get => _name; 
            set
            {
                Errors.Clear();
                if (_name == value)
                    return;

                if (value == "wrong")
                {
                    Errors.Add("Name can't be wrong.");
                    return;
                }
                _name = value;
            }
        }

        public int Value { get; set; } = 20;

        public bool IsTest { get; set; }

        public EnumTest EnumTest { get; set; }

        public string FilePath { get; set; } = "";
        public DateTime Date { get; set; } = DateTime.Now;
        public TimeSpan Duration { get; set; } = TimeSpan.FromSeconds(1);

        public TestClass()
        {
            Name = "Default";
        }

        public TestClass(string name, int value)
        {
            Name = name;
            Value = value;
        }

        public override string ToString() { return $"{Name} : {Value}"; }

        public override bool Equals(object? obj) { return obj is TestClass value && Name == value.Name; }

        public override int GetHashCode() { return Name.GetHashCode(); }

        public ErrorValidation Errors { get; } = new ErrorValidation();
        public IEnumerable GetErrors(string? propertyName) => Errors.GetErrors(propertyName);

        public bool HasErrors => Errors.HasErrors;
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged
        {
            add => Errors.ErrorsChanged += value;
            remove => Errors.ErrorsChanged -= value;
        }
    }

    public class TestClassWithSub
    {
        public List<string> StringArray { get; set; } = new List<string>() { "Toto" };
        public List<TestClass> ArraySub { get; set; } = new List<TestClass>();
        public TestClass Sub { get; set; } = new TestClass("tata", 10);
    }

    public static class Tests
    {
        public static TestClass Value { get; set; } = new TestClass("Minus", -1);
        public static TestClassWithSub ValueWithSub { get; set; } = new TestClassWithSub();

        public static List<TestClass> Values
        {
            // CreateValue a new instance because of CollectionViewSource.GetDefaultView
            get => new List<TestClass>
            {
                new TestClass("One", 1),
                new TestClass("Two", 2),
                new TestClass("Three", 3),
                new TestClass("Four", 4),
                new TestClass("Five", 5),
                new TestClass("Six", 6),
                new TestClass("Seven", 7),
                new TestClass("Eight", 8),
                new TestClass("Nine", 9),
                new TestClass("Ten", 10),
                new TestClass("Eleven", 11),
                new TestClass("Twelve", 12),
                new TestClass("Thirteen", 13),
                new TestClass("Fourteen", 14),
                new TestClass("Fifteen", 15),
                new TestClass("Sixteen", 16),
                new TestClass("Seventeen", 17),
                new TestClass("Eighteen", 18),
                new TestClass("Nineteen", 19),
                new TestClass("Twenty", 20),
                new TestClass("Twenty-One", 21),
            };
        }

        public static List<string> StringValues
        {
            get => new List<string>
            {
                "One",
                "Two",
                "Three",
                "Four",
                "Five",
                "Six",
                "Seven",
                "Eight",
                "Nine",
                "Ten",
                "Eleven",
                "Twelve",
                "Thirteen",
                "Fourteen",
                "Fifteen",
                "Sixteen",
                "Seventeen",
                "Eighteen",
                "Nineteen",
                "Twenty",
                "Twenty-One"
            };
        }
    }
}
