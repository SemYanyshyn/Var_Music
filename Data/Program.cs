using System; // writeline і datetime
using System.Collections.Generic; // для IEnumerable 
using System.Globalization; // формат для дат і чисел
using System.Linq; // для запитів
using System.Xml.Linq; // щоб писати просто XELEMENT скорочено а не повну назву + для XML дерев

namespace DiagnosticCenter // для об'єднаннясів в проекті
{
  public static class DiagnosticProcessor
{
    public static XElement GetTaskA(
        IEnumerable<XElement> recordsTree, //передається список xml елементів (будь-який XML має структуру xml дерева). 
        IEnumerable<XElement> examinationsTree, // а ось ці ...Tree це просто параметри, але параметри задаються в запитах нижче де виклик GetTaskA
        IEnumerable<XElement> doctorsTree,
        IEnumerable<XElement> categoriesTree)
    {
      var query = from r in recordsTree
                  // Джоїн по сирих тегах
                  join e in examinationsTree on (int)r.Element("ExaminationId") equals (int)e.Element("Id")
                  // Групування за цифрою з тегу
                  group new { r, e } by (int)e.Element("DoctorId") into docGroup
                  join d in doctorsTree on docGroup.Key equals (int)d.Element("Id") //docGroup.Key — це DoctorId. знайди лікаря для цієї групи обстежень.
                  orderby (string)d.Element("Surname")
                  select new XElement("Doctor",
                      new XAttribute("Surname", (string)d.Element("Surname")),

                      from item in docGroup
                      group item by (int)item.e.Element("CategoryId") into catGroup
                      join c in categoriesTree on catGroup.Key equals (int)c.Element("Id")
                      orderby (string)c.Element("Name")
                      select new XElement("Category",
                          new XAttribute("Name", (string)c.Element("Name")),
                          new XAttribute("Count", catGroup.Count()) //у категорію запиши, скільки разів лікар виконав це обстеження
                      )
                  );

      return new XElement("TaskA", query);
    }

    public static XElement GetTaskB(
        IEnumerable<XElement> recordsTree,
        IEnumerable<XElement> examinationsTree,
        IEnumerable<XElement> categoriesTree,
        IEnumerable<XElement> patientsTree)
    {
      var query = from r in recordsTree
                    // Парсимо дату прямо з тегу
                  let recDate = DateTime.ParseExact((string)r.Element("Date"), "yyyy-MM-dd", CultureInfo.InvariantCulture)
                  join e in examinationsTree on (int)r.Element("ExaminationId") equals (int)e.Element("Id")
                  group new { r, e } by recDate.Month into monthGroup
                  orderby monthGroup.Key
                  select new XElement("Month",
                      new XAttribute("Number", monthGroup.Key),

                      from mItem in monthGroup
                      group mItem by (int)mItem.e.Element("CategoryId") into catGroup
                      join c in categoriesTree on catGroup.Key equals (int)c.Element("Id")
                      orderby (string)c.Element("Name")

                      // Рахуємо витрати прямо з гілок дерева
                      let patientSpends = from cItem in catGroup
                                          group cItem by (int)cItem.r.Element("PatientId") into patGroup
                                          join p in patientsTree on patGroup.Key equals (int)p.Element("Id")
                                          select new
                                          {
                                            Surname = (string)p.Element("Surname"),
                                            TotalSpent = patGroup.Count() * (decimal)c.Element("Price")
                                          }

                      let maxSpent = patientSpends.Max(x => x.TotalSpent)

                      select new XElement("Category",
                          new XAttribute("Name", (string)c.Element("Name")),
                          new XAttribute("MaxSpent", maxSpent),

                          from ps in patientSpends
                          where ps.TotalSpent == maxSpent
                          orderby ps.Surname
                          select new XElement("Patient",
                              new XAttribute("Surname", ps.Surname)
                          )
                      )
                  );

      return new XElement("TaskB", query);
    }
  }
  class Program
  {
    static void Main()
    {
      var doctors = XDocument.Load("doctors.xml").Descendants("Doctor");
      var patients = XDocument.Load("patients.xml").Descendants("Patient");
      var categories = XDocument.Load("categories.xml").Descendants("Category");
      var examinations = XDocument.Load("examinations.xml").Descendants("Examination");

      var records = XDocument.Load("records1.xml").Descendants("Record")
          .Concat(XDocument.Load("records2.xml").Descendants("Record")); //Робить 1 список XML об'єктів(XML дерева)

      var taskA = DiagnosticProcessor.GetTaskA(records, examinations, doctors, categories);
      var taskB = DiagnosticProcessor.GetTaskB(records, examinations, categories, patients);

      var finalXmlTree = new XElement("Lox", taskA, taskB);
      finalXmlTree.Save("results.xml");

      Console.WriteLine("Файл results.xml успiшно згенеровано на деревах!");
    }
  }
}
