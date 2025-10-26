using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace HomeExercise.Tasks.ObjectComparison;

public class ObjectComparison
{
    [Test]
    [Description("Проверка текущего царя")]
    [Category("ToRefactor")]
    public void CheckCurrentTsar()
    {
        var actualTsar = TsarRegistry.GetCurrentTsar();

        var expectedTsar = new Person("Ivan IV The Terrible", 54, 170, 70,
            new Person("Vasili III of Russia", 28, 170, 60, null));

        /// <summary>
        /// Такой подход автоматически проверяет все свойства объекта, включая вложенные
        /// Тест не будет требовать изменений при добавлении новых свойств в класс Person
        /// При падении теста, FluentAssertions предоставляет подробную информацию о том, какие именно свойства не совпали
        /// </summary>

        actualTsar.Should()
            .BeEquivalentTo(expectedTsar, options => options.Excluding(t => t.Id).Excluding(t => t.Parent.Id));
    }

    [Test]
    [Description("Альтернативное решение. Какие у него недостатки?")]
    public void CheckCurrentTsar_WithCustomEquality()
    {
        var actualTsar = TsarRegistry.GetCurrentTsar();
        var expectedTsar = new Person("Ivan IV The Terrible", 54, 170, 70,
            new Person("Vasili III of Russia", 28, 170, 60, null));

        // Какие недостатки у такого подхода?
        // Недостатки подхода:
        // 1) Требуется ручное обновление метода сравнения при добавлении новых свойств в класс Person,
        // 2) Тест менее информативен при падении. Он просто укажет, что объекты не равны, не показывая какие именно свойства не совпали.
        ClassicAssert.True(AreEqual(actualTsar, expectedTsar));
    }

    private bool AreEqual(Person? actual, Person? expected)
    {
        if (actual == expected) return true;
        if (actual == null || expected == null) return false;
        return
            actual.Name == expected.Name
            && actual.Age == expected.Age
            && actual.Height == expected.Height
            && actual.Weight == expected.Weight
            && AreEqual(actual.Parent, expected.Parent);
    }
}
