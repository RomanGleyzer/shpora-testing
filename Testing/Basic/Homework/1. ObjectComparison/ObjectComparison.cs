using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace HomeExercise.Tasks.ObjectComparison;

public class TsarComparison
{
    [Test]
    [Description("Проверка текущего царя")]
    [Category("ToRefactor")]
    public void CheckCurrentTsar()
    {
        var actualTsar = TsarRegistry.GetCurrentTsar();

        var expectedTsar = new Person(
            name: "Ivan IV The Terrible",
            age: 54,
            height: 170,
            weight: 70,
            parent: new Person(
                name: "Vasili III of Russia",
                age: 28,
                height: 170,
                weight: 60,
                parent: null!
            )
        );

        /// <summary>
        /// Такой подход автоматически проверяет все свойства объекта, включая вложенные
        /// Тест не будет требовать изменений при добавлении новых свойств в класс Person
        /// При падении теста, FluentAssertions предоставляет подробную информацию о том, какие именно свойства не совпали
        /// </summary>

        actualTsar.Should().BeEquivalentTo(expectedTsar, options => options.Excluding(t => t.Id).Excluding(t => t.Parent.Id));
    }

    [Test]
    [Description("Альтернативное решение. Какие у него недостатки?")]
    public void CheckCurrentTsar_WithCustomEquality()
    {
        var actualTsar = TsarRegistry.GetCurrentTsar();
        
        var expectedTsar = new Person(
            name: "Ivan IV The Terrible",
            age: 54,
            height: 170,
            weight: 70,
            new Person(
                name: "Vasili III of Russia",
                age: 28,
                height: 170,
                weight: 60,
                parent: null!
            )
        );
        
        // Какие недостатки у такого подхода?
        // Недостатки подхода:
        // 1) Требуется ручное обновление метода сравнения при добавлении новых свойств в класс Person,
        // 2) Тест менее информативен при падении. Он просто укажет, что объекты не равны, не показывая какие именно свойства не совпали.
        ClassicAssert.True(AreEqual(actualTsar, expectedTsar));
        
        // Вызываем Stack Overflow exception
        // var p1 = new Person("A", 1, 1, 1, null!);
        // p1.Parent = p1;
        //
        // var p2 = new Person("A", 1, 1, 1, null!);
        // p2.Parent = p2;
        //
        // ClassicAssert.True(AreEqual(p1, p2));
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