using System.Collections.Generic;
using System.Xml.Linq;
using Xunit;

public class DiagnosticFixture
{
    public IEnumerable<XElement> Doctors { get; private set; }
    public IEnumerable<XElement> Patients { get; private set; }
    public IEnumerable<XElement> Categories { get; private set; }
    public IEnumerable<XElement> Examinations { get; private set; }
    public IEnumerable<XElement> Records { get; private set; }

    public DiagnosticFixture()
    {
        Doctors = XElement.Parse(@"
<Doctors>
    <Doctor>
        <Id>1</Id>
        <Surname>House</Surname>
    </Doctor>
</Doctors>").Descendants("Doctor");

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

public class DiagnosticProcessorTests : IClassFixture<DiagnosticFixture>
{
    private readonly DiagnosticFixture _fixture;

    public DiagnosticProcessorTests(DiagnosticFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
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