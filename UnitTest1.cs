using System.Collections.Generic; // // для IEnumerable 
using System.Xml.Linq; // // щоб писати просто XELEMENT скорочено а не повну назву + для XML дерев
using Xunit; 

public class DiagnosticFixture // Це клас, який зберігає тестові дані для тестів.
{
    // Це властивості, у яких зберігаються тестові XML-дані.
    public IEnumerable<XElement> Doctors { get; private set; }
    // private set — змінити значення можна тільки всередині DiagnosticFixture
    // Doctors — список XML-елементів <Doctor>
    public IEnumerable<XElement> Patients { get; private set; }
    public IEnumerable<XElement> Categories { get; private set; }
    public IEnumerable<XElement> Examinations { get; private set; }
    public IEnumerable<XElement> Records { get; private set; }

    public DiagnosticFixture() // Це конструктор класу DiagnosticFixture 
    {
        Doctors = XElement.Parse(@"
<Doctors>
    <Doctor>
        <Id>1</Id>
        <Surname>House</Surname>
    </Doctor>
</Doctors>").Descendants("Doctor");

        // C# бере цей текст і робить з нього справжнє XML-дерево
        // Ці блоки XML потрібні, щоб мати тестові дані прямо в тестах, без окремих .xml файлів.
        Patients = XElement.Parse(@" 
<Patients>
    <Patient>
        <Id>1</Id>
        <Surname>Smith</Surname>
    </Patient>
    <Patient>
        <Id>2</Id>
        <Surname>Doe</Surname>
    </Patient>
</Patients>").Descendants("Patient");

        Categories = XElement.Parse(@"
<Categories>
    <Category>
        <Id>1</Id>
        <Name>MRI</Name>
        <Price>1000</Price>
    </Category>
</Categories>").Descendants("Category");

        Examinations = XElement.Parse(@"
<Examinations>
    <Examination>
        <Id>1</Id>
        <CategoryId>1</CategoryId>
        <DoctorId>1</DoctorId>
    </Examination>
</Examinations>").Descendants("Examination");

        Records = XElement.Parse(@"
<Records>
    <Record>
        <Date>2024-01-10</Date>
        <ExaminationId>1</ExaminationId>
        <PatientId>1</PatientId>
    </Record>
    <Record>
        <Date>2024-01-15</Date>
        <ExaminationId>1</ExaminationId>
        <PatientId>1</PatientId>
    </Record>
    <Record>
        <Date>2024-01-20</Date>
        <ExaminationId>1</ExaminationId>
        <PatientId>2</PatientId>
    </Record>
</Records>").Descendants("Record");
    }
}

public class DiagnosticProcessorTests : IClassFixture<DiagnosticFixture> // це клас, у якому будуть тести для DiagnosticProcessor
// : (наслідування) DiagnosticProcessorTests використовує можливість IClassFixture
// IClassFixture — це штука з xUnit, яка каже: створи один об’єкт з тестовими даними і дай його тестовому класу
// Тобто xUnit сам створить: new DiagnosticFixture()
// <DiagnosticFixture> - Це означає який саме клас треба використати як fixture(джерело тестових даних)
{
    private readonly DiagnosticFixture _fixture; // Це поле класу, у якому ми зберігаємо об’єкт DiagnosticFixture.

    public DiagnosticProcessorTests(DiagnosticFixture fixture) // Це конструктор класу DiagnosticProcessorTests
    {
        _fixture = fixture; // xUnit автоматично створює DiagnosticFixture і передає його сюди і Потім ми зберігаємо його(об'єкт) в поле класу
    }

    [Fact] // це позначка для xUnit, що метод який під ним є тестом. Тобто xUnit бачить [Fact] і розуміє: цей метод треба запустити під час тестування.
    // Без [Fact] метод просто існує, але як тест не запуститься.
    public void TaskATest()
    {
        var expected = XElement.Parse(@"
<TaskA>
    <Doctor Surname=""House"">
        <Category Name=""MRI"" Count=""3"" />
    </Doctor>
</TaskA>");

        var result = DiagnosticProcessor.GetTaskA(
            _fixture.Records,
            _fixture.Examinations,
            _fixture.Doctors,
            _fixture.Categories
        );

        Assert.True(XNode.DeepEquals(expected, result), result.ToString());
    }

    [Fact]
    public void TaskBTest()
    {
        var expected = XElement.Parse(@"
<TaskB>
    <Category Name=""MRI"" MaxSpent=""2000"">
        <Patient Surname=""Smith"" />
    </Category>
</TaskB>");

        var result = DiagnosticProcessor.GetTaskB(
            _fixture.Records,
            _fixture.Examinations,
            _fixture.Categories,
            _fixture.Patients
        );

        Assert.True(XNode.DeepEquals(expected, result), result.ToString());
    }
}