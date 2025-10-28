using FluentAssertions;
using NUnit.Framework;

namespace HomeExercise.Tasks.NumberValidator;

[TestFixture]
public class NumberValidatorTests
{
    #region Допустимая разрядность
    
    [TestCase(-1, 2, true)]
    [TestCase(0, 2, true)]
    public void Ctor_InvalidPrecision_ThrowsArgumentException(int precision, int scale, bool onlyPositive)
    {
        var act = () => new NumberValidator(precision, scale, onlyPositive);
        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("precision must be a positive number");
    }

    #endregion

    #region Некорректная дробность
    
    [TestCase(5, -1, true)]
    public void Ctor_InvalidScale_Negative_ThrowsArgumentException(int precision,  int scale, bool onlyPositive)
    {
        var act = () => new NumberValidator(precision, scale, onlyPositive);
        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("*non-negative*"); // узнал, что можно передавать часть строки
    }

    [TestCase(1, 2, true)]
    [TestCase(1, 1, true)]
    public void Ctor_WithScaleGreaterOrEqualToPrecision_ThrowsArgumentException(int precision, int scale, bool onlyPositive)
    {
        var act = () => new NumberValidator(precision, scale, onlyPositive);
        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("*less or equal*"); // узнал, что можно передавать часть строки
    }
    
    #endregion

    #region Корректные аргументы для конструктора

    [Test]
    public void Ctor_ValidArguments_DoesNotThrow()
    {
        var act = () => new NumberValidator(1, 0, true);
        act.Should().NotThrow();
    }
    
    #endregion

    #region Передаваемое число в формате пустой строки или null
    
    [TestCase("", 17, 2, true)]
    [TestCase(null, 17, 2, true)]
    public void IsValidNumber_NullOrEmpty_ReturnsFalse(string value, int precision, int scale, bool onlyPositive)
    {
        var validator = new NumberValidator(precision, scale, onlyPositive);
        var result = validator.IsValidNumber(value);
        result.Should().BeFalse();
    }
    
    #endregion

    #region Некорректный формат передаваемого числа
    
    [Test]
    public void IsValidNumber_MultipleFractionSeparators_ReturnsFalse()
    {
        var validator = new NumberValidator(10, 4, true);
        var result = validator.IsValidNumber("1..2");
        result.Should().BeFalse();
    }
    
    [Test]
    public void IsValidNumber_OnlySigns_ReturnsFalse()
    {
        var validator = new NumberValidator(15, 1, true);
        var result = validator.IsValidNumber("+-.");
        result.Should().BeFalse();
    }
    
    [Test]
    public void IsValidNumber_OnlyDot_ReturnsFalse()
    {
        var validator = new NumberValidator(22, 8, true);
        var result = validator.IsValidNumber(".");
        result.Should().BeFalse();
    }

    [Test]
    public void IsValidNumber_DotLastSign_ReturnsFalse()
    {
        var validator = new NumberValidator(9, 4, true);
        var result = validator.IsValidNumber("1.");
        result.Should().BeFalse();
    }

    [Test]
    public void IsValidNumber_DotFirstSign_ReturnsFalse()
    {
        var validator = new NumberValidator(7, 3, true);
        var result = validator.IsValidNumber(".1");
        result.Should().BeFalse();
    }
    
    [Test]
    public void IsValidNumber_ContainsSpace_ReturnsFalse()
    {
        var validator = new NumberValidator(17, 6, true);
        var result = validator.IsValidNumber("5 2");
        result.Should().BeFalse();
    }

    [Test]
    public void IsValidNumber_ContainsUnderscore_ReturnsFalse()
    {
        var validator = new NumberValidator(11, 3, true);
        var result = validator.IsValidNumber("4_4");
        result.Should().BeFalse();
    }

    [Test]
    public void IsValidNumber_ContainsLettersAndUnderscore_ReturnsFalse()
    {
        var validator = new NumberValidator(10, 5, true);
        var result = validator.IsValidNumber("abcd_4");
        result.Should().BeFalse();
    }
    
    [Test]
    public void IsValidNumber_DoubleMinus_ReturnsFalse()
    {
        var validator = new NumberValidator(12, 2, true);
        var result = validator.IsValidNumber("--1");
        result.Should().BeFalse();
    }
    
    #endregion

    #region Корректное передаваемое число

    [TestCase("+1", 4, 2, true)]
    [TestCase("+0.1", 9, 1, true)]
    [TestCase("000.10", 5, 2, true)]
    [TestCase("000,10", 8, 4, true)]
    [TestCase("10", 2, 0, true)]
    public void IsValidNumber_ValidFormat_ReturnsTrue(string value, int precision, int scale, bool onlyPositive)
    {
        var validator = new NumberValidator(precision, scale, onlyPositive);
        var result = validator.IsValidNumber(value);
        result.Should().BeTrue();
    }
    
    #endregion

    #region Количество допустимых цифр в числе, включая знак, целую и дробную часть, без точки
    
    [TestCase("17.9", 3, 1, true, true)]
    [TestCase("171.9", 3, 1, true, false)]
    [TestCase("+17", 3, 2, true, true)]
    [TestCase("+17", 2, 0, true, false)]
    public void IsValidNumber_RespectsPrecisionLimit_ReturnsExpected(string value, int precision, int scale,
        bool onlyPositive, bool expected)
    {
        var validator = new NumberValidator(precision, scale, onlyPositive);
        var result = validator.IsValidNumber(value);
        result.Should().Be(expected);
    }
    
    #endregion

    #region Количество знаков после запятой

    [TestCase("1.0", 10, 0, true, false)]
    [TestCase("1.44", 10, 2, true, true)]
    [TestCase("1.4", 10, 2, true, true)]
    [TestCase("1,040", 10, 3, true, true)]
    [TestCase("1.41414141", 10, 2, true, false)]
    public void IsValidNumber_RespectsScale_ReturnsExpected(string value, int precision, int scale, bool onlyPositive,
        bool expected)
    {
        var validator = new NumberValidator(precision, scale, onlyPositive);
        var result = validator.IsValidNumber(value);
        result.Should().Be(expected);
    }
    
    #endregion

    #region Проверка параметра onlyPositive (только положительные)

    [TestCase("-4.59", 4, 2, false, true)]
    [TestCase("-6", 2, 0, true, false)]
    [TestCase("+5", 2, 0, false, true)]
    public void IsValidNumber_RespectsOnlyPositive_ReturnsExpected(string value, int precision, int scale, bool onlyPositive,
        bool expected)
    {
        var validator = new NumberValidator(precision, scale, onlyPositive);
        var result = validator.IsValidNumber(value);
        result.Should().Be(expected);
    }
    
    #endregion
}