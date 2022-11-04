using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using FluentAssertions;
using System;


namespace MyStringCalculator
{
    internal class MyStringCalculator
    {
        public int Add(string numbers)
        {
            if (numbers == string.Empty)
                return 0;

            var separators = ParseSeparators(ref numbers);

            var nums = numbers.Split(separators).Select(int.Parse);

            if(nums.Any(n => n < 0))
            {
               var list = nums.Where(n => n < 0).ToArray();

                throw new ArgumentException("negatives not allowed: " + string.Join(",", list));
            }
            return nums.Sum();
        }

        private char[] ParseSeparators(ref string numbers)
        {
            var separators = new List<char> { ',', '\n' };

            if (numbers.StartsWith("//"))
            {
                char optionalSeparator = numbers[2];

                int newLineSymbolPosition = numbers.IndexOf('\n');

                numbers = numbers.Substring(newLineSymbolPosition + 1);

                separators.Add(optionalSeparator);
            }

            return separators.ToArray();
        }
    }


    [TestFixture]
    public class MyStringCalculator_Should
    {
        private MyStringCalculator _calculator;

        [SetUp]
        public void Setup()
        {
            _calculator = new MyStringCalculator();
        }


        [TestCase("", ExpectedResult = 0)]
        [TestCase("1", ExpectedResult = 1)]
        [TestCase("1, 5", ExpectedResult = 6)]
        [TestCase("1,2,3,4,5", ExpectedResult = 15)]
        public int ReturnSum_WhenNumbersAreSeparatedByComma(string numbers)
        {
            return _calculator.Add(numbers);
        }

        [TestCase("1\n3", ExpectedResult = 4)]
        [TestCase("1\n3\n5", ExpectedResult = 9)]
        public int ReturnSum_WhenNumbersAreSeparatedByNewLine(string numbers)
        {
            return _calculator.Add(numbers);
        }

        [TestCase("1,3\n5", ExpectedResult = 9)]
        public int ReturnSum_WhenNumbersAreSeparatedByCommaAndNewLine(string numbers)
        {
            return _calculator.Add(numbers);
        }

        [Test]
        public void ThrowsException_WhenNewLineAfterComma()
        {
            Action action = () => _calculator.Add("1,\n");

            action.Should().Throw<Exception>();
        }

        [TestCase("//;\n1;2", ExpectedResult = 3)]
        [TestCase("//;\n1;2,3", ExpectedResult = 6)]
        [TestCase("//;\n1;2,3\n4", ExpectedResult = 10)]
        public int ReturnSun_WithOptionalSeparator(string numbers)
        {
            return _calculator.Add(numbers);
        }

        [Test]
        public void ThrowsException_WhenInvalidSeparatorsSequence()
        {
            Action action = () => _calculator.Add("1,\n");

            action.Should().Throw<Exception>("//;\n1;,3");
        }

        [Test]
        public void ThrowsArgumentException_WhenNegativeNumbersExist()
        {
            Action action = () => _calculator.Add("-1");

            action.Should().Throw<ArgumentException>();
        }

        [Test]
        public void ThrowsArgumentExceptionWithMessage_WhenNegativeNumbersExist()
        {
            Action action = () => _calculator.Add("-1,-2,3,-4,5");

            action.Should().Throw<ArgumentException>().WithMessage("negatives not allowed: -1,-2,-4");
        }

    }
}
