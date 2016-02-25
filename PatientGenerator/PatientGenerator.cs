using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Patient;
using System.IO;
using System.Reflection;
using System.Xml;

namespace PatientGenerator
{
  public class PatientGenerator : IPatientGenerator
  {
    #region Constants

    private const string DocumentTag = "Document";
    private const string DocumentTitleAttribName = "Title";
    private const string DocumentVersionAttribName = "Version";
    private const string DocumentTitleValue = "Hospital Ids info for simulation mode";
    private const string DocumentVersionValue = "1.0";
    private const string HospitalsTag = "Hospitals";
    private const string HospitalTag = "Hospital";
    private const string IdTag = "Id";
    private const string HostpitalIdsFile = "HospitalIds.xml";

    #endregion

    #region Attributes

    private IList<string> HospitalIds;

    #endregion

    #region Constructor

    public PatientGenerator()
    {
      HospitalIds = new List<string>();

      var xmlFileDir = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
      if (xmlFileDir == null)
      {
        throw new DirectoryNotFoundException("Could not find the specified directory");
      }
      var xmlFilepath = Path.Combine(xmlFileDir, HostpitalIdsFile);
      ReadXmlFile(xmlFilepath);
    }

    #endregion

    #region IPatientLeaving implementation

    public IPatientArrival GeneratePatientArrival()
    {
      return new PatientArrival(GeneratorHelper.RandomUpperChars(4) + GeneratorHelper.RandomNumericalChars(8),
                                HospitalIds[GeneratorHelper.RandomNumericalValue(HospitalIds.Count)],
                                DateTime.Now,
                                DiseaseType.Influenza);
    }

    public IPatientCare GeneratePatientCare()
    {
      return new PatientCare("", "", DateTime.Now, "");
    }

    public IPatientLeaving GeneratePatientLeaving()
    {
      return new PatientLeaving("", "", DateTime.Now);
    }

    #endregion

    #region Private Functions

    private void ReadXmlFile(string xmlFilepath)
    {
      if (!File.Exists(xmlFilepath))
      {
        throw new FileNotFoundException(string.Format("Could not find file: {0}", xmlFilepath));
      }

      // First, try to create an XmlReader for the specified file path.
      using (var xmlReader = XmlReader.Create(xmlFilepath))
      {
        var documentTagFound = false;
        var hospitalsTagFound = false;
        var hospitalTagFound = false;
        var docTitle = string.Empty;
        var docVersion = string.Empty;

        // Parse the file and collect required attributes and values. 
        while (xmlReader.Read())
        {
          if (xmlReader.IsStartElement())
          {
            switch (xmlReader.Name)
            {
              case DocumentTag:
                documentTagFound = true;
                // if an attribute is not found, the value will be Null
                docTitle = xmlReader[DocumentTitleAttribName];
                docVersion = xmlReader[DocumentVersionAttribName];
                break;
              case HospitalsTag:
                hospitalsTagFound = true;
                break;
              case HospitalTag:
                hospitalTagFound = true;
                HospitalIds.Add(ReadHospitalId(xmlReader));
                break;
            }
          }
        }

        // Validate the information retrieved and throw if missing or invalid.
        if (!documentTagFound)
        {
          throw new ApplicationException("Unrecognized XML file format provided: No Document Tag Found!");
        }

        if (string.IsNullOrEmpty(docTitle))
        {
          throw new ApplicationException("Unrecognized XML file format provided: Document Tag missing Title attribute!");
        }

        if (string.IsNullOrEmpty(docVersion))
        {
          throw new ApplicationException("Unrecognized XML file format provided: Document Tag missing Version attribute!");
        }

        if (!docTitle.Equals(DocumentTitleValue))
        {
          throw new ApplicationException("Unrecognized XML file format provided: Invalid Document Title!");
        }

        if (!docVersion.Equals(DocumentVersionValue))
        {
          throw new ApplicationException("Unrecognized XML file format provided: Invalid Document Version!");
        }

        if (!hospitalsTagFound)
        {
          throw new ApplicationException("Unrecognized XML file format provided: Hospitals Tag Not Found!");
        }

        if (!hospitalTagFound)
        {
          throw new ApplicationException("Unrecognized XML file format provided: Hospital Tag Not Found!");
        }
      }
    }

    private static string ReadHospitalId(XmlReader xmlReader)
    {
      // Read Id attribute
      var id = xmlReader[IdTag];
      if (id == null)
      {
        throw new ApplicationException("Unrecognized XML file format provided: Id Tag Not Found!!");
      }

      return id;
    }

    #endregion
  }
}
