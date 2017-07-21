using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UniSearchScriptBase;

namespace UniSearchTestWpf
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SubDemo(object sender, RoutedEventArgs e) {
            var test_str = @"~~~(~~$name.tag ~ [tall, gay guy] & ~$age >= 20 | ~($k ))";
//          var shs = FilterBuilder<TestClass>.Parse(test_str);
//            var qk = new List<TestClass>().Where(shs);
            var k1 = new[] {1, 2, 3};
            var k2 = new List<int> {1, 2, 3};

            // ReSharper disable once ConvertClosureToMethodGroup
            TypeHelper.Init(name => Type.GetType(name));
            TypeHelper.Override[(typeof(Level2), "abc")] = new PropInfo(obj => obj.To<Level2>().L3, typeof(Level3), false, false);
                

            var dict = TypeHelper.TypeDict;
            //var tc = TypeHelper.GetPropDict(typeof(TestCompClass));

            var str = ".any.l1s/.l2.abc.nums.count";
            var strs = FilterBuilder.ReplaceProp(str);
            var t0 = FilterBuilder<TestCompClass[]>.GetPropType(strs);

            var k = 0;

        }


        private static void SubTestClass() {
            var e1 = new TestClass("Smith", 25, new[] {new Tag(" "), new Tag("short")}, true, EnmGender.Male);
            var e2 = new TestClass("Mary", 15, new[] {new Tag("short")}, true, EnmGender.Female);
            var e3 = new TestClass("John", 86, null, false, EnmGender.Other);
            var ie = new[] {e1, e2, e3}.ToList();



            var type = ie.GetType();
            var genericTypeArguments = type.GenericTypeArguments;
            var h = genericTypeArguments;


            var testType = type.GenericTypeArguments[0];
            var props = testType.GetProperties();

            var propP = props.Select(prop => prop.PropertyType.Name);
            var propI = props.Select(prop => prop.PropertyType.GetInterfaces().Any(i => i.Name == "IEnumerable"));
            var propG = props.Select(prop => prop.PropertyType.GetGenericArguments());
        }
    }

    public enum EnmGender { Male,Female,Other}

    public struct Tag {
        public Tag(string name) { this.Name = name; }
        public string Name { get; set; }
    }

    public class TestClass {
        public TestClass(string name, int age, IEnumerable<Tag> tag, bool alive, EnmGender gender) {
            this.Name = name;
            this.Age = age;
            this.Tag = tag;
            this.Alive = alive;
            this.Gender = gender;
        }
        public string Name { get; set; }
        public int Age { get; set; }
        public IEnumerable<Tag> Tag { get; set; }
        public bool Alive { get; set; }
        public EnmGender Gender { get; set; }

        public double GetAge() => this.Age;
    }
}
