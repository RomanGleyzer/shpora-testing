using FluentAssertions;
using NUnit.Framework;

namespace HomeExercise.Tasks.NumberValidator;

[TestFixture]
public class NumberValidatorTests
{
    [TestCase(-1, 2, true, TestName = "Исключение, когда precision отрицательный")]
    [TestCase(0, 2, true, TestName = "Исключение, когда precision равен нулю")]
    [TestCase(5, -1, true, TestName = "Исключение, когда scale отрицательный")]
    [TestCase(1, 2, true, TestName = "Исключение, когда scale больше precision")]
    [TestCase(1, 1, true, TestName = "Исключение исключение, когда scale равен precision")]
    public void Ctor_Should_Throw_On_Invalid_Arguments(int precision, int scale, bool onlyPositive)
    {
        var act = () => new NumberValidator(precision, scale, onlyPositive);
        act.Should().Throw<ArgumentException>();
    }

    [TestCase(1, 0, true, TestName = "Не выбрасывает исключение на корректных элемент")]
    public void Ctor_Should_Not_Throw_On_Valid_Arguments(int precision, int scale, bool onlyPositive)
    {
        var act = () => new NumberValidator(precision, scale, onlyPositive);
        act.Should().NotThrow();
    }

    [TestCase("", 17, 2, true, TestName = "Возвращает false, когда значение пустое")]
    [TestCase(null, 17, 2, true, TestName = "Возвращает false, когда значение null")]
    public void Should_Return_False_On_NullOrEmpty(string value, int precision, int scale, bool onlyPositive)
    {
        var validator = new NumberValidator(precision, scale, onlyPositive);
        var result = validator.IsValidNumber(value);
        result.Should().BeFalse();
    }

    [TestCase("1..2", 10, 4, true,
        TestName = "Возвращает false, когда число содержит несколько разделителей дробной части")]
    [TestCase("+-.", 15, 1, true, TestName = "Возвращает false, когда строка содержит только знаки")]
    [TestCase(".", 22, 8, true, TestName = "Возвращает false, когда строка состоит только из точки")]
    [TestCase("1.", 9, 4, true, TestName = "Возвращает false, когда число оканчивается точкой")]
    [TestCase(".1", 7, 3, true, TestName = "Возвращает false, когда число начинается с точки")]
    [TestCase("5 2", 17, 6, true, TestName = "Возвращает false, когда число содержит пробел")]
    [TestCase("4_4", 11, 3, true, TestName = "Возвращает false, когда число содержит символ подчёркивания")]
    [TestCase("abcd_4", 10, 5, true, TestName = "Возвращает false, когда строка содержит буквы и подчёркивание")]
    [TestCase("--1", 12, 2, true, TestName = "Возвращает false, когда число содержит двойной минус")]
    public void Should_Return_False_On_InvalidFormat(string value, int precision, int scale, bool onlyPositive)
    {
        var validator = new NumberValidator(precision, scale, onlyPositive);
        var result = validator.IsValidNumber(value);
        result.Should().BeFalse();
    }

    [TestCase("+1", 4, 2, true, TestName = "Возвращает true, когда целое число со знаком плюс")]
    [TestCase("+0.1", 9, 1, true, TestName = "Возвращает true, когда дробное число со знаком плюс")]
    [TestCase("000.10", 5, 2, true,
        TestName = "Возвращает true, когда число с ведущими нулями и точкой в качестве разделителя")]
    [TestCase("000,10", 8, 4, true,
        TestName = "Возвращает true, когда число с ведущими нулями и запятой в качестве разделителя")]
    [TestCase("10", 2, 0, true, TestName = "Возвращает true, когда число целое и точно соответствует precision")]
    public void Should_Return_True_On_ValidFormat(string value, int precision, int scale, bool onlyPositive)
    {
        var validator = new NumberValidator(precision, scale, onlyPositive);
        var result = validator.IsValidNumber(value);
        result.Should().BeTrue();
    }

    [TestCase("17.9", 3, 1, true, true, TestName = "возвращает true, когда общее количество цифр равно precision")]
    [TestCase("171.9", 3, 1, true, false, TestName = "возвращает false, когда общее количество цифр больше precision")]
    [TestCase("+17", 3, 2, true, true, TestName = "возвращает true, когда знак плюс + цифры = precision")]
    [TestCase("+17", 2, 0, true, false, TestName = "возвращает false, когда знак плюс превышает precision")]
    public void IsValidNumber_Should_Respect_Precision_Limit(string value, int precision, int scale,
        bool onlyPositive, bool expected)
    {
        var validator = new NumberValidator(precision, scale, onlyPositive);
        var result = validator.IsValidNumber(value);
        result.Should().Be(expected);
    }

    [TestCase("1.0", 10, 0, true, false, TestName = "возвращает false, когда scale равно 0, но есть дробная часть")]
    [TestCase("1.44", 10, 2, true, true, TestName = "возвращает true, когда длина дробной части равна scale")]
    [TestCase("1.4", 10, 2, true, true, TestName = "возвращает true, когда длина дробной части меньше scale")]
    [TestCase("1,040", 10, 3, true, true,
        TestName = "возвращает true, когда дробная часть с ведущими нулями помещается в scale")]
    [TestCase("1.41414141", 10, 2, true, false,
        TestName = "возвращает false, когда длина дробной части больше scale")]
    public void IsValid_Respects_Scale(string input, int precision, int scale,
        bool onlyPositive, bool expected)
    {
        var validator = new NumberValidator(10, scale, onlyPositive);
        var result = validator.IsValidNumber(input);
        result.Should().Be(expected);
    }

    [TestCase("-4.59", 4, 2, false, true,
        TestName = "возвращает true для отрицательного числа при onlyPositive равно false")]
    [TestCase("-6", 2, 0, true, false,
        TestName = "возвращает false для отрицательного числа при onlyPositive равно true")]
    [TestCase("+5", 2, 0, false, true,
        TestName = "возвращает true для положительного числа со знаком плюс, когда onlyPositive равно false")]
    public void IsValid_Respects_OnlyPositive(string input, int precision, int scale,
        bool onlyPositive, bool expected)
    {
        var validator = new NumberValidator(precision, scale, onlyPositive);
        var result = validator.IsValidNumber(input);
        result.Should().Be(expected);
    }
}