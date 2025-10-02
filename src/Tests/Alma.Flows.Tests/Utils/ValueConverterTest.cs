using Alma.Flows.Core.Activities.Models;
using Alma.Flows.Models.Activities;
using Alma.Flows.Utils;
using System.Globalization;

namespace Alma.Flows.Tests.Utils
{
    public class ValueConverterTest
    {
        private enum TestEnum
        {
            A,
            B,
            C
        }

        [SetUp]
        public void SetUp()
        {
            // Ensure culture-independent numeric/date conversions
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        }

        [Test]
        public void Convert_Generic_Primitives_ShouldConvert()
        {
            // string
            Assert.That(ValueConverter.Convert<string>("abc"), Is.EqualTo("abc"));

            // int
            Assert.That(ValueConverter.Convert<int>("123"), Is.EqualTo(123));

            // long
            Assert.That(ValueConverter.Convert<long>("9223372036854775806"), Is.EqualTo(9223372036854775806L));

            // double
            Assert.That(ValueConverter.Convert<double>("123.45"), Is.EqualTo(123.45));

            // decimal
            Assert.That(ValueConverter.Convert<decimal>("123.45"), Is.EqualTo(123.45m));

            // bool
            Assert.That(ValueConverter.Convert<bool>("true"), Is.True);

            // DateTime
            var dt = ValueConverter.Convert<DateTime>("2024-05-01");
            Assert.That(dt.Date, Is.EqualTo(new DateTime(2024, 5, 1)));
        }

        [Test]
        public void Convert_Generic_Null_String_ShouldReturnNull()
        {
            string? s = ValueConverter.Convert<string>(null);
            Assert.That(s, Is.Null);
        }

        [Test]
        public void Convert_NonGeneric_Null_ShouldReturnNull()
        {
            Assert.That(ValueConverter.Convert(typeof(int), null), Is.Null);
            Assert.That(ValueConverter.Convert(typeof(DateTime), null), Is.Null);
            Assert.That(ValueConverter.Convert(typeof(Dictionary<string, string>), null), Is.Null);
        }

        [Test]
        public void Convert_Generic_Enum_ShouldParseOrDefault()
        {
            // Parse value (case-insensitive)
            var e1 = ValueConverter.Convert<TestEnum>("b");
            Assert.That(e1, Is.EqualTo(TestEnum.B));

            // Empty -> first enum value
            var e2 = (TestEnum?)ValueConverter.Convert(typeof(TestEnum), "");
            Assert.That(e2, Is.EqualTo(TestEnum.A));
        }

        [Test]
        public void Convert_NonGeneric_Dictionary_ShouldDeserialize()
        {
            // Empty -> empty dictionary
            var empty = (Dictionary<string, string>?)ValueConverter.Convert(typeof(Dictionary<string, string>), "");
            Assert.That(empty, Is.Not.Null);
            Assert.That(empty!.Count, Is.EqualTo(0));

            // Valid json
            var json = "{\"a\":\"1\",\"b\":\"2\"}";
            var dict = (Dictionary<string, string>?)ValueConverter.Convert(typeof(Dictionary<string, string>), json);
            Assert.That(dict, Is.Not.Null);
            Assert.That(dict!["a"], Is.EqualTo("1"));
            Assert.That(dict["b"], Is.EqualTo("2"));
        }

        [Test]
        public void Convert_NonGeneric_FormFieldCollection_ShouldDeserialize()
        {
            // Empty -> empty collection
            var empty = (ICollection<FormField>?)ValueConverter.Convert(typeof(ICollection<FormField>), "");
            Assert.That(empty, Is.Not.Null);
            Assert.That(empty!.Count, Is.EqualTo(0));

            var json = "[ { \"Name\": \"firstName\", \"Label\": \"First Name\", \"Type\": 0, \"Placeholder\": \"Enter\", \"Required\": true } ]";
            var list = (ICollection<FormField>?)ValueConverter.Convert(typeof(ICollection<FormField>), json);
            Assert.That(list, Is.Not.Null);
            Assert.That(list!.Count, Is.EqualTo(1));
            var item = list.First();
            Assert.That(item.Name, Is.EqualTo("firstName"));
            Assert.That(item.Label, Is.EqualTo("First Name"));
            Assert.That(item.Placeholder, Is.EqualTo("Enter"));
            Assert.That(item.Required, Is.True);
        }

        [Test]
        public void Convert_NonGeneric_ParameterOption_ShouldCreateWithValue()
        {
            var po = (ParameterOption?)ValueConverter.Convert(typeof(ParameterOption), "x");
            Assert.That(po, Is.Not.Null);
            Assert.That(po!.Value, Is.EqualTo("x"));
            Assert.That(po.DisplayName, Is.EqualTo(string.Empty));
        }

        [Test]
        public void Convert_NonGeneric_UnsupportedType_ShouldThrow()
        {
            Assert.Throws<InvalidOperationException>(() => ValueConverter.Convert(typeof(Guid), "abc"));
        }
    }
}